using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models
{
    public class MonitoringDevice
    {
        public int CompanyIndex { get; set; }
        public Dictionary<string, int> LineInDeviceSerialList { get; set; }
        public Dictionary<string, int> LineOutDeviceSerialList { get; set; }
        public Dictionary<string, int> LineInputDeviceSerialList { get; set; }
        public List<int> ListLineParking { get; set; }

        public MonitoringDevice()
        {
            LineInDeviceSerialList = new Dictionary<string, int>();
            LineOutDeviceSerialList = new Dictionary<string, int>();
            LineInputDeviceSerialList = new Dictionary<string, int>();
            ListLineParking = new List<int>();
        }
    }
}
