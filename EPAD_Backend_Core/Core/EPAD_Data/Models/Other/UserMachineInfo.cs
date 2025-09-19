using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models.Other
{
    public class UserMachineInfo
    {
        public int Index { get; set; }

        public string EmployeeATID { get; set; }
        public string EmployeeCode { get; set; }
        public string FullName { get; set; }
        public string AliasName { get; set; }

        public string DepartmentName { get; set; }

        public string SerialNumber { get; set; }
        public string NameOnMachine { get; set; }
        public string CardNumber { get; set; }
        public int Privilege { get; set; }
        public string PrivilegeName { get; set; }
        public string Password { get; set; }

        public int Finger1 { get; set; }
        public int Finger2 { get; set; }
        public int Finger3 { get; set; }
        public int Finger4 { get; set; }

        public int Finger5 { get; set; }
        public int Finger6 { get; set; }
        public int Finger7 { get; set; }
        public int Finger8 { get; set; }

        public int Finger9 { get; set; }
        public int Finger10 { get; set; }
        public int FaceTemplate { get; set; }

        public UserMachineInfo()
        {
            Finger1 = Finger2 = Finger3 = Finger4 = Finger5 = Finger6 = Finger7 = Finger8 = Finger9 = Finger10 = 0;
        }
    }
}
