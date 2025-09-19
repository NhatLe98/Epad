using System;
using System.ComponentModel.DataAnnotations;

namespace EPAD_Data.Entities
{
    public class Audit_Model
    {
        public DateTime CreatedDate { get; set; }
        [StringLength(100)]
        public string CreatedUser { get; set; }
        public DateTime? UpdatedDate { get; set; }
        [StringLength(100)]
        public string UpdatedUser { get; set; }
    }
}
