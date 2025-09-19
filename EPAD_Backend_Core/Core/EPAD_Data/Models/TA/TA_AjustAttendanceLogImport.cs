using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models
{
    public class TA_AjustAttendanceLogImport
    {
        public string EmployeeATID { get; set; }
        public string EmployeeCode { get; set; }
        public string FullName { get; set; }
        public string Date { get; set; }
        public string In { get; set; }
        public string Out { get; set; }
        public string VerifyMode { get; set; }
        public string SerialNumber { get; set; }
        public string Note { get; set; }
        public string Error { get; set; }
        public DateTime DateFormat { get; set; }
        public DateTime TimeIn { get; set; }
        public DateTime TimeOut { get; set; }
        public short VerifyModeFormat { get; set; }
    }
}
