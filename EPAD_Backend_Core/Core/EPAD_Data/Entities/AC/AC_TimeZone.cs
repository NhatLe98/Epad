using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EPAD_Data.Entities
{
    public class AC_TimeZone
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UID { get; set; }
        public string SunStart1 { get; set; }
        public string SunEnd1 { get; set; }
        public string SunStart2 { get; set; }
        public string SunEnd2 { get; set; }
        public string SunStart3 { get; set; }
        public string SunEnd3 { get; set; }
        public string MonStart1 { get; set; }
        public string MonEnd1 { get; set; }
        public string MonStart2 { get; set; }
        public string MonEnd2 { get; set; }
        public string MonStart3 { get; set; }
        public string MonEnd3 { get; set; }
        public string TuesStart1 { get; set; }
        public string TuesEnd1 { get; set; }
        public string TuesStart2 { get; set; }
        public string TuesEnd2 { get; set; }
        public string TuesStart3 { get; set; }
        public string TuesEnd3 { get; set; }
        public string WedStart1 { get; set; }
        public string WedEnd1 { get; set; }
        public string WedStart2 { get; set; }
        public string WedEnd2 { get; set; }
        public string WedStart3 { get; set; }
        public string WedEnd3 { get; set; }
        public string ThursStart1 { get; set; }
        public string ThursEnd1 { get; set; }
        public string ThursStart2 { get; set; }
        public string ThursEnd2 { get; set; }
        public string ThursStart3 { get; set; }
        public string ThursEnd3 { get; set; }
        public string FriStart1 { get; set;}
        public string FriEnd1 { get; set;}
        public string FriStart2 { get; set; }
        public string FriEnd2 { get; set; }
        public string FriStart3 { get; set; }
        public string FriEnd3 { get; set; }
        public string SatStart1 { get; set; }
        public string SatEnd1 { get; set; }
        public string SatStart2 { get; set; }
        public string SatEnd2 { get; set; }
        public string SatStart3 { get; set; }
        public string SatEnd3 { get; set; }
        public string Holiday1Start1 { get; set; }
        public string Holiday1End1 { get; set; }
        public string Holiday1Start2 { get; set; }
        public string Holiday1End2 { get; set; }
        public string Holiday1Start3 { get; set; }
        public string Holiday1End3 { get; set; }
        public string Holiday2Start1 { get; set; }
        public string Holiday2End1 { get; set; }
        public string Holiday2Start2 { get; set; }
        public string Holiday2End2 { get; set; }
        public string Holiday2Start3 { get; set; }
        public string Holiday2End3 { get; set; }
        public string Holiday3Start1 { get; set; }
        public string Holiday3End1 { get; set; }
        public string Holiday3Start2 { get; set; }
        public string Holiday3End2 { get; set; }
        public string Holiday3Start3 { get; set; }
        public string Holiday3End3 { get; set; }
        public int CompanyIndex { get; set; }
        public string UpdatedUser { get; set; }
        public DateTime UpdatedDate { get; set; }
        [StringLength(200)]
        public string Name { get; set; }
        public string UIDIndex { get; set; }
        public string Description { get; set; }
    }
}
