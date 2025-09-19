using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace EPAD_Data.Entities
{
    public class IC_PrivilegeMachineRealtime
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Index { get; set; }
        [StringLength(50)]
        public string UserName { get; set; }
        public short PrivilegeGroup { get; set; }
        [StringLength(500)]
        public string GroupDeviceIndex { get; set; }
        [StringLength(50)]
        public string DeviceModule { get; set; }
        public string DeviceSerial { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        [StringLength(50)]
        public string UpdatedUser { get; set; }
        public int CompanyIndex { get; set; }
    }
}
