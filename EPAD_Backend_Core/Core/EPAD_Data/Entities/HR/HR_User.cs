using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EPAD_Data.Entities
{
    public class HR_User
    {
        [Key]
        [Column(TypeName = "varchar(100)",Order =0)]
        public string EmployeeATID { get; set; }

        /// <summary>
        /// Tự gen khi add
        /// </summary>
        [Key]
        [Required]
        public int CompanyIndex { get; set; }

        [StringLength(50)]
        public string EmployeeCode { get; set; }

        [StringLength(200)]
        public string FullName { get; set; }

        public byte[] Avatar { get; set; }

        /// <summary>
        /// 0: Nữ,
        /// 1: Nam,
        /// Null or Other => Khác
        /// </summary>
        public short? Gender { get; set; }

        public int? DayOfBirth { get; set; }

        public int? MonthOfBirth { get; set; }

        public int? YearOfBirth { get; set; }

        //[StringLength(200)]
        //public string NameOnMachine { get; set; }

        /// <summary>
        /// 0 || 1: Nhân viên, 2: Khách, 3: Học sinh, 4: Phụ huynh, 5: bảo mẫu, 6: Nhà thầu, 7: Giáo viên
        /// </summary>
        public int? EmployeeType { get; set; }
        public int? EmployeeTypeIndex { get; set; }

        public DateTime? CreatedDate { get; set; }

        /// <summary>
        /// Tự gen khi add / update
        /// </summary>
        public DateTime? UpdatedDate { get; set; }

        /// <summary>
        /// Tự gen khi add / update
        /// </summary>
        [StringLength(50)]
        public string UpdatedUser { get; set; }

        [StringLength(150)]
        public string Address { get; set; }

        [StringLength(200)]
        public string Description { get; set; }
        public string Note { get; set; }
        public string UserName { get; set; }
        public bool IsAllowPhone { get; set; }
        public string Nric { get; set; }

    }
}