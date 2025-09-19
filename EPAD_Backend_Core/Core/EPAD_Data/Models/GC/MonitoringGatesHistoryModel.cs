using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models
{
    public class MonitoringGatesHistoryModel
    {
        public string EmployeeATID { get; set; }
        public string EmployeeCode { get; set; }
        public string CustomerName { get; set; }
        public string CustomerImage { get; set; }
        public int GateIndex { get; set; }
        public string GateName { get; set; }
        public int LineIndex { get; set; }
        public string LineName { get; set; }
        public DateTime CheckTime { get; set; }
        public string DepartmentName { get; set; }
        public string InOutMode { get; set; }
        public string CardNumber { get; set; }
        public string Note { get; set; }
        public string Error { get; set; }
        public string VerifyMode { get; set; }
        public string PhoneNumber { get; set; }
        public string StatusLog { get; set; }
    }
}
