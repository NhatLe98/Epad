using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace EPAD_Data.Entities
{
    public class IC_PrivilegeDeviceDetails
    {
        public int PrivilegeIndex { get; set; }

        [Column(TypeName = "varchar(50)", Order = 1)]
        public string SerialNumber { get; set; }

        [StringLength(20)]
        public string Role { get; set; }

        public int CompanyIndex { get; set; }

        public DateTime? UpdatedDate { get; set; }

        [StringLength(50)]
        public string UpdatedUser { get; set; }

    }
}
