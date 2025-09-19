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
    public class TA_LeaveDateTypeController : ApiControllerBase
    {
        ITA_LeaveDateTypeService _TA_LeaveDateTypeService;
        public TA_LeaveDateTypeController(IConfiguration configuration, IServiceProvider pServiceProvider) : base(pServiceProvider)
        {
            _TA_LeaveDateTypeService = TryResolve<ITA_LeaveDateTypeService>();
        }

        [Authorize]
        [ActionName("GetLeaveDateTypeAtPage")]
        [HttpGet]
        public async Task<IActionResult> GetLeaveDateTypeAtPage(int page, string filter, int limit)
        {
            UserInfo user = GetUserInfo();

            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }

            var data = await _TA_LeaveDateTypeService.GetDataGrid(user.CompanyIndex, page, limit, filter);
            return ApiOk(data);
        }

        [Authorize]
        [ActionName("GetLeaveDateTypeByCompanyIndex")]
        [HttpGet]
        public async Task<IActionResult> GetLeaveDateTypeByCompanyIndex()
        {
            UserInfo user = GetUserInfo();

            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }

            var data = await _TA_LeaveDateTypeService.GetAllLeaveDateType(user.CompanyIndex);
            return ApiOk(data);
        }

        [Authorize]
        [ActionName("GetLeaveDateTypeByIndex")]
        [HttpGet]
        public async Task<IActionResult> GetLeaveDateTypeByIndex(int LeaveDateTypeIndex)
        {
            UserInfo user = GetUserInfo();

            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }

            var data = await _TA_LeaveDateTypeService.GetLeaveDateTypeByIndex(LeaveDateTypeIndex);
            return ApiOk(data);
        }

        [Authorize]
        [ActionName("AddLeaveDateType")]
        [HttpPost]
        public async Task<IActionResult> AddLeaveDateType([FromBody] TA_LeaveDateType data)
        {
            UserInfo user = GetUserInfo();

            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }

            data.UpdatedUser = user.FullName;
            data.UpdatedDate = DateTime.Now;

            var nameExist = await _TA_LeaveDateTypeService.GetLeaveDateTypeByName(data.Name, user.CompanyIndex);
            if (nameExist != null && nameExist.Count > 0)
            {
                return ApiError("NameExisted");
            }

            var codeExist = await _TA_LeaveDateTypeService.GetLeaveDateTypeByCode(data.Code, user.CompanyIndex);
            if (codeExist != null && codeExist.Count > 0)
            {
                return ApiError("CodeExisted");
            }

            var isSuccess = await _TA_LeaveDateTypeService.AddLeaveDateType(data);

            return ApiOk(isSuccess);
        }

        [Authorize]
        [ActionName("UpdateLeaveDateType")]
        [HttpPost]
        public async Task<IActionResult> UpdateLeaveDateType([FromBody] TA_LeaveDateType data)
        {
            UserInfo user = GetUserInfo();
            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }

            data.UpdatedUser = user.FullName;
            data.UpdatedDate = DateTime.Now;

            var nameExist = await _TA_LeaveDateTypeService.GetLeaveDateTypeByName(data.Name, user.CompanyIndex);
            if (nameExist != null && nameExist.Count > 0 && nameExist.Any(x => x.Index != data.Index))
            {
                return ApiError("NameExisted");
            }

            var codeExist = await _TA_LeaveDateTypeService.GetLeaveDateTypeByCode(data.Code, user.CompanyIndex);
            if (codeExist != null && codeExist.Count > 0 && codeExist.Any(x => x.Index != data.Index))
            {
                return ApiError("CodeExisted");
            }

            var dataExist = await _TA_LeaveDateTypeService.GetLeaveDateTypeByIndex(data.Index);
            if (dataExist == null)
            {
                return ApiError("LeaveDateTypeNotExist");
            }

            var isSuccess = await _TA_LeaveDateTypeService.UpdateLeaveDateType(data);

            return ApiOk(isSuccess);
        }

        [Authorize]
        [ActionName("DeleteLeaveDateType")]
        [HttpDelete]
        public async Task<IActionResult> DeleteLeaveDateType(List<int> index)
        {
            UserInfo user = GetUserInfo();

            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }

            var isLeaveDateTypeUsing = await _TA_LeaveDateTypeService.IsLeaveDateTypeUsing(index);
            if (isLeaveDateTypeUsing)
            {
                return ApiError("LeaveDateTypeIsUsing");
            }

            var isSuccess = await _TA_LeaveDateTypeService.DeleteLeaveDateType(index);

            return ApiOk(isSuccess);
        }
    }
}
