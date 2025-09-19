using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data
{
    public class HeaderTableEmployeeShift
    {
        public HeaderTableEmployeeShift()
        {
            Columns = new List<ColumnData>();
            Rows = new List<RowData>();
        }
        public List<ColumnData> Columns { get; set; }
        public List<RowData> Rows { get; set; }
        public int total { get; set; }
    }

    public class ColumnData
    {
        public int Index { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
    }

    public class ColumnHeader
    {
        public string field { get; set; }
        public string headerName { get; set; }
        public int width { get; set; }
        public bool sortable { get; set; }
        public bool display { get; set; }
        public string type { get; set; }
        public bool checkboxSelection { get; set; }
        public bool headerCheckboxSelection { get; set; }
        public bool headerCheckboxSelectionFilteredOnly { get; set; }
        public int Index { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
    }


    public class RowData
    {
        public RowData()
        {
            ListColumnUsed = new List<string>();
            ListColumnObjectUsed = new List<ColumnObjectUsed>();
        }
        public long Index { get; set; }
        public string EmployeeATID { get; set; }
        public string EmployeeCode { get; set; }
        public string EmployeeName { get; set; }
        public string DepartmentName { get; set; }
        public long? DepartmentIndex { get; set; }
        public string EmployeeShiftJson { get; set; }
        public string ShiftName { get; set; }
        public List<string> ListColumnUsed { get; set; }
        public List<ColumnObjectUsed> ListColumnObjectUsed { get; set; }
    }

    public class ColumnObjectUsed
    {
        public int Index { get; set; }
        public string KeyMain { get; set; }
        public string ShiftName { get; set; }
        public bool IsSchedule { get; set; }

    }
}
