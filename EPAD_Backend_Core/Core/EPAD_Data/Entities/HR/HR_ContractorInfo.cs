using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace EPAD_Data.Entities
{
    /// <summary>
    /// Nhà thầu
    /// </summary>
    public class HR_ContractorInfo
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

        /// <summary>
        /// CMND / CCCD
        /// </summary>
        [StringLength(100)]
        public string NRIC { get; set; }

        /// <summary>
        /// Thuộc công ty...
        /// </summary>
        [StringLength(200)]
        public string Company { get; set; }
        
        /// <summary>
        /// Địa chỉ
        /// </summary>
        [StringLength(500)]
        public string Address { get; set; }
        

        [StringLength(100)]
        public string Email { get; set; }

        [StringLength(50)]
        public string Phone { get; set; }

        /// <summary>
        /// Not required, EmployeeATID
        /// </summary>
        [Column(TypeName = "nvarchar(100)")]
        public string ContactPerson { get; set; }

        /// <summary>
        /// Hiệu lực từ thời gian
        /// </summary>
        public DateTime? FromTime { get; set; }

        /// <summary>
        /// Hiệu lực đến thời gian
        /// </summary>
        public DateTime? ToTime { get; set; }

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
