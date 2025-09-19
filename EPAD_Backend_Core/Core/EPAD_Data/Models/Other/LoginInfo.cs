using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models.Other
{
    public class LoginInfo
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string ServiceId { get; set; }

    }

    public class LoginInfoAVN
    {
        public string username { get; set; }
        public string password { get; set; }
        public string grant_type { get; set; }
    }

    public class LoginInfoEMS
    {
        public string username { get; set; }
        public string password { get; set; }
    }

    public class UserInfoEMS
    {
        public string id { get; set; }
        public string employeeName { get; set; }
        public string username { get; set; }
        public string token { get; set; }
        public bool requiredChangePassword { get; set; }
    }
}
