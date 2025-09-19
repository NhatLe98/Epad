using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace EPAD_Data.Entities
{
    public class AC_AccHoliday
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UID { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TimeZone { get; set; }
        public string HolidayName { get; set; }
        public int CompanyIndex { get; set; }
        public string UpdatedUser { get; set; }
        public DateTime UpdatedDate { get; set; }
        public int DoorIndex { get; set; }
        public int TimezoneRange { get; set; }
        public int HolidayType { get; set; }
        public bool Loop { get; set; }
        public int HolidayUID { get; set; }
    }
}
