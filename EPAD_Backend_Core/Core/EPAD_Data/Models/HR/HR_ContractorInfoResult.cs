using EPAD_Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models
{
    public class HR_ContractorInfoResult : HR_ContractorInfo, IHR_User
    {
        public string Avatar { get; set; }
        public string EmployeeCode { get; set; }
        public string FullName { get; set; }
        public short? Gender { get; set; }
        public int? DayOfBirth { get; set; }
        public int? MonthOfBirth { get; set; }
        public int? YearOfBirth { get; set; }
        public string NameOnMachine { get; set; }
        public int? EmployeeType { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string CardNumber { get; set; }
    }
}
