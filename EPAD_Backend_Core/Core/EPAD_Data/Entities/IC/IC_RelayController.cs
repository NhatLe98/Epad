using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EPAD_Data.Entities
{
    public class IC_RelayController
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Index { get; set; }

        [StringLength(200)]
        [Required]
        public string Name { get; set; }

        [StringLength(50)]
        public string IpAddress { get; set; }
        public int Port { get; set; }
        [StringLength(500)]
        public string Description { get; set; }
        [StringLength(200)]
        public string RelayType { get; set; }

        public DateTime? CreatedDate { get; set; }

        public DateTime? UpdatedDate { get; set; }

        [StringLength(50)]
        public string UpdatedUser { get; set; }

        [Required]
        public int CompanyIndex { get; set; }
        public int SignalType { get; set; }
    }
}
