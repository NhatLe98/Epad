using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Entities
{
    public class IC_CardNumberInfo
    {
        public long Index { get; set; }
        public string EmployeeATID { get; set; }
        public string CardNumber { get; set; }
        public bool? Status { get; set; }
        public DateTime DateCreated { get; set; }
    }
}
