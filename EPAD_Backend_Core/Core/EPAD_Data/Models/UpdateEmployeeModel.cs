using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models
{
    public class UpdateEmployeeModel
    {
        public string EmployeeATID { get; set; }
        public int CompanyIndex { get; set; }
        public string CustomerName { get; set; }
        public string UpdatedUser { get; set; }
    }
}
