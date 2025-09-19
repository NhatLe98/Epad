using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models.TimeLog
{
    public class TimeLogRequest
    {
        public List<TimeLog> TimeLogs { get; set; }
        public TimeLogRequest()
        {
            TimeLogs = new List<TimeLog>();
        }
    }
}
