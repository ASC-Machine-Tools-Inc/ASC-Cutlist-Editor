using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AscCutlistEditor.Common;
using AscCutlistEditor.Frameworks;
using AscCutlistEditor.Models;
using ExcelDataReader;
using GongSolutions.Wpf.DragDrop;
using Microsoft.Win32;

namespace AscCutlistEditor.ViewModels
{
    // Represents a list of rows that can contain a list of drag n' droppable parts.
    internal class FlatPartRowsViewModel : ObservableObject
    {
        private const int DefaultDisplayWidthPx = 300;

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
                int partsToAdd = 0;
                while (partsToAdd < cutlist.Quantity)
                {
                    PartRows.Add(new PartRow
                    {
                        Parts = FlatPartViewModel.Instance.CreatePart(cutlist)
                    });
                    partsToAdd++;
                }
            }

            Debug.WriteLine("Calling CutlistsToDisplayLengths...");
            CutlistsToDisplayLengths(cutlists);

            // DEBUG write part message
            Debug.WriteLine(string.Concat(PartRows.Select(p =>
                p.Parts[0].DisplayLabel + " " + p.Parts[0].DisplayLength + "\n")));
        }

        /* Recalculates the length for all the parts, scaling accordingly so
         * the longest part matches the default length and all the shorter
         * ones scale to that.
         */

        private void CutlistsToDisplayLengths(List<Cutlist> cutlists)
        {
            /*
            if (cutlists.Count != PartRows.Count)
            {
                return;
            }
            */

            double maxLength = cutlists.Max(c => c.Length);
            Debug.WriteLine("Max: " + maxLength);

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
                    PartRows[partRowIndex].Parts[0].DisplayLength = partLength;
                }

                currRowIndex += cutlist.Quantity;
            }
        }
    }

    /*
    // TODO: handle reassigning parts
    // SEE: https://github.com/punker76/gong-wpf-dragdrop/wiki/Usage
    internal class FlatPartDragDropModel : IDropTarget
    {
    }
    */
}