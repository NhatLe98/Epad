using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models
{
    public class WalkerHistoryInfo
    {
        public string EmployeeATID { get; set; }
        public DateTime CheckTime { get; set; }
        public bool Success { get; set; }
        public short InOut { get; set; }
        public string Error { get; set; }
        public int LineIndex { get; set; }
        public string CardNumber { get; set; }
        public string ObjectType { get; set; }
        public string FullName { get; set; }
        public string Note { get; set; }

        public string Department { get; set; }
        public string Position { get; set; }
        public string CompanyName { get; set; }
        public string WorkContent { get; set; }
        public string ContactPerson { get; set; }
        public string StudentCode { get; set; }
        public string Class { get; set; }
        public string ClassTeacher { get; set; }
        public string RegisterImage { get; set; }
        public string VerifyImage { get; set; }

        public short ApproveStatus { get; set; }
        public long LogIndex { get; set; }
    }
}
