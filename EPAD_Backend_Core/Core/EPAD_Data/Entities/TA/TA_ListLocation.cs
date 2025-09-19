using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace EPAD_Data.Entities
{
    public class TA_ListLocation
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Index { get; set; }
        [Required]
        public string LocationName { get; set; }
        public string Address { get; set;}
        [Required]
        public string Coordinates { get; set; }
        public string Radius { get; set; }
        [StringLength(2000)]
        public string Description { get; set; }
        public int CompanyIndex { get; set; }
        [StringLength(50)]
        public string UpdatedUser { get; set; }
        public DateTime UpdatedDate { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
