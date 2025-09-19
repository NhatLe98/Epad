using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models
{
    public class TA_AnnualLeaveImportParam
    {
        public string EmployeeATID { get; set; }
        public string EmployeeCode { get; set; }
        public string EmployeeName { get; set; }
        public string AnnualLeave { get; set; }
        public string ErrorMessage { get; set; }
    }
}
