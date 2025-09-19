using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Entities
{
    public class AC_AreaAndDoor
    {
        public int AreaIndex { get; set; }
        public int DoorIndex { get; set; }
        public int CompanyIndex { get; set; }
        public string UpdatedUser { get; set; }
        public DateTime UpdatedDate { get; set; }
    }
}
