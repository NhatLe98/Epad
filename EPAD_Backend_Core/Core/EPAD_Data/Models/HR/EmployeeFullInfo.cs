using System;

namespace EPAD_Data.Models
{
    public class EmployeeFullInfo
    {
        public byte[] Avatar { get; set; }
        public string EmployeeATID { get; set; }
        public string EmployeeCode { get; set; }
        public string CardNumber { get; set; }
        public string FullName { get; set; }
        public string FirstName { get; set; }
        public string MidName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public bool? Gender { get; set; }
        public DateTime? JoinedDate { get; set; }
        public string Department { get; set; }
        public string DepartmentCode { get; set; }
        public bool RootDepartment { get; set; }
        public long? DepartmentIndex { get; set; }
        public string Position { get; set; }
        public long? PositionIndex { get; set; }
        public string Title { get; set; }
        public long? TitleIndex { get; set; }
        public string EmployeeKind { get; set; }
        public int? EmployeeKindIndex { get; set; }

        public bool? IsManager { get; set; }
        public long? ManagedDepartment { get; set; }
        public string ManagedOtherDepartment { get; set; }
        public string DirectManager { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public short? DayOfBirth { get; set; }
        public short? MonthOfBirth { get; set; }
        public short? YearOfBirth { get; set; }
        public string SocialInsNo { get; set; }
        public string TaxNumber { get; set; }
        public string NRIC { get; set; }
        public int CompanyIndex { get; set; }
        public EmployeeType? EmployeeType { get; set; }
        public bool IsExtraDriver { get; set; }
        public string UserName { get; set; }
        public string NameFilter { get; set; }
        public bool IsParentDepartment { get; set; }

    }
}
