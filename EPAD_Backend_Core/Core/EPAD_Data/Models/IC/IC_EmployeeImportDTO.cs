using EPAD_Data.Entities.HR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EPAD_Backend_Core.Models.DTOs
{
    public class IC_EmployeeImportDTO
    {
        public string EmployeeATID { get; set; }
        public string EmployeeCode { get; set; }
        public string FullName { get; set; }
        public string CardNumber { get; set; }
        public string Password { get; set; }
        public int Gender { get; set; }
        public string DepartmentName { get; set; }
        public string TeamName { get; set; }
        public string NameOnMachine { get; set; }
        public string Position { get; set; }
        public string JoinedDate { get; set; }
        public string StoppedDate { get; set; }
        public string DateOfBirth { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string ErrorMessage { get; set; }
        public string EmployeeTypeName { get; set; }
        public string Address { get; set; }
        public string Note { get; set; }
        public string Nric { get; set; }
        public int IsAllowPhone { get; set; }
        public string ParentName1 { get; set; }
        public string ParentName2 { get; set; }
        public string ParentEmail1 { get; set; }
        public string ParentEmail2 { get; set; }
        public string ParentPhone1 { get; set; }
        public string ParentPhone2 { get; set; }
        public List<HR_UserContactInfo> HR_ContactInfo { get; set; }
        public IC_EmployeeImportDTO()
        {
            HR_ContactInfo = new List<HR_UserContactInfo>();
        }
    }

}
