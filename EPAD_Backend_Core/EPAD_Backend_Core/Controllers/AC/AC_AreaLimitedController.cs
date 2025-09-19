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
    [Route("api/AC_AreaLimited/[action]")]
    [ApiController]
    public class AC_AreaLimitedController : ApiControllerBase
    {
        private readonly IAC_AreaLimitedService _IAC_AreaLimitedService;
        private IMemoryCache cache;
        private EPAD_Context context;
        public AC_AreaLimitedController(IServiceProvider pProvider) : base(pProvider)
        {
            context = TryResolve<EPAD_Context>();
            _IAC_AreaLimitedService = TryResolve<IAC_AreaLimitedService>();
            cache = TryResolve<IMemoryCache>();
        }

        [Authorize]
        [ActionName("GetAllAreaLimited")]
        [HttpGet]
        public IActionResult GetAllAreaLimited()
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            IEnumerable<object> dep;
            var areaList = _IAC_AreaLimitedService.Where(t => t.CompanyIndex == user.CompanyIndex).ToList();
            dep = from area in areaList
                  orderby area.Name
                  select new
                  {
                      value = area.Index,
                      label = area.Name
                  };
            result = Ok(dep);
            return result;
        }

        [Authorize]
        [ActionName("GetAreaLimitedAtPage")]
        [HttpGet]
        public IActionResult GetAreaLimitedAtPage(int page, string filter, int limit)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }
            var result = _IAC_AreaLimitedService.GetDataGrid(user.CompanyIndex, page, limit, filter);
            return ApiOk(result);
        }

        [Authorize]
        [ActionName("AddAreaLimited")]
        [HttpPost]
        public IActionResult AddAreaLimited([FromBody] AC_AreaLimitedDTO param)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            var checkName = _IAC_AreaLimitedService.Where(t => t.Name.Equals(param.Name)).FirstOrDefault();

            if (checkName != null)
            {
                return BadRequest("ExistAreaLimitedName");
            }

            AC_AreaLimited area = new AC_AreaLimited();
            area.Name = param.Name;
            area.Description = param.Description;
            area.UpdatedDate = DateTime.Now;
            area.CompanyIndex = user.CompanyIndex;
            context.AC_AreaLimited.Add(area);
            context.SaveChanges();
            var listAreaAndDoor = new List<AC_AreaLimitedAndDoor>();
            foreach (var item in param.DoorIndexes)
            {
                var areaAndDoor = new AC_AreaLimitedAndDoor();
                areaAndDoor.AreaLimited = area.Index;
                areaAndDoor.DoorIndex = item;
                areaAndDoor.UpdatedDate = DateTime.Now;
                areaAndDoor.UpdatedUser = user.UserName;
                areaAndDoor.CompanyIndex = user.CompanyIndex;
                listAreaAndDoor.Add(areaAndDoor);
            }
            context.AC_AreaLimitedAndDoor.AddRange(listAreaAndDoor);
            context.SaveChanges();
            return Ok();
        }

        [Authorize]
        [ActionName("UpdateAreaLimited")]
        [HttpPost]
        public IActionResult UpdateAreaLimited([FromBody] AC_AreaLimitedDTO param)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }
            var updateData = context.AC_AreaLimited.Where(t => t.Index == param.Index).FirstOrDefault();
            var checkName = context.AC_AreaLimited.Where(t => t.Name.Equals(param.Name)).FirstOrDefault();
            if (checkName != null && checkName.Index != updateData.Index)
            {
                return BadRequest("ExistAreaLimitedName");
            }
            var area = context.AC_AreaLimited.FirstOrDefault(t => t.CompanyIndex == user.CompanyIndex && t.Index == param.Index);
            area.Name = param.Name;
            area.Description = param.Description;
            area.UpdatedDate = DateTime.Now;
            area.CompanyIndex = user.CompanyIndex;

            var statusDelete = _IAC_AreaLimitedService.DeleteAreaLimitedAndDoor(new List<int> { area.Index });
            if (!statusDelete)
            {
                return BadRequest("UpdateAreaLimitedFail");
            }
            var listAreaAndDoor = new List<AC_AreaLimitedAndDoor>();
            foreach (var item in param.DoorIndexes)
            {
                listAreaAndDoor.Add(new AC_AreaLimitedAndDoor()
                {
                    AreaLimited = area.Index,
                    DoorIndex = item,
                    UpdatedDate = DateTime.Now,
                    UpdatedUser = user.UserName,
                    CompanyIndex = user.CompanyIndex
                });
            }
            context.AC_AreaLimitedAndDoor.AddRange(listAreaAndDoor);
            context.SaveChanges();
            return Ok();
        }

        [Authorize]
        [ActionName("DeleteAreaLimited")]
        [HttpPost]
        public IActionResult DeleteGroupDevice([FromBody] List<AC_AreaLimitedDTO> lsparam)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }
            var deleteDataList = context.AC_AreaLimited.Where(t => t.CompanyIndex == user.CompanyIndex && lsparam.Select(x => x.Index).Contains(t.Index)).ToList();
            var deleteAreaLimitedAndDoor = context.AC_AreaLimitedAndDoor.Where(x => deleteDataList.Select(z => z.Index).Contains(x.AreaLimited)).ToList();
            foreach (var param in lsparam)
            {
                var deleteData = deleteDataList.FirstOrDefault(x => x.Index == param.Index);
                if (deleteData == null)
                {
                    return NotFound("AreaLimitedNotExist");
                }
                else
                {
                    var areaLimitedAndDoor = deleteAreaLimitedAndDoor.Where(x => x.AreaLimited == param.Index).ToList();
                    context.AC_AreaLimitedAndDoor.RemoveRange(areaLimitedAndDoor);
                    context.AC_AreaLimited.Remove(deleteData);
                }
            }
            context.SaveChanges();
            result = Ok();
            return result;
        }

    }
}
