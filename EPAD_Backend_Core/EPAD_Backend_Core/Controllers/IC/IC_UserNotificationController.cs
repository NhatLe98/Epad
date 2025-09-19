using EPAD_Backend_Core.Base;
using EPAD_Backend_Core.WebUtilitys;
using EPAD_Common.Types;
using EPAD_Data.Models;
using EPAD_Logic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EPAD_Backend_Core.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("api/UserNotification/[action]")]
    [ApiController]
    public class IC_UserNotificationController : ApiControllerBase
    {
        private IMemoryCache _cache;
        private IIC_UserNotificationLogic _iIC_UserNotificationLogic;
        public IC_UserNotificationController(IServiceProvider provider):base(provider)
        {
            this._cache = TryResolve<IMemoryCache>();
            this._iIC_UserNotificationLogic = TryResolve<IIC_UserNotificationLogic>();
        }
        [ActionName("PostMany")]
        [HttpPost]
        public IActionResult PostMany([FromBody] List<AddedParam> addedParams)
        {
            UserInfo user = UserInfo.GetFromCache(_cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized("TokenExpired");
            }
            addedParams.Add(new AddedParam { Key = "UserName", Value = user.UserName });

            IEnumerable<IC_UserNotificationDTO> notificationList = _iIC_UserNotificationLogic.GetMany(addedParams);
            result = Ok(notificationList);

            return result;
        } 

        [ActionName("Delete")]
        [HttpPost]
        public IActionResult Delete([FromBody] IC_UserNotificationDTO notify)
        {
            UserInfo user = UserInfo.GetFromCache(_cache, User.Identity.Name);
            ActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized("TokenExpired");
            }
            if(notify == null || notify.UserName != user.UserName)
            {
                return  BadRequest("PermissionDenied");
            }
            if (_iIC_UserNotificationLogic.Delete(notify.Index) > 0)
            {
                return Ok();
            }
            
            return BadRequest("PermissionDenied");
        }
        [ActionName("DeleteAll")]
        [HttpPost]
        public IActionResult DeleteAll([FromBody] List<IC_UserNotificationDTO> listNotify)
        {
            UserInfo user = UserInfo.GetFromCache(_cache, User.Identity.Name);
            ActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }
            if (listNotify.Count == 0)
                return Ok();
            foreach (var item in listNotify)
            {
                if (item == null || item.UserName != user.UserName)
                {
                    return BadRequest("PermissionDenied");
                }
            }
            if (_iIC_UserNotificationLogic.DeleteList(listNotify.Select(e=>e.Index).ToList()))
            {
                return Ok();
            }

            return BadRequest("PermissionDenied");
        }
    }
}
