using EPAD_Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models
{
    public class TA_AnnualLeaveDTO : TA_AnnualLeave
    {
        public long DepartmentIndex { get; set; }
        public string DepartmentName { get; set; }
        public string FullName { get; set; }
        public string EmployeeCode { get; set; }
    }
}
