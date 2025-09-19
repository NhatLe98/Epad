using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models
{
    public class TA_AjustAttendanceLogParam
    {
        public string Filter { get; set; }
        public int Page { get; set; }
        public int Limit { get; set; }
        public List<long> Departments { get; set; }
        public List<string> EmployeeATIDs { get; set; }
        public string FromDate { get; set; }
        public string ToDate { get; set; }

        public int FilterByType { get; set; }
    }
}
