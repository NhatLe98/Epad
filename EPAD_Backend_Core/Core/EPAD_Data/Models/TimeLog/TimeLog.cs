using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models.TimeLog
{
    public class TimeLog
    {
        public string EmployeeATID { get; set; }
        public string EmployeeCode { get; set; }
        public DateTime Time { get; set; }
        public string MachineSerial { get; set; }
        public short InOutMode { get; set; }
        public short SpecifiedMode { get; set; }
        public string Action { get; set; }
        public int? DeviceNumber { get; set; }
        public string DeviceId { get; set; }
    }
}
