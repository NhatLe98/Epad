using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EPAD_Data.Entities
{
    public class IC_EmployeeAndDepartment
    {
        [Key, Column(Order = 0)]
        public int DepartmentIndex { get; set; }

        [Column(TypeName = "varchar(100)", Order = 1)]
        [Key]
        public string EmployeeATID { get; set; }

        [Key, Column(Order = 2)]
        public int CompanyIndex { get; set; }

        public DateTime? UpdatedDate { get; set; }

        [StringLength(50)]
        public string UpdatedUser { get; set; }
    }

}