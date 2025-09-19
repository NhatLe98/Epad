using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EPAD_Data.Entities
{
    public class IC_Config
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Index { get; set; }

        [Required]
        public int CompanyIndex { get; set; }

        [Column(TypeName = "ntext")]
        public string TimePos { get; set; }

        [StringLength(100)]
        public string EventType { get; set; }

        public int? PreviousDays { get; set; }

        [StringLength(100)]
        public string ProceedAfterEvent { get; set; }

        [Column(TypeName = "ntext")]
        public string Email { get; set; }

        public bool SendMailWhenError { get; set; }

        public bool AlwaysSend { get; set; }

        [StringLength(200)]
        public string TitleEmailSuccess { get; set; }

        [Column(TypeName = "ntext")]
        public string BodyEmailSuccess { get; set; }

        [StringLength(200)]
        public string TitleEmailError { get; set; }

        [Column(TypeName = "ntext")]
        public string BodyEmailError { get; set; }

        [Column(TypeName = "ntext")]
        public string CustomField { get; set; }
        public double? BodyTemperature { get; set; }

        public DateTime? UpdatedDate { get; set; }

        [StringLength(50)]
        public string UpdatedUser { get; set; }
    }
}