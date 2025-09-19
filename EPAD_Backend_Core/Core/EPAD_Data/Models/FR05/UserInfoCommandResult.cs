using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models.FR05
{
    public class UserInfoCommandResult
    {
        public UserInfoCommandResult()
        {
            UserInfos = new List<UserInfoOnMachine>();
            UserIdsFailed = new List<string>();
            UserIdsSuccess = new List<string>();
        }
        public List<UserInfoOnMachine> UserInfos { get; set; }
        public List<string> UserIdsSuccess { get; set; }
        public List<string> UserIdsFailed { get; set; }
        public string Error { get; set; }
    }
}
