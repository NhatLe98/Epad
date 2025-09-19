using EPAD_Backend_Core.Base;
using EPAD_Background.Schedule.Job;
using EPAD_Common.Extensions;
using EPAD_Common.FileProvider;
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
    public class EzPortalFileController : ApiControllerBase
    {
        private IMemoryCache cache;
        private readonly IStoreFileProvider _FileHandler;
        private readonly CheckWarningViolation _CheckWarningViolation;
        public EzPortalFileController(IServiceProvider pProvider) : base(pProvider)
        {
            cache = TryResolve<IMemoryCache>();
            _FileHandler = TryResolve<IStoreFileProvider>();
            _CheckWarningViolation = TryResolve<CheckWarningViolation>();
        }

        [Authorize]
        [ActionName("GetFile")]
        [HttpPost]
        public IActionResult GetFile([FromBody] EzFile request)
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            try
            {
                return new FileContentResult(FileHelper.ReadAndDecodeToBytes(_FileHandler.getPath(request.Url + request.Name, user.CompanyIndex)), "application/octet-stream");
            }
            catch
            {
                return ApiError("FileNotFound");
            }
        }

        [Authorize]
        [ActionName("GetFilePath")]
        [HttpPost]
        public IActionResult GetFilePath([FromBody] EzFile request)
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            try
            {
                var data = FileHelper.ReadAndDecodeToBytes(_FileHandler.getPath(request.Url + request.Name, user.CompanyIndex));
                return ApiOk(data);
            }
            catch
            {
                return ApiError("FileNotFound");
            }
        }

        [Authorize]
        [ActionName("ExportExcelVehicleMonitoringHistory")]
        [HttpPost]
        public async Task<IActionResult> ExportExcelVehicleMonitoringHistory(ExportExcelModel param)
        {
            string filename = "VehicleMonitoringHistory_" + DateTime.Now.ToFileTime() + ".xlsx";
            var stream = await _CheckWarningViolation.CreateExcelByVehicleMOnitoringHistory(param.Data, param.LogType, param.Status);

            //return new FileContentResult(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
            return ApiOk(new ExportExcelResponse() { Byte = stream.ToArray(), FileName = filename });

            //return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", filename);
        }
    }
}
