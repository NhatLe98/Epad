using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace EPAD_Data.Entities
{
    public class TA_Rules_Shift
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Index { get; set; }
        public int CompanyIndex { get; set; }
        public string UpdatedUser { get; set; }
        public DateTime UpdatedDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int? RuleInOut { get; set; }
        public DateTime EarliestAttendanceRangeTime { get; set; }
        public DateTime LatestAttendanceRangeTime { get; set; }
        public bool CheckOutOvernightTime { get; set; }
        public bool AllowedDoNotAttendance { get; set; }
        public int? MissingCheckInAttendanceLogIs { get; set; }
        public int? MissingCheckOutAttendanceLogIs { get; set; }
        public int? LateCheckInMinutes { get; set; }
        public int? EarlyCheckOutMinutes { get; set; }
        public int? MaximumAnnualLeaveRegisterByMonth { get; set; }
        public int? MaximumAnnualLeaveRegisterByYear { get; set; }
        public bool RoundingWorkedTime { get; set; }
        public int? RoundingWorkedTimeNum { get; set; }
        public int? RoundingWorkedTimeType { get; set; }
        public bool RoundingOTTime { get; set; }
        public int? RoundingOTTimeNum { get; set; }
        public int? RoundingOTTimeType { get; set; }
        public bool RoundingWorkedHour { get; set; }
        public int? RoundingWorkedHourNum { get; set; }
        public int? RoundingWorkedHourType { get; set; }
    }
}
