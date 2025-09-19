using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace EPAD_Data.Models.TimeLog
{
    public class TimeLogRequestLog
    {
        public TimeLogRequest TimeLogReq { get; set; }
        public string Content { get; set; }
        public HttpStatusCode StatusCode { get; set; }
    }

    public class CotrollerWarningRequestModel
    {
        public int Index { get; set; }
        public int ControllerIndex { get; set; }
        public List<int> ChannelIndexs { get; set; }
        public int Type { get; set; }
        public List<int?> LineIndexs { get; set; }
        public int RulesWarningIndex { get; set; }
        public int CompanyIndex { get; set; }
    }

    public class RelayControllerParam
    {
        public int ControllerIndex { get; set; }
        public List<int> ListChannel { get; set; }
        public bool SetOn { get; set; }
        public bool AutoOff { get; set; }
        public short Second { get; set; }
        public short ChannelInputGood { get; set; }
        public short ChannelInputNotGood { get; set; }
    }
}
