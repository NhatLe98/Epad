using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EPAD_Data.Entities
{
    public class GC_Rules_General_Log //Mở rộng của quy định chung
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Index { get; set; }
        [Required]
        [StringLength(100)]
        public string AreaGroupIndex { get; set; }
        public bool UseDeviceMode { get; set; }
        public bool UseSequenceLog { get; set; }
        public bool UseMinimumLog { get; set; }
        public bool UseTimeLog { get; set; }
        public int UseMode { get; set; } //Chọn chế độ: 0: UseDeviceMode, 1: UseSequenceLog, 2: UseMinimumLog, 3: UseTimeLog
        public int MinimumLog { get; set; }

        public DateTime? FromEarlyDate { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public DateTime? ToLateDate { get; set; }
        public bool FromIsNextDay { get; set; }
        public bool ToIsNextDay { get; set; }
        public bool ToLateIsNextDay { get; set; }

        [Required]
        public int RuleGeneralIndex { get; set; }

        [Required]
        public int CompanyIndex { get; set; }
        public DateTime? CreatedDate { get; set; }

        public DateTime? UpdatedDate { get; set; }

        [StringLength(50)]
        public string UpdatedUser { get; set; }
    }
}
