using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EPAD_Data.Entities
{
    public class IC_DeviceHistory
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Index { get; set; }
        public string SerialNumber { get; set; }
        public DateTime Date { get; set; }
        public DateTime UpdatedDate { get; set; }
        public short Status { get; set; }
    }
}
