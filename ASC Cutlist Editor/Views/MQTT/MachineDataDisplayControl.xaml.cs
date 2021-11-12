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
using AscCutlistEditor.ViewModels.MQTT;

namespace AscCutlistEditor.Views.MQTT
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

        public void ButtonTest(object sender, RoutedEventArgs e)
        {
            // Toggle visibility instead of using Show to reuse our window,
            // preventing issues with multiple plotviews.
            DetailedData.DataContext = (MachineMessageViewModel)DataContext;

            DetailedData.Visibility = Visibility.Visible;
        }
    }
}