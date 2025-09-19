using System;

namespace EPAD_Data.Models
{
    public class IC_ServiveDTO
    {
        public int Index { get; set; }

        public string Name { get; set; }
        public string ServiceType { get; set; }

        public string Description { get; set; }

        public DateTime? CreatedDate { get; set; }

        public DateTime? UpdatedDate { get; set; }

        public string UpdatedUser { get; set; }

        public int CompanyIndex { get; set; }
    }
}
