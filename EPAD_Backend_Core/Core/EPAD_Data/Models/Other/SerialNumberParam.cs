using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models
{
    public class SerialNumberParam
    {
        public string AliasName { get; set; }

        public string SerialNumber { get; set; }
        public string IPAddress { get; set; }
    }

    public class DeviceLineBasicInfo
    {
        public int Index { get; set; }
        public string Name { get; set; }
        public string SerialNumber { get; set; }
    }
}
