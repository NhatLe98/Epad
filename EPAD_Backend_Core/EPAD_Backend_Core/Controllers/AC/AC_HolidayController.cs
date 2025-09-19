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
using EPAD_Common.Extensions;

namespace EPAD_Backend_Core.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("api/AC_Holiday/[action]")]
    [ApiController]
    public class AC_HolidayController : ApiControllerBase
    {

        private readonly IAC_HolidayService _IAC_HolidayService;
        private IMemoryCache cache;
        private EPAD_Context context;
        public AC_HolidayController(IServiceProvider pProvider) : base(pProvider)
        {
            context = TryResolve<EPAD_Context>();
            _IAC_HolidayService = TryResolve<IAC_HolidayService>();
            cache = TryResolve<IMemoryCache>();
        }

        [Authorize]
        [ActionName("GetAllHoliday")]
        [HttpGet]
        public IActionResult GetAllHoliday()
        {
            var user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            var holidayLst = context.AC_AccHoliday.Where(t => t.CompanyIndex == user.CompanyIndex).ToList();
            var dep = from holiday in holidayLst
                      orderby holiday.HolidayName
                      select new
                      {
                          value = holiday.UID.ToString(),
                          label = holiday.HolidayName
                      };
            result = Ok(dep);

            return result;
        }

        [Authorize]
        [ActionName("GetAllHolidayUID")]
        [HttpGet]
        public IActionResult GetAllHolidayUID()
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            IEnumerable<object> dep;
            var areaList = context.AC_AccHoliday.Where(t => t.CompanyIndex == user.CompanyIndex && t.HolidayUID != 0).ToList();
            dep = from area in areaList
                  orderby area.HolidayName
                  select new
                  {
                      UID = area.HolidayUID,
                      Name = area.HolidayName
                  };
            result = Ok(dep);

            return result;
        }


        [Authorize]
        [ActionName("GetHolidayAtPage")]
        [HttpGet]
        public IActionResult GetHolidayAtPage(int page, string filter, int limit)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }
            var result = _IAC_HolidayService.GetDataGrid(user.CompanyIndex, page, limit, filter);
            return ApiOk(result);
        }

        [Authorize]
        [ActionName("AddHoliday")]
        [HttpPost]
        public IActionResult AddHoliday([FromBody] AC_HolidayDTO param)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            //var checkName = context.AC_AccHoliday.Where(t => t.HolidayName.Equals(param.HolidayName)).FirstOrDefault();

            //if (checkName != null)
            //{
            //    return BadRequest("ExistHolidayName");
            //}

            var allHoliday = _DbContext.AC_AccHoliday.Select(x => x.HolidayUID).ToList();
            var listHoliday = new List<AC_AccHoliday>();
            foreach (var holidayInfo in param.DoorIndexes)
            {
                var holiday = new AC_AccHoliday();
                holiday.HolidayName = param.HolidayName;
                holiday.DoorIndex = holidayInfo;
                holiday.UpdatedDate = DateTime.Now;
                holiday.StartDate = param.StartDateString.TryGetDateTime();
                holiday.EndDate = param.EndDateString.TryGetDateTime();
                holiday.CompanyIndex = user.CompanyIndex;
                holiday.HolidayType = param.HolidayType;
                holiday.TimeZone = param.TimeZone;
                holiday.Loop = param.Loop;

                for (int i = 2; i < 50; i++)
                {
                    if (!allHoliday.Contains(i))
                    {
                        holiday.HolidayUID = i;
                        break;
                    }
                }
                listHoliday.Add(holiday);
            }

            context.AC_AccHoliday.AddRange(listHoliday);
            context.SaveChanges();

            return Ok();
        }

        [Authorize]
        [ActionName("UpdateHoliday")]
        [HttpPost]
        public IActionResult UpdateHoliday([FromBody] AC_HolidayDTO param)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }
            var updateData = context.AC_AccHoliday.Where(t => t.UID == param.UID).FirstOrDefault();
            var checkName = context.AC_AccHoliday.Where(t => t.HolidayName.Equals(param.HolidayName)).FirstOrDefault();

            if (checkName != null && checkName.UID != updateData.UID)
            {
                return BadRequest("ExistHolidayName");
            }

            updateData.HolidayName = param.HolidayName;
            updateData.DoorIndex = param.DoorIndex;
            updateData.StartDate = param.StartDateString.TryGetDateTime();
            updateData.EndDate = param.EndDateString.TryGetDateTime();
            updateData.UpdatedDate = DateTime.Now;
            updateData.CompanyIndex = user.CompanyIndex;
            updateData.HolidayType = param.HolidayType;
            updateData.Loop = param.Loop;
            updateData.TimeZone = param.TimeZone;

            if (updateData.HolidayUID == 0)
            {
                var allHoliday = _DbContext.AC_AccHoliday.Select(x => x.HolidayUID).ToList();
                for (int i = 2; i < 50; i++)
                {
                    if (!allHoliday.Contains(i))
                    {
                        updateData.HolidayUID = i;
                        break;
                    }
                }
            }

            context.SaveChanges();
            return Ok();
        }

        [Authorize]
        [ActionName("DeleteHoliday")]
        [HttpPost]
        public IActionResult DeleteHoliday([FromBody] List<AC_HolidayDTO> lsparam)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }
            var lstDataDelete = new List<AC_AccHoliday>();

            foreach (var param in lsparam)
            {
                var deleteData = context.AC_AccHoliday.Where(t => t.CompanyIndex == user.CompanyIndex && t.UID == param.UID).FirstOrDefault();
                lstDataDelete.Add(deleteData);
                if (deleteData == null)
                {
                    return NotFound("HolidayNotExist");
                }
                else
                {
                    context.AC_AccHoliday.Remove(deleteData);
                }
            }
            context.SaveChanges();

            var lstIndex = lstDataDelete.Select(x => x.DoorIndex).ToList();
            result = Ok(lstIndex);
            return result;
        }
    }
}
