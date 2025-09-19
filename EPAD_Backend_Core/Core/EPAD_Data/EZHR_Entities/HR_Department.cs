using System;
using System.Collections.Generic;

namespace EPAD_Data.Entities
{
    public partial class HR_Department
    {
        public long Index { get; set; }
        public int CompanyIndex { get; set; }
        public string Name { get; set; }
        public string NameInEng { get; set; }
        public string Code { get; set; }
        public long? ParentIndex { get; set; }
        public string CompanyId { get; set; }
        public string Description { get; set; }
        public double? WorkingDays { get; set; }
        public double? WorkingHours { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string UpdatedUser { get; set; }
        public string Location { get; set; }
        public string PhoneNumber { get; set; }
        public string Note { get; set; }
        public string ContactEmail { get; set; }
        public DateTime? DateOfCreation { get; set; }
    }
}
