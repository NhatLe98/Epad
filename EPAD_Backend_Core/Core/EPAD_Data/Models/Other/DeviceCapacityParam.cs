using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models
{
    public class DeviceCapacityParam
    {
        public string SerialNumber { get; set; }
        public int UserCount { get; set; }
        public int FingerCount { get; set; }
        public int AttendanceLogCount { get; set; }
        public int FaceCount { get; set; }
        public int UserCapacity { get; set; }
        public int FingerCapacity { get; set; }
        public int AttendanceLogCapacity { get; set; }
        public int FaceCapacity { get; set; }
    }
}
