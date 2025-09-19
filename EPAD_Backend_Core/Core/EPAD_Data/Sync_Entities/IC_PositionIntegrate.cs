using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace EPAD_Data.Entities
{
    [Table("IC_Position")]
    public class IC_PositionIntegrate
    {
        public string Code { get; set; }
        public string PositionName { get; set; }
        public string PositionEngName { get; set; }
        public bool? Status { get; set; }
        public DateTime? DateUpdated { get; set; }
        public DateTime DateCreated { get; set; }
    }
}
