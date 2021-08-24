using AscCutlistEditor.Frameworks;
using AscCutlistEditor.ViewModels.Cutlists;
using AscCutlistEditor.ViewModels.FlatParts;
using System.Collections.ObjectModel;
using System.Windows.Input;
using AscCutlistEditor.Models;
using AscCutlistEditor.MQTT;

namespace AscCutlistEditor.ViewModels
{
    internal class MainViewModel : ObservableObject
    {
        // Collection that tracks the visibility of the UI elements.
        // Visibility order: Cutlist, 2D, 3D.
        public ObservableCollection<bool> UiVisibility { get; } =
            new ObservableCollection<bool>(new[] { true, true, true });

        public CutlistImportViewModel CutlistViewModel { get; }
        public FlatPartRowsViewModel FlatPartRowsViewModel { get; }

        public Generator Generator { get; }

        public MainViewModel()
        {
            CutlistViewModel = new CutlistImportViewModel(DrawParts);

            FlatPartRowsViewModel = new FlatPartRowsViewModel();
            Generator = new Generator();

            // Move to button
            Generator.Start();
        }

        /// <summary>
        /// Toggles the cutlist and its corresponding splitter's visibility.
        /// </summary>
        public ICommand ToggleCutlistCommand => new DelegateCommand(() => ToggleView(0));

        /// <summary>
        /// Toggles the flat part view and its corresponding splitters' visibility.
        /// </summary>
        public ICommand ToggleFlatViewCommand => new DelegateCommand(() => ToggleView(1));

        /// <summary>
        /// Toggles the 3D view and its corresponding splitter's visibility.
        /// </summary>
        public ICommand Toggle3DCommand => new DelegateCommand(() => ToggleView(2));

        /// <summary>
        /// Draw the 2D and 3D views from the current cutlist.
        /// </summary>
        public ICommand DrawPartsCommand => new DelegateCommand(DrawParts);

        /// <summary>
        /// Remove the current loaded cutlist, clearing the UI.
        /// </summary>
        public ICommand ClearCutlistCommand => new DelegateCommand(ClearCutlist);

        private void ToggleView(int index)
        {
            UiVisibility[index] = !UiVisibility[index];
        }

        /// <summary>
        /// Catch the request from the cutlist view model to draw the parts
        /// after parsing a valid csv.
        /// </summary>
        private void DrawParts()
        {
            FlatPartRowsViewModel.CreateRows(
                CutlistViewModel.Cutlists);
        }

        /// <summary>
        /// Clear the cutlist and all UI elements drawn from it.
        /// </summary>
        private void ClearCutlist()
        {
            CutlistViewModel.ClearUi();
            FlatPartRowsViewModel.ClearUi();
        }
    }
}