using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models
{
    public class AC_GroupDTO
    {
        public int UID { get; set; }
        public int Verify { get; set; }
        public bool ValidHoliday { get; set; }
        public string TimeZoneString { get; set; }
        public int CompanyIndex { get; set; }
        public string UpdatedUser { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string Name { get; set; }
        public int Timezone { get; set; }
        public string TimezoneName { get; set; }
        public int DoorIndex { get; set; }
        public string DoorName { get; set; }
    }
}
