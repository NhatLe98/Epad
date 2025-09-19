using EPAD_Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models
{
    public class TA_AjustAttendanceLogDTO : TA_AjustAttendanceLog
    {
        public long DepartmentIndex { get; set; }
        public string DepartmentName { get; set; }
        public string FullName { get; set; }
        public string EmployeeCode { get; set; }
        public string InOutModeString { get; set; }
        public string VerifyModeString { get; set; }
        public bool IsError { get; set; }
        public bool IsWarning { get; set; }
        public string Day { get; set; }
        public string DeviceName { get; set; }
        public string ProcessedCheckTimeString { get; set; }
        public string indexStt { get; set; }
    }
}
