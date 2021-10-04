using System;
using System.Diagnostics;
using System.Windows;
using AscCutlistEditor.ViewModels;
using AscCutlistEditor.ViewModels.MQTT;

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
            Debug.WriteLine("Saving!");
            SqlConnectionViewModel.Save();
        }
    }
}