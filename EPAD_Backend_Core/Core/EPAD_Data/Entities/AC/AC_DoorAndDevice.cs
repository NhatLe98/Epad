using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Entities
{
    public class AC_DoorAndDevice
    {
        public int DoorIndex { get; set; }
        public string SerialNumber { get; set; }
        public int CompanyIndex { get; set; }
        public string UpdatedUser { get; set; }
        public DateTime UpdatedDate { get; set; }
    }
}
