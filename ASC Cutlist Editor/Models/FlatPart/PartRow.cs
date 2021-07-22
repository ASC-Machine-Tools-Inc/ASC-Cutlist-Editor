using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace AscCutlistEditor.Models
{
    public class PartRow
    {
        public ObservableCollection<Part> Parts { get; set; }
    }
}