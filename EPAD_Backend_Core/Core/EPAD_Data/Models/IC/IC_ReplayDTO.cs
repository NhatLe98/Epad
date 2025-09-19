using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models
{
    public class CallControllerParam
    {
        public int Id { get; set; }
        public long TimeLogIndex { get; set; }
    }
    public class ControllerParam
    {
        public int Index { get; set; }
        public string Name { get; set; }
        public string IPAddress { get; set; }
        public int Port { get; set; }
        public string Description { get; set; }
        public string RelayType { get; set; }
        public bool SetOn { get; set; }
        public int SecondsNumberOff { get; set; }
        public int SignalType { get; set; }
        public List<ChannelParam> ListChannel { get; set; }

    }
    public class ChannelParam
    {
        public short Index { get; set; }
        public double NumberOfSecondsOff { get; set; }
        public bool ChannelStatus { get; set; }
        public int SignalType { get; set; }
    }

    public class RelayDTO
    {

        public string relay1 { get; set; }
        public string relay2 { get; set; }
        public string relay3 { get; set; }
        public string relay4 { get; set; }
        public string relay5 { get; set; }
        public string relay6 { get; set; }
        public string relay7 { get; set; }
        public string relay8 { get; set; }
    }
}
