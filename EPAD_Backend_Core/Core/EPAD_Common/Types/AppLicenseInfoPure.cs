using System;

namespace EPAD_Common.Types
{
    public class AppLicenseInfoPure
    {
        public DateTime ValidTo { get; set; }
        public DateTime ExpiredDate { get; set; }
        public int MaximumEmployee { get; set; }
        public int MaximumPortalUser { get; set; }
        public int MaximumMobileAppUser { get; set; }
        public string ComputerIdentify { get; set; }
    }
}
