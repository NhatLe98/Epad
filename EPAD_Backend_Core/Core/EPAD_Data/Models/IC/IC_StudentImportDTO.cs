using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EPAD_Backend_Core.Models.DTOs
{
    public class IC_StudentImportDTO
    {
        public string EmployeeATID { get; set; }
        public string EmployeeCode { get; set; }
        public string FullName { get; set; }
        public string CardNumber { get; set; }
        public string Password { get; set; }
        public int Gender { get; set; }
        public string ClassName { get; set; }
        public string GradeName { get; set; }
        public string NameOnMachine { get; set; }
        public string ErrorMessage { get; set; }
    }

}
