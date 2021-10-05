using AscCutlistEditor.Frameworks;
using AscCutlistEditor.ViewModels.Cutlists;
using AscCutlistEditor.ViewModels.MQTT;
using AscCutlistEditor.ViewModels.Parts;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using AscCutlistEditor.Utility.MQTT;

namespace AscCutlistEditor.ViewModels
{
    internal class MainViewModel : ObservableObject
    {
        // Collection that tracks the visibility of the UI elements.
        // Visibility order: Cutlist, 2D, 3D.
        public ObservableCollection<bool> UiVisibility { get; } =
            new ObservableCollection<bool>(new[] { true, true, true });

        public CutlistImportViewModel CutlistViewModel { get; }

        public PartCollectionViewModel PartCollectionViewModel { get; }

        public MachineConnectionsViewModel MachineConnectionsViewModel { get; }

        public MockMachineData MockMachineData { get; }

        public SqlConnectionViewModel SqlConnectionViewModel { get; }

        public MainViewModel()
        {
            CutlistViewModel = new CutlistImportViewModel(DrawParts);

            PartCollectionViewModel = new PartCollectionViewModel();

            MachineConnectionsViewModel = new MachineConnectionsViewModel();

            MockMachineData = new MockMachineData(MachineConnectionsViewModel);

            SqlConnectionViewModel = new SqlConnectionViewModel();
        }

        /// <summary>
        /// Toggles the cutlist and its corresponding splitter's visibility.
        /// </summary>
        public ICommand ToggleCutlistCommand => new DelegateCommand(() => ToggleView(0));

        /// <summary>
        /// Toggles the flat part view and its corresponding splitters' visibility.
        /// </summary>
        public ICommand ToggleFlatViewCommand => new DelegateCommand(() => ToggleView(1));

        /// <summary>
        /// Toggles the 3D view and its corresponding splitter's visibility.
        /// </summary>
        public ICommand Toggle3DCommand => new DelegateCommand(() => ToggleView(2));

        /// <summary>
        /// Draw the 2D and 3D views from the current cutlist.
        /// </summary>
        public ICommand DrawPartsCommand => new DelegateCommand(DrawParts);

        /// <summary>
        /// Remove the current loaded cutlist, clearing the UI.
        /// </summary>
        public ICommand ClearCutlistCommand => new DelegateCommand(ClearCutlist);

        /// <summary>
        /// Start listening for machine connections.
        /// </summary>
        public ICommand StartListeningCommand => new DelegateCommand(StartListening);

        /// <summary>
        /// Create a new mock connection with MockMachineData to listen to.
        /// </summary>
        public ICommand AddMockConnectionCommand => new DelegateCommand(AddMockConnection);

        public ICommand ConnectToSqlServerCommand => new DelegateCommand(ConnectToSqlServer);

        private void ToggleView(int index)
        {
            UiVisibility[index] = !UiVisibility[index];
        }

        /// <summary>
        /// Catch the request from the cutlist view model to draw the parts
        /// after parsing a valid csv.
        /// </summary>
        private void DrawParts()
        {
            PartCollectionViewModel.CreateRows(
                CutlistViewModel.Cutlists);
        }

        /// <summary>
        /// Clear the cutlist and all UI elements drawn from it.
        /// </summary>
        private void ClearCutlist()
        {
            CutlistViewModel.ClearUi();
            PartCollectionViewModel.ClearUi();
        }

        private void StartListening()
        {
            // TODO: debug this, what if we add mock connections before we start listening??
            MachineConnectionsViewModel.Start();
        }

        private void AddMockConnection()
        {
            MockMachineData.AddMockClient();
        }

        private void ConnectToSqlServer()
        {
            MessageBox.Show("Yo!");
            SqlConnectionViewModel conn = new SqlConnectionViewModel();
            conn.CreateConnectionString(
                (string)SqlConnectionViewModel["DataSource"],
                (string)SqlConnectionViewModel["DatabaseName"],
                (string)SqlConnectionViewModel["Username"],
                (string)SqlConnectionViewModel["Password"]
                );
        }
    }
}