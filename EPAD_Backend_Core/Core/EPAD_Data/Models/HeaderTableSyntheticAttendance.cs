

using System.Collections.Generic;

namespace EPAD_Data
{
    public class HeaderTableSyntheticAttendance
    {
        public HeaderTableSyntheticAttendance()
        {
            Columns = new List<HeaderSyntheticAttendance>();
            Rows = new List<dynamic>();
        }
        public List<HeaderSyntheticAttendance> Columns { get; set; }
        public List<dynamic> Rows { get; set; }
    }


    public class HeaderSyntheticAttendance
    {
        public string field { get; set; }
        public string headerName { get; set; }
        public int width { get; set; }
        public int minWidth { get; set; }
        public bool sortable { get; set; }
        public bool pinned { get; set; }
        public bool display { get; set; }
        public string type { get; set; }
        public bool checkboxSelection { get; set; }
        public bool headerCheckboxSelection { get; set; }
        public bool headerCheckboxSelectionFilteredOnly { get; set; }
        public int Index { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
    }


    public class Row
    {
        public Row()
        {
            ListColumnObjectUsed = new List<ColumnObjectSyntheticAttendance>();
        }
        public long Index { get; set; }
        public string EmployeeATID { get; set; }
        public string EmployeeCode { get; set; }
        public string EmployeeName { get; set; }
        public string DepartmentName { get; set; }
        public string EmployeeShiftJson { get; set; }
        public List<ColumnObjectSyntheticAttendance> ListColumnObjectUsed { get; set; }
    }

    public class ColumnObjectSyntheticAttendance
    {
        public int Index { get; set; }
        public int KeyMain { get; set; }
        public string Type { get; set; }

    }
}
