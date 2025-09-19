using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models.Other
{
    public class ListUserAndFilter
    {
        public List<string> listUser { get; set; }
        public List<string> listDevice { get; set; }
        public string filter { get; set; }
    }



    public class ListDepartmentAndFilter
    {
        public List<long> departmentIndexs { get; set; }
        public string filter { get; set; }
        public List<int> ListWorkingStatus { get; set; }
        public int EmployeeType { get; set; }
        public List<int> ListEmployeeType { get; set; }
    }
}
