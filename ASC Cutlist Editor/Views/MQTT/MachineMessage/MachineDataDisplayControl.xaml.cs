using System.Windows;
using AscCutlistEditor.ViewModels.MQTT.MachineMessage;

namespace AscCutlistEditor.Views.MQTT.MachineMessage
{
    /// <summary>
    /// Interaction logic for MachineDataDisplayControl.xaml
    /// </summary>
    public partial class MachineDataDisplayControl
    {
        internal MachineDetailedDataWindow DetailedData;

        public MachineDataDisplayControl()
        {
            InitializeComponent();

            // Grab the same context so we can access the detailed view.
            DetailedData = new MachineDetailedDataWindow();
        }

        public void OpenDetails(object sender, RoutedEventArgs e)
        {
            // Toggle visibility instead of using Show to reuse our window,
            // preventing issues with multiple plotviews.
            DetailedData.DataContext = (MachineMessageViewModel)DataContext;

            DetailedData.Visibility = Visibility.Visible;
        }
    }
}