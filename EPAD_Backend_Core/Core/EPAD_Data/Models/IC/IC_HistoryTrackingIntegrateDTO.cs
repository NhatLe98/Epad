using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models
{
    public class IC_HistoryTrackingIntegrateDTO
    {
        public int Index { get; set; }
        public string JobName { get; set; }
        public DateTime RunTime { get; set; }
        public short Success { get; set; }
        public short Failed { get; set; }
        public short DataNew { get; set; }
        public short DataUpdate { get; set; }
        public short DataDelete { get; set; }
        public int CompanyIndex { get; set; }
        public string RunTimeString { get; set; }
    }
}
