using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Entities
{
    public class Cat_Position
    {
        public string Code { get; set; }
        public string PositionName { get; set; }
        public string PositionEngName { get; set; }
    }

    public class Cat_PositionApiResult
    {
        public bool success { get; set; }
        public List<Cat_Position> data { get; set; }
        public string Message { get; set; }
    }
}
