using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EPAD_Data.Models
{
    public class IC_DepartmentDTO
    {
        public long Index { get; set; }
        public int CompanyIndex { get; set; }
        public string Code { get; set; }
        public string Location { get; set; }
        public string Description { get; set; }
        public string Name { get; set; }
        public long? ParentIndex { get; set; }
        public string ParentName { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string UpdatedUser { get; set; }
        public int? OrgUnitID { get; set; }
        public int? OrgUnitParentNode { get; set; }
        public int? OVNID { get; set; }
        public bool? IsUpdate { get; set; }
        public bool? IsInsert { get; set; }
        public string ParentCode { get; set; }
        public bool IsStore { get; set; }
        public int? JobGrade { get; set; }
        public int Type { get; set; }
        public bool? IsContractorDepartment { get; set; }
        public bool? IsDriverDepartment { get; set; }
    }
}
