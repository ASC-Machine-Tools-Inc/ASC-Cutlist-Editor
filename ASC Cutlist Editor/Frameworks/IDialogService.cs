using System.Windows;

namespace AscCutlistEditor.Frameworks
{
    /// <summary>
    /// Interface for displaying message boxes.
    /// </summary>
    public interface IDialogService
    {
        void ShowMessageBox(
            string title,
            string contents,
            string cancelText);
    }
}