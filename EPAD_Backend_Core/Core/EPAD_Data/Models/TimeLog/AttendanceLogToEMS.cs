using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models.TimeLog
{
    public class AttendanceLogToEMS
    {
        public int WeekNumber { get; set; }

        public List<WeekTimeLogDetailRequest> WeekTimeLogDetails { get; set; }
    }

    public class WeekTimeLogDetailRequest
    {
        public string EmployeeATID { get; set; }
        public string RoomId { get; set; }
        public DateTime CheckTime { get; set; }
        public int CompanyIndex { get; set; }
        public bool InOutMode { get; set; }
        public string WorkCode { get; set; }
    }
}
