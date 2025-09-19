using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models
{
    public class AttendanceLog
    {
        public string EmployeeATID { get; set; }
        public string EmployeeCode { get; set; }
        public string SerialNumber { get; set; }
        public string IPAddress { get; set; }
        public int? Port { get; set; }
        public DateTime CheckTime { get; set; }
        public short? VerifyMode { get; set; }

        public short InOutMode { get; set; }
        public int? WorkCode { get; set; }
        public int? Reserve1 { get; set; }
        public DateTime? UpdatedDate { get; set; }

        public string DeviceId { get; set; }
        public string DeviceName { get; set; }
        public string NameOnMachine { get; set; }

        public int? DeviceNumber { get; set; }
        public string FullName { get; set; }
        public string DepartmentName { get; set; }
    }
}
