using EPAD_Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models
{
    public class TA_Rules_ShiftDTO : TA_Rules_Shift
    {
        public int RuleInOutOther { get; set; }
        public string EarliestAttendanceRangeTimeString { get; set; }
        public string LatestAttendanceRangeTimeString { get; set; }
        public List<TA_Rules_Shift_InOutDTO> RuleInOutTime { get; set; }
    }

    public class TA_Rules_Shift_InOutDTO: TA_Rules_Shift_InOut
    {
        public string FromTimeString { get; set; }
        public string ToTimeString { get; set; }
    }
}
