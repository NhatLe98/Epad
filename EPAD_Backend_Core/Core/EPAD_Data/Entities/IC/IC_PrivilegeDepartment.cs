using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EPAD_Data.Entities
{
    public class IC_PrivilegeDepartment
    {
        public int PrivilegeIndex { get; set; }

        public long DepartmentIndex { get; set; }

        [StringLength(20)]
        public string Role { get; set; }

        public int CompanyIndex { get; set; }

        public DateTime? UpdatedDate { get; set; }

        [StringLength(50)]
        public string UpdatedUser { get; set; }
    }
}
