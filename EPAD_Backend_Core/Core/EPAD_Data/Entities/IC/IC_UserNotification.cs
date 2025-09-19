using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace EPAD_Data.Entities
{
    public class IC_UserNotification
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Index { get; set; }

        [Column(TypeName = "nvarchar(100)", Order = 0)]
        public string UserName { get; set; }
        [StringLength(100)]
        public string Title { get; set; }
        public string Message { get; set; }
        public short Type { get; set; }
        public short Status { get; set; }

    }
}
