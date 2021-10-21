using AscCutlistEditor.Frameworks;
using AscCutlistEditor.Utility.MQTT;
using AscCutlistEditor.ViewModels.Cutlists;
using AscCutlistEditor.ViewModels.MQTT;
using AscCutlistEditor.ViewModels.Parts;
using System.Collections.ObjectModel;
using System.Windows.Input;

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

            SqlConnectionViewModel = new SqlConnectionViewModel();

            MachineConnectionsViewModel = new MachineConnectionsViewModel(SqlConnectionViewModel);

            MockMachineData = new MockMachineData(MachineConnectionsViewModel);
        }

        /// <summary>
        /// Toggles the cutlist and its corresponding splitter's visibility.
        /// </summary>
        public ICommand ToggleCutlistCommand =>
            new DelegateCommand(() => ToggleView(0));

        /// <summary>
        /// Toggles the flat part view and its corresponding splitters' visibility.
        /// </summary>
        public ICommand ToggleFlatViewCommand =>
            new DelegateCommand(() => ToggleView(1));

        /// <summary>
        /// Toggles the 3D view and its corresponding splitter's visibility.
        /// </summary>
        public ICommand Toggle3DCommand =>
            new DelegateCommand(() => ToggleView(2));

        /// <summary>
        /// Draw the 2D and 3D views from the current cutlist.
        /// </summary>
        public ICommand DrawPartsCommand => new DelegateCommand(DrawParts);

        /// <summary>
        /// Remove the current loaded cutlist, clearing the UI.
        /// </summary>
        public ICommand ClearCutlistCommand =>
            new DelegateCommand(() =>
            {
                CutlistViewModel.ClearUi();
                PartCollectionViewModel.ClearUi();
            });

        /// <summary>
        /// Start listening for new connections under the chosen topic.
        /// </summary>
        public ICommand StartListeningForConnectionsCommand =>
            new DelegateCommand(async () => await MachineConnectionsViewModel.Start());

        /// <summary>
        /// Resets the list of connections (does not stop connection listener!)
        /// </summary>
        public ICommand RefreshConnectionTabsCommand =>
            new DelegateCommand(() => MachineConnectionsViewModel.Refresh());

        /// <summary>
        /// Create a new mock connection with MockMachineData to listen to.
        /// </summary>
        public ICommand AddMockConnectionCommand =>
            new DelegateCommand(() => MockMachineData.AddMockClient());

        /// <summary>
        /// Test the current connection string.
        /// </summary>
        public ICommand TestSqlConnectionCommand =>
            new DelegateCommand(async () =>
                await SqlConnectionViewModel.TestConnection());

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
            PartCollectionViewModel.CreateRows(CutlistViewModel.Cutlists);
        }
    }
}