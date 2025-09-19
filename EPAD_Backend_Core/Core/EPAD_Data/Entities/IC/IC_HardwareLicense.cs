using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace EPAD_Data.Entities
{
    public class IC_HardwareLicense
    {
        [Key]
        public int Index { get; set; }
        public int CompanyIndex { get; set; }
        [StringLength(2000)]
        public string Data { get; set; }
        [StringLength(50)]
        public string FileName { get; set; }
        [StringLength(500)]
        public string Note { get; set; }
        public DateTime? UpdatedDate { get; set; }
        [StringLength(50)]
        public string UpdatedUser { get; set; }
    }
}
