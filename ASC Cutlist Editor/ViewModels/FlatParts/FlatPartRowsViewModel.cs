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
        public const int DefaultDisplayWidthPx = 500;

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

            /* Create a row with a part for each line's quantity.
             * TODO: might replace this later with just one row and a way to
             * split parts, because otherwise it'll be too much to scroll. The
             * single part will have the number of parts it represents on it.
             */
            foreach (Cutlist cutlist in cutlists)
            {
                for (int i = 0; i < cutlist.Quantity; i++)
                {
                    PartRows.Add(new PartRow
                    {
                        Parts = FlatPartViewModel.Instance.CreatePart(cutlist)
                    });
                }
            }

            // Calculate the lengths for all of the cutlists.
            // TODO: maybe can do that in method?
            CutlistsToDisplayLengths(cutlists);
        }

        // Recalculates the length for all the parts, scaling accordingly so
        // the longest part matches the default length and all the shorter
        // ones scale to that.
        private void CutlistsToDisplayLengths(List<Cutlist> cutlists)
        {
            double maxLength = cutlists.Max(c => c.Length);

            // Calculate the part lengths based on the corresponding cutlist.
            // Example: the max length part will be 300px wide, while one 2/3rds
            //          as big will be only 200px wide.
            int currRowIndex = 0;
            foreach (Cutlist cutlist in cutlists)
            {
                // Iterate through the part rows for each cutlist group.
                for (int rowIndex = 0; rowIndex < cutlist.Quantity; rowIndex++)
                {
                    // Retrieve the correct row from last left
                    // off processing the last cutlist.
                    int partRowIndex = currRowIndex + rowIndex;

                    // Since we only call this on initialization, we only need to
                    // change the length for the single part in Parts.
                    int partLength = Convert.ToInt32(cutlist.Length /
                        maxLength * DefaultDisplayWidthPx);
                    PartRows[partRowIndex].Parts[0].PartGrid.Width = partLength;
                }

                currRowIndex += cutlist.Quantity;
            }
        }
    }
}