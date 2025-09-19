using EPAD_Data;
using System.Collections.Generic;

namespace EPAD_Backend_Core.Models
{
    public class DownloadUserMasterRequest
    {
        public List<UserSyncAuthMode> AuthModes { get; set; } = new List<UserSyncAuthMode>();
        public List<string> SerialNumbers { get; set; } = new List<string>();
        public bool IsOverwriteData { get; set; }
        public List<string> EmployeeATIDs { get; set; } = new List<string>();
        public TargetDownloadUser TargetDownloadUser { get; set; }
    }
}
