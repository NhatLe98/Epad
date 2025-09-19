using System;
using System.Collections.Generic;

namespace EPAD_Data.Models
{
    public class UserBasicInfoReponse
    {
        public string EmployeeATID { get; set; }
        public string EmployeeCode { get; set; }
        public string FullName { get; set; }
        public string EmployeeType { get; set; }
        public string PositionIndex { get; set; }
        public string DepartmentNameLV2 { get; set; }
        public string CardNumber { get; set; }
        public bool? IsActive { get; set; }
        public string Email { get; set; }
        public DateTime? ToDate { get; set; }
    }

    public class UserDetailInfoReponse
    {
        public string EmployeeATID { get; set; }
        public string EmployeeCode { get; set; }
        public string FullName { get; set; }
        public string EmployeeType { get; set; }
        public string PositionIndex { get; set; }
        public string DepartmentNameLV2 { get; set; }
        public string CardNumber { get; set; }
        public bool? IsActive { get; set; }
        public string Email { get; set; }
        public DateTime? ToDate { get; set; }
        public int CompanyIndex { get; set; }
        public string Gender { get; set; }
        public string NameOnMachine { get; set; }
        public string NickName { get; set; }
        public int DepartmentIndex { get; set; }
        public string DepartmentName { get; set; }
        public string PositionName { get; set; }
        public string FormTeacher { get; set; }
        public string Class { get; set; }
        public int GradeName { get; set; }
        public string Nationality { get; set; }

    }
    public class UserContactInfoReponse
    {
        public string EmployeeATID { get; set; }
        public string EmployeeCode { get; set; }
        public string FullName { get; set; }
        public string EmployeeType { get; set; }
        public string PositionIndex { get; set; }
        public string DepartmentNameLV2 { get; set; }
        public string CardNumber { get; set; }
        public bool? IsActive { get; set; }
        public string Email { get; set; }
        public DateTime? ToDate { get; set; }
        public int CompanyIndex { get; set; }
        public string Gender { get; set; }
        public string NameOnMachine { get; set; }
        public string NickName { get; set; }
        public int DepartmentIndex { get; set; }
        public string DepartmentName { get; set; }
        public string PositionName { get; set; }
        public string FormTeacher { get; set; }
        public string Class { get; set; }
        public int GradeName { get; set; }
        public string Nationality { get; set; }
        public string Avatar { get; set; }
        public string Address { get; set; }
        public DateTime? FromDate { get; set; }
    }

    public class UserPersonalInfoReponse
    {
        public string EmployeeATID { get; set; }
        public string EmployeeCode { get; set; }
        public string FullName { get; set; }
        public string EmployeeType { get; set; }
        public string PositionIndex { get; set; }
        public string DepartmentNameLV2 { get; set; }
        public string CardNumber { get; set; }
        public bool? IsActive { get; set; }
        public string Email { get; set; }
        public DateTime? ToDate { get; set; }
        public int CompanyIndex { get; set; }
        public string Gender { get; set; }
        public string NameOnMachine { get; set; }
        public string NickName { get; set; }
        public int DepartmentIndex { get; set; }
        public string DepartmentName { get; set; }
        public string PositionName { get; set; }
        public string FormTeacher { get; set; }
        public string Class { get; set; }
        public int GradeName { get; set; }
        public string Nationality { get; set; }
        public string Avatar { get; set; }
        public string Address { get; set; }
        public DateTime? FromDate { get; set; }
        public int DayOfBirth { get; set; }
        public int MonthOfBirth { get; set; }
        public int YearOfBirth { get; set; }
        public string Password { get; set; }

    }
    public class EmployeesRequest
    { 
        public List<string> employees { get; set; }
    }

}
