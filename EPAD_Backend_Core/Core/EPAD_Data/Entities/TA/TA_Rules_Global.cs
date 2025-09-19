using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace EPAD_Data.Entities
{
    public class TA_Rules_Global
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Index { get; set; }
        public int CompanyIndex { get; set; }
        public int? MaximumAnnualLeaveRegisterByMonth { get; set; }
        public int LockAttendanceTime { get; set; }
        public int OverTimeNormalDay { get; set; }
        public int NightOverTimeNormalDay { get; set; }
        public int OverTimeLeaveDay { get; set; }
        public int NightOverTimeLeaveDay { get; set; }
        public int OverTimeHoliday { get; set; }
        public int NightOverTimeHoliday { get; set; }
        public DateTime? NightShiftStartTime { get; set; }
        public DateTime? NightShiftEndTime { get; set; }
        public bool NightShiftOvernightEndTime { get; set; }
        public bool IsAutoCalculateAttendance { get; set; }
        public string TimePos { get; set; }
    }
}
