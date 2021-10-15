using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace AscCutlistEditor.Frameworks
{
    /// <summary>
    /// Interface for displaying message boxes.
    /// </summary>
    public interface IDialogService
    {
        void ShowMessageBox(
            string messageBoxText,
            string caption,
            MessageBoxButton button,
            MessageBoxImage image);
    }
}