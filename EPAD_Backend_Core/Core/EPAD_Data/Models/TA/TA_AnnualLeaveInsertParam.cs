using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models
{
    public class TA_AnnualLeaveInsertParam
    {
        public List<string> EmployeeATIDs { get; set; }
        public double AnnualLeave { get; set; }
        public int Index { get; set; }
    }
}
