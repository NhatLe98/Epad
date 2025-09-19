using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EPAD_Data.Entities
{
    public class GC_TruckExtraDriverLog
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Index { get; set; }
        public string TripCode { get; set; }
        public string ExtraDriverName { get; set; }
        public string ExtraDriverCode { get; set; }
        public DateTime BirthDay { get; set; }
        public string CardNumber { get; set; }
        public bool IsInactive { get; set; }
        public string Description { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string UpdatedUser { get; set; }
    }
}
