using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EPAD_Data.Entities
{
    public class IC_AttendancelogIntegrate
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Index { get; set; }

        [Column(TypeName = "varchar(100)", Order = 0)]
        public string EmployeeATID { get; set; }

        [Column(TypeName = "varchar(50)", Order = 1)]
        public string SerialNumber { get; set; }

        public DateTime CheckTime { get; set; }

        public int CompanyIndex { get; set; }

        public short? VerifyMode { get; set; }

        public short InOutMode { get; set; }

        public int? WorkCode { get; set; }

        public int? Reserve1 { get; set; }
        public int? FaceMask { get; set; }
        public double? BodyTemperature { get; set; }

        public DateTime? UpdatedDate { get; set; }

        [StringLength(50)]
        public string UpdatedUser { get; set; }

        [StringLength(50)]
        public string Function { get; set; }
        public bool? IsSend { get; set; }
        public DateTime? AccessDate { get; set; }
        public DateTime? AccessTime { get; set; }
        public int? DeviceNumber { get; set; }
        public string DeviceId { get; set; }
        public string RoomCode { get; set; }
    }
}