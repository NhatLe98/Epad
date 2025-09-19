using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EPAD_Data.Entities
{
    public class IC_Employee_Shift : Audit_Model
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        public int CompanyIndex { get; set; }
        public string EmployeeATID { get; set; }
        public int IC_ShiftId { get; set; }
        public DateTime ShippedDate { get; set; }
        public DateTime ShiftFromTime { get; set; }
        public DateTime ShiftToTime { get; set; }
        public DateTime ShiftApplyDate { get; set; }
        public IC_Shift IC_Shift { get; set; }
    }
}
