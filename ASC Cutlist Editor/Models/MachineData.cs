using System;
using System.Collections.Generic;
using System.Text;

namespace AscCutlistEditor.Models
{
    internal class MachineData
    {
        public string ConnectionStatus { get; set; }

        public string JobNumber { get; set; }

        public string LineStatus { get; set; }

        public DateTime TimeStamp { get; set; }
    }
}