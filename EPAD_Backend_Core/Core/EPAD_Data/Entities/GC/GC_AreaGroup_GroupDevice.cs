using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EPAD_Data.Entities
{
    public class GC_AreaGroup_GroupDevice
    {
        [Required]
        public int AreaGroupIndex { get; set; }
        [Required]
        public int DeviceGroupIndex { get; set; }
        [Required]
        public int CompanyIndex { get; set; }
        public DateTime? UpdatedDate { get; set; }

        [StringLength(50)]
        public string UpdatedUser { get; set; }
    }
}
