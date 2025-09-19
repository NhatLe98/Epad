using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EPAD_Data.Entities
{
    public class IC_Camera
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Index { get; set; }

        [StringLength(200)]
        [Required]
        public string Name { get; set; }

        [Column(TypeName = "varchar(50)")]
        public string Serial { get; set; }

        [StringLength(50)]
        public string IpAddress { get; set; }
        public int Port { get; set; }

        [Column(TypeName = "varchar(50)")]
        public string UserName { get; set; }

        [Column(TypeName = "varchar(500)")]
        public string Password { get; set; }
        [StringLength(2000)]
        public string Description { get; set; }
        [Column(TypeName = "varchar(10)")]
        public string Type { get; set; }

        public DateTime? CreatedDate { get; set; }

        public DateTime? UpdatedDate { get; set; }

        [StringLength(50)]
        public string UpdatedUser { get; set; }

        [Required]
        public int CompanyIndex { get; set; }
    }
}
