using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EPAD_Backend_Core.Models.DTOs
{
    public class IC_ParentImportDTO
    {
        public string EmployeeATID { get; set; }
        public string EmployeeCode { get; set; }
        public string FullName { get; set; }
        public string CardNumber { get; set; }
        public string Password { get; set; }
        public int Gender { get; set; }
        public string NameOnMachine { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string DateOfBirth { get; set; }
        public string StudentCode { get; set; }
        public string ErrorMessage { get; set; }
    }

}
