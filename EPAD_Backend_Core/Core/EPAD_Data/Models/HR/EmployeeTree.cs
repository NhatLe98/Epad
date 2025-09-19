using System.Collections.Generic;

namespace EPAD_Data.Models
{
    public class EmployeeTree
    {
        public string ID { get; set; }
        public string IndexID { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public int Level { get; set; }

        public List<EmployeeTree> ListChildrent { get; set; }
        public EmployeeTree()
        {
            ListChildrent = new List<EmployeeTree>();
        }
    }
}
