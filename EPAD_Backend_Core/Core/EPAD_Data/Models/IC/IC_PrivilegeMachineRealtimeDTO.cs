using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models.IC
{
    public class IC_PrivilegeMachineRealtimeDTO
    {
        public string UserName { get; set; }
        public List<string> ListUserName { get; set; }
        public short PrivilegeGroup { get; set; }
        public string PrivilegeGroupName { get; set; }
        public string GroupDeviceIndex { get; set; }
        public string GroupDeviceName { get; set; }
        public List<int> ListGroupDeviceIndex { get; set; }
        public List<string> ListGroupDeviceName { get; set; }
        public string DeviceModule { get; set; }
        public string DeviceModuleName { get; set; }
        public List<string> ListDeviceModule { get; set; }
        public List<string> ListDeviceModuleName { get; set; }
        public string DeviceSerial { get; set; }
        public string DeviceName { get; set; }
        public List<string> ListDeviceSerial { get; set; }
        public List<string> ListDeviceName { get; set; }
    }
}
