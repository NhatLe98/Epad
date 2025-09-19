using EPAD_Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models
{
    public class GateLineData
    {
        public int GateIndex { get; set; }
        public int LineIndex { get; set; }
        public string LineName { get; set; }
        public bool InGroup { get; set; }

    }
    public class GateLineParam
    {
        public int GateIndex { get; set; }
        public List<int> ListLineIndex { get; set; }
    }
    public class LineModel : GC_Lines
    { 
        public List<int> GateIndexes { get; set; }
    }
}
