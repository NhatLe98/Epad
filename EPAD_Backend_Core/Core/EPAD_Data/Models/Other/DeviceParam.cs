using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EPAD_Data.Models
{
    public class DeviceParam
    {
        [Required]
        [MinLength(1)]
        public string SerialNumber { get; set; }
        public string AliasName { get; set; }
        public string IPAddress { get; set; }
        public int? Port { get; set; }

        public string FirmwareVersion { get; set; }
        public string Stamp { get; set; }
        public string OpStamp { get; set; }
        public string PhotoStamp { get; set; }

        public int? UserCapacity { get; set; }
        public int? AttendanceLogCapacity { get; set; }
        public int? FingerCapacity { get; set; }
        public int? FaceCapacity { get; set; }

        public int? ErrorDelay { get; set; }
        public int? Delay { get; set; }
        public string TransTimes { get; set; }
        public int? TransInterval { get; set; }

        public string TransFlag { get; set; }
        public int? Realtime { get; set; }
        public short? Encrypt { get; set; }
        public short? TimeZoneclock { get; set; }
        public string Reserve1 { get; set; }
        public string Reserve2 { get; set; }
        public int? Reserve3 { get; set; }
        public string ATTLOGStamp { get; set; }
        public string OPERLOGStamp { get; set; }
        public string ATTPHOTOStamp { get; set; }
        public string SMSStamp { get; set; }
        public string USER_SMSStamp { get; set; }
        public string USERINFOStamp { get; set; }
        public string FINGERTMPStamp { get; set; }
        public int? DeviceNumber { get; set; }
        public int? DeviceType { get; set; }
        public string UseSDK { get; set; }
        public string UsePush { get; set; }
        public string ConnectionCode { get; set; }
        public string DeviceId { get; set; }
        public string DeviceModel { get; set; }
        public int? DeviceStatus { get; set; }
        public string DeviceModule { get; set; }
        public int? GroupDeviceID { get; set; }
        public int? ServiceID { get; set; }
        public string Note { get; set; }
        public string Account { get; set; }
        public int? ParkingType { get; set; }
        public bool? IsUsingOffline { get; set; }
    }

}
