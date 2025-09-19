using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models.HR
{
    public class Rules_InOutTimeParam
    {
        public int Index { get; set; }
        public string FromDate { get; set; }
        public string Description { get; set; }
        public DateTime? CheckInTime { get; set; }
        public string CheckInTimeString { get; set; }
        public int MaxEarlyCheckInMinute { get; set; }
        public int MaxLateCheckInMinute { get; set; }
        public DateTime? CheckOutTime { get; set; }
        public string CheckOutTimeString { get; set; }
        public int MaxEarlyCheckOutMinute { get; set; }
        public int MaxLateCheckOutMinute { get; set; }
    }
}
