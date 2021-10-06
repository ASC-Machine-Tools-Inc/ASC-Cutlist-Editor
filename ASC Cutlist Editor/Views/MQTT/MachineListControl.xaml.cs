using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.VisualBasic;

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