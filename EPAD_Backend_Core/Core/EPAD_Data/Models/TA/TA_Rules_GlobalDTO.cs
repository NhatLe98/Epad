using EPAD_Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models
{
    public class TA_Rules_GlobalDTO : TA_Rules_Global
    {
        public string NightShiftStartTimeString { get; set; }
        public string NightShiftEndTimeString { get; set; }

        public List<string> ListTimePos { get; set; }
    }
}
