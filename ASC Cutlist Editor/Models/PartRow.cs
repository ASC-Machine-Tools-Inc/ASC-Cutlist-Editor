using AscCutlistEditor.Views;
using System.Collections.ObjectModel;

namespace AscCutlistEditor.Models
{
    // Represents a row that we can place SingePartControls in and move around.
    public class PartRow
    {
        public ObservableCollection<SinglePartControl> Parts { get; set; }
    }
}