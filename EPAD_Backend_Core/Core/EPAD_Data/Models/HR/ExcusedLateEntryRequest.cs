using EPAD_Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models.HR
{
    public class ExcusedLateEntryRequest
    {
        public int page { get; set; }
        public int limit { get; set; }
        public string filter { get; set; }
        public string from { get; set; }
        public string to { get; set; }
        public List<int> departments { get; set; }
    }

    public class ExcusedLateEntryParam : HR_ExcusedLateEntry
    {
        public List<string> EmployeeATIDs { get; set; } 
        public string LateDateString { get; set; }
    }

    public class ExcusedLateEntryImportParam
    {
        public string EmployeeATID { get; set; }
        public string FullName { get; set; }
        public string Class { get; set; }
        public string LateDate { get; set; }
        public string Description { get; set; }
        public string ErrorMessage { get; set; }

    }
}
