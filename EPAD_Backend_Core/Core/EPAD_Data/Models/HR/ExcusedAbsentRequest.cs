using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models.HR
{
    public class ExcusedAbsentRequest
    {
        public int page { get; set; }
        public int limit { get; set; }
        public string filter { get; set; }
        public string from { get; set; }
        public string to { get; set; }
        public List<int> departments { get; set; }
    }

    public class ExcusedAbsentParam
    {
        public int Index { get; set; }
        public List<string> EmployeeATIDs { get; set; } 
        public DateTime AbsentDate { get; set; }
        public string AbsentDateString { get; set; }
        public int ExcusedAbsentReasonIndex { get; set; }
        public string Description { get; set; }
    }

    public class ExcusedAbsentImportParam
    {
        public string EmployeeATID { get; set; }
        public string FullName { get; set; }
        public string Class { get; set; }
        public string AbsentDate { get; set; }
        public string ExcusedAbsentReason { get; set; }
        public string Description { get; set; }
        public string ErrorMessage { get; set; }

    }
}
