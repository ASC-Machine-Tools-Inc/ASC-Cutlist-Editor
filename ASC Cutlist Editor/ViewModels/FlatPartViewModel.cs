using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
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
        // Draws a 2D view of the parts from a cutlist.
        public void DrawParts(List<Cutlist> cutlists)
        {
        }
    }
}