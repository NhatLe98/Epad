using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Sync_Entities
{
    public class ECMSResponse
    {
        public Data Data { get; set; }
        public string MessageDetail { get; set; }
        public string Status { get; set; }
        public string MessageCode { get; set; }
    }

    public class Data
    {
        public int DataNew { get; set; }
        public int DataUpdate { get; set; }
        public bool Success { get; set; }
        public int DataDelete { get; set; }
    }

    public class ECMSRequestTime
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }
}
