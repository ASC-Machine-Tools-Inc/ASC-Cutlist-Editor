using AscCutlistEditor.Frameworks;
using System.Windows;

namespace AscCutlistEditor.Models.MQTT
{
    internal class Dialog : IDialogService
    {
        public void ShowMessageBox(
            string messageBoxText,
            string caption,
            MessageBoxButton button,
            MessageBoxImage icon)
        {
            MessageBox.Show(messageBoxText, caption, button, icon);
        }
    }
}