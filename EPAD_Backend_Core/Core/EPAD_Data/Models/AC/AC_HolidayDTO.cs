using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models
{
    public class AC_HolidayDTO
    {
        public int UID { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TimeZone { get; set; }
        public string HolidayName { get; set; }
        public int DoorIndex { get; set; }
        public List<int> DoorIndexes { get; set; }
        public string DoorName { get; set; }
        public string TimezoneName { get; set; }
        public string StartDateString { get; set; }
        public string EndDateString { get; set; }
        public int TimezoneRange { get; set; }
        public string TimezoneRangeName { get; set; }
        public int HolidayType { get; set; }
        public bool Loop { get; set; }
        public string LoopName { get; set; }
        public string HolidayTypeName { get; set; }
    }
}
