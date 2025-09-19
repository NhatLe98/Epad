using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace EPAD_Data.Entities
{
    public class TA_Shift
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Index { get; set; }
        public int CompanyIndex { get; set; }
        public string UpdatedUser { get; set; }
        public DateTime UpdatedDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int RulesShiftIndex { get; set; }
        public bool IsPaidHolidayShift { get; set; }
        public DateTime? PaidHolidayStartTime { get; set; }
        public DateTime? PaidHolidayEndTime { get; set; }
        public bool PaidHolidayEndOvernightTime { get; set; }
        public DateTime? CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }
        public bool CheckOutOvernightTime { get; set; }
        public bool IsBreakTime { get; set; }
        public DateTime? BreakStartTime { get; set; }
        public DateTime? BreakEndTime { get; set; }
        public bool BreakStartOvernightTime { get; set; }
        public bool BreakEndOvernightTime { get; set; }
        public bool DetermineBreakTimeByAttendanceLog { get; set; }
        //OT first
        public bool IsOTFirst { get; set; }
        public DateTime? OTStartTimeFirst { get; set; }
        public DateTime? OTEndTimeFirst { get; set; }

        //OT later
        public bool IsOT { get; set; }
        public DateTime? OTStartTime { get; set; }
        public DateTime? OTEndTime { get; set; }
        public bool OTStartOvernightTime { get; set; }
        public bool OTEndOvernightTime { get; set; }
        public int? AllowLateInMinutes { get; set; }
        public int? AllowEarlyOutMinutes { get; set; }
        public float TheoryWorkedTimeByShift { get; set; }
    }
}
