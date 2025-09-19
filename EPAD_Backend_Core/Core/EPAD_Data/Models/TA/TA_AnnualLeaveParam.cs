using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models
{
    public class TA_AnnualLeaveParam
    {
        public string filter { get; set; }
        public int page { get; set; }
        public int limit { get; set; }
        public List<long> departments { get; set; }
        public List<string> employeeatids { get; set; }
    }
}
