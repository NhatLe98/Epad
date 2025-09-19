using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Backend_Core.Models.DTOs
{
    public class IC_CustomerImportDTO
    {
        public string EmployeeATID { get; set; }
        public string EmployeeCode { get; set; }
        public string FullName { get; set; }
        public string CardNumber { get; set; }
        public string Password { get; set; }
        public int Gender { get; set; }
        public string Department { get; set; }
        public string TeamName { get; set; }
        public string NameOnMachine { get; set; }
        public string Position { get; set; }
        public string JoinedDate { get; set; }
        public string StoppedDate { get; set; }

        public string StartTime { get; set; }
        public string EndTime { get; set; }

        public string DateOfBirth { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string ErrorMessage { get; set; }
        public string EmployeeTypeName { get; set; }
        public string Address { get; set; }
        public string Note { get; set; }
        public string Nric { get; set; }
        public int IsAllowPhone { get; set; }
        public string ContactDepartment { get; set; }
        public string ContactPerson { get; set; }
        public string ContactPersonPhoneNumber { get; set; }
        public string WorkingContent { get; set; }
        public string Company { get; set; }
        public string StudentOfParent { get; set; }
        public long DepartmentIndex { get; set; }
        public long ContactDepartmentIndex { get; set; }
        public long PositionIndex { get; set; }
        public string PositionName { get; set; }
        public string UpdateUser { get; set; }
        public DateTime? Timestamp { get; set; }
    }
}
