using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models
{
    public class IC_ControllerDTO
    {
        public int Index { get; set; }
        public string Name { get; set; }
        public string IPAddress { get; set; }
        public int Port { get; set; }
        public string IDController { get; set; }
    }
}
