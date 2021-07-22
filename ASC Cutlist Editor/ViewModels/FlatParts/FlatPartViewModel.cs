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
    // Represents a list of parts in a part row.
    internal class FlatPartViewModel : ObservableObject
    {
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

        public static FlatPartViewModel Instance { get; } = new FlatPartViewModel();

        // Draws a 2D view of the parts from a cutlist.
        public ObservableCollection<Part> CreatePart(Cutlist cutlist)
        {
            // Refresh the current list of parts.
            Parts = new ObservableCollection<Part>();

            Parts.Add(new Part
            {
                DisplayLabel = GetPartLabel(cutlist)
            });

            return Parts;
        }

        // Generates the label string for a part from a cutlist.
        public string GetPartLabel(Cutlist cutlist)
        {
            return "PROFILE X  LENGTH: " + cutlist.Length + "  " + cutlist.Quantity + "x";
        }
    }
}