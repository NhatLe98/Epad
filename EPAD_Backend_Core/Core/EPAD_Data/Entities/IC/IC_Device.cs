using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EPAD_Data.Entities
{
    public class IC_Device
    {
        [Key]
        [Column(TypeName = "varchar(50)", Order = 1)]
        public string SerialNumber { get; set; }

        [Key]
        public int CompanyIndex { get; set; }

        [StringLength(100)]
        public string AliasName { get; set; }

        [Column(TypeName = "varchar(50)")]
        public string IPAddress { get; set; }

        public int? Port { get; set; }

        public DateTime? LastConnection { get; set; }

        public int? UserCount { get; set; }

        public int? FingerCount { get; set; }

        public int? AttendanceLogCount { get; set; }

        public int? OperationLogCount { get; set; }
        public int? AdminCount { get; set; }
        public int? FaceCount { get; set; }


        [StringLength(50)]
        public string FirmwareVersion { get; set; }

        [Column(TypeName = "varchar(15)")]
        public string Stamp { get; set; }

        [Column(TypeName = "varchar(15)")]
        public string OpStamp { get; set; }

        [Column(TypeName = "varchar(15)")]
        public string PhotoStamp { get; set; }

        public int? ErrorDelay { get; set; }

        public int? Delay { get; set; }

        [Column(TypeName = "varchar(60)")]
        public string TransTimes { get; set; }

        public int? TransInterval { get; set; }

        [Column(TypeName = "varchar(300)")]
        public string TransFlag { get; set; }

        public int? Realtime { get; set; }

        public short? Encrypt { get; set; }

        public short? TimeZoneclock { get; set; }

        [StringLength(50)]
        public string Reserve1 { get; set; }

        [StringLength(50)]
        public string Reserve2 { get; set; }

        public int? Reserve3 { get; set; }

        [StringLength(15)]
        public string ATTLOGStamp { get; set; }

        [StringLength(15)]
        public string OPERLOGStamp { get; set; }

        [StringLength(15)]
        public string ATTPHOTOStamp { get; set; }

        [StringLength(15)]
        public string SMSStamp { get; set; }

        [StringLength(15)]
        public string USER_SMSStamp { get; set; }

        [StringLength(15)]
        public string USERINFOStamp { get; set; }

        [StringLength(15)]
        public string FINGERTMPStamp { get; set; }

        public int? DeviceNumber { get; set; }

        public int? DeviceType { get; set; }

        public DateTime? CreatedDate { get; set; }

        public DateTime? UpdatedDate { get; set; }

        [StringLength(50)]
        public string UpdatedUser { get; set; }

        public bool? UseSDK { get; set; }

        public bool? UsePush { get; set; }

        public int? UserCapacity { get; set; }
        public int? AttendanceLogCapacity { get; set; }
        public int? FingerCapacity { get; set; }
        public int? FaceCapacity { get; set; }

        [StringLength(50)]
        public string ConnectionCode { get; set; }
        [StringLength(50)]
        public string DeviceId { get; set; }
        [StringLength(50)]
        public string DeviceModel { get; set; }
        public int? DeviceStatus { get; set; }
        [StringLength(50)]
        public string DeviceModule { get; set; }
        public bool IsSendMailLastDisconnect { get; set; }
        public string Note { get; set; }
        public string Account { get; set; }
        public int ParkingType { get; set; }
        public bool IsUsingOffline { get; set; }
    }
}