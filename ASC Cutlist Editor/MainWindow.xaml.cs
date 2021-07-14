using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using ASC_Cutlist_Editor.Models;
using ExcelDataReader;
using Microsoft.Win32;

namespace ASC_Cutlist_Editor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            CutlistGrid.DataContext = new List<Cutlist>();
        }

        private void ButtonAddName_Click(object sender, RoutedEventArgs e)
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

            // Get the selected file name and display its contents in a DataGrid.
            if (result == true)
            {
                // Needed for .NET core to fix this exception: "No data is available for encoding 1252".
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                using var stream = File.Open(dlg.FileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var reader = ExcelReaderFactory.CreateCsvReader(stream);

                var cutlists = new List<Cutlist>();

                // Skip rows before header
                for (int i = 0; i < 18; i++)
                {
                    if (!reader.Read()) return;
                }

                while (reader.Read())
                {
                    try
                    {
                        // Skip empty rows
                        int qty = int.Parse(reader.GetString(3));
                        if (qty == 0)
                        {
                            continue;
                        }

                        cutlists.Add(new Cutlist
                        {
                            ID = int.Parse(reader.GetString(0)),
                            Name = reader.GetString(1),
                            Length = double.Parse(reader.GetString(2)),
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

                CutlistFilename.Text = Path.GetFileName(dlg.FileName);
                CutlistGrid.DataContext = cutlists;
            }
        }
    }
}