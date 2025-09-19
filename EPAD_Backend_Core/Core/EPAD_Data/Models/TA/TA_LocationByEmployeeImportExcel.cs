using EPAD_Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models
{
    public class TA_LocationByEmployeeImportExcel
    {
        public string EmployeeATID { get; set; }
        public string EmployeeCode { get; set; }
        public string FullName { get; set; }
        public string LocationName { get; set; }
        public string ErrorMessage { get; set; }
        public int LocationIndex { get; set; }
    }
}
