using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EPAD_Data.Entities
{
    public class GC_Rules_GeneralAccess_Gates
    {
        [Required]
        public int RulesGeneralIndex { get; set; }
        public int GateIndex { get; set; }
        public string LineIndexs { get; set; }


        [Required]
        public int CompanyIndex { get; set; }


        public DateTime? UpdatedDate { get; set; }

        [StringLength(50)]
        public string UpdatedUser { get; set; }
    }
}
