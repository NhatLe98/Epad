using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EPAD_Data.Entities
{
    public class IC_HistoryTrackingIntegrate
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Index { get; set; }
        [StringLength(100)]
        [Required]
        public string JobName { get; set; }
        public DateTime RunTime { get; set; }
        public bool IsSuccess { get; set; }
        public int DataNew { get; set; }
        public int DataUpdate { get; set; }
        public int DataDelete { get; set; }
        public string Reason { get; set; }
        public int CompanyIndex { get; set; }

    }
}