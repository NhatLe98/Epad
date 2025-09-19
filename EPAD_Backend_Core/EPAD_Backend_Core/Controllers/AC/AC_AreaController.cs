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
    [Route("api/AC_Area/[action]")]
    [ApiController]
    public class AC_AreaController : ApiControllerBase
    {
        private readonly IAC_AreaService _IAC_AreaService;
        private IMemoryCache cache;
        private EPAD_Context context;
        public AC_AreaController(IServiceProvider pProvider) : base(pProvider)
        {
            context = TryResolve<EPAD_Context>();
            _IAC_AreaService = TryResolve<IAC_AreaService>();
            cache = TryResolve<IMemoryCache>();
        }

        [Authorize]
        [ActionName("GetAllArea")]
        [HttpGet]
        public IActionResult GetAllArea()
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            IEnumerable<object> dep;
            var areaList = context.AC_Area.Where(t => t.CompanyIndex == user.CompanyIndex).ToList();
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
        [ActionName("GetAreaAtPage")]
        [HttpGet]
        public IActionResult GetAreaAtPage(int page, string filter, int limit)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }
            var result = _IAC_AreaService.GetDataGrid(user.CompanyIndex, page, limit, filter);
            return ApiOk(result);
        }

        [Authorize]
        [ActionName("AddArea")]
        [HttpPost]
        public IActionResult AddArea([FromBody] AC_AreaDTO param)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            var checkName = context.AC_Area.Where(t => t.Name.Equals(param.Name)).FirstOrDefault();

            if (checkName != null)
            {
                return BadRequest("ExistAreaName");
            }

            AC_Area area = new AC_Area();
            area.Name = param.Name;
            area.Description = param.Description;
            area.UpdatedDate = DateTime.Now;
            area.CompanyIndex = user.CompanyIndex;

            context.AC_Area.Add(area);
            context.SaveChanges();
            return Ok();
        }

        [Authorize]
        [ActionName("UpdateArea")]
        [HttpPost]
        public IActionResult UpdateArea([FromBody] AC_AreaDTO param)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }
            var updateData = context.AC_Area.Where(t => t.Index == param.Index).FirstOrDefault();
            var checkName = context.AC_Area.Where(t => t.Name.Equals(param.Name)).FirstOrDefault();

            if (checkName != null && checkName.Index != updateData.Index)
            {
                return BadRequest("ExistAreaName");
            }

            var area = context.AC_Area.Where(t => t.CompanyIndex == user.CompanyIndex && t.Index == param.Index).FirstOrDefault();
            area.Name = param.Name;
            area.Description = param.Description;
            area.UpdatedDate = DateTime.Now;
            area.CompanyIndex = user.CompanyIndex;

            context.SaveChanges();
            return Ok();
        }

        [Authorize]
        [ActionName("DeleteArea")]
        [HttpPost]
        public IActionResult DeleteGroupDevice([FromBody] List<AC_AreaDTO> lsparam)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }
            foreach (var param in lsparam)
            {
                var deleteData = context.AC_Area.Where(t => t.CompanyIndex == user.CompanyIndex && t.Index == param.Index).FirstOrDefault();

                var checkServiceHasDevice = context.AC_AreaAndDoor.Where(t => t.CompanyIndex == user.CompanyIndex && t.AreaIndex == param.Index).FirstOrDefault();

                if (deleteData == null)
                {
                    return NotFound("AreaNotExist");
                }
                else if (checkServiceHasDevice != null)
                {
                    return NotFound("AreaHasDoor");
                }
                else
                {
                    context.AC_Area.Remove(deleteData);
                }
            }
            context.SaveChanges();

            result = Ok();
            return result;
        }

    }
}
