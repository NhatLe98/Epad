using EPAD_Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models
{
    public class GatesModel 
    {
        public int Index { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsMandatory { get; set; }
        public List<int> Lines { get; set; }
        public LinesParam LineDevice { get; set; }
    }
    public class GateTree
    {
        public string ID { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public int ParentIndex { get; set; }

        public List<GateTree> ListChildrent { get; set; }
        public GateTree()
        {
            ListChildrent = new List<GateTree>();

        }
    }

}
