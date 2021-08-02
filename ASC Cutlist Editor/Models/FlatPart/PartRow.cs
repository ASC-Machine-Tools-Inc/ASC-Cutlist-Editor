using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using ASC_Cutlist_Editor.Views;

namespace AscCutlistEditor.Models
{
    public class PartRow
    {
        public ObservableCollection<SinglePartControl> Parts { get; set; }

        public string ListBoxName { get; set; }
    }
}