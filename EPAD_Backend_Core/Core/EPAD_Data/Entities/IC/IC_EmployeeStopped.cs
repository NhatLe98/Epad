using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace EPAD_Data.Entities
{
    public class IC_EmployeeStopped
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Index { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string EmployeeATID { get; set; }
        [Required]
        public DateTime StoppedDate { get; set; }
        [Required]
        public string Reason { get; set; } 
        [Required]
        public int CompanyIndex { get; set; }
        public DateTime? CreateDate { get; set; }
        public DateTime? UpdatedDate { get; set; }

        [StringLength(100)]
        public string UpdatedUser { get; set; }
    }
}
