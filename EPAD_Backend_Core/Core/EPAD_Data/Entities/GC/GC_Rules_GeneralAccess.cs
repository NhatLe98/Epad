using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EPAD_Data.Entities
{
    public class GC_Rules_GeneralAccess
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Index { get; set; }
        [StringLength(200)]
        [Required]
        public string Name { get; set; }
        [StringLength(200)]
        public string NameInEng { get; set; }
        /*  các qui định về thời gian vào ra cho phép  */
        public bool CheckInByShift { get; set; }
        [Column(TypeName = "smalldatetime")]
        public DateTime? CheckInTime { get; set; }
        public int MaxEarlyCheckInMinute { get; set; }
        public int MaxLateCheckInMinute { get; set; }
        public bool CheckOutByShift { get; set; }
        public DateTime? BeginLastHaftTime { get; set; }
        public DateTime? EndFirstHaftTime { get; set; }
        [Column(TypeName = "smalldatetime")]
        public DateTime? CheckOutTime { get; set; }
        public int MaxEarlyCheckOutMinute { get; set; }
        public int MaxLateCheckOutMinute { get; set; }
        public bool AllowFreeInAndOutInTimeRange { get; set; }
        public bool AllowEarlyOutLateInMission { get; set; }
        public int MissionMaxEarlyCheckOutMinute { get; set; }
        public int MissionMaxLateCheckInMinute { get; set; }
        public bool AdjustByLateInEarlyOut { get; set; }
        /*  các qui định liên quan đến ra giữa giờ có đăng ký  */
        public bool AllowInLeaveDay { get; set; }
        public bool AllowInMission { get; set; }
        public bool AllowInBreakTime { get; set; }
        /*  các qui định liên quan đến ra giữa giờ không đăng ký */
        public bool AllowCheckOutInWorkingTime { get; set; }
        public string AllowCheckOutInWorkingTimeRange { get; set; }
        public int MaxMinuteAllowOutsideInWorkingTime { get; set; }
        /*  các qui định cấm vào ra  */
        public bool DenyInLeaveWholeDay { get; set; }
        public bool DenyInMissionWholeDay { get; set; }
        public bool DenyInStoppedWorkingInfo { get; set; }

        /* các quy định check log theo khu vực*/
        public bool CheckLogByAreaGroup { get; set; }
        public bool CheckLogByShift { get; set; } // Tích hợp ca làm việc

        public bool UseDefault { get; set; }

        [Required]
        public int CompanyIndex { get; set; }
        public DateTime? CreatedDate { get; set; }

        public DateTime? UpdatedDate { get; set; }

        [StringLength(50)]
        public string UpdatedUser { get; set; }
    }
}
