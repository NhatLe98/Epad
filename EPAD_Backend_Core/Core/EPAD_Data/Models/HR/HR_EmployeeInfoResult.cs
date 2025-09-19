using EPAD_Data.Entities;
using EPAD_Data.Entities.HR;
using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models
{
    public class HR_EmployeeInfoResult : HR_EmployeeInfo, IHR_User
    {
        public string Avatar { get; set; }
        public string EmployeeCode { get; set; }
        public string FullName { get; set; }
        public short? Gender { get; set; }
        public string BirthDay { get; set; }
        public int? DayOfBirth { get; set; }
        public int? MonthOfBirth { get; set; }
        public int? YearOfBirth { get; set; }
        public string NameOnMachine { get; set; }
        public int? EmployeeType { get; set; }
        public int? EmployeeTypeIndex { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string CardNumber { get; set; }

        public long DepartmentIndex { get; set; }
        public string DepartmentName { get; set; }
        public string DepartmentCode { get; set; }
        //Using AEON
        public bool IsDepartmentChildren { get; set; }
        public string DepartmentNameSymbol { get; set; }

        public long OldDepartmentIndex { get; set; }
        public string OldDepartmentName { get; set; }
        public string ApproveStatus { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public bool? IsSync { get; set; }
        public string Password { get; set; }
        public long TitleIndex { get; set; }
        public long PositionIndex { get; set; }
        public string PositionName { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int WorkingInfoIndex { get; set; }
        public string Address { get; set; }
        public string Description { get; set; }
        public string Note { get; set; }
        public short TotalFingerTemplate { get; set; }
        public bool IsEmployee { get; set; }
        public string UserType { get; set; }
        public string EmployeeTypeName { get; set; }
        public string WorkingStatus { get; set; }
        public bool IsAllowPhone { get; set; }
        public string Nric { get; set; }
        public List<string> ListFinger { get; set; }
        public List<HR_UserContactInfo> HR_ContactInfo { get; set; }
        public HR_EmployeeInfoResult()
        {
            HR_ContactInfo = new List<HR_UserContactInfo>();
        }
    }
}
