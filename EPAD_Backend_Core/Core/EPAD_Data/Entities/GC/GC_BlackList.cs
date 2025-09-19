using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace EPAD_Data.Entities
{
    public class GC_BlackList
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Index { get; set; }
        public string EmployeeATID { get; set; }
        public string FullName { get; set; }
        public string Nric { get; set; }
        public bool IsEmployeeSystem{ get; set; }
        public DateTime FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string Reason { get; set; }
        public string ReasonRemove { get; set; }
        public int CompanyIndex { get; set; }
        public DateTime? UpdatedDate { get; set; }
        [StringLength(100)]
        public string UpdatedUser { get; set; }
    }
}
