using EPAD_Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models
{
    public class BlackListParams
    {
        public int Index { get; set; }
        public string EmployeeATID { get; set; }
        public string FullName { get; set; }
        public string Nric { get; set; }
        public bool IsEmployeeSystem { get; set; }
        public DateTime FromDate { get; set; }
        public string FromDateString { get; set; }
        public DateTime? ToDate { get; set; }
        public string ToDateString { get; set; }
        public string Reason { get; set; }
        public string ReasonRemove { get; set; }
        public string ErrorMessage { get; set; }
        public long RowIndex { get; set; }
    }
    public class BlackListFilter
    {
        public string EmployeeATID { get; set; }
        public string Nric { get; set; }
        public string Filter { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }

    public class RemoveEmployeeInBlackListParam
    {
        public int Index { get; set; }
        public DateTime ToDate { get; set; }
        public string ReasonRemoveBlackList { get; set; }

    }
}
