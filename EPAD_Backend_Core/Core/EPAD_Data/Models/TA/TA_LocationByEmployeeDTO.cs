using EPAD_Data.Entities;
using System.Collections.Generic;

namespace EPAD_Data.Models
{
    public class TA_LocationByEmployeeDTO : TA_LocationByEmployee
    {
        public int EmployeeIndexDTO { get; set; }
        public string LocationName { get; set; }
        public string Address { get; set; }
        public string DepartmentName { get; set; }
        public string FullName { get; set;}
        public int DepartmentIndex { get; set; }
        public List<int> DepartmentList { get; set; }
        public List<string> EmployeeATIDs { get; set; }
    }
}
