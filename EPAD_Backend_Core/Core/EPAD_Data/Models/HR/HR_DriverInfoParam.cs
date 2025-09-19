using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models
{
    public class HR_DriverInfoParam
    {
        public string VehiclePlate { get; set; }
        public List<string> Status { get; set; }
        public List<long> DepartmentIDs { get; set; }
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public string Filter { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}
