using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EPAD_Data.Entities
{
    public class IC_EmployeeType
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Index { get; set; }

        [StringLength(200)]
        [Required]
        public string Code { get; set; }

        [StringLength(200)]
        [Required]
        public string Name { get; set; }

        [StringLength(200)]
        public string NameInEng { get; set; }
        public bool IsUsing { get; set; }
        [StringLength(500)]
        public string Description { get; set; }

        [Required]
        public int CompanyIndex { get; set; }
        public DateTime? UpdatedDate { get; set; }

        [StringLength(100)]
        public string UpdatedUser { get; set; }
    }
}