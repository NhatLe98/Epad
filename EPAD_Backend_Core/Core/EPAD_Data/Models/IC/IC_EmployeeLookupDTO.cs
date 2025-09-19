using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models
{
    public class IC_EmployeeLookupDTO
    {
        public string EmployeeATID { get; set; }
        public string FullName { get; set; }
        public long? DepartmentIndex { get; set; }
        public string Department { get; set; }
        public long PositionIndex { get; set; }
        public string PositionName { get; set; }
        public string CardNumber { get; set; }
        public string DepartmentCode { get; set; }
        public bool IsDepartmentChildren { get; set; }
    }
}
