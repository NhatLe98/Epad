using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models.HR
{
    public class HR_EmployeeInfoRequest
    {
        public string Filter { get; set; }
        public List<long> DepartmentIDs { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int? UserType { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public List<int> ListWorkingStatus { get; set; }
    }
}
