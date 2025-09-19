using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EPAD_Data.Entities
{
    public class TA_Holiday
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Index { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public DateTime HolidayDate { get; set; }
        public bool IsPaidWhenNotWorking { get; set; }
        public bool IsRepeatAnnually { get; set; }
        public int CompanyIndex { get; set; }
        public string UpdatedUser { get; set; }
        public DateTime UpdatedDate { get; set; }
    }
}
