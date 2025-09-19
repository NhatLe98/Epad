using System;
using System.Collections.Generic;
using System.Linq;
using EPAD_Backend_Core.Base;
using EPAD_Backend_Core.WebUtilitys;
using EPAD_Common.Utility;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace EPAD_Backend_Core.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("api/Setting/[action]")]
    [ApiController]
    public class IC_SettingController : ApiControllerBase
    {
        private EPAD_Context context;
        private IMemoryCache cache;
        public IC_SettingController(IServiceProvider provider):base(provider)
        {
            context = TryResolve<EPAD_Context>();
            cache = TryResolve<IMemoryCache>();
        }

        [Authorize]
        [ActionName("AddPersonalAccessToken")]
        [HttpPost]
        public IActionResult AddPersonalAccessToken([FromBody]PersonalAccessTokenRequest request)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            if (user == null) return Unauthorized("TokenExpired");

            var id = Guid.NewGuid().ToString();
            var accessToken = StringHelper.SHA256(id);
            context.Add(new IC_AccessToken()
            {
                CompanyIndex = user.CompanyIndex,
                CreatedDate = DateTime.Now,
                ExpiredDate = request.ExpiredDate,
                AccessToken = accessToken,
                Name = request.Name,
                Scope = request.Scopes,
                Note = "",
                UpdatedDate = DateTime.Now,
                UpdatedUser = user.UserName
            });
            context.SaveChanges();
            return Ok(id);
        }

        [Authorize]
        [ActionName("GetPersonalAccessToken")]
        [HttpGet]
        public IActionResult GetPersonalAccessToken(string token,int limit)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            if (user == null) return Unauthorized("TokenExpired");
            var lstAccessToken = context.IC_AccessToken.Where(x => x.CompanyIndex == user.CompanyIndex).Take(limit).ToList();
            var result = lstAccessToken.Select(x => new
            {
                x.Index,
                x.CompanyIndex,
                x.Name,
                x.ExpiredDate,
                x.CreatedDate,
                x.Scope,
                x.Note
            });
            return Ok(result);
        }

        [Authorize]
        [ActionName("RevokeAccessToken")]
        [HttpPost]
        public IActionResult RevokeAccessToken([FromBody]RevokeAccessTokenRequest request)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            if (user == null) return Unauthorized("TokenExpired");

            var accessToken = context.IC_AccessToken.FirstOrDefault(x => x.Index == request.ID);
            if(accessToken != null)
            {
                context.Remove(accessToken);
                context.SaveChanges();
                return Ok();
            }
            else
            {
                return NotFound("MSG_PersonalAccessTokenNotFound");
            }
        }
    }

    public class PersonalAccessTokenRequest
    {
        public string Name { get; set; }
        public string Scopes { get; set; }
        public DateTime? ExpiredDate { get; set; }
    }

    public class RevokeAccessTokenRequest
    {
        public int ID { get; set; }
    }
}