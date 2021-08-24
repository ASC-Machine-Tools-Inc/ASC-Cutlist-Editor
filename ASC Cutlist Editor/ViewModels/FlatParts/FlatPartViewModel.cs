using AscCutlistEditor.Frameworks;
using AscCutlistEditor.Models;
using AscCutlistEditor.Views;
using System;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using AscCutlistEditor.Utility;

namespace AscCutlistEditor.ViewModels.FlatParts
{
    // Does the UI work for representing a list of parts in a part row.
    internal class FlatPartViewModel : ObservableObject
    {
        // Draws a 2D view of the part from a cutlist.
        public static ObservableCollection<SinglePartControl> CreatePart(Cutlist cutlist, int partWidth)
        {
            // Refresh the current list of parts.
            ObservableCollection<SinglePartControl> parts =
                new ObservableCollection<SinglePartControl>();

            Brush brush = Helpers.LengthToColor(cutlist.Length);

            // Run on UI thread.
            // Select the correct dispatcher: if Application.Current is null,
            // we're running unit tests and should use CurrentDispatcher instead.
            Dispatcher dispatcher = Application.Current != null ?
                Application.Current.Dispatcher :
                Dispatcher.CurrentDispatcher;
            dispatcher.Invoke(() =>
            {
                SinglePartControl part = new SinglePartControl()
                {
                    PartGrid = { Width = partWidth },
                    PartRect = { Fill = brush },
                    PartLabel = { Text = GetPartLabel(cutlist) }
                };
                parts.Add(part);
            });

            return parts;
        }

        // Generates the label string for a part from a cutlist.
        public static string GetPartLabel(Cutlist cutlist)
        {
            return "PROFILE X  LENGTH: " + cutlist.Length + "  " + cutlist.Quantity + "x";
        }
    }
}