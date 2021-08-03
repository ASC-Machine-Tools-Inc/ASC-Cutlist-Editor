using AscCutlistEditor.Frameworks;
using AscCutlistEditor.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using ASC_Cutlist_Editor.Views;

namespace AscCutlistEditor.ViewModels
{
    // Represents a list of rows in the 2D view panel that can contain a list of drag n' droppable parts.
    internal class FlatPartRowsViewModel : ObservableObject
    {
        public static readonly int DefaultDisplayWidthPx = 500;
        public readonly int CutlistSizeCutoff = 8;

        private ObservableCollection<PartRow> _partRows = new ObservableCollection<PartRow>();

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
        public void CreateRows(List<Cutlist> cutlists)
        {
            // Refresh the current list of parts.
            PartRows = new ObservableCollection<PartRow>();

            // Grab the max cutlist length to scale them all by.
            double maxLength = cutlists.Max(c => c.Length);

            // Create a row with a part for each cutlist line in the CSV.
            foreach (Cutlist cutlist in cutlists)
            {
                int partsToAdd;
                // Only display one part for big cutlists.
                if (cutlist.Quantity >= CutlistSizeCutoff)
                {
                    partsToAdd = 1;
                }
                else
                {
                    partsToAdd = cutlist.Quantity;
                }

                for (int i = 0; i < partsToAdd; i++)
                {
                    PartRows.Add(new PartRow
                    {
                        Parts = FlatPartViewModel.Instance.CreatePart(cutlist)
                    });

                    // Update the length of the part we just added.
                    int partLength = Convert.ToInt32((cutlist.Length / maxLength) *
                                                     DefaultDisplayWidthPx);
                    PartRows[^1].Parts[0].PartGrid.Width = partLength;
                }
            }
        }
    }
}