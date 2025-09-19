using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models.TimeLog
{
    public class AttendanceLogRealTime: BaseAttendanceLog
    {
        public int? EmployeeType { get; set; }
        public string DepartmentName { get; set; }
        public string Avatar { get; set; }
        public string PositionName { get; set; }
        public long PositionIndex { get; set; }
        public string CardNumber { get; set; }
        public string DepartmentNameSymbol { get; set; }
        public string DeviceName { get; set; }
        public string InOutModeString { get; set; }
        public string Nric { get; set; }
    }

    public class AttendanceLogRequest
    {
        public List<LogInfo> ListAttendanceLog { get; set; }
        public string RoomName { get; set; }
    }

    public class AttendanceLogPram
    {
        public List<LogInfo> ListAttendanceLog { get; set; }
        public string SerialNumber { get; set; }
    }
}
