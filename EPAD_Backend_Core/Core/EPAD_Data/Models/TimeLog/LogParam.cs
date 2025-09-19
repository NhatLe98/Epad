using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models.TimeLog
{
    public class LogParam
    {
        public long Index { get; set; }
        public int InOut { get; set; }
        public bool OpenController { get; set; }
        public int LineIndex { get; set; }
        public string Note { get; set; }
        public string UserName { get; set; }
        public bool IsException { get; set; }
        public string ExceptionReason { get; set; }
        public string EmployeeATID { get; set; }
        public string CardNumber { get; set; }
    }
}
