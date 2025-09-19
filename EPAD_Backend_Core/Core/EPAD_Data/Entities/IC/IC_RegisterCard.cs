using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace EPAD_Data.Entities
{
    public class IC_RegisterCard
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Index { get; set; }
        public DateTime CreatedDate { get; set; }
        public string EmployeeName { get; set; }
        public int VehicleTypeId { get; set; }
        public int Price { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Account { get; set; }
        public string Password { get; set; }
        public string EmployeeATID { get; set; }
        public string Plate { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string Branch { get; set; }
        public string Note { get; set; }
        public int CategoryId { get; set; }
        public string CardNumber { get; set; }

    }
}
