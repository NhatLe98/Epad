using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Entities
{
    public class EM_NannyClassroom
    {
        public int Index { get; set; }
        public string EmployeeATID { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string Class { get; set; }
        public string Note { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string UpdatedUser { get; set; }
        public int CompanyIndex { get; set; }
    }
}
