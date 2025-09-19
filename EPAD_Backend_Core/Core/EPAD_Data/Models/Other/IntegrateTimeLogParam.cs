using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models
{
    public class IntegrateTimeLogParam
    {
        public List<AttendanceLog> ListLogs { get; set; }
        public bool WriteToDatabase { get; set; }
        public bool WriteToFile { get; set; }
        public string WriteToFilePath { get; set; }
        public string IntegrateTime { get; set; }
        public int FileType { get; set; }
        public bool IsFirst { get; set; }
        public bool IsLast { get; set; }
    }
}
