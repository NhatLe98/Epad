using EPAD_Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;


namespace EPAD_Data.Models
{
    public class TA_LeaveRegistrationDTO : TA_LeaveRegistration
    {
        public string FullName { get; set; }
        public string EmployeeCode { get; set; }
        public long DepartmentIndex { get; set; }
        public string DepartmentName { get; set; }
        public string LeaveDateString { get; set; }
    }

    public class LeaveRegistrationModel : TA_LeaveRegistrationDTO
    { 
        public int Page { get; set; }
        public int PageSize { get; set; }
        public string Filter { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string FromDateString { get; set; }
        public string ToDateString { get; set; }
        public List<long> ListDepartmentIndex { get; set; }
        public List<string> ListEmployeeATID { get; set; }
        public string LeaveDateTypeName { get; set; }
        public string LeaveDurationTypeName { get; set; }
        public float TotalWork { get; set; }
        public string HaftLeaveTypeName { get; set; }
        public bool FirstHaftLeave { get; set; }
        public bool LastHaftLeave { get; set; }
        public string ErrorMessage { get; set; }
    }
}
