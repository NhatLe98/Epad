using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace EPAD_Data.Entities
{
    public class IC_Controller
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Index { get; set; }
        [Column(TypeName = "nvarchar(50)")]
        public string IDController { get; set; }

        [Column(TypeName = "nvarchar(50)")]
        public string Name { get; set; }

        [Column(TypeName = "nvarchar(50)")]
        public string IPAddress { get; set; }
        public int Port { get; set; }

        [Required]
        public int CompanyIndex { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public DateTime? CreateDate { get; set; }

        [StringLength(50)]
        public string UpdatedUser { get; set; }
    }
}
