using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models
{
    public class IntegrateLogResult
    {
        public int SuccessDB { get; set; }
        public int FailDB { get; set; }
        public List<string> ErrorsDB { get; set; }
        public int SuccessFile { get; set; }
        public int FailFile { get; set; }
        public List<string> ErrorsFile { get; set; }

    }
}
