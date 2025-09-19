using System.Collections.Generic;

namespace EPAD_Data.Models
{
    public class UserInfoOnMachine
    {
        public string UserID { get; set; }
        public string NameOnDevice { get; set; }
        public string PasswordOndevice { get; set; }
        public string CardNumber { get; set; }

        public int Privilege { get; set; }
        public bool Enable { get; set; }
        public List<FingerInfo> FingerPrints { get; set; }
        public FaceInfo Face { get; set; }
        public FaceInfoV2 FaceInfoV2 { get; set; }
        public string GroupID { get; set; }
        public string TimeZone { get; set; }
        public string EmployeeATID { get; set; }
        public UserInfoOnMachine(string pUserID)
        {
            long userId = 0;
            long.TryParse(pUserID, out userId);
            UserID = userId > 0 ? userId.ToString() : pUserID;
            NameOnDevice = "user";
            CardNumber = "0";
            //Privilege = 1;

            FingerPrints = new List<FingerInfo>();
            Enable = true;
        }
        public UserInfoOnMachine()
        {
            Enable = true;
        }
    }
}
