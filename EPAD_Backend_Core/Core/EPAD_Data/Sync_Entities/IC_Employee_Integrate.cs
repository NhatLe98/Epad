using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EPAD_Data.Entities
{
    public class IC_Employee_Integrate
    {
        public int Index { get; set; }
        public string EmployeeATID { get; set; }
        public string EmployeeCode { get; set; }
        public string CardNumber { get; set; }
        public string FullName { get; set; }
        public string Department { get; set; }
        public string DepartmentCode { get; set; }
        public string Position { get; set; }
        public string Status { get; set; }
        public DateTime UpdatedDate { get; set; }
        public DateTime? StoppedDate { get; set; }
        public int? OrgUnitID { get; set; }
        public int? OrgUnitParentNode { get; set; }
    }
}
