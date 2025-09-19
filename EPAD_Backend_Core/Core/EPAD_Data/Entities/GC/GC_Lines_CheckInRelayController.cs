using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace EPAD_Data.Entities
{
    public class GC_Lines_CheckInRelayController
    {

        [Required]
        public int LineIndex { get; set; }
        [Required]
        public int RelayControllerIndex { get; set; }
        [Required]
        public int CompanyIndex { get; set; }
        public short OpenDoorChannelIndex { get; set; }
        public short FailAlarmChannelIndex { get; set; }

        public DateTime? UpdatedDate { get; set; }

        [StringLength(50)]
        public string UpdatedUser { get; set; }
    }
}
