using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models.IC
{
    public class IC_EmployeeParamDTO
    {
        public string EmployeeATID { get; set; }
        public string EmployeeCode { get; set; }
        public string FullName { get; set; }
        public string CardNumber { get; set; }
        public short? Gender { get; set; }
        public string NameOnMachine { get; set; }
        public int DepartmentIndex { get; set; }
        public string DepartmentCode { get; set; }
        public string PositionName { get; set; }
        public DateTime JoinedDate { get; set; }
        public string ImageUpload { get; set; }
        public string Password { get; set; }
        public string Biometrics { get; set; }
        public List<string> ListFinger { get; set; }
    }
}
