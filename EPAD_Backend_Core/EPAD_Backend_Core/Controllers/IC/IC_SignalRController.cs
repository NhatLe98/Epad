using System;
using EPAD_Backend_Core.Base;
using EPAD_Backend_Core.WebUtilitys;
using EPAD_Data;
using EPAD_Data.Models;
using EPAD_Logic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using static EPAD_Data.Models.IC_SignalRDTO;

namespace EPAD_Backend_Core.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("api/SignalR/[action]")]
    [ApiController]
    public class IC_SignalRController : ApiControllerBase
    {
        static private EPAD_Context context;
        private IMemoryCache cache;
        private IIC_SignalRLogic _iC_SignalRLogic;
        public IC_SignalRController(IServiceProvider provider) : base(provider)
        {
            context = TryResolve<EPAD_Context>();
            cache = TryResolve<IMemoryCache>();
            _iC_SignalRLogic = TryResolve<IIC_SignalRLogic>();
        }

        [ActionName("PostUserFingerDevive")]
        [HttpPost]
        public IActionResult PostUserFingerDevive([FromBody] UserFingerDeviceParam pUserFinger)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }
            _iC_SignalRLogic.PostPushUserFingerDevice(user.CompanyIndex, pUserFinger);
            return Ok();
        }

    }
}
