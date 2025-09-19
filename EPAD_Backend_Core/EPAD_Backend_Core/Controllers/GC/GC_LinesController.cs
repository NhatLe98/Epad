using EPAD_Backend_Core.Base;
using EPAD_Common.Types;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using EPAD_Data.Models.Other;
using EPAD_Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EPAD_Backend_Core.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class GC_LinesController : ApiControllerBase
    {
        private IMemoryCache cache;
        IGC_LinesService _GC_LinesService;
        IGC_Lines_CheckInDeviceService _GC_Lines_CheckInDeviceService;
        IGC_Lines_CheckOutDeviceService _GC_Lines_CheckOutDeviceService;
        IIC_DeviceService _IC_DeviceService;
        public GC_LinesController(IServiceProvider pProvider) : base(pProvider)
        {
            cache = TryResolve<IMemoryCache>();
            _GC_LinesService = TryResolve<IGC_LinesService>();
            _IC_DeviceService = TryResolve<IIC_DeviceService>();
            _GC_Lines_CheckInDeviceService = TryResolve<IGC_Lines_CheckInDeviceService>();
            _GC_Lines_CheckOutDeviceService = TryResolve<IGC_Lines_CheckOutDeviceService>();
        }

        [Authorize]
        [ActionName("GetAll")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            UserInfo user = GetUserInfo();

            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }

            var grid = await _GC_LinesService.GetAllData(user.CompanyIndex);
            return ApiOk(grid);
        }

        [Authorize]
        [ActionName("GetAllDevicesLines")]
        [HttpGet]
        public async Task<IActionResult> GetAllDevicesLines()
        {
            UserInfo user = GetUserInfo();

            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }
            var listDevicePrivilege = await _IC_DeviceService.GetAllWithPrivilegeFull(user);
            var listSerials = listDevicePrivilege.Select(x => new SerialNumberParam
            {
                AliasName = x.AliasName,
                SerialNumber = x.SerialNumber,
                IPAddress = x.IPAddress
            }).ToList();
            var listLineDeviceIn = await _GC_Lines_CheckInDeviceService.GetDataByCompanyIndex(user.CompanyIndex);
            var listIn = listLineDeviceIn.Select(x 
                => GetDeviceLineObject(x.LineIndex, x.CheckInDeviceSerial, listSerials)).ToList();
            var listLineDeviceOut = await _GC_Lines_CheckOutDeviceService.GetDataByCompanyIndex(user.CompanyIndex);
            var listOut = listLineDeviceOut.Select(x 
                => GetDeviceLineObject(x.LineIndex, x.CheckOutDeviceSerial, listSerials)).ToList();
            listIn.AddRange(listOut);
            listIn = listIn.Distinct().ToList();

            return ApiOk(listIn);
        }

        private DeviceLineBasicInfo GetDeviceLineObject(int lineIndex, string serialNumber, List<SerialNumberParam> listSerials)
        {
            var checkExist = listSerials.Where(t => t.SerialNumber.Equals(serialNumber)).FirstOrDefault();
            return new DeviceLineBasicInfo()
            {
                Index = lineIndex,
                SerialNumber = serialNumber,
                Name = checkExist != null ? checkExist.AliasName : "Deleted"
            };
        }

        [Authorize]
        [ActionName("GetAllLine")]
        [HttpGet]
        public async Task<IActionResult> GetAllLine()
        {
            UserInfo user = GetUserInfo();

            var result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }

            var data = await _GC_LinesService.GetDataByCompanyIndex(user.CompanyIndex);
            return ApiOk(data);
        }

        [Authorize]
        [ActionName("GetAllLineBasic")]
        [HttpGet]
        public async Task<IActionResult> GetAllLineBasic()
        {
            UserInfo user = GetUserInfo();

            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }
            var lineData = await _GC_LinesService.GetDataByCompanyIndex(user.CompanyIndex);
            List<LineBasicInfo> listLine = lineData.Select(x => new LineBasicInfo()
            {
                Index = x.Index,
                Name = x.Name
            }
            ).ToList();

            return ApiOk(listLine);
        }

        [Authorize]
        [ActionName("GetLineBySerialNumber")]
        [HttpGet]
        public async Task<IActionResult> GetLineBySerialNumber(string serialNumber)
        {
            UserInfo user = GetUserInfo();

            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }

            var line = await _GC_LinesService.GetLineBySerialNumber(serialNumber);

            return ApiOk(line);
        }

        [Authorize]
        [ActionName("AddLine")]
        [HttpPost]
        public async Task<IActionResult> AddLine([FromBody] LinesParam param)
        {
            UserInfo user = GetUserInfo();

            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }

            if (param.Name.Trim() == "")
            {
                return ApiError("PleaseFillAllRequiredFields");
            }
            var checkData = await _GC_LinesService.GetDataByNameAndCompanyIndex(param.Name, user.CompanyIndex);
            if (checkData != null)
            {
                return ApiError("LineIsExisted");
            }
            var isAddSuccess = await _GC_LinesService.AddLine(param, user);

            return ApiOk(isAddSuccess);
        }

        [Authorize]
        [ActionName("UpdateLine")]
        [HttpPut]
        public async Task<IActionResult> UpdateLine([FromBody] LinesParam param)
        {
            UserInfo user = GetUserInfo();

            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }

            if (param.Name.Trim() == "")
            {
                return ApiError("PleaseFillAllRequiredFields");
            }
            var checkData = await _GC_LinesService.GetDataByNameAndCompanyIndex(param.Name, user.CompanyIndex);
            if (checkData != null && checkData.Index != param.Index)
            {
                return ApiError("LineIsExisted");
            }
            var updateData = await _GC_LinesService.GetDataByIndex(param.Index);
            if (updateData == null)
            {
                return ApiError("LineNotExist");
            }
            //var checkData = await _GC_LinesService.GetDataByNameAndCompanyIndex(param.Name, user.CompanyIndex);
            //if (checkData != null && checkData.Index != param.Index)
            //{
            //    return ApiError("LineIsExisted");
            //}

            var isUpdateSuccess = await _GC_LinesService.UpdateLine(param, user);

            return ApiOk(isUpdateSuccess);
        }

        [Authorize]
        [ActionName("DeleteLines")]
        [HttpDelete]
        public async Task<IActionResult> DeleteLines([FromBody] List<int> listIndex)
        {
            UserInfo user = GetUserInfo();
            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }

            var isGateUsing = await _GC_LinesService.CheckLineUsing(listIndex, user.CompanyIndex);
            if (isGateUsing)
            {
                return ApiConflict("LinesIsUsing");
            }

            var deleteErrorMessage = await _GC_LinesService.TryDeleteLine(listIndex, user.CompanyIndex);
            return ApiOk(deleteErrorMessage);
        }
    }
}
