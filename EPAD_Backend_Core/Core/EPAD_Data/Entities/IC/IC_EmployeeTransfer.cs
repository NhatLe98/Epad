using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EPAD_Data.Entities
{
    public class IC_EmployeeTransfer
    {
        [Column(TypeName = "varchar(100)", Order = 0)]
        public string EmployeeATID { get; set; }

        public int NewDepartment { get; set; }

        public DateTime FromTime { get; set; }

        public int CompanyIndex { get; set; }

        public long? OldDepartment { get; set; }

        public bool RemoveFromOldDepartment { get; set; }

        public bool AddOnNewDepartment { get; set; }

        public DateTime ToTime { get; set; }

        public bool? IsSync { get; set; }
        [StringLength(2000)]
        public string Description { get; set; }

        public DateTime? CreatedDate { get; set; }

        public DateTime? UpdatedDate { get; set; }

        [StringLength(50)]
        public string UpdatedUser { get; set; }
        public short Status { get; set; }
        public DateTime? ApprovedDate { get; set; }
        [StringLength(50)]
        public string ApprovedUser { get; set; }
       
    }
}