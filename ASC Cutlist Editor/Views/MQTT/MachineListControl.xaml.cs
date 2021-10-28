using System.Windows;
using System.Windows.Controls;

namespace AscCutlistEditor.Views.MQTT
{
    /// <summary>
    /// Interaction logic for MachineListControl.xaml
    /// </summary>
    public partial class MachineListControl : UserControl
    {
        public MachineListControl()
        {
            InitializeComponent();
        }

        private void OpenSqlSettings(object sender, RoutedEventArgs e)
        {
            SqlSettingsControl settings = new SqlSettingsControl
            {
                // Grab the data context of MainWindow so we can access our settings.
                DataContext = Application.Current.MainWindow?.DataContext
            };
            settings.ShowDialog();
        }
    }
}