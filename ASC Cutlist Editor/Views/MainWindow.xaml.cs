using AscCutlistEditor.ViewModels;
using AscCutlistEditor.ViewModels.MQTT;
using System;
using System.Windows;

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

        private static void MainWindow_Closed(object sender, EventArgs e)
        {
            SqlConnectionViewModel.Save();
        }
    }
}