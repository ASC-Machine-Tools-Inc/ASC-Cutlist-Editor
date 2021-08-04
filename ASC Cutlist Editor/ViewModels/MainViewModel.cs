using AscCutlistEditor.Frameworks;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

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

        // Draw the 2D and 3D views from the current cutlist.
        public ICommand DrawPartsCommand => new DelegateCommand(DrawParts);

        private void ToggleView(int index)
        {
            UiVisibility[index] = !UiVisibility[index];
        }

        // Catch the request from the cutlist view model
        // to draw the parts after parsing a valid csv.
        private void DrawParts()
        {
            // Do nothing for empty cutlists.
            if (CutlistViewModel.Cutlists.Count == 0)
            {
                return;
            }

            FlatPartRowsViewModel.CreateRowsAsync(CutlistViewModel.Cutlists);
        }
    }
}