using System.Collections.Generic;

namespace EPAD_Data.Models
{
    public class UserInfoParamV2
    {
        public List<UserInfoOnMachineV2> ListUserInfo { get; set; }
        public string SerialNumber { get; set; }
        public bool IsOverwriteData { get; set; }
    }
}
