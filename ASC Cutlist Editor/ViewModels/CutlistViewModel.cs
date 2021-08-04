using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using AscCutlistEditor.Common;
using AscCutlistEditor.Frameworks;
using AscCutlistEditor.Models;
using ExcelDataReader;
using Microsoft.Win32;

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

        /**
         * Event handler for importing CSVs into cutlists.
         */
        public ICommand ImportCutlistCommand => new DelegateCommand(ImportCutlistCsv);

        private async void ImportCutlistCsv()
        {
            await ImportCutlistCsvAsync();
        }

        /**
         * Prompt the user to select a file in a valid cutlist format and read
         * its contents into the program.
         */

        private async Task ImportCutlistCsvAsync()
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

                await ParseCutlistCsvAsync(dlg);

                // If successful, hide the import button.
                ImportVisibility = false;
            }
            catch (Exception)  // Catch File and FileFormat exceptions.
            {
                MessageBox.Show(
                    "The chosen file could not be parsed properly.",
                    "ASC Cutlist Editor", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                Console.WriteLine("Format parsing error.");
            }
        }

        /**
         * Process the given file, checking that it's in a valid cutlist format
         * and if so, copy its contents into the list of cutlists.
         *
         * See BryanCutlist.csv and AndrewCutlist.csv under ExampleCutlists for
         * cutlist formats.
         *
         * @param  dlg the OpenFileDialog to read data in from after choosing a file.
         */

        private async Task ParseCutlistCsvAsync(OpenFileDialog dlg)
        {
            // Needed for .NET core to fix this exception: "No data is available for encoding 1252".
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            using FileStream stream = File.Open(dlg.FileName, FileMode.Open,
                FileAccess.Read, FileShare.ReadWrite);
            using IExcelDataReader reader = ExcelReaderFactory.CreateCsvReader(stream);

            int rowsToSkip = 0;
            reader.Read();  // Advance reader to first row.

            string header = reader.GetString(0);
            switch (header)
            {
                case "HEADER 1:":  // Bryan's format.
                    rowsToSkip = 17;
                    break;

                case "CUTLIST":  // Andrew's format.
                    rowsToSkip = 1;
                    break;

                default:  // Invalid format.
                    throw new FileFormatException();
            }

            // Skip some rows at the start based off the given format.
            for (int i = 0; i < rowsToSkip; i++)
            {
                if (!reader.Read()) return;
            }

            while (reader.Read())
            {
                // Asynchronously load in the cutlists from the file.
                Cutlist cutlist = await Task.Run(() => ParseCutlistCsvHelper(reader, header));
                if (cutlist != null)
                {
                    Cutlists.Add(cutlist);
                }
            }

            // Send the call up to the main viewmodel
            // for drawing the 2D parts after parsing.
            _drawParts.Invoke();
        }

        /**
         * Do the dirty work of parsing the file into neat Cutlists.
         */

        private Cutlist ParseCutlistCsvHelper(IExcelDataReader reader, string header)
        {
            Cutlist cutlist = null;

            switch (header)
            {
                case "HEADER 1:":  // Bryan's format.
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

                case "CUTLIST":  // Andrew's format.
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