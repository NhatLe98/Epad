using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Entities
{
    public class Cat_OrgStructure
    {
        public string code { get; set; }
        public string OrgStructureName { get; set; }
        public string ParentCode { get; set; }
        public string StatusFormat { get; set; }
    }

    public class Cat_OrgStructureApiResult
    {
        public bool success { get; set; }
        public List<Cat_OrgStructure> data { get; set; }
        public string Message { get; set; }
    }
}
