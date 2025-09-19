using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
           
namespace EPAD_Data.Entities
{
    public class GC_ParkingLotAccessed
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Index { get; set; }
        [Required]
        public int ParkingLotIndex { get; set; }
        [Required]
        [StringLength(30)]
        public string EmployeeATID { get; set; }
        [Required]
        public string CustomerIndex { get; set; }
        [Required]
        public short AccessType { get; set; }
        [Required]
        public DateTime FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        [StringLength(500)]
        public string Description { get; set; }

        [Required]
        public int CompanyIndex { get; set; }
        public DateTime? UpdatedDate { get; set; }

        [StringLength(100)]
        public string UpdatedUser { get; set; }
    }
}
