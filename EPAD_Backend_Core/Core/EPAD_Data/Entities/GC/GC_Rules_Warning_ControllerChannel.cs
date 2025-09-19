using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace EPAD_Data.Entities
{
    public class GC_Rules_Warning_ControllerChannel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Index { get; set; }
        [Required]
        public int ControllerIndex { get; set; }
        [Required]
        public int ChannelIndex { get; set; }
        [Required]
        public int RulesWarningIndex { get; set; }
        [Required]
        public int Type { get; set; } // 0: Speaker, 1: Led
        public int? LineIndex { get; set; }
        public int? GateIndex { get; set; } //Không có gate thì là cảnh báo tập trung
        public string SerialNumber { get; set; } //Không có mcc thì là cảnh báo tập trung

        [Required]
        public int CompanyIndex { get; set; }
        public DateTime? CreatedDate { get; set; }

        public DateTime? UpdatedDate { get; set; }

        [StringLength(50)]
        public string UpdatedUser { get; set; }
    }
}
