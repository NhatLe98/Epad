using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EPAD_Data.Entities
{
    public class GC_Rules_General //Quy định chung
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Index { get; set; }
        [StringLength(200)]
        [Required]
        public string Name { get; set; }
        [StringLength(200)]
        public string NameInEng { get; set; }
        [Required]
        public DateTime FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        [Required]
        public int CompanyIndex { get; set; }
        public DateTime? CreatedDate { get; set; }

        public DateTime? UpdatedDate { get; set; }

        [StringLength(50)]
        public string UpdatedUser { get; set; }

        [StringLength(50)]
        public string StartTimeDay { get; set; }

        public int MaxAttendanceTime { get; set; }
        public bool IsUsing { get; set; }
        public bool? IsBypassRule { get; set; }
        public int PresenceTrackingTime { get; set; }
        public bool RunWithoutScreen { get; set; }
        public bool IgnoreInLog { get; set; }
    }
}
