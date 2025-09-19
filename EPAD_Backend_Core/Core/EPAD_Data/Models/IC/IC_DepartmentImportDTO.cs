using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models.IC
{
    public class IC_DepartmentImportDTO
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public string ParentName { get; set; }
        public string Description { get; set; }
        public string ErrorMessage { get; set; }
        public int? ParentIndex { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int CompanyIndex { get; set; }
        public bool? IsDriverDepartment { get; set; }
        public bool? IsContractorDepartment { get; set; }
    }
}
