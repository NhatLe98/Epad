using System.Collections.Generic;

namespace EPAD_Data.Models
{
    public class EmployeeTreeNew
    {
        public string id { get; set; }
        public string Code { get; set; }
        public string label { get; set; }
        public string Type { get; set; }
        public int Level { get; set; }

        public List<EmployeeTreeNew> children { get; set; }
        public EmployeeTreeNew()
        {
            children = new List<EmployeeTreeNew>();
        }
    }
}
