using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models
{
    public class IntegrateLogMongo
    {
        public DateTime IntegrateTime { get; set; }
        public IntegrateLogParam Param { get; set; }
        public int LogCount { get; set; }
        public int CompanyIndex { get; set; }

        public bool Success { get; set; }
        public string Error { get; set; }
        public string Data { get; set; }
    }
}
