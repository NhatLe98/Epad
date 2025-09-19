using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models
{
    public class IntergateLogParam
    {
        public string SerialNumber { get; set; }

        public DateTime FromTIme { get; set; }
        public DateTime ToTime { get; set; }
        public string IPAddress { get; set; }
    }
}
