using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EPAD_Data.Entities
{
    public class GC_TimeLog
    {
        [Required]
        [Column(TypeName = "nvarchar(30)")]
        public string EmployeeATID { get; set; }
        [Required]
        public DateTime Time { get; set; }
        [Required]
        public int CompanyIndex { get; set; }

        [Column(TypeName = "nvarchar(20)")]
        public string MachineSerial { get; set; }
        public short? InOutMode { get; set; }
        public short? SpecifiedMode { get; set; }
        [Column(TypeName = "varchar(5)")]
        public string Action { get; set; }
        public DateTime? SystemTime { get; set; }
        [Column(TypeName = "ntext")]
        public string ExtendData { get; set; }
        [Column(TypeName = "varchar(10)")]
        public string ObjectAccessType { get; set; }
        public int CustomerIndex { get; set; }
        [Column(TypeName = "varchar(10)")]
        public string LogType { get; set; }
        [Column(TypeName = "nvarchar(200)")]
        public string PlatesRegistered { get; set; }
        public short Status { get; set; }
        [Column(TypeName = "nvarchar(200)")]
        public string Error { get; set; }
        [Column(TypeName = "nvarchar(200)")]
        public string Note { get; set; }
        public int LineIndex { get; set; }
        public int GateIndex { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Index { get; set; }
        public short ApproveStatus { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string UpdatedUser { get; set; }
        [Column(TypeName = "varchar(100)")]
        public string VerifyMode { get; set; }
        public int LeaveType { get; set; }
        public bool? IsException { get; set; }
        public string ReasonException { get; set; }
        public string CardNumber { get; set; }
    }
}
