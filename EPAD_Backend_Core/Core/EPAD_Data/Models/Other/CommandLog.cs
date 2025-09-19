using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models
{
    public class CommandLog
    {
        public int ServiceIndex { get; set; }
        public string ServiceName { get; set; }
        public int CompanyIndex { get; set; }
        public CommandResult Command { get; set; }

        public CommandLog(UserInfo pUser, CommandResult pCommand)
        {
            ServiceIndex = pUser.Index;
            ServiceName = pUser.ServiceName;
            CompanyIndex = pUser.CompanyIndex;
        }
    }
}
