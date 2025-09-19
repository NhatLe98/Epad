using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.HTTPClient
{
    public class LateInEarlyOutApprovedResult
    {
        public string ID { get; set; }
        public string EmployeeATID { get; set; }
        public string EmployeeCode { get; set; }
        public string FullName { get; set; }
        public string DepartmentName { get; set; }
        public string PositionName { get; set; }
        public string Reason { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        public int LateIn { get; set; }
        public int EarlyOut { get; set; }
        public int LateOut { get; set; }
        public int EarlyIn { get; set; }

        public int TotalLateInEarlyOut { get; set; }
        public bool HasCardCount { get; set; }
        public bool CalculateOT { get; set; }
        public bool AppliedInHoliday { get; set; }
        public bool AppliedInOffDay { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }

    public class LateInEarlyOutApprovedResultReponse
    {
        public string Status { get; set; }
        public string MessageCode { get; set; }
        public string MessageDetail { get; set; }
        public List<LateInEarlyOutApprovedResult> Data { get; set; }
    }
}
