using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models
{
    public class EmployeeIntegrateResult
    {
        public List<int> ListIndexSuccess { get; set; }
        public List<int> ListIndexError { get; set; }
        public List<string> ListError { get; set; }
        public EmployeeIntegrateResult()
        {
            ListIndexSuccess = new List<int>();
            ListIndexError = new List<int>();
            ListError = new List<string>();
        }
    }
}
