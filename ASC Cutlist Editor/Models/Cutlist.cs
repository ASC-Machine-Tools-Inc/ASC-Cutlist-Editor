using System;
using System.Collections.Generic;
using System.Text;

namespace ASC_Cutlist_Editor.Models
{
    public class Cutlist
    {
        public int ID { get; set; }

        public string Name { get; set; }

        public double Length { get; set; }

        public int Quantity { get; set; }

        public int Made { get; set; }

        public int Left { get; set; }

        public int Bundle { get; set; }
    }
}