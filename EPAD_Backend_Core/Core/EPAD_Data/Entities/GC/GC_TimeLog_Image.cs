using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace EPAD_Data.Entities
{
    public class GC_TimeLog_Image
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Index { get; set; }

        public long TimeLogIndex { get; set; }
        [Column(TypeName = "text")]
        public string Image1 { get; set; }
        [Column(TypeName = "text")]
        public string Image2 { get; set; }
        [Column(TypeName = "text")]
        public string Image3 { get; set; }
        [Column(TypeName = "text")]
        public string Image4 { get; set; }
        [Column(TypeName = "text")]
        public string Image5 { get; set; }

        [Column(TypeName = "nvarchar(30)")]
        [StringLength(100)]
        public string Info1 { get; set; }
        [Column(TypeName = "nvarchar(30)")]
        [StringLength(100)]
        public string Info2 { get; set; }
        [Column(TypeName = "nvarchar(30)")]
        [StringLength(100)]
        public string Info3 { get; set; }
        [Column(TypeName = "nvarchar(30)")]
        [StringLength(100)]
        public string Info4 { get; set; }
        [Column(TypeName = "nvarchar(30)")]
        [StringLength(100)]
        public string Info5 { get; set; }

        public int CompanyIndex { get; set; }

        public DateTime? UpdatedDate { get; set; }

        [StringLength(50)]
        public string UpdatedUser { get; set; }
    }
}
