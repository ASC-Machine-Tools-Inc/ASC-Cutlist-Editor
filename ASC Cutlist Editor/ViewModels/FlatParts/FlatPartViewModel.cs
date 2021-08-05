using System;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using AscCutlistEditor.Frameworks;
using AscCutlistEditor.Models;
using AscCutlistEditor.Views;

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

            // Convert the cutlist length into a color for that part.
            PropertyInfo[] properties = typeof(Brushes).GetProperties();
            int randomIndex = Convert.ToInt32(cutlist.Length) % properties.Length;
            Brush brush = (SolidColorBrush)properties[randomIndex]
                .GetValue(null, null);

            // Run on UI thread.
            // Application.Current.Dispatcher works when running program
            // Dispatcher.CurrentDispatcher works for tests??
            Dispatcher.CurrentDispatcher.Invoke(() =>
            {
                Thread staThread = new Thread(async () =>
                {
                    SinglePartControl part = new SinglePartControl()
                    {
                        PartGrid = { Width = partWidth },
                        PartRect = { Fill = brush },
                        PartLabel = { Text = GetPartLabel(cutlist) }
                    };
                    parts.Add(part);
                });
                staThread.SetApartmentState(ApartmentState.STA);
                staThread.Start();
                staThread.Join();
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