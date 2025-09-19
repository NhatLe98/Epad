using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EPAD_Data.Entities
{
    public class IC_RelayControllerChannel
    {
        public int RelayControllerIndex { get; set; }
        public short ChannelIndex { get; set; }
        public int CompanyIndex { get; set; }
        public double NumberOfSecondsOff { get; set; }
        public DateTime? UpdatedDate { get; set; }

        [StringLength(50)]
        public string UpdatedUser { get; set; }
        public int SignalType { get; set; }
    }
}
