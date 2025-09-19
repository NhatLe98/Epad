using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace EPAD_Data.Entities
{
    public class AC_AccGroup
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int UID { get; set; }
        public int Verify { get; set; }
        public bool ValidHoliday { get; set; }
        public string TimeZoneString { get; set; }
        public int CompanyIndex { get; set; }
        public string UpdatedUser { get; set; }
        public DateTime UpdatedDate { get; set; }
        [StringLength(200)]
        public string Name { get; set; }
        public int Timezone { get; set; }
        public int DoorIndex { get; set; }
    }
}
