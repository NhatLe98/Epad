using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace EPAD_Data.Models
{
    public class CommandGroup
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public bool Excuted { get; set; }
        public List<CommandResult> ListCommand { get; set; }
        [JsonIgnore]
        public DateTime CreatedTime { get; set; }
        [JsonIgnore]
        public DateTime FinishedTime { get; set; }
        [JsonIgnore]
        public string EventType { get; set; }
        [JsonIgnore]
        public List<string> Errors { get; set; }

        public CommandGroup()
        {
            Excuted = false;
            ListCommand = new List<CommandResult>();

            CreatedTime = DateTime.Now;
            Errors = new List<string>();
        }
        public CommandGroup(string pID,string pName)
        {
            ID = pID;
            Name = pName;
            Excuted = false;
            ListCommand = new List<CommandResult>();

            CreatedTime = DateTime.Now;
            Errors = new List<string>();
        }
    }
}