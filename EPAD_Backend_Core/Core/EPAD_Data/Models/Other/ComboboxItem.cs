using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models
{
    public class ComboboxItem
    {
        public string label { get; set; }
        public object value { get; set; }
    }

    public class ComboboxDeviceItem
    {
        public string label { get; set; }
        public object value { get; set; }
        public bool status { get; set; }
    }
}
