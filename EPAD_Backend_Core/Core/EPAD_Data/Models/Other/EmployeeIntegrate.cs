using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models
{
    public class EmployeeIntegrate
    {
        public int Index { get; set; }
        public string EmployeeATID { get; set; }
        public string EmployeeCode { get; set; }
        public string CardNumber { get; set; }
        public string FullName { get; set; }
        public string DepartmentCode { get; set; }
        public string DepartmentName { get; set; }
        public string Position { get; set; }
        public string Status { get; set; }
        public DateTime UpdatedDate { get; set; }
        public DateTime? StoppedDate { get; set; }
        public int? WorkCode { get; set; }
        public int? OrgUnitID { get; set; }
        public int? OrgUnitParentNode { get; set; }
        public bool? StatusStandard { get; set; }
        public DateTime FromDate { get; set; }
        public string Note { get; set; }
    }
}
