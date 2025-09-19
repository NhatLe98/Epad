using EPAD_Backend_Core.Base;
using EPAD_Common.Types;
using EPAD_Data.Entities.HR;
using EPAD_Data.Models;
using EPAD_Data.Models.HR;
using EPAD_Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EPAD_Backend_Core.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class HR_PositionInfoController : ApiControllerBase
    {
        private readonly IHR_PositionInfoService _HR_PositionInfoService;

        public HR_PositionInfoController(IServiceProvider pProvider) : base(pProvider)
        {
            _HR_PositionInfoService = TryResolve<IHR_PositionInfoService>();
        }

        [Authorize]
        [ActionName("GetHRPositionInfoAtPage")]
        [HttpGet]
        public async Task<ActionResult> Get([FromQuery] string filter, [FromQuery] int page, [FromQuery] int pageSize)
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();
            List<AddedParam> addedParams = new List<AddedParam>();
            addedParams.Add(new AddedParam { Key = "Filter", Value = filter });
            addedParams.Add(new AddedParam { Key = "PageIndex", Value = page });
            addedParams.Add(new AddedParam { Key = "PageSize", Value = pageSize });
            var allEmployee = await _HR_PositionInfoService.GetPage(addedParams, user.CompanyIndex);
            return ApiOk(allEmployee);
        }

        [Authorize]
        [ActionName("Post_HR_PositionInfo")]
        [HttpPost]
        public async Task<ActionResult<HR_ClassInfoResult>> Post_HR_PositionInfo([FromBody] HR_PositionInfoResult value)
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();
            var check = await _HR_PositionInfoService.FirstOrDefaultAsync(x => x.CompanyIndex == user.CompanyIndex && x.Code == value.Code);
            if (check != null)
                return ApiError("PosistionInfoExist");
            try
            {
                HR_PositionInfo c = _Mapper.Map<HR_PositionInfoResult, HR_PositionInfo>(value);
                await _HR_PositionInfoService.InsertAsync(c);
                await SaveChangeAsync();
                return ApiOk(c);
            }
            catch (Exception ex)
            {
                return ApiError(ex.Message);
            }
        }

        [Authorize]
        [ActionName("Put_HR_PositionInfo")]
        [HttpPut("{positionIndex}")]
        public async Task<ActionResult<HR_PositionInfoResult>> Put_HR_PositionInfo(long positionIndex, [FromBody] HR_PositionInfoResult value)
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            var c = await _HR_PositionInfoService.FirstOrDefaultAsync(x => x.Index == positionIndex && x.CompanyIndex == user.CompanyIndex, true);
            try
            {
                c = _Mapper.Map(value,c);
                _HR_PositionInfoService.Update(c);
                await SaveChangeAsync();

                return ApiOk(c);
            }
            catch (Exception ex)
            {
                return ApiError(ex.Message);
            }
        }

        [Authorize]
        [ActionName("Delete_HR_PositionInfo")]
        [HttpDelete("{positionIndex}")]
        public async Task<IActionResult> Delete_HR_PositionInfo(long positionIndex)
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            try
            {
                await _HR_PositionInfoService.DeleteAsync(x => x.Index == positionIndex && x.CompanyIndex == user.CompanyIndex);
                await SaveChangeAsync();

                return ApiOk();
            }
            catch (Exception ex)
            {
                return ApiError(ex.Message);
            }
        }

        [Authorize]
        [ActionName("DeleteHRPostionInfoMulti")]
        [HttpDelete]
        public async Task<IActionResult> DeletePositionInfoMulti([FromBody] long[] listIndex)
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();
            var positionLookup = listIndex.ToHashSet();

            try
            {
                await _HR_PositionInfoService.DeleteAsync(x => positionLookup.Contains(x.Index) && x.CompanyIndex == user.CompanyIndex);
                await SaveChangeAsync();

                return ApiOk();
            }
            catch (Exception ex)
            {
                return ApiError(ex.Message);
            }
        }

        [Authorize]
        [ActionName("Get_HR_PositionInfos")]
        [HttpGet]
        public async Task<ActionResult<List<HR_PositionInfoResult>>> Get_HR_PositionInfos()
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();
            
            var allEmployee = await _HR_PositionInfoService.GetAllPositionInfo(user.CompanyIndex);
            return ApiOk(allEmployee);
        }

        [Authorize]
        [ActionName("Get_HR_PositionInfo")]
        [HttpGet("{positionIndex}")]
        public async Task<ActionResult<HR_PositionInfoResult>> Get_HR_PositionInfo(long index)
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            var positionInfo = await _HR_PositionInfoService.GetPositionInfoByIndex(index, user.CompanyIndex);
            return ApiOk(positionInfo);
        }

        [Authorize]
        [ActionName("GetPositionName")]
        [HttpGet("{index}")]
        public async Task<ActionResult<string>> GetPositionName(long index)
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            if(index <= 0) return ApiOk("");

            var position = await _HR_PositionInfoService.GetPositionInfoByIndex(index, user.CompanyIndex);
            return ApiOk(position.Name);
        }


    }
}
