using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace EPAD_Data.Entities
{
    public class IC_VehicleLog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Index { get; set; }
        public int IntegrateId { get; set; }
        public string EmployeeATID { get; set; }
        public string CardNumber { get; set; }
        public string Plate { get; set; }
        public int VehicleTypeId { get; set; }
        public int CategoryId { get; set; }
        public string Status { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string ComputerIn { get; set; }
        public string ComputerOut { get; set; }
        public string Reason { get; set; }
        public string Note { get; set; }
        public string UpdatedUser { get; set; }
        public string IntegrateString { get; set; }
        public int IsDelete { get; set; }
    }
}
