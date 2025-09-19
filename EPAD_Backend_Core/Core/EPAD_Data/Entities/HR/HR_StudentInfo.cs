using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace EPAD_Data.Entities
{
    /// <summary>
    /// Học sinh
    /// </summary>
    public class HR_StudentInfo
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
        public string ClassID { get; set; }

        public DateTime? UpdatedDate { get; set; }

        [StringLength(50)]
        public string UpdatedUser { get; set; }
    }
}
