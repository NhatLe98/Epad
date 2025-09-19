using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace EPAD_Data.Entities
{
    public partial class HR_Employee
    {
        public int CompanyIndex { get; set; }
        public string EmployeeATID { get; set; }
        public byte[] Image { get; set; }
        public string EmployeeCode { get; set; }
        public string CardNumber { get; set; }
        public string LastName { get; set; }
        public string MidName { get; set; }
        public string FirstName { get; set; }
        public string NickName { get; set; }
        public bool? Gender { get; set; }
        public DateTime? JoinedDate { get; set; }
        public short? DayOfBirth { get; set; }
        public short? MonthOfBirth { get; set; }
        public short? YearOfBirth { get; set; }
        public string PlaceOfBirth { get; set; }
        public int? Ethnic { get; set; }
        public int? Religion { get; set; }
        public int? Nationality { get; set; }
        public int? HouseHold { get; set; }
        public string NativeCity { get; set; }
        public string Nric { get; set; }
        public DateTime? DateOfNric { get; set; }
        public string PlaceOfNric { get; set; }
        public int? EmployeeKind { get; set; }
        public int? EducationIndex { get; set; }
        public string EducationLevel { get; set; }
        public short? MaritalSatus { get; set; }
        public string TaxNumber { get; set; }
        public bool? Active { get; set; }
        public bool? MarkForDelete { get; set; }
        public DateTime? MaritalDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string UpdatedUser { get; set; }
        public DateTime? WorkingDate { get; set; }
        public DateTime? UnionJoinedDate { get; set; }
        public int? NativeCityIndex { get; set; }
        public string NativeCityDistrict { get; set; }
        public string SocialInsNo { get; set; }
        public string NativeCityWards { get; set; }
        public int? NativeCityDistrictIndex { get; set; }
        public int? NativeCityWardIndex { get; set; }
        public string NativeCityCity { get; set; }
        public string NativeCityNo { get; set; }
        public string NativeCityStreet { get; set; }

        [NotMapped]
        public string Avatar { get; set; }
    }
}
