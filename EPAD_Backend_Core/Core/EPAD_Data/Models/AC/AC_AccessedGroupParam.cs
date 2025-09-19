using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models
{
    public class AC_AccessedGroupParam
    {
        public string filter { get; set; }
        public int page { get; set; }
        public int limit { get; set; }
        public List<long> departments { get; set; }
        public List<int> groups { get; set; }
    }
}
