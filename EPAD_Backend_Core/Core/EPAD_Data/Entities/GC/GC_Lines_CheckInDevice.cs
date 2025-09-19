using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace EPAD_Data.Entities
{
    public class GC_Lines_CheckInDevice
    {
        [Required]
        public int LineIndex { get; set; }

        [StringLength(50)]
        public string CheckInDeviceSerial { get; set; }

        [Required]
        public int CompanyIndex { get; set; }

        public DateTime? UpdatedDate { get; set; }

        [StringLength(50)]
        public string UpdatedUser { get; set; }
    }
}
