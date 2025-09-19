using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace EPAD_Data.Entities
{
    public class GC_Rules_Warning_EmailSchedule
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Index { get; set; }
        [Required]
        public int DayOfWeekIndex { get; set; }
        [Required]
        public TimeSpan Time { get; set; }
        public DateTime? LatestDateSendMail { get; set; }

        [Required]
        public int CompanyIndex { get; set; }
        [Required]
        public int RulesWarningIndex { get; set; }
        public DateTime? CreatedDate { get; set; }

        public DateTime? UpdatedDate { get; set; }

        [StringLength(50)]
        public string UpdatedUser { get; set; }
    }
}
