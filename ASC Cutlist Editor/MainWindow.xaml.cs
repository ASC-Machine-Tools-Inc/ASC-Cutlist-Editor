using System.Windows;
using ASC_Cutlist_Editor.ViewModels;
using AscCutlistEditor.ViewModels;

namespace AscCutlistEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }
    }
}