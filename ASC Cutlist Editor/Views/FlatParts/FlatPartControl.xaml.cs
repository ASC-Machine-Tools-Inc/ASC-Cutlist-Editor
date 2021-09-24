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

        // Mouse wheel scrolling for part view (doesn't work while dragging)
        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scrollViewer = (ScrollViewer)sender;
            if (e.Delta < 0)
            {
                scrollViewer.LineDown();
            }
            else
            {
                scrollViewer.LineUp();
            }
            e.Handled = true;
        }
    }
}