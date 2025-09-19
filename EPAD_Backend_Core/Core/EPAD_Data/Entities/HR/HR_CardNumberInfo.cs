using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace EPAD_Data.Entities
{
    /// <summary>
    /// Quản lý thông tin card
    /// </summary>
    public class HR_CardNumberInfo
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Index { get; set; }

        [Column(TypeName = "varchar(100)", Order = 0)]
        public string EmployeeATID { get; set; }

        [Column(TypeName = "varchar(30)")]
        public string CardNumber { get; set; }

        public bool? IsActive { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }

        [StringLength(50)]
        public string UpdatedUser { get; set; }

        [Required]
        public int CompanyIndex { get; set; }
    }
}
