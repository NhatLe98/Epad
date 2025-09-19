using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace EPAD_Data.Entities
{
    public class IC_MailHistory
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Index { get; set; }
        public int CompanyIndex { get; set; }
        [StringLength(500)]
        [Required]
        public string Title { get; set; }

        [StringLength(4000)]
        public string Content { get; set; }
        [StringLength(500)]
        [Required]
        public string EmailTo { get; set; }
        public string EmailCC { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public short Status { get; set; }
        public int Times { get; set; }
    }
}
