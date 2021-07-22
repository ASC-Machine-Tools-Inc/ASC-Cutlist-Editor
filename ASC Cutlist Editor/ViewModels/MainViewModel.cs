using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        // Collection that tracks the visibility of the UI elements.
        // Visibility order: Cutlist, 2D, 3D.
        public ObservableCollection<bool> UiVisibility { get; set; } =
            new ObservableCollection<bool>(new[] { true, true, true });

        public CutlistViewModel CutlistViewModel { get; }
        public FlatPartRowsViewModel FlatPartRowsViewModel { get; }
        public FlatPartViewModel FlatPartViewModel { get; }

        public MainViewModel()
        {
            CutlistViewModel = new CutlistViewModel(DrawParts);
            FlatPartRowsViewModel = new FlatPartRowsViewModel();
            FlatPartViewModel = new FlatPartViewModel();
        }

        // Toggles the cutlist and its corresponding splitter's visibility.
        public ICommand ToggleCutlistCommand => new DelegateCommand(() => ToggleView(0));

        // Toggles the flat part view and its corresponding splitters' visibility.
        public ICommand ToggleFlatViewCommand => new DelegateCommand(() => ToggleView(1));

        // Toggles the 3D view and its corresponding splitter's visibility.
        public ICommand Toggle3DCommand => new DelegateCommand(() => ToggleView(2));

        private void ToggleView(int index)
        {
            UiVisibility[index] = !UiVisibility[index];
        }

        // Catch the request from the cutlist view model
        // to draw the parts after parsing a valid csv.
        private void DrawParts()
        {
            FlatPartRowsViewModel.CreateRows(CutlistViewModel.Cutlists);
        }
    }
}