using EPAD_Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models
{
    public class TA_HolidayDTO : TA_Holiday
    {
        public string HolidayDateString { get; set; }
    }
}
