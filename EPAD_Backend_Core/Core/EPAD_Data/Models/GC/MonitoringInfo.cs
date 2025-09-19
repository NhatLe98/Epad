using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models
{
    public class MonitoringInfo
    {
        public long Index { get; set; }
        public string EmployeeATID { get; set; }
        public string Avatar { get; set; }
        public string CheckTime { get; set; }
        public bool Success { get; set; }
        public int InOut { get; set; }
        public string Error { get; set; }
        public int CompanyIndex { get; set; }
        public int LineIndex { get; set; }
        public int CustomerIndex { get; set; }

        public void SetBasicInfo(string pEmpATID, short pInOut, bool success,
            string error, DateTime pNow, int pCompanyIndex, long pIndex, int pLineIndex, string pAvatar)
        {
            EmployeeATID = pEmpATID;
            CheckTime = pNow.ToString("dd/MM/yyyy HH:mm:ss");
            Success = success;
            Error = error;
            InOut = pInOut;
            CompanyIndex = pCompanyIndex;
            Index = pIndex;
            LineIndex = pLineIndex;
            Avatar = pAvatar;
        }
    }
}
