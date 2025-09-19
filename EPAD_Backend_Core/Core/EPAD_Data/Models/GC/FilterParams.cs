using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models
{
    public class FilterParams
    {
        public string FromTime { get; set; }
        public string ToTime { get; set; }
        public int[] Lines { get; set; }
        public string LogType { get; set; }
        public int Status { get; set; }
        public string SearchKey { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public List<string> EmployeeIndexes { get; set; }
        public List<long> DepartmentIndexes { get; set; }
        public List<int> RulesWarningIndexes { get; set; }
        public string StatusLog { get; set; }
        public string Filter { get; set; }
    }
}
