using System;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using ASC_Cutlist_Editor.Views;
using AscCutlistEditor.Frameworks;
using AscCutlistEditor.Models;

namespace ASC_Cutlist_Editor.ViewModels.FlatParts
{
    // Represents a list of parts in a part row.
    internal class FlatPartViewModel : ObservableObject
    {
        private ObservableCollection<SinglePartControl> _parts = new ObservableCollection<SinglePartControl>();

        public ObservableCollection<SinglePartControl> Parts
        {
            get => _parts;
            set
            {
                _parts = value;

                RaisePropertyChangedEvent("Parts");
            }
        }

        public static FlatPartViewModel Instance { get; } = new FlatPartViewModel();

        // Draws a 2D view of the parts from a cutlist.
        public ObservableCollection<SinglePartControl> CreatePart(Cutlist cutlist, int partWidth)
        {
            // Refresh the current list of parts.
            Parts = new ObservableCollection<SinglePartControl>();

            // Convert the cutlist length into a color for that part.
            PropertyInfo[] properties = typeof(Brushes).GetProperties();
            int randomIndex = Convert.ToInt32(cutlist.Length) % properties.Length;
            Brush brush = (SolidColorBrush)properties[randomIndex].GetValue(null, null);

            // Run on UI thread.
            Application.Current.Dispatcher.Invoke(() =>
            {
                SinglePartControl part = new SinglePartControl
                {
                    PartGrid = { Width = partWidth },
                    PartRect = { Fill = brush },
                    PartLabel = { Text = GetPartLabel(cutlist) }
                };

                Parts.Add(part);
            });

            return Parts;
        }

        // Generates the label string for a part from a cutlist.
        public string GetPartLabel(Cutlist cutlist)
        {
            return "PROFILE X  LENGTH: " + cutlist.Length + "  " + cutlist.Quantity + "x";
        }
    }
}