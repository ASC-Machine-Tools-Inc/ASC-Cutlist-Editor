using AscCutlistEditor.Common;
using AscCutlistEditor.Frameworks;
using AscCutlistEditor.Models;
using ExcelDataReader;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace AscCutlistEditor.ViewModels
{
    internal class CutlistViewModel : ObservableObject
    {
        // Method for rendering the part views.
        private readonly Action _drawParts;

        public CutlistViewModel(Action drawParts)
        {
            _drawParts = drawParts;
        }

        // The list of cutlists read in from a given file.
        private ObservableCollection<Cutlist> _cutlists = new ObservableCollection<Cutlist>();

        public ObservableCollection<Cutlist> Cutlists
        {
            get => _cutlists;
            set
            {
                _cutlists = value;
                RaisePropertyChangedEvent("Cutlists");
            }
        }

        // The filename containing the current cutlists.
        private string _filename;

        public string Filename
        {
            get => _filename;
            set
            {
                _filename = value;
                RaisePropertyChangedEvent("Filename");
            }
        }

        // The current visibility of the "import cutlist" button and label.
        private bool _importVisibility = true;

        public bool ImportVisibility
        {
            get => _importVisibility;
            set
            {
                _importVisibility = value;
                RaisePropertyChangedEvent("ImportVisibility");
            }
        }

        public ICommand ImportCutlistCommand => new DelegateCommand(ImportCutlist);

        /**
         * Prompt the user to select a file in a valid cutlist format and read
         * its contents into the program.
         */

        private void ImportCutlist()
        {
            OpenFileDialog dlg = new OpenFileDialog
            {
                // Set filter for file extension and default file extension.
                DefaultExt = ".csv",
                Filter = "CSV Files (*.csv)|*.csv"
            };

            bool? result = dlg.ShowDialog();
            if (result != true) return;

            Filename = Path.GetFileName(dlg.FileName);

            try
            {
                // Reset the cutlists.
                Cutlists = new ObservableCollection<Cutlist>();

                ParseCsv(dlg);

                // If successful, hide the old button.
                ImportVisibility = false;
            }
            catch (Exception)  // Catch File and FileFormat exceptions
            {
                MessageBox.Show(
                    "The chosen file could not be parsed properly.",
                    "ASC Cutlist Editor", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                Console.WriteLine("Format parsing error.");
                return;
            }

            // Send the call up to the main viewmodel for drawing the 2D parts.
            _drawParts.Invoke();
        }

        /**
         * Process the given file, checking that it's in a valid cutlist format
         * and if so, copy its contents into the list of cutlists.
         *
         * @param  dlg the OpenFileDialog to read data in from after choosing a file.
         */

        private async void ParseCsv(OpenFileDialog dlg)
        {
            // Needed for .NET core to fix this exception: "No data is available for encoding 1252".
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            using FileStream stream = File.Open(dlg.FileName, FileMode.Open,
                FileAccess.Read, FileShare.ReadWrite);
            using IExcelDataReader reader = ExcelReaderFactory.CreateCsvReader(stream);

            reader.Read();
            string header = reader.GetString(0);
            switch (header)
            {
                case "HEADER 1:":  // Bryan's format.
                    await ParseCsvIntoCutlists(reader, 0);
                    break;

                case "CUTLIST":  // Andrew's format.
                    await ParseCsvIntoCutlists(reader, 1);
                    break;

                default:  // Invalid format.
                    throw new FileFormatException();
            }
        }

        /**
         * Populate the cutlists from the reader using the given cutlist file format.
         * See BryanCutlist.csv and AndrewCutlist.csv under ExampleCutlists.
         * (These correspond to the format parameters 0 and 1, respectively).
         *
         * @param  reader The IExcelDataReader instance that  contains the cutlist to process.
         */

        private async Task ParseCsvIntoCutlists(IExcelDataReader reader, int format)
        {
            // Skip some rows at the start based off the given format.
            int rowsToSkip = 0;
            switch (format)
            {
                case 0:
                    rowsToSkip = 17;
                    break;

                case 1:
                    rowsToSkip = 1;
                    break;
            }

            for (int i = 0; i < rowsToSkip; i++)
            {
                if (!reader.Read()) return;
            }

            while (reader.Read())
            {
                Cutlist cutlist = await Task.Run(() => ParseCsvIntoCutlistsHelper(reader, format));
                if (cutlist != null)
                {
                    Cutlists.Add(cutlist);
                }
            }
        }

        private Cutlist ParseCsvIntoCutlistsHelper(IExcelDataReader reader, int format)
        {
            Cutlist cutlist = null;

            switch (format)
            {
                case 0:
                    // Skip empty rows in the file.
                    int qty = int.Parse(reader.GetString(3));
                    if (qty == 0)
                    {
                        break;
                    }

                    cutlist = new Cutlist
                    {
                        ID = int.Parse(reader.GetString(0)),
                        Name = reader.GetString(1),
                        Length = Math.Round(LengthParser.ParseString(reader.GetString(2)), 2),
                        Quantity = qty,
                        Made = int.Parse(reader.GetString(4)),
                        Left = int.Parse(reader.GetString(5)),
                        Bundle = int.Parse(reader.GetString(6))
                    };
                    break;

                case 1:
                    cutlist = new Cutlist
                    {
                        ID = int.Parse(reader.GetString(0)),
                        Length = double.Parse(reader.GetString(5)),
                        Quantity = int.Parse(reader.GetString(6)),
                        Made = int.Parse(reader.GetString(7)),
                        Bundle = int.Parse(reader.GetString(8))
                    };

                    // Skip feed info.
                    for (int i = 0; i < 11; i++)
                    {
                        reader.Read();
                    }
                    break;
            }

            return cutlist;
        }
    }
}