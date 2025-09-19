using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EPAD_Data.Entities
{
    public class GC_CustomerVehicle
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Index { get; set; }
        [Required]
        public int CompanyIndex { get; set; }
        [Required]
        [Column(TypeName = "nvarchar(30)")]
        public string EmployeeATID { get; set; }

        public short Type { get; set; }

        [Column(TypeName = "nvarchar(30)")]
        public string Plate { get; set; }

        [Column(TypeName = "nvarchar(100)")]
        public string Branch { get; set; }

        [Column(TypeName = "nvarchar(100)")]
        public string Color { get; set; }

        public DateTime? CreatedDate { get; set; }

        public DateTime? UpdatedDate { get; set; }

        [StringLength(50)]
        public string UpdatedUser { get; set; }
    }
}
