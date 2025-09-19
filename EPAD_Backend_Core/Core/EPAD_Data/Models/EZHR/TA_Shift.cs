using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models.EZHR
{
    public class TA_Shift
    {
        public int Index { get; set; }
        public int CompanyIndex { get; set; }
        public string Name { get; set; }
        public string NameInEng { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public DateTime? StartTimeRequired { get; set; }
        public DateTime? EndTimeRequired { get; set; }
        public DateTime? StartTimeNonOT { get; set; }
        public DateTime? EndTimeNonOT { get; set; }
        public float? RequiredWorkingHours { get; set; }
        public float? TotalNonOT { get; set; }
        public float? TotalOT { get; set; }
        public float? TotalForCardCount { get; set; }
        public float? CardCount { get; set; }
        public bool? Cover { get; set; }
        public bool? SpecialShift { get; set; }
        public bool? IsServiceShift { get; set; }
        public short? Service_LogRule { get; set; }
        public int? Rules_AllowTimeInOutIndex { get; set; }
        public int? Rules_TimeLogIndex { get; set; }
        public int? Rules_GeneralIndex { get; set; }
        public string Description { get; set; }
        public DateTime? ActiveTo { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string UpdatedUser { get; set; }
        public bool? CoverWithBreakInOut { get; set; }
        public float? CardCountIfMissingLog { get; set; }
    }
}
