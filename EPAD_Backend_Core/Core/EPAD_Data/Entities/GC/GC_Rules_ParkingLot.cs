using System;
using System.Collections.Generic;
using System.Text;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EPAD_Data.Entities
{
    public class GC_Rules_ParkingLot
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Index { get; set; }
        [StringLength(200)]
        [Required]
        public string Name { get; set; }

        [StringLength(200)]
        public string NameInEng { get; set; }


        public bool? UseTimeLimitParking { get; set; }
        public int? LimitDayNumber { get; set; }

        public bool? UseCardDependent { get; set; }
        public bool? UseRequiredParkingLotAccessed { get; set; }
        public bool? UseRequiredEmployeeVehicle { get; set; }


        [Required]
        public int CompanyIndex { get; set; }
        public DateTime? CreatedDate { get; set; }

        public DateTime? UpdatedDate { get; set; }

        [StringLength(50)]
        public string UpdatedUser { get; set; }
    }
}
