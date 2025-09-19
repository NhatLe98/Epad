using EPAD_Backend_Core.Base;
using EPAD_Backend_Core.WebUtilitys;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using EPAD_Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EPAD_Backend_Core.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("api/AC_Group/[action]")]
    [ApiController]
    public class AC_GroupController : ApiControllerBase
    {
        private readonly IAC_GroupService _IAC_GroupService;
        private IMemoryCache cache;
        private EPAD_Context context;
        public AC_GroupController(IServiceProvider pProvider) : base(pProvider)
        {
            context = TryResolve<EPAD_Context>();
            _IAC_GroupService = TryResolve<IAC_GroupService>();
            cache = TryResolve<IMemoryCache>();
        }

        [Authorize]
        [ActionName("GetAllGroup")]
        [HttpGet]
        public IActionResult GetAllGroup()
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            IEnumerable<object> dep;
            dep = from area in context.AC_AccGroup.Where(t => t.CompanyIndex == user.CompanyIndex)

                  orderby area.Name
                  select new
                  {
                      value = area.UID.ToString(),
                      label = area.Name,
                      doorIndex = area.DoorIndex,
                      timezone = area.Timezone,
                  };
            result = Ok(dep);

            return result;
        }

        [Authorize]
        [ActionName("GetAllGroupLst")]
        [HttpGet]
        public IActionResult GetAllGroupLst()
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            IEnumerable<object> dep;
            dep = context.AC_AccGroup.Where(t => t.CompanyIndex == user.CompanyIndex);
            result = Ok(dep);

            return result;
        }

        [Authorize]
        [ActionName("GetGroupAtPage")]
        [HttpGet]
        public IActionResult GetGroupAtPage(int page, string filter, int limit)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }
            var result = _IAC_GroupService.GetDataGrid(user.CompanyIndex, page, limit, filter);
            return ApiOk(result);
        }

        [Authorize]
        [ActionName("AddGroup")]
        [HttpPost]
        public IActionResult AddGroup([FromBody] AC_AccGroup param)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            var checkName = context.AC_AccGroup.Where(t => t.Name.Equals(param.Name)).FirstOrDefault();

            if (checkName != null)
            {
                return BadRequest("ExistGroupName");
            }

            var timezoneStr = new List<string>();

            if (param.Timezone != 0)
            {
                var timezone = _DbContext.AC_TimeZone.FirstOrDefault(x => x.UID == param.Timezone);
                if (timezone != null)
                {
                    timezoneStr = timezone.UIDIndex.Split(',').ToList();
                }
            }
            var allAccGroup = _DbContext.AC_AccGroup.Select(x => x.UID).ToList();

            var group = new AC_AccGroup();
            group.Name = param.Name;
            group.Verify = 0;
            group.UpdatedDate = DateTime.Now;
            group.CompanyIndex = user.CompanyIndex;
            group.Timezone = param.Timezone;
            group.DoorIndex = param.DoorIndex;


            for (int i = 2; i < 100; i++)
            {

                if (!allAccGroup.Contains(i))
                {
                    group.UID = i;
                    break;
                }
            }
            context.AC_AccGroup.Add(group);
            context.SaveChanges();
            return Ok();
        }

        [Authorize]
        [ActionName("UpdateGroup")]
        [HttpPost]
        public IActionResult UpdateGroup([FromBody] AC_AccGroup param)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }
            var updateData = context.AC_AccGroup.Where(t => t.UID == param.UID).FirstOrDefault();
            var checkName = context.AC_AccGroup.Where(t => t.Name.Equals(param.Name)).FirstOrDefault();

            if (checkName != null && checkName.UID != updateData.UID)
            {
                return BadRequest("ExistGroupName");
            }

            var timezoneStr = new List<string>();

            var area = context.AC_AccGroup.Where(t => t.CompanyIndex == user.CompanyIndex && t.UID == param.UID).FirstOrDefault();
            area.Name = param.Name;
            area.UpdatedDate = DateTime.Now;
            area.CompanyIndex = user.CompanyIndex;
            area.Timezone = param.Timezone;
            area.DoorIndex = param.DoorIndex;

            area.ValidHoliday = true;

            context.SaveChanges();
            return Ok();
        }

        [Authorize]
        [ActionName("DeleteGroup")]
        [HttpPost]
        public IActionResult DeleteGroup([FromBody] List<AC_AccGroup> lsparam)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }


            foreach (var param in lsparam)
            {
                var deleteData = context.AC_AccGroup.Where(t => t.CompanyIndex == user.CompanyIndex && t.UID == param.UID).FirstOrDefault();
                var checkData = context.AC_AccessedGroup.Any(x => x.GroupIndex == param.UID);
                var checkDataDepartmentAcc = context.AC_DepartmentAccessedGroup.Any(x => x.GroupIndex == param.UID);

                if (deleteData == null)
                {
                    return NotFound("AccGroupNotExist");
                }
                else if (checkData || checkDataDepartmentAcc)
                {
                    return NotFound("ACGroupIsUsing");
                }
                else
                {
                    context.AC_AccGroup.Remove(deleteData);
                }
            }
            context.SaveChanges();

            result = Ok();
            return result;
        }


    }
}
