using System.ComponentModel;
using System.Windows;

namespace AscCutlistEditor.Views.MQTT.MachineMessage
{
    /// <summary>
    /// Interaction logic for MachineDetailedDataWindow.xaml
    /// </summary>
    public partial class MachineDetailedDataWindow : Window
    {
        public MachineDetailedDataWindow()
        {
            InitializeComponent();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            // Toggle visibility instead of using Show to reuse our window,
            // preventing issues with multiple plotviews.
            e.Cancel = true;
            Visibility = Visibility.Hidden;
        }
    }
}