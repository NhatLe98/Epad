using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models
{
    public class AllConfig
    {
        public Dictionary<string, Config> Data;
        List<Config> Configs;
        public AllConfig()
        {
            Data = new Dictionary<string, Config>();
            Configs = new List<Config>();
        }
    }
}
