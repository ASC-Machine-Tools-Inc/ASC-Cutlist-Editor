using AscCutlistEditor.Frameworks;
using AscCutlistEditor.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace AscCutlistEditor.ViewModels.FlatParts
{
    internal class FlatPartRowsViewModel : ObservableObject
    {
        private ObservableCollection<PartRow> _partRows;
        private bool _flatPartButtonRow = false;

        public int DefaultDisplayWidthPx = 500;
        public int LeftOffsetPx = 30;

        /// <summary>
        /// If the number of parts for a cutlist is greater than this,
        /// merge them together into one part row.
        /// </summary>
        public int CutlistMergeCutoff = 8;

        /// <summary>
        /// If the number of part rows is greater than this, load them
        /// asynchronously instead.
        /// </summary>
        public int SyncLoadCutoff = 20;

        /// <summary>
        /// Represents a list of rows in the 2D view panel that can contain a list of drag n' droppable parts.
        /// </summary>
        public FlatPartRowsViewModel()
        {
            PartRows = new ObservableCollection<PartRow>();
        }

        public ObservableCollection<PartRow> PartRows
        {
            get => _partRows;
            set
            {
                _partRows = value;
                RaisePropertyChangedEvent("PartRows");
            }
        }

        /// <summary>
        /// The visibility of the cutlist action buttons row.
        /// </summary>
        public bool FlatPartButtonRowVisibility
        {
            get => _flatPartButtonRow;
            set
            {
                _flatPartButtonRow = value;
                RaisePropertyChangedEvent("FlatPartButtonRowVisibility");
            }
        }

        /// <summary>
        /// Event handler for creating PartRows.
        /// </summary>
        /// <param name="cutlists">List of cutlists to create rows from.</param>
        public async void CreateRows(ObservableCollection<Cutlist> cutlists)
        {
            await CreateRowsAsync(cutlists);
        }

        /// <summary>
        /// Clear the UI elements for the flat parts.
        /// </summary>
        public void ClearUi()
        {
            PartRows = new ObservableCollection<PartRow>();
            FlatPartButtonRowVisibility = false;
        }

        // Creates the rows and parts from a cutlist.
        internal async Task CreateRowsAsync(ObservableCollection<Cutlist> cutlists)
        {
            // Refresh the current list of parts.
            PartRows = new ObservableCollection<PartRow>();

            // Grab the max cutlist length to scale them all by.
            double maxLength = cutlists.Max(c => c.Length);

            // Show the UI buttons.
            FlatPartButtonRowVisibility = true;

            // Determine whether or not to load in the part rows asynchronously
            // using the number of total parts to draw: synchronous loading at
            // the start prevents UI flashing.
            int totalQuantity = 0;
            bool loadAsync = false;
            foreach (Cutlist cutlist in cutlists)
            {
                int partsToAdd = cutlist.Quantity >= CutlistMergeCutoff ?
                    1 : cutlist.Quantity;
                totalQuantity += partsToAdd;

                if (totalQuantity > SyncLoadCutoff)
                {
                    loadAsync = true;
                    break;
                }
            }

            foreach (Cutlist cutlist in cutlists)
            {
                // Only display one part for big cutlists.
                int partsToAdd = cutlist.Quantity >= CutlistMergeCutoff ?
                    1 : cutlist.Quantity;

                int partProportionalLength = Convert.ToInt32(
                    (cutlist.Length / maxLength) * DefaultDisplayWidthPx);

                // Update the part rows as they get parsed in.
                for (int i = 0; i < partsToAdd; i++)
                {
                    // Offset parts after the first one.
                    int leftOffset = i == 0 ? 0 : LeftOffsetPx;

                    // Current index for the part out of the total.
                    // (43 parts vs 1 of 43 parts)
                    int count = partsToAdd == 1 ? 0 : i + 1;

                    if (loadAsync)
                    {
                        PartRows.Add(await Task.Run(() => new PartRow
                        {
                            Parts = FlatPartViewModel
                                .CreatePart(cutlist, partProportionalLength, count),
                            LeftOffset = new Thickness(leftOffset, 0, 0, 0)
                        }));
                    }
                    else
                    {
                        PartRows.Add(new PartRow
                        {
                            Parts = FlatPartViewModel
                                .CreatePart(cutlist, partProportionalLength, count),
                            LeftOffset = new Thickness(leftOffset, 0, 0, 0)
                        });
                    }
                }
            }
        }
    }
}