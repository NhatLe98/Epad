using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.HTTPClient
{
    public class GetListLeaveRequest
    {
        public int ID { get; set; }
        public int PageSize { get; set; }
        public List<string> ListEmployeeATID { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public List<int> LeaveTypeIndex { get; set; }
    }
}
