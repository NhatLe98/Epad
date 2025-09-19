using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models.FR05
{
    public class LogCommandResult
    {
        public LogCommandResult()
        {
            LogInfos = new List<LogInfo>();
            userIdsSuccess = new List<string>();
            userIdsFailure = new List<string>();
        }
        public List<LogInfo> LogInfos { get; set; }
        public string Error { get; set; }
        public List<string> userIdsSuccess { get; set; }
        public List<string> userIdsFailure { get; set; }
    }
}
