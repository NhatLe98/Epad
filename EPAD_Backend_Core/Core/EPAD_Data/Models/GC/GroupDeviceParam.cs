using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models
{
    public class GroupDeviceParam
    {
        public int Index { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<string> ListMachine { get; set; }

    }
}
