using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EPAD_Data.Entities
{
    public class GC_TruckDriverLog
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Index { get; set; }
        [Required]
        public string TripCode { get; set; }
        [Required]
        public DateTime Time { get; set; }
        [Required]
        public int CompanyIndex { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string UpdatedUser { get; set; }
        public short? InOutMode { get; set; }
        public string MachineSerial { get; set; }
        public string CardNumber { get; set; }
        public bool IsInactive { get; set; }
        public string Note { get; set; }
        public string Description { get; set; }
        public bool? IsException { get; set; }
        public string ReasonException { get; set; }
    }
}
