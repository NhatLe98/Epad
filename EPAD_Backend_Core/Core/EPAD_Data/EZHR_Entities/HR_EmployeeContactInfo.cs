using System;
using System.Collections.Generic;

namespace EPAD_Data.Entities
{
    public partial class HR_EmployeeContactInfo
    {
        public int CompanyIndex { get; set; }
        public string EmployeeATID { get; set; }
        public string PermamentAddress { get; set; }
        public string TemporaryAddress { get; set; }
        public string HomePhone { get; set; }
        public string MobilePhone { get; set; }
        public string Email { get; set; }
        public string YahooMessenger { get; set; }
        public string Skype { get; set; }
        public string Website { get; set; }
        public string Fax { get; set; }
        public string Description { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string UpdatedUser { get; set; }
        public string EmergencyContactPerson { get; set; }
        public string EmergencyContactPersonMobile { get; set; }
        public int? Address_WardIndex { get; set; }
        public int? Address_DistrictIndex { get; set; }
        public int? Address_CityIndex { get; set; }
        public string Address_Ward { get; set; }
        public string Address_District { get; set; }
        public string Address_City { get; set; }
        public string Address_No { get; set; }
        public string Address_Street { get; set; }
        public string Presenter { get; set; }
        public string EmergencyContactPerson2 { get; set; }
        public string EmergencyContactPersonMobile2 { get; set; }
        public string PersonalEmail { get; set; }
        public string AttachmentFile { get; set; }
        public int? PermamentAddress_CityIndex { get; set; }
        public int? PermamentAddress_DistrictIndex { get; set; }
        public int? PermamentAddress_WardIndex { get; set; }
        public string PermamentAddress_Address { get; set; }
        public int? TemporaryAddress_CityIndex { get; set; }
        public int? TemporaryAddress_DistrictIndex { get; set; }
        public int? TemporaryAddress_WardIndex { get; set; }
        public string TemporaryAddress_Address { get; set; }
    }
}
