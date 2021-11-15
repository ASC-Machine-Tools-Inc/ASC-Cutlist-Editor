using AscCutlistEditor.ViewModels;
using AscCutlistEditor.ViewModels.MQTT;
using System;
using System.Windows;
using AscCutlistEditor.Models.MQTT;
using AscCutlistEditor.Views.MQTT;

namespace AscCutlistEditor.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Closed += MainWindow_Closed;
            DataContext = new MainViewModel();
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

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            UserSqlSettings.Save();

            // Close all windows.
            Application.Current.Shutdown();
        }
    }
}