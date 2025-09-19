using EPAD_Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models.IC
{
    public class IC_DepartmentParentDTO
    {
        public int Index { get; set; }
        public string Name { get; set; }
        public bool? IsDriverDepartment { get; set; }
        public bool? IsContractorDepartment { get; set; }
    }
    public class IC_DepartmentParentModel
    {
        public int DepartmentParentIndex { get; set; }
        public string DepartmentParentName { get; set; }
        public List<int> DepartmentIndexList { get; set; }
    }
}
