using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace EPAD_Data.Entities
{
    public class GC_Rules_General_AreaGroup
    {
        [Required]
        public int AreaGroupIndex { get; set; }
        [Required]
        public int Rules_GeneralIndex { get; set; }
        public int Priority { get; set; }
        [Required]
        public int CompanyIndex { get; set; }
        public DateTime? UpdatedDate { get; set; }

        [StringLength(50)]
        public string UpdatedUser { get; set; }
    }
}
