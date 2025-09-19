using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models.WebAPIHeader
{
    public class WebAPIHeader
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public WebAPIHeader(string pName, string pValue)
        {
            Name = pName; Value = pValue;
        }
    }

    public class LoginApiInfo
    {
        public string AccessToken { get; set; }
        public string TokenType { get; set; }
        public int ExpiresIn { get; set; }
    }
 
}
