using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EPAD_Data.Entities
{
    public class AC_StateLog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Index { get; set; }
        public DateTime Time { get; set; }
        public string Sensor { get; set; }
        public string Relay { get; set; }
        public string Alarm { get; set; }
        public string Door { get; set; }
        public string SerialNumber { get; set; }
    }
}
