using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using AscCutlistEditor.Frameworks;

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