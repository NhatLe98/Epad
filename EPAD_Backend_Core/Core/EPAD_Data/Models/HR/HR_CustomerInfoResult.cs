using EPAD_Data.Entities;
using System;
using System.Collections.Generic;

namespace EPAD_Data.Models
{
    public class HR_CustomerInfoResult : HR_CustomerInfo, IHR_User
    {
        public string Avatar { get; set; }
        public string EmployeeCode { get; set; }
        public string FullName { get; set; }
        public short? Gender { get; set; }
        public int? DayOfBirth { get; set; }
        public int? MonthOfBirth { get; set; }
        public int? YearOfBirth { get; set; }
        public string NameOnMachine { get; set; }
        public int? EmployeeType { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? FromTime { get; set; }
        public DateTime? ToTime { get; set; }
        public string CardNumber { get; set; } 
        public DateTime CardNumberUpdatedDate { get; set; }
        public string Password { get; set; }
        public string IsVIPString { get; set; }
        public long? DepartmentIndex { get; set; }
        public string DepartmentName { get; set; }
        public string AllowPhone { get; set; }
        public string BirthDay { get; set; }
        public string ContactDepartmentName { get; set; }
        public string StartTimeStr { get; set; }
        public string EndTimeStr { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public List<string> StudentOfParent { get; set; }
        public string StudentOfParentString { get; set; }
        public string ContactPersonName { get; set; }

        public string ContactPersonGuest { get; set; }
        public string ContactDepartmentGuest { get; set; }

        public long PositionIndex { get; set; }
        public string PositionName { get; set; }

        public long CardUserIndex { get; set; }

    }

    public class CustomerRequestModel
    { 
        public List<string> employeeATID { get; set; }
        public string filter { get; set; }
        public int page { get; set; }
        public int pageSize { get; set; }
        public int employeeType { get; set; }
        public List<string> studentOfParent { get; set; }
        public List<long> filterDepartments { get; set; }
    }
}
