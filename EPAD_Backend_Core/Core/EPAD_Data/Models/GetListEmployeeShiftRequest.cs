using System;
using System.Collections.Generic;

namespace EPAD_Data.Models
{
    public class GetListEmployeeShiftRequest
    {
        public DateTime FromDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<int> ShiftIds { get; set; }
    }
}
