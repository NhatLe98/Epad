using System;

namespace EPAD_Data.Entities
{
    public class HR_EmployeeReport
    {
        public int CompanyIndex { get; set; }
        public string EmployeeATID { get; set; }
        public string EmployeeCode { get; set; }
        public string CardNumber { get; set; }
        public string FullName { get; set; }
        public bool? Gender { get; set; }
        public string NameOnMachine { get; set; }
        public long? DepartmentIndex { get; set; }
        public string DepartmentName { get; set; }
        public DateTime? JoinedDate { get; set; }

    }
}
