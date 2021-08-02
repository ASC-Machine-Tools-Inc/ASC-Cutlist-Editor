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
using System.Windows.Media;
using ASC_Cutlist_Editor.Views;
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
        private ObservableCollection<SinglePartControl> _parts = new ObservableCollection<SinglePartControl>();

        public ObservableCollection<SinglePartControl> Parts
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
        public ObservableCollection<SinglePartControl> CreatePart(Cutlist cutlist)
        {
            // Refresh the current list of parts.
            Parts = new ObservableCollection<SinglePartControl>();

            SinglePartControl part = new SinglePartControl
            {
                PartGrid = { Width = 300 },
                PartRect = { Fill = (SolidColorBrush)new BrushConverter().ConvertFromString("#76EB7E") },
                PartLabel = { Text = GetPartLabel(cutlist) }
            };

            Parts.Add(part);
            return Parts;
        }

        // Generates the label string for a part from a cutlist.
        public string GetPartLabel(Cutlist cutlist)
        {
            return "PROFILE X  LENGTH: " + cutlist.Length + "  " + cutlist.Quantity + "x";
        }
    }
}