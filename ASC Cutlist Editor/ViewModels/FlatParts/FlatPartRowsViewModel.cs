using AscCutlistEditor.Frameworks;
using AscCutlistEditor.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using ASC_Cutlist_Editor.Views;

namespace AscCutlistEditor.ViewModels
{
    // Represents a list of rows in the 2D view panel that can contain a list of drag n' droppable parts.
    internal class FlatPartRowsViewModel : ObservableObject
    {
        public readonly int DefaultDisplayWidthPx = 500;
        public readonly int CutlistMergeCutoff = 8;
        public readonly int SyncLoadCutoff = 20;

        private ObservableCollection<PartRow> _partRows;

        public ObservableCollection<PartRow> PartRows
        {
            get => _partRows;
            set
            {
                _partRows = value;
                RaisePropertyChangedEvent("PartRows");
            }
        }

        // Creates the rows and parts from a cutlist.
        public async void CreateRowsAsync(ObservableCollection<Cutlist> cutlists)
        {
            // Refresh the current list of parts.
            PartRows = new ObservableCollection<PartRow>();

            // Grab the max cutlist length to scale them all by.
            double maxLength = cutlists.Max(c => c.Length);

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
                    if (loadAsync)
                    {
                        PartRows.Add(await Task.Run(() => new PartRow
                        {
                            Parts = FlatPartViewModel.Instance
                                .CreatePart(cutlist, partProportionalLength)
                        }));
                    }
                    else
                    {
                        PartRows.Add(new PartRow
                        {
                            Parts = FlatPartViewModel.Instance
                                .CreatePart(cutlist, partProportionalLength)
                        });
                    }
                }
            }
        }
    }
}