using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace EPAD_Data.Entities.HR
{
    public class HR_Rules_InOutTime
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Index { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int CompanyIndex { get; set; }
        public string Description { get; set; }
        public DateTime? UpdatedDate { get; set; }
        [StringLength(100)]
        public string UpdatedUser { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? CheckInTime { get; set; }
        public int MaxEarlyCheckInMinute { get; set; }
        public int MaxLateCheckInMinute { get; set; }
        public DateTime? CheckOutTime { get; set; }
        public int MaxEarlyCheckOutMinute { get; set; }
        public int MaxLateCheckOutMinute { get; set; }
        public bool IsBoarding { get; set; }
        public bool IsSession { get; set; }
        public bool IsDayBoarding { get; set; }
    }
}
