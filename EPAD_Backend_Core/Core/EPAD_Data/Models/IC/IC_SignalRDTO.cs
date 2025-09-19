using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EPAD_Data.Models
{
    public class IC_SignalRDTO
    {
        /// <summary>
        /// For user finger
        /// </summary>
        public class UserFingerDeviceParam
        {
            public int CompanyIndex { get; set; }
            public string Finger { get; set; }
            public int FingerIndex { get; set; }
            public string Erorr { get; set; }
        }

        /// <summary>
        /// for push notification
        /// </summary>
        public class Notification
        {
            public int CompanyIndex { get; set; }
            public List<SerialNumberCommandParam> Message { get; set; }
        }

        public class SerialNumberCommandParam
        {
            public string SerialNumber { get; set; }
            public string Result { get; set; }
            public string Erorr { get; set; }
            public string IPAddress { get; set; }
        }
    }
}
