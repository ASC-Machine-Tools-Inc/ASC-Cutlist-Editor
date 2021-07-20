using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Input;
using AscCutlistEditor.Common;
using AscCutlistEditor.Frameworks;
using AscCutlistEditor.Models;
using ExcelDataReader;
using Microsoft.Win32;

namespace AscCutlistEditor.ViewModels
{
    internal class MainViewModel : ObservableObject
    {
        private bool _cutlistVisibility = true;

        public bool CutlistVisibility
        {
            get => _cutlistVisibility;
            set
            {
                _cutlistVisibility = value;
                RaisePropertyChangedEvent("CutlistVisibility");
            }
        }

        private bool _2DVisibility;
        private bool _3DVisibility;
        private bool splitter1Visibility;
        private bool splitter2Visibility;

        public CutlistViewModel CutlistViewModel { get; }

        public MainViewModel()
        {
            CutlistVisibility = true;

            CutlistViewModel = new CutlistViewModel();
        }

        public ICommand ToggleCutlistCommand => new DelegateCommand(ToggleCutlist);

        private void ToggleCutlist()
        {
            CutlistVisibility = !CutlistVisibility;
        }
    }
}