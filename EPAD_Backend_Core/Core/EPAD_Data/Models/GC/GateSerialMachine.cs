using EPAD_Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models.GC
{
    public class GateSerialMachine : GC_Gates
    {
        public List<string> MachineList { get; set; }
    }
}
