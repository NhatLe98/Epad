using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Common.HTTPClient
{
    public class OAuth2Response
    {
        public string access_token { get; set; }
        public double expires_in { get; set; }
        public string token_type { get; set; }
    }

    public class OAuth2Request
    {
        public string grant_type { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        public string lang { get; set; }
    }
    public class OAuth2ResponseEz
    {
        public string Token { get; set; }
        public double ExpiresIn { get; set; }
        public string TokenType { get; set; }
    }
}
