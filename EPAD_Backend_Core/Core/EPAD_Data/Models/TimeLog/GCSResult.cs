using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models.TimeLog
{
    public class GCSResult
    {
        public string Status { get; set; }
        public string MessageCode { get; set; }
        public string MessageDetail { get; set; }
        public object Data { get; set; }
    }
}
