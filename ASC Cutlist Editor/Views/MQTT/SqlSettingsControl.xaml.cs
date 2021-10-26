using System;
using System.Windows;
using AscCutlistEditor.Models.MQTT;
using AscCutlistEditor.ViewModels;

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

        // Update settings message on close.
        private void SqlSettings_Closed(object sender, EventArgs e)
        {
            MainViewModel model = DataContext as MainViewModel;
            model?.SqlConnectionViewModel.UpdateSettingsRequiredMessage();
        }

        // Close and reopen the settings so the reset changes apply.
        private void ResetSqlSettings(object sender, RoutedEventArgs e)
        {
            MessageBoxResult dialogResult = MessageBox.Show(
                "Your current settings will be cleared.",
                "Reset settings",
                MessageBoxButton.OKCancel,
                MessageBoxImage.Warning);
            if (dialogResult == MessageBoxResult.OK)
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
}