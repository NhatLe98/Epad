using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.HTTPClient
{
    public class LeaveDayBasicInfo
    {
        public long Index { get; set; }
        public long? EventIndex { get; set; }
        public string EmployeeATID { get; set; }
        public string EmployeeCode { get; set; }
        public string FullName { get; set; }
        public string DepartmentName { get; set; }
        public DateTime LeaveDate { get; set; }
        public DateTime FromTime { get; set; }
        public DateTime ToTime { get; set; }
        public int LeaveDayTypeIndex { get; set; }
        public string LeaveDayType { get; set; }
        public string LeaveDayTypeInEng { get; set; }
        public string Note { get; set; }
        public string Type { get; set; }
        public float LeaveHours { get; set; }

    }

    public class LeaveDayBasicInfoReponse
    {
        public string Status { get; set; }
        public string MessageCode { get; set; }
        public string MessageDetail { get; set; }
        public List<LeaveDayBasicInfo> Data { get; set; }
    }
}
