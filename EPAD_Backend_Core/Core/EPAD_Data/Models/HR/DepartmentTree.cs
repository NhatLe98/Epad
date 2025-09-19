using System.Collections.Generic;

namespace EPAD_Data.Models
{
    public class DepartmentTree
    {
        public string Index { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public int Level { get; set; }

        public List<DepartmentTree> ListChildrent { get; set; }
        public DepartmentTree()
        {
            ListChildrent = new List<DepartmentTree>();
        }
    }
}
