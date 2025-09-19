using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EPAD_Data.Entities
{
    public class IC_UserAccount
    {
        [Key]
        [Column(TypeName = "varchar(100)", Order = 0)]
        public string EmployeeATID { get; set; }

        [Column(TypeName = "varchar(50)", Order =0)]
        public string UserName { get; set; }

        public int CompanyIndex { get; set; }

        [StringLength(500)]
        [Required]
        public string Password { get; set; }

        [StringLength(100)]
        [Required]
        public string Name { get; set; }

        [StringLength(20)]
        public string ResetPasswordCode { get; set; }

        public bool Disabled { get; set; }

        public DateTime? LockTo { get; set; }

        public DateTime? CreatedDate { get; set; }

        public DateTime? UpdatedDate { get; set; }

        [StringLength(50)]
        public string UpdatedUser { get; set; }

        public short AccountPrivilege { get; set; }

        
    }
}