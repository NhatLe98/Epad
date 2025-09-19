using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models
{
    public class CheckRuleResult
    {
        private int Status { get; set; }
        private string Error { get; set; }
        public string MoreInfo { get; set; }
        private bool Success { get; set; }
        public int LeaveType { get; set; }
        public CheckRuleResult()
        {
            Status = 0;
            Success = true;
        }
        public void SetSuccess(bool pSuccess)
        {
            Success = pSuccess;
        }
        public void SetStatus(EMonitoringError error)
        {
            Status = (int)error;
            Error = GCSEnum.GetErrorByStatus(error);
            if (Status == (int)EMonitoringError.NoError)
            {
                Success = true;
            }
            else
            {
                Success = false;
            }
        }

        public bool GetSuccess()
        {
            return Success;
        }
        public int GetStatus()
        {
            return Status;
        }
        public string GetError()
        {
            return Error;
        }

    }
}
