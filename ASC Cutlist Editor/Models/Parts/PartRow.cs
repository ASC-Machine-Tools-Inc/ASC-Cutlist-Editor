using System.Collections.ObjectModel;
using System.Windows;
using AscCutlistEditor.Views.FlatParts;

namespace AscCutlistEditor.Models.Parts
{
    // Represents a row that we can place SingePartControls in and move around.
    public class PartRow
    {
        public ObservableCollection<SinglePartControl> Parts { get; set; }

        // Margin applied if part is part of a stack to make it visually distinct.
        public Thickness LeftOffset { get; set; }
    }
}