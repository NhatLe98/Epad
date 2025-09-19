using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EPAD_Data.Models
{
    public class IC_CommandSystemGroupDTO
    {
        public int Index { get; set; }
        public string GroupName { get; set; }
        public bool Excuted { get; set; }

        public DateTime? CreatedDate { get; set; }

        public DateTime? UpdatedDate { get; set; }

        public string EventType { get; set; }

        public string ExternalData { get; set; }

        public string UpdatedUser { get; set; }

        public int CompanyIndex { get; set; }
    }
}
