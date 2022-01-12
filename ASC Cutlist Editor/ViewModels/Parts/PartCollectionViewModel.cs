using AscCutlistEditor.Frameworks;
using AscCutlistEditor.Views.Bundles;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using AscCutlistEditor.Models.Cutlists;
using AscCutlistEditor.Models.Parts;

namespace AscCutlistEditor.ViewModels.Parts
{
    internal class PartCollectionViewModel : ObservableObject
    {
        private ObservableCollection<PartRow> _partRows;
        private ObservableCollection<SingleBundleControl> _bundles;

        private bool _flatPartButtonRow;

        public int DefaultDisplayWidthPx = 500;
        public int LeftOffsetPx = 30;

        /// <summary>
        /// If the number of parts for a cutlist is greater than this,
        /// merge them together into one part row.
        /// </summary>
        public int CutlistMergeCutoff = 8;

        /// <summary>
        /// If the number of part rows/bundles is greater than this,
        /// load them asynchronously instead.
        /// </summary>
        public int SyncLoadCutoff = 20;

        /// <summary>
        /// Represents a list of rows in the 2D view panel that can contain a list of drag n' droppable parts.
        /// </summary>
        public PartCollectionViewModel()
        {
            PartRows = new ObservableCollection<PartRow>();
            Bundles = new ObservableCollection<SingleBundleControl>();
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

        public ObservableCollection<SingleBundleControl> Bundles
        {
            get => _bundles;
            set
            {
                _bundles = value;
                RaisePropertyChangedEvent("Bundles");
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
            Bundles = new ObservableCollection<SingleBundleControl>();
            FlatPartButtonRowVisibility = false;
        }

        // Creates the rows and parts from a cutlist.
        internal async Task CreateRowsAsync(ObservableCollection<Cutlist> cutlists)
        {
            // Refresh the current list of parts.
            PartRows = new ObservableCollection<PartRow>();
            Bundles = new ObservableCollection<SingleBundleControl>();

            // Show the UI buttons.
            FlatPartButtonRowVisibility = true;

            // Grab the max cutlist length to scale them all by.
            double maxLength = cutlists.Max(c => c.Length);

            // Determine whether or not to load in the part rows asynchronously
            // using the number of total parts to draw: synchronous loading at
            // the start prevents UI flashing.
            int totalQuantity = 0;
            bool loadPartsAsync = false;
            foreach (Cutlist cutlist in cutlists)
            {
                int partsToAdd = cutlist.Quantity >= CutlistMergeCutoff ?
                    1 : cutlist.Quantity;
                totalQuantity += partsToAdd;

                if (totalQuantity > SyncLoadCutoff)
                {
                    loadPartsAsync = true;
                    break;
                }
            }

            // Determine whether to load the bundles in asynchronously.
            bool loadBundlesAsync = cutlists.Count > SyncLoadCutoff;

            foreach (Cutlist cutlist in cutlists)
            {
                // Only display one part for big cutlists.
                int partsToAdd = cutlist.Quantity >= CutlistMergeCutoff ?
                    1 : cutlist.Quantity;

                int partProportionalLength = Convert.ToInt32(
                    cutlist.Length / maxLength * DefaultDisplayWidthPx);

                // Update the part rows as they get parsed in.
                for (int i = 0; i < partsToAdd; i++)
                {
                    // Offset parts after the first one.
                    int leftOffset = i == 0 ? 0 : LeftOffsetPx;

                    // Current index for the part out of the total.
                    // (43 parts vs 1 of 43 parts)
                    int count = partsToAdd == 1 ? 0 : i + 1;

                    if (loadPartsAsync)
                    {
                        PartRows.Add(await Task.Run(() => new PartRow
                        {
                            Parts = PartViewModel
                                .CreatePart(cutlist, partProportionalLength, count),
                            LeftOffset = new Thickness(leftOffset, 0, 0, 0)
                        }));
                    }
                    else
                    {
                        PartRows.Add(new PartRow
                        {
                            Parts = PartViewModel
                                .CreatePart(cutlist, partProportionalLength, count),
                            LeftOffset = new Thickness(leftOffset, 0, 0, 0)
                        });
                    }
                }

                // Load in the bundles.
                if (loadBundlesAsync)
                {
                    Bundles.Add(await Task.Run(() =>
                        PartViewModel.CreateBundle(cutlist)));
                }
                else
                {
                    Bundles.Add(PartViewModel.CreateBundle(cutlist));
                }
            }
        }
    }
}