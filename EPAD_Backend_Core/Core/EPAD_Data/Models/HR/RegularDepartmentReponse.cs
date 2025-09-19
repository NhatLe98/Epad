using System.Collections.Generic;

namespace EPAD_Data.Models
{
    public class RegularDepartmentReponse
    {
        public int DepartmentIndex { get; set; }
        public string DepartmentName { get; set; }
        public List<RegularDepartmentReponse> ListChildrent { get; set; }
    }

    public class RegularDepartmentDataReponse
    {
        public int DepartmentIndex { get; set; }
        public string DepartmentName { get; set; }
    }
}
