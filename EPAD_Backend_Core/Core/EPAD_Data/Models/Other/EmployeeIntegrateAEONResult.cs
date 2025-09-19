using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models
{
    public class EmployeeIntegrateAEONResult
    {
        public List<string> ListIndexSuccess { get; set; }
        public List<string> ListIndexError { get; set; }
        public List<string> ListError { get; set; }
        public EmployeeIntegrateAEONResult()
        {
            ListIndexSuccess = new List<string>();
            ListIndexError = new List<string>();

            ListError = new List<string>();
        }
    }
}
