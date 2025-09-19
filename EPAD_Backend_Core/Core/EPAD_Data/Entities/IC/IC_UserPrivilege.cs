using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EPAD_Data.Entities
{
    public class IC_UserPrivilege
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Index { get; set; }

        [StringLength(100)]
        [Required]
        public string Name { get; set; }

        [Required]
        public bool UseForDefault { get; set; }

        [Required]
        public bool IsAdmin { get; set; }
        [StringLength(2000)]
        public string Note { get; set; }

        public DateTime? CreatedDate { get; set; }

        public DateTime? UpdatedDate { get; set; }

        [StringLength(50)]
        public string UpdatedUser { get; set; }

        [Required]
        public int CompanyIndex { get; set; }
    }
}