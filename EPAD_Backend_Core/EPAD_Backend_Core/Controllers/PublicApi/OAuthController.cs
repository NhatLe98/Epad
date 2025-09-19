using EPAD_Backend_Core.Provider;
using EPAD_Backend_Core.WebUtilitys;
using EPAD_Data.Models;
using Microsoft.AspNetCore.Mvc;
using System;

namespace EPAD_Backend_Core.Controllers.PublicApi
{
    [Route("api/v1/oauth2")]
    [ApiController]
    public class OAuthController : PublicBaseController
    {
        public OAuthController(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        [ActionName("token")]
        [HttpPost]
        public IActionResult Token([FromBody] OAuthRequest req)
        {
            IActionResult response = Unauthorized();

            UserInfo user = null;
            CompanyInfo company = null;
            string error = "";

            bool result = LoginProcess.ClientLogin(req.username, req.password, ref error, ref user, ref company, _epadContext, _otherContext, _cache);

            if (result == true)
            {
                string guid = "";
                var tokenString = Provider.TokenProvider.CreateJsonWebTokenForClient(_cache, user, ref guid);
                response = Ok(new { access_token = tokenString, token_type = "bearer", expires_in = TimeSpan.FromMinutes(5).TotalSeconds });
            }
            else
            {
                response = Unauthorized(new { error = error });
            }

            return response;
        }

    }

    public class OAuthRequest
    {
        public string grant_type { get; set; } = "password";
        public string username { get; set; }
        public string password { get; set; }
        public string lang { get; set; } = "vi";
    }
}
