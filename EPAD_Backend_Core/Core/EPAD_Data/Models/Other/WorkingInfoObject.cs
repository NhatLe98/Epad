using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models
{
    public class WorkingInfoObject
    {
        public string Index { get; set; }
        public string EmployeeATID { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public long DepartmentIndex { get; set; }
        public bool? IsSync { get; set; }
    }
}
