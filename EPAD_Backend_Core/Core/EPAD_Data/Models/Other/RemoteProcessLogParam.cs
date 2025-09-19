using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models
{
    public class RemoteProcessLogParam
    {
        public string SerialNumber { get; set; }
        public string IPAddress { get; set; }
        public int Port { get; set; }
        public string TaskName { get; set; }
        public List<SystemLog> ListSystemLog { get; set; }

    }
}
