using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models.IC
{
    public class IC_EmployeeStoppedImportDTO
    {
        public string EmployeeATID { get; set; }
        public string FullName { get; set; }
        public string StoppedDate { get; set; }
        public string Reason { get; set; }
        public string ErrorMessage { get; set; }
    }
}
