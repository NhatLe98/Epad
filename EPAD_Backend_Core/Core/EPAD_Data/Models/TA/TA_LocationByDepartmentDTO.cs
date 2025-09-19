using EPAD_Data.Entities;
using System.Collections.Generic;

namespace EPAD_Data.Models
{
    public class TA_LocationByDepartmentDTO : TA_LocationByDepartment
    {
        public int DepartmentIndexDTO { get; set; }
        public string LocationName { get; set; }
        public string Address { get; set; }
        public string DepartmentName { get; set; }
        public List<int> DepartmentList { get; set; }
    }
}
