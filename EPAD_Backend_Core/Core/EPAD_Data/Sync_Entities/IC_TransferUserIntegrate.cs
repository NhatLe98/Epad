using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace EPAD_Data.Entities
{
    [Table("IC_TransferUser")]
    public class IC_TransferUserIntegrate
    {
        public long Index { get; set; }
        public string EmployeeATID { get; set; }
        public string DepartmentTo { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public DateTime? DateCreated { get; set; }
    }
}
