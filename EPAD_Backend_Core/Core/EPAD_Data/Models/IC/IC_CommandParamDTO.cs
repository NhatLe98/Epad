using System;
using System.Collections.Generic;

namespace EPAD_Data.Models
{
    public class IC_CommandParamDTO
    {
        public List<UserInfoOnMachine> ListEmployee { get; set; }
        public List<string> ListSerialNumber { get; set; }
        public string ExternalData { get; set; }
        public CommandAction Action { get; set; }
        public string CommandName { get; set; }
        public DateTime FromTime { get; set; }
        public DateTime ToTime { get; set; }
        public bool IsOverwriteData { get; set; }
        public int Privilege { get; set; }
        public string AuthenMode { get; set; }

        public IC_CommandParamDTO()
        {
            ListEmployee = new List<UserInfoOnMachine>();
            ListSerialNumber = new List<string>();
        }
    }

    public class IC_GroupCommandParamDTO
    {
        public List<CommandResult> ListCommand { get; set; }
        public string GroupName { get; set; }
        public string ExternalData { get; set; }
        public string  EventType { get; set; }
        public int CompanyIndex { get; set; }
        public string UserName { get; set; }
    }

    public class IC_UserinfoOnMachineParam {
        public List<string> ListEmployeeaATID { get; set; }
        public string AuthenMode { get; set; }
        public List<string> ListSerialNumber { get; set; }
        public int CompanyIndex { get; set; }
        public bool FullInfo { get; set; }
        public IC_UserinfoOnMachineParam() {
            ListEmployeeaATID = new List<string>();
            ListSerialNumber = new List<string>();
        }
    }
}
