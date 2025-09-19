using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace EPAD_Data.Entities
{
    public class TA_Rules_Shift_InOut
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Index { get; set; }
        public int RuleShiftIndex { get; set; }
        public string UpdatedUser { get; set; }
        public DateTime UpdatedDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public int CompanyIndex { get; set; }
        public int TimeMode { get; set; }
        public DateTime FromTime { get; set; }
        public bool FromOvernightTime { get; set; }
        public DateTime ToTime { get; set; }
        public bool ToOvernightTime { get; set; }
    }
}
