﻿using AscCutlistEditor.Frameworks;
using System.Diagnostics;
using System.Windows;

namespace AscCutlistEditorTests.Common
{
    internal class Mocks
    {
        public class MockSettings : ISettings
        {
            public string DataSource { get; set; }
            public string DatabaseName { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
        }

        public class MockDialog : IDialogService
        {
            public void ShowMessageBox(
                string messageBoxText,
                string caption,
                MessageBoxButton button,
                MessageBoxImage icon)
            {
                Debug.WriteLine("Testing display message: " + messageBoxText);
            }
        }
    }
}