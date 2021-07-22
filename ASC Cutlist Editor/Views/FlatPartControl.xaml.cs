using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using AscCutlistEditor.Models;
using AscCutlistEditor.ViewModels;

namespace ASC_Cutlist_Editor.Views
{
    /// <summary>
    /// Interaction logic for FlatPartControl.xaml
    /// </summary>
    public partial class FlatPartControl : UserControl
    {
        public class PartRow
        {
            public ObservableCollection<AscCutlistEditor.Models.PartRow> Parts { get; set; }
        }

        public FlatPartControl()
        {
            InitializeComponent();
        }

        // Don't use, but TODO: example for scrolling with drag n drop
        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scrollViewer = (ScrollViewer)sender;
            if (e.Delta < 0)
            {
                scrollViewer.LineRight();
            }
            else
            {
                scrollViewer.LineLeft();
            }
            e.Handled = true;
        }

        // Dynamically create the view with the various parts when a CSV is parsed.
        private void DrawParts()
        {
            /* This definitely is goofy, especially that we're even doing anything
             * in the code behind. At some point down the line I'd want to look
             * into setting up some sort of template so we can display the 2D
             * part representations from a list, so that when we modify it in
             * the UI it modifies that list as well.

            List<Cutlist> cutlist = ((CutlistViewModel)(this.DataContext)).Cutlists;
            FlatPartList.Children.Add();
            */
        }
    }
}