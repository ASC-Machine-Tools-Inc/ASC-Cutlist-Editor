using System;
using System.Windows;
using AscCutlistEditor.Models.MQTT;
using AscCutlistEditor.ViewModels;
using ModernWpf.Controls;

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
        private async void ResetSqlSettings(object sender, RoutedEventArgs e)
        {
            ContentDialog dialog = new ContentDialog
            {
                Title = "Reset your settings?",
                Content = "Your current connection settings and table names will " +
                          "be cleared, and your column names will be reset. Are " +
                          "you sure?",
                PrimaryButtonText = "Reset",
                CloseButtonText = "Cancel"
            };

            if (await dialog.ShowAsync() == ContentDialogResult.Primary)
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

        private async void DebugReset(object sender, RoutedEventArgs e)
        {
            ContentDialog dialog = new ContentDialog
            {
                Title = "Reset settings",
                Content = "DEBUG PURPOSES ONLY - RESET TO TEST SETTINGS. ARE YOU SURE?",
                PrimaryButtonText = "Reset",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary
            };

            if (await dialog.ShowAsync() == ContentDialogResult.Primary)
            {
                Close();

                DebugHelpers.DebugHelpers.DebugReset();

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