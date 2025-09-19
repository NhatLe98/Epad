using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Sync_Entities
{
    public class IC_Employee1Office
    {
        public bool error { get; set; }
        public List<IC_Employee1OfficeData> data { get; set; }
        public string total_item { get; set; }
    }

    public class IC_Employee1OfficeData
    {
        public string ID { get; set; }
        public string code { get; set; }
        public string attendace_code { get; set; }
        public string name { get; set; }
        public string job_status { get; set; }
        public string job_date_out { get; set; }
        public string job_out_reason { get; set; }
        public string gender { get; set; }
        public string email { get; set; }
        public string user_id { get; set; }
        public int? department_api { get; set; }
        public int? position_api { get; set; }
        public int? job_title_api { get; set; }
        public string job_date_join { get; set; }
    }
}
