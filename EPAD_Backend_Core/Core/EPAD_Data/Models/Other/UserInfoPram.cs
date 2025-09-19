using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models
{
    public class UserInfoPram
    {
        public List<UserInfoOnMachine> ListUserInfo { get; set; }
        public string SerialNumber { get; set; }
        public bool IsOverwriteData { get; set; }
        public int SystemCommandIndex { get; set; }

    }
}
