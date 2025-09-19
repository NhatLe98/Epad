using EPAD_Backend_Core.Base;
using EPAD_Common.Types;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using EPAD_Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static EPAD_Common.Utility.AppUtils;

namespace EPAD_Backend_Core.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class TA_HolidayController : ApiControllerBase
    {
        ITA_HolidayService _TA_HolidayService;
        public TA_HolidayController(IConfiguration configuration, IServiceProvider pServiceProvider) : base(pServiceProvider)
        {
            _TA_HolidayService = TryResolve<ITA_HolidayService>();
        }

        [Authorize]
        [ActionName("GetHolidayAtPage")]
        [HttpGet]
        public async Task<IActionResult> GetHolidayAtPage(int page, string filter, int limit)
        {
            UserInfo user = GetUserInfo();

            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }

            var data = await _TA_HolidayService.GetDataGrid(user.CompanyIndex, page, limit, filter);
            return ApiOk(data);
        }

        [Authorize]
        [ActionName("GetHolidayByCompanyIndex")]
        [HttpGet]
        public async Task<IActionResult> GetHolidayByCompanyIndex()
        {
            UserInfo user = GetUserInfo();

            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }

            var data = await _TA_HolidayService.GetAllHoliday(user.CompanyIndex);
            return ApiOk(data);
        }

        [Authorize]
        [ActionName("GetHolidayByIndex")]
        [HttpGet]
        public async Task<IActionResult> GetHolidayByIndex(int HolidayIndex)
        {
            UserInfo user = GetUserInfo();

            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }

            var data = await _TA_HolidayService.GetHolidayByIndex(HolidayIndex);
            return ApiOk(data);
        }

        [Authorize]
        [ActionName("AddHoliday")]
        [HttpPost]
        public async Task<IActionResult> AddHoliday([FromBody] TA_HolidayDTO data)
        {
            UserInfo user = GetUserInfo();

            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }

            var format = "yyyy-MM-dd";
            if (DateTime.TryParseExact(data.HolidayDateString, format, null,
                        System.Globalization.DateTimeStyles.None, out DateTime date))
            {
                data.HolidayDate = date;
            }
            else
            {
                return ApiError("DateWrongFormat");
            }

            data.UpdatedUser = user.FullName;
            data.UpdatedDate = DateTime.Now;

            var nameExist = await _TA_HolidayService.GetHolidayByName(data.Name, user.CompanyIndex);
            if (nameExist != null && nameExist.Count > 0)
            {
                return ApiError("NameExisted");
            }

            var codeExist = await _TA_HolidayService.GetHolidayByCode(data.Code, user.CompanyIndex);
            if (codeExist != null && codeExist.Count > 0)
            {
                return ApiError("CodeExisted");
            }

            var isSuccess = await _TA_HolidayService.AddHoliday(data);

            return ApiOk(isSuccess);
        }

        [Authorize]
        [ActionName("UpdateHoliday")]
        [HttpPost]
        public async Task<IActionResult> UpdateHoliday([FromBody] TA_HolidayDTO data)
        {
            UserInfo user = GetUserInfo();
            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }

            var format = "yyyy-MM-dd";
            if (DateTime.TryParseExact(data.HolidayDateString, format, null,
                        System.Globalization.DateTimeStyles.None, out DateTime date))
            {
                data.HolidayDate = date;
            }
            else
            {
                return ApiError("DateWrongFormat");
            }

            data.UpdatedUser = user.FullName;
            data.UpdatedDate = DateTime.Now;

            var nameExist = await _TA_HolidayService.GetHolidayByName(data.Name, user.CompanyIndex);
            if (nameExist != null && nameExist.Count > 0 && nameExist.Any(x => x.Index != data.Index))
            {
                return ApiError("NameExisted");
            }

            var codeExist = await _TA_HolidayService.GetHolidayByCode(data.Code, user.CompanyIndex);
            if (codeExist != null && codeExist.Count > 0 && codeExist.Any(x => x.Index != data.Index))
            {
                return ApiError("CodeExisted");
            }

            var dataExist = await _TA_HolidayService.GetHolidayByIndex(data.Index);
            if (dataExist == null)
            {
                return ApiError("HolidayNotExist");
            }

            var isSuccess = await _TA_HolidayService.UpdateHoliday(data);

            return ApiOk(isSuccess);
        }

        [Authorize]
        [ActionName("DeleteHoliday")]
        [HttpDelete]
        public async Task<IActionResult> DeleteHoliday(List<int> index)
        {
            UserInfo user = GetUserInfo();

            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }

            var isSuccess = await _TA_HolidayService.DeleteHoliday(index);

            return ApiOk(isSuccess);
        }
    }
}
