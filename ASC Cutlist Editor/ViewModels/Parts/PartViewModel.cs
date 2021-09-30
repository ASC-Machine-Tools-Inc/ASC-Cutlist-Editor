using AscCutlistEditor.Frameworks;
using AscCutlistEditor.Models;
using AscCutlistEditor.Utility;
using AscCutlistEditor.Views;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using AscCutlistEditor.Views.Bundles;
using AscCutlistEditor.Views.FlatParts;

namespace AscCutlistEditor.ViewModels.Parts
{
    // Does the UI work for representing a list of parts in a part row.
    internal class PartViewModel : ObservableObject
    {
        /// <summary>
        /// Draws a 2D view of the part from a cutlist.
        /// </summary>
        /// <param name="cutlist">The cutlist to create a part from.</param>
        /// <param name="partWidth">The width in px of the part.</param>
        /// <param name="count">
        /// Which part this represents out of the cutlist, ranging from 1 to the
        /// quantity of the cutlist. If this is 0, then it denotes that this
        /// part represents the entire cutlist.
        /// </param>
        /// <returns>
        /// An observable collection with one SinglePartControl representing
        /// a part from the cutlist.
        /// </returns>
        public static ObservableCollection<SinglePartControl> CreatePart(Cutlist cutlist, int partWidth, int count)
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
                    PartLabel = { Text = GetPartLabel(cutlist, count) }
                };
                parts.Add(part);
            });

            return parts;
        }

        /// <summary>
        /// Draws a 3D view of the part from a cutlist.
        /// </summary>
        /// <param name="cutlist">The cutlist to create a bundle from.</param>
        /// <returns>A BundleControl representing the cutlist.</returns>
        public static SingleBundleControl CreateBundle(Cutlist cutlist)
        {
            return new SingleBundleControl();
        }

        /// <summary>
        /// Generates the label string for a part from a cutlist.
        /// </summary>
        /// <param name="cutlist">The cutlist to generate a part from.</param>
        /// <param name="count">
        /// The current number for this part out of the cutlist's quantity.
        /// 0 means that this part represents the entire cutlist (quantity is
        /// over the cutoff for drawing single parts).
        /// </param>
        /// <returns>A label string to display on a flat part.</returns>
        public static string GetPartLabel(Cutlist cutlist, int count)
        {
            if (count == 0)
            {
                return "P" + cutlist.ID + "  LENGTH: " + cutlist.Length + "  " +
                       cutlist.Quantity + " parts";
            }
            else
            {
                return "P" + cutlist.ID + "  LENGTH: " + cutlist.Length + "  " +
                       count + " of " + cutlist.Quantity + " parts";
            }
        }
    }
}