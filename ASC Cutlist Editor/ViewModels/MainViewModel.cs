using AscCutlistEditor.Frameworks;
using AscCutlistEditor.ViewModels.Cutlists;
using AscCutlistEditor.ViewModels.FlatParts;
using System.Collections.ObjectModel;
using System.Windows.Input;
using AscCutlistEditor.ViewModels.MQTT;
using OxyPlot;
using OxyPlot.Series;

namespace AscCutlistEditor.ViewModels
{
    internal class MainViewModel : ObservableObject
    {
        // Collection that tracks the visibility of the UI elements.
        // Visibility order: Cutlist, 2D, 3D.
        public ObservableCollection<bool> UiVisibility { get; } =
            new ObservableCollection<bool>(new[] { true, true, true });

        public CutlistImportViewModel CutlistViewModel { get; }
        public FlatPartRowsViewModel FlatPartRowsViewModel { get; }

        public MachineDataViewModel MachineDataViewModel { get; }

        public MainViewModel()
        {
            CutlistViewModel = new CutlistImportViewModel(DrawParts);

            FlatPartRowsViewModel = new FlatPartRowsViewModel();

            MachineDataViewModel = new MachineDataViewModel();

            // Test plot model
            // Create the plot model
            var tmp = new PlotModel { Title = "Simple example", Subtitle = "using OxyPlot" };

            // Create two line series (markers are hidden by default)
            var series1 = new LineSeries { Title = "Series 1", MarkerType = MarkerType.Circle };
            series1.Points.Add(new DataPoint(0, 0));
            series1.Points.Add(new DataPoint(10, 18));
            series1.Points.Add(new DataPoint(20, 12));
            series1.Points.Add(new DataPoint(30, 8));
            series1.Points.Add(new DataPoint(40, 15));

            var series2 = new LineSeries { Title = "Series 2", MarkerType = MarkerType.Square };
            series2.Points.Add(new DataPoint(0, 4));
            series2.Points.Add(new DataPoint(10, 12));
            series2.Points.Add(new DataPoint(20, 16));
            series2.Points.Add(new DataPoint(30, 25));
            series2.Points.Add(new DataPoint(40, 5));

            // Add the series to the plot model
            tmp.Series.Add(series1);
            tmp.Series.Add(series2);

            // Axes are created automatically if they are not defined

            // Set the Model property, the INotifyPropertyChanged event will make the WPF Plot control update its content
            PlotModel = tmp;
        }

        public PlotModel PlotModel { get; set; }

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
        /// Connects to the machines to start reading KPI data.
        /// </summary>
        public ICommand SetupMqttCommand => new DelegateCommand(SetupMqtt);

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
            FlatPartRowsViewModel.CreateRows(
                CutlistViewModel.Cutlists);
        }

        /// <summary>
        /// Clear the cutlist and all UI elements drawn from it.
        /// </summary>
        private void ClearCutlist()
        {
            CutlistViewModel.ClearUi();
            FlatPartRowsViewModel.ClearUi();
        }

        /// <summary>
        /// Start the server and client for MQTTnet, and try connecting
        /// to the machines.
        /// </summary>
        private void SetupMqtt()
        {
            MachineDataViewModel.Start();
        }
    }
}