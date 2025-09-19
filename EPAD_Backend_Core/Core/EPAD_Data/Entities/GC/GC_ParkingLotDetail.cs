using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace EPAD_Data.Entities
{
    public class GC_ParkingLotDetail
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Index { get; set; }
        [Required]
        public int ParkingLotIndex { get; set; }
        [Required]
        public int GateIndex { get; set; }
        [Required]
        public int LineIndex { get; set; }
        [Required]
        public int CompanyIndex { get; set; }
        public DateTime? UpdatedDate { get; set; }

        [StringLength(50)]
        public string UpdatedUser { get; set; }
    }
}
