using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Entities
{
    public class Cat_Shift
    {
        public string Code { get; set; }
        public string ShiftName { get; set; }
        public DateTime InTime { get; set; }
        public double CoOut { get; set; }
        public DateTime OutTime { get; set; }
    }
}
