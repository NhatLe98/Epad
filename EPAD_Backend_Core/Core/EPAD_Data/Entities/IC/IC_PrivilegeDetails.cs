using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EPAD_Data.Entities
{
    public class IC_PrivilegeDetails
    {
        public int PrivilegeIndex { get; set; }

        [StringLength(100)]
        public string FormName { get; set; }

        [StringLength(20)]
        public string Role { get; set; }

        public int CompanyIndex { get; set; }

        public DateTime? UpdatedDate { get; set; }

        [StringLength(50)]
        public string UpdatedUser { get; set; }

       
    }
}