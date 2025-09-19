using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data
{
    public class HeaderTableAjust
    {
        public HeaderTableAjust()
        {
            Columns = new List<ColumnData>();
            Rows = new List<dynamic>();
        }
        public List<ColumnData> Columns { get; set; }
        public List<dynamic> Rows { get; set; }
    }

   
}
