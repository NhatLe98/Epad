using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models.EZHR
{
    public class EzHRConfiguration
    {
        public const string Section = nameof(EzHRConfiguration);
        public string Host { get; set; }
        public string ClientID { get; set; }
        public string ClientSecret { get; set; }
    }
}
