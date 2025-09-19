using EPAD_Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models
{
    public class TA_ShiftDTO : TA_Shift
    {
        public string PaidHolidayStartTimeString { get; set; }
        public string PaidHolidayEndTimeString { get; set; }
        public string CheckInTimeString { get; set; }
        public string CheckOutTimeString { get; set; }
        public string BreakStartTimeString { get; set; }
        public string BreakEndTimeString { get; set; }
        public string OTStartTimeFirstString { get; set; }
        public string OTEndTimeFirstString { get; set; }
        public string OTStartTimeString { get; set; }
        
        public string OTEndTimeString { get; set; }
    }
}
