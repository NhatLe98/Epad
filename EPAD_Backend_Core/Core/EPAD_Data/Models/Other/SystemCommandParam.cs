using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models
{
    public class SystemCommandParam
    {
        public int Index { get; set; }
        public string SerialNumber { get; set; }
        public string CommandName { get; set; }
        public string Command { get; set; }
        public string Params { get; set; }
        public string EmployeeATIDs { get; set; }
        public DateTime? RequestedTime { get; set; }
        public DateTime? ExcutedTime { get; set; }
        public bool Excuted { get; set; }
        public string Error { get; set; }
    }
}
