using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models
{
    public class AC_UserMasterDTO
    {
        public int CompanyIndex { get; set; }
        public string EmployeeATID { get; set; }
        public string EmployeeCode { get; set; }
        public string FullName { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string DepartmentName { get; set; }
        public int AreaIndex { get; set; }
        public string AreaName { get; set; }
        public int DoorIndex { get; set; }
        public string DoorName { get; set; }
        public int Operation { get; set; }
        public string OperationString { get; set; }
        public string TimezoneName { get; set; }
        public string UpdatedDateString { get; set; }
    }
}
