using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
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
        private List<Cutlist> _cutlists = new List<Cutlist>();
        private string _filename;
        private readonly Action _drawParts;

        public CutlistViewModel(Action drawParts)
        {
            _drawParts = drawParts;
        }

        public List<Cutlist> Cutlists
        {
            get => _cutlists;
            set
            {
                _cutlists = value;
                RaisePropertyChangedEvent("Cutlists");
            }
        }

        public string Filename
        {
            get => _filename;
            set
            {
                _filename = value;
                RaisePropertyChangedEvent("Filename");
            }
        }

        public ICommand ImportCutlistCommand => new DelegateCommand(ImportCutlist);

        private void ImportCutlist()
        {
            // Create OpenFileDialog.
            OpenFileDialog dlg = new OpenFileDialog
            {
                // Set filter for file extension and default file extension.
                DefaultExt = ".csv",
                Filter = "CSV Files (*.csv)|*.csv"
            };

            // Display OpenFileDialog by calling ShowDialog method.
            bool? result = dlg.ShowDialog();

            // Get the selected file name and display its contents in a DataGrid,
            // but exit if a file wasn't selected.
            if (result != true) return;

            Filename = Path.GetFileName(dlg.FileName);
            ParseCsv(dlg);

            // Send the call up to the main viewmodel for drawing the 2D parts.
            _drawParts.Invoke();
        }

        private void ParseCsv(OpenFileDialog dlg)
        {
            // Needed for .NET core to fix this exception: "No data is available for encoding 1252".
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            using FileStream stream = File.Open(dlg.FileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using IExcelDataReader reader = ExcelReaderFactory.CreateCsvReader(stream);

            var cutlists = new List<Cutlist>();

            // Skip rows before the cutlist header.
            for (int i = 0; i < 18; i++)
            {
                if (!reader.Read()) return;
            }

            while (reader.Read())
            {
                try
                {
                    // Skip empty rows in the file.
                    // TODO: handle different formats
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
                    // TODO: handle some kind of error for empty cutlists?
                    MessageBox.Show(
                        "The chosen file could not be parsed properly.",
                        "ASC Cutlist Editor", MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    Console.WriteLine("Format parsing error.");
                    return;
                }
            }

            Cutlists = cutlists;
        }
    }
}