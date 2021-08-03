using AscCutlistEditor.Common;
using AscCutlistEditor.Frameworks;
using AscCutlistEditor.Models;
using ExcelDataReader;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
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
        private List<Cutlist> _cutlists = new List<Cutlist>();

        public List<Cutlist> Cutlists
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

        // Prompt the user to select a file in a valid cutlist format and read
        // its contents into the program.
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
            ParseCsv(dlg);

            // Send the call up to the main viewmodel for drawing the 2D parts.
            _drawParts.Invoke();

            // If everything was successful, hide the main button for importing cutlists.
            ImportVisibility = false;
        }

        // Process the given file, checking that it's in a valid cutlist format
        // and if so, copy its contents into the list of cutlists.
        private void ParseCsv(OpenFileDialog dlg)
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
                    ParseCsvFormat(reader, 0);
                    break;

                case "CUTLIST":  // Andrew's format.
                    ParseCsvFormat(reader, 1);
                    break;

                default:  // Invalid format.
                    MessageBox.Show(
                        "The chosen file's format could not be parsed properly.",
                        "ASC Cutlist Editor",
                        button: MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    Console.WriteLine("Format parsing error.");
                    break;
            }
        }

        /**
         * Populate the cutlists from the reader using a given cutlist file format.
         * @param  reader The IExcelDataReader instance that  contains the cutlist to process.
         * @param  format The int representing the format to parse the file through.
         *              - 0: Bryan's file format. See BryanCutlist.csv under ExampleCutlists.
         *              - 1: Andrew's file format. See AndrewCutlist.csv under ExampleCutlists.
         */

        private void ParseCsvFormat(IExcelDataReader reader, int format)
        {
            if (format != 0 && format != 1)
            {
                throw new ArgumentException("Invalid format parameter!");
            }

            var cutlists = new List<Cutlist>();

            switch (format)
            {
                case 0:  // Bryan's format.
                    // Skip rows before the cutlists.
                    for (int i = 0; i < 17; i++)
                    {
                        if (!reader.Read()) return;
                    }

                    while (reader.Read())
                    {
                        try
                        {
                            // Skip empty rows in the file.
                            int qty = int.Parse(reader.GetString(3));
                            if (qty == 0)
                            {
                                continue;
                            }

                            cutlists.Add(new Cutlist
                            {
                                ID = int.Parse(reader.GetString(0)),
                                Name = reader.GetString(1),
                                Length = Math.Round(LengthParser.ParseString(reader.GetString(2)), 2),
                                Quantity = qty,
                                Made = int.Parse(reader.GetString(4)),
                                Left = int.Parse(reader.GetString(5)),
                                Bundle = int.Parse(reader.GetString(6)),
                            });
                        }
                        catch (FormatException)
                        {
                            MessageBox.Show(
                                "The chosen file could not be parsed properly.",
                                "ASC Cutlist Editor", MessageBoxButton.OK,
                                MessageBoxImage.Error);
                            Console.WriteLine("Format parsing error.");
                            return;
                        }
                    }
                    break;

                case 1:  // Andrew's format.
                    // Skip row before the cutlists.
                    reader.Read();

                    while (reader.Read())
                    {
                        try
                        {
                            cutlists.Add(new Cutlist
                            {
                                ID = int.Parse(reader.GetString(0)),
                                Length = double.Parse(reader.GetString(5)),
                                Quantity = int.Parse(reader.GetString(6)),
                                Made = int.Parse(reader.GetString(7)),
                                Bundle = int.Parse(reader.GetString(8)),
                            });

                            // Skip feed info.
                            for (int i = 0; i < 11; i++)
                            {
                                reader.Read();
                            }
                        }
                        catch (FormatException)
                        {
                            MessageBox.Show(
                                "The chosen file could not be parsed properly.",
                                "ASC Cutlist Editor", MessageBoxButton.OK,
                                MessageBoxImage.Error);
                            Console.WriteLine("Format parsing error.");
                            return;
                        }
                    }
                    break;
            }

            Cutlists = cutlists;
        }
    }
}