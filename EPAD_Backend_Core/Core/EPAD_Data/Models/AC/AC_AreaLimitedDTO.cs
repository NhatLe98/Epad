using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models
{
    public class AC_AreaLimitedDTO
    {
        public int Index { get; set; }
        public string Name { get; set; }
        public List<int> DoorIndexes { get; set; }
        public string DoorName { get; set; }
        public string Description { get; set; }
    }
}
