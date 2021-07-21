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
using Microsoft.Win32;

namespace AscCutlistEditor.ViewModels
{
    internal class FlatPartViewModel : ObservableObject
    {
        private const int DefaultDisplayWidthPx = 300;

        private ObservableCollection<Part> _parts = new ObservableCollection<Part>();

        public ObservableCollection<Part> Parts
        {
            get => _parts;
            set
            {
                _parts = value;
                RaisePropertyChangedEvent("Parts");
            }
        }

        // Draws a 2D view of the parts from a cutlist.
        public void DrawParts(List<Cutlist> cutlists)
        {
            // Refresh the current list of parts.
            Parts = new ObservableCollection<Part>();

            foreach (Cutlist cutlist in cutlists)
            {
                Parts.Add(new Part
                {
                    DisplayLabel = CutlistToDisplayLabel(cutlist)
                });
            }

            CutlistsToDisplayLengths(cutlists);

            Debug.WriteLine(string.Concat(Parts.Select(p => p.DisplayLabel + " " + p.DisplayLength + "\n")));
        }

        // Generates the label string for a part from a cutlist.
        private string CutlistToDisplayLabel(Cutlist cutlist)
        {
            return "PROFILE " + cutlist.Length;
        }

        /* Recalculates the length for all the parts, scaling accordingly so
         * the longest part matches the default length and all the shorter
         * ones scale to that.
         */

        private void CutlistsToDisplayLengths(List<Cutlist> cutlists)
        {
            if (cutlists.Count != Parts.Count)
            {
                return;
            }

            double maxLength = cutlists.Max(c => c.Length);

            // Calculate the part lengths based on the corresponding cutlist.
            // Example: the max length part will be 300px wide, while one 2/3rds
            //          as big will be only 200px wide.
            for (int i = 0; i < cutlists.Count; i++)
            {
                Parts[i].DisplayLength = Convert.ToInt32(cutlists[i].Length / maxLength * DefaultDisplayWidthPx);
            }
        }
    }
}