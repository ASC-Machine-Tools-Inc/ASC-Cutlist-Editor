using AscCutlistEditor.Frameworks;
using AscCutlistEditor.Models;
using ExcelDataReader;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace AscCutlistEditor.ViewModels.Cutlists
{
    internal class CutlistImportViewModel : ObservableObject
    {
        private ObservableCollection<Cutlist> _cutlists = new ObservableCollection<Cutlist>();
        private readonly Action _drawParts;
        private string _filename;
        private bool _importVisibility = true;

        /// <summary>
        /// Class <c>CutlistImportViewModel</c> handles all the UI controls for cutlists.
        /// Also handles file opening - all logic kept in CutlistParseViewModel.
        /// </summary>
        /// <param name="drawParts">Action for rendering the part views.</param>
        public CutlistImportViewModel(Action drawParts)
        {
            _drawParts = drawParts;
        }

        /// <summary>
        /// The list of cutlists read in from a given file.
        /// </summary>
        public ObservableCollection<Cutlist> Cutlists
        {
            get => _cutlists;
            set
            {
                _cutlists = value;
                RaisePropertyChangedEvent("Cutlists");
            }
        }

        /// <summary>
        /// The filename containing the current cutlists.
        /// </summary>
        public string Filename
        {
            get => _filename;
            set
            {
                _filename = value;
                RaisePropertyChangedEvent("Filename");
            }
        }

        /// <summary>
        /// The current visibility of the "import cutlist" button and label.
        /// </summary>
        public bool ImportVisibility
        {
            get => _importVisibility;
            set
            {
                _importVisibility = value;
                RaisePropertyChangedEvent("ImportVisibility");
            }
        }

        /// Event handler for importing CSVs into cutlists.
        public ICommand ImportCutlistCommand => new DelegateCommand(ImportCutlistCsv);

        private async void ImportCutlistCsv()
        {
            OpenFileDialog dlg = new OpenFileDialog
            {
                // Set filter for file extension and default file extension.
                DefaultExt = ".csv",
                Filter = "CSV Files (*.csv)|*.csv"
            };

            await ImportCutlistCsvAsync(dlg);
        }

        /// Prompt the user to select a file in a valid cutlist format and read
        /// its contents into the program.
        private async Task ImportCutlistCsvAsync(OpenFileDialog dlg)
        {
            bool? result = dlg.ShowDialog();
            if (result != true) return;

            Filename = Path.GetFileName(dlg.FileName);

            try
            {
                // Needed for .NET core to fix this exception:
                // "No data is available for encoding 1252".
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                // Create the reader from the chosen file.
                await using FileStream stream = File.Open(dlg.FileName, FileMode.Open,
                    FileAccess.Read, FileShare.ReadWrite);
                using IExcelDataReader reader = ExcelReaderFactory.CreateCsvReader(stream);

                Cutlists = await CutlistParseViewModel.ParseCutlistCsvAsync(reader);

                // If successful, update the UI. Send the call up to the
                // main viewmodel for drawing the 2D parts after parsing.
                _drawParts.Invoke();
                ImportVisibility = false;
            }
            catch (Exception)  // Catch File and FileFormat exceptions.
            {
                MessageBox.Show(
                    "The chosen file could not be parsed properly.",
                    "ASC Cutlist Editor", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                Console.WriteLine("Format parsing error.");
                Filename = null;
            }
        }
    }
}