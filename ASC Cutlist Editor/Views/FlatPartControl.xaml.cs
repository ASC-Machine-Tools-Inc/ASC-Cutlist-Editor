using System.Windows.Controls;
using System.Windows.Input;

namespace AscCutlistEditor.Views
{
    /// <summary>
    /// Interaction logic for FlatPartControl.xaml
    /// </summary>
    public partial class FlatPartControl
    {
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
    }
}