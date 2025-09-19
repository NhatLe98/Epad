using EPAD_Data.Entities;
using EPAD_Data.Entities.HR;
using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models.HR
{
    public class VStarEmployeeInfoResult
    {
        public string EmployeeATID { get; set; }
        public string EmployeeCode { get; set; }
        public string FullName { get; set; }
        public string CardNumber { get; set; }
        public long? DepartmentIndex { get; set; }
        public int TeamIndex { get; set; }
        public string DepartmentName { get; set; }
        public string TeamName { get; set; }
        public string EmployeeTypeName { get; set; }
        public long EmployeeType { get; set; }
    }
}
