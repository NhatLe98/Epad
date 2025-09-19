using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models
{
    public class CommandCustomerRequest : IC_CommandRequestDTO
    {

        public int? DataStorageTime { get; set; }
        public string NRIC { get; set; }
        public string Company { get; set; }
        public string Address { get; set; }
        public bool? IsVIP { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string ContactPersonATIDs { get; set; }
        public DateTime? ExtensionTime { get; set; }
        public string WorkContent { get; set; }
        public short BikeType { get; set; }
        public string BikeModel { get; set; }
        public string BikePlate { get; set; }
        public string BikeDescription { get; set; }
        public string NRICFrontImage { get; set; }
        public string NRICBackImage { get; set; }
        public string LicensePlateFrontImage { get; set; }
        public string LicensePlateBackImage { get; set; }
        public int? RulesCustomerIndex { get; set; }
        public List<AccompanyingPersonParam> AccompanyingPersonList { get; set; }
        public byte[] IdentityImage { get; set; }
        public short NumberOfContactPerson { get; set; }
        public string EmployeeATID { get; set; }

        public int CompanyIndex { get; set; }
        public string UserName { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string UpdatedUser { get; set; }

        public string CustomerFaceImage { get; set; }
        public string RegisterCode { get; set; }
        public string CustomerID { get; set; }
        public string CustomerName { get; set; }
        public string CardNumber { get; set; }
        public bool Gender { get; set; }
        public DateTime RegisterTime { get; set; }
    }
    public class AccompanyingPersonParam
    {
        public string Index { get; set; }
        public string CardNumber { get; set; }
        public string Name { get; set; }
    }
}
