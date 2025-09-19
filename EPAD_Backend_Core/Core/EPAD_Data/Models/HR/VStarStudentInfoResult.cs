using EPAD_Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models.HR
{
    public class VStarStudentInfoResult
    {
        public string EmployeeATID { get; set; }
        public string EmployeeCode { get; set; }
        public string FullName { get; set; }
        public string CardNumber { get; set; }
        public string ClassIndex { get; set; }
        public int GradeIndex { get; set; }
        public string ClassName { get; set; }
        public string GradeName { get; set; }
    }
}
