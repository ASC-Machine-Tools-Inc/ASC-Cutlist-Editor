using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AscCutlistEditor.Views.MQTT
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
            e.Cancel = true;
            Visibility = Visibility.Hidden;
        }
    }
}