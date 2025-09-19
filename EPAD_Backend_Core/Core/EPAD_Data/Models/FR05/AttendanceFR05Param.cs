using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models.FR05
{
    public class AttendanceFR05Param
    {
        public string DeviceKey { get; set; }
        public string PersonId { get; set; }
        public string Time { get; set; }
        public string Type { get; set; }
        public string Path { get; set; }
        public string ImageBase64 { get; set; }
        public string Data { get; set; }
        public string IP { get; set; }
        public string SearchCode { get; set; }
        public string LivenessScore { get; set; }
        public string Temperature { get; set; }
    }
}
