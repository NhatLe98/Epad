using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace EPAD_Data.Entities
{
    public class AC_Door
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Index { get; set; }
        public string Name { get; set; }
        public string isUsingGroup { get; set; }
        public int CompanyIndex { get; set; }
        public string UpdatedUser { get; set; }
        public DateTime UpdatedDate { get; set; }
        public int DoorOpenTimezoneUID { get; set; }
        public int Timezone { get; set; }
        public string DoorSettingDescription { get; set; }
    }
}
