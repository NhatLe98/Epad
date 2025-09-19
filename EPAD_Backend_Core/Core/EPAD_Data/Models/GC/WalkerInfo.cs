using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models
{
    public class WalkerInfo : MonitoringInfo
    {
        public string ObjectType { get; set; }
        public List<InfoDetail> ListInfo { get; set; }
        public string RegisterImage { get; set; }
        public string VerifyImage { get; set; }
        public WalkerInfo()
        {
            ListInfo = new List<InfoDetail>();
        }
    }
}
