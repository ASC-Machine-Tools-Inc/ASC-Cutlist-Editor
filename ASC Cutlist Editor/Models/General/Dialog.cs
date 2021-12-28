using System.Windows;
using AscCutlistEditor.Frameworks;
using ModernWpf.Controls;

namespace AscCutlistEditor.Models.General
{
    internal class Dialog : IDialogService
    {
        public async void ShowMessageBox(
            string title,
            string contents,
            string cancelText)
        {
            ContentDialog dialog = new ContentDialog
            {
                Title = title,
                Content = contents,
                CloseButtonText = cancelText
            };

            await dialog.ShowAsync();
        }
    }
}