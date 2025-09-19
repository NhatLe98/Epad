using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models
{
    public class HR_UserLookupInfo
    {
        public string EmployeeATID { get; set; }
        public int EmployeeType { get; set; }
        public string DepartmentName { get; set; }

        public string DisplayName { get { return FullName + " - " + DepartmentName; } }

        public string FullName { get; set; }
        public string BirthDay { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public short? Gender { get; set; }
        public int DepartmentIndex { get; set; }
    }
}
