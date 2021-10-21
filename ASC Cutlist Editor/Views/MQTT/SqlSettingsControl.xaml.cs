using System.Windows;
using AscCutlistEditor.Models.MQTT;

namespace AscCutlistEditor.Views.MQTT
{
    /// <summary>
    /// Interaction logic for SqlSettingsControl.xaml
    /// </summary>
    public partial class SqlSettingsControl : Window
    {
        public SqlSettingsControl()
        {
            InitializeComponent();
        }

        // Close and reopen the settings so the reset changes apply.
        private void ResetSqlSettings(object sender, RoutedEventArgs e)
        {
            Close();

            UserSqlSettings.Reset();

            SqlSettingsControl settings = new SqlSettingsControl
            {
                // Grab the data context of MainWindow so we can access our settings.
                DataContext = Application.Current.MainWindow?.DataContext
            };
            settings.ShowDialog();
        }
    }
}