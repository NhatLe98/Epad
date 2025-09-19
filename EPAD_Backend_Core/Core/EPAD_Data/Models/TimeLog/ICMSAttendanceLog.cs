using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models.TimeLog
{
    public class ICMSAttendanceLog
    {
        public string EmployeeATID { get; set; }
        public string IP { get; set; }
        public int IsInValid { get; set; }
        public int AttState { get; set; }
        public int VerifyMode { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public int Day { get; set; }
        public int Hour { get; set; }
        public int Minute { get; set; }
        public int Second { get; set; }
    }
}
