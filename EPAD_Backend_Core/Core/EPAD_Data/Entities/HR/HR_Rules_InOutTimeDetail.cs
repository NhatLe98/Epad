using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EPAD_Data.Entities
{
    public class HR_Rules_InOutTimeDetail
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Index { get; set; }
        public int RulesIndex { get; set; }
        public int TypeRules {  get; set; }
        public DateTime CheckInTime { get; set; }
        public int MaxEarlyCheckInMinute { get; set; }
        public int MaxLateCheckInMinute { get; set; }
        public DateTime CheckOutTime { get; set; }
        public int MaxEarlyCheckOutMinute { get; set; }
        public int MaxLateCheckOutMinute { get; set; }
        public int CompanyIndex {  get; set; }
        public DateTime? UpdateDate { get; set; }
        public DateTime? CreateDate { get; set; }
        public string UpdateUser { get; set; }
    }
}
