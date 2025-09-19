using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace EPAD_Data.Entities
{
    /// <summary>
    /// Nhân viên
    /// </summary>
    public class HR_EmployeeInfo
    {
        [Key]
        [Column(TypeName = "varchar(100)", Order = 0)]
        public string EmployeeATID { get; set; }

        /// <summary>
        /// Tự gen khi add
        /// </summary>
        [Key]
        [Required]
        public int CompanyIndex { get; set; }

        public DateTime? JoinedDate { get; set; }

        [StringLength(200)]
        public string Email { get; set; }

        [StringLength(50)]
        public string Phone { get; set; }

        /// <summary>
        /// Tự gen khi add / update
        /// </summary>
        public DateTime? UpdatedDate { get; set; }

        /// <summary>
        /// Tự gen khi add / update
        /// </summary>
        [StringLength(50)]
        public string UpdatedUser { get; set; }

    }
}
