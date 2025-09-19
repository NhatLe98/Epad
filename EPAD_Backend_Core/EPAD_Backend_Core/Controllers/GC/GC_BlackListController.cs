using ClosedXML.Excel;
using EPAD_Backend_Core.Base;
using EPAD_Common.Types;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using EPAD_Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace EPAD_Backend_Core.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class GC_BlackListController : ApiControllerBase
    {
        private IMemoryCache cache;
        private readonly IHostingEnvironment _hostingEnvironment;
        IHR_UserService _HR_UserService;
        IGC_ParkingLotService _GC_ParkingLotService;
        IGC_BlackListService _GC_BlackListService;
        public GC_BlackListController(IServiceProvider pProvider) : base(pProvider)
        {
            cache = TryResolve<IMemoryCache>();
            _hostingEnvironment = TryResolve<IHostingEnvironment>();
            _HR_UserService = TryResolve<IHR_UserService>();
            _GC_ParkingLotService = TryResolve<IGC_ParkingLotService>();
            _GC_BlackListService = TryResolve<IGC_BlackListService>();
        }

        [Authorize]
        [ActionName("GetByFilter")]
        [HttpPost]
        public async Task<IActionResult> GetByFilter(BlackListFilter param)
        {
            UserInfo user = GetUserInfo();

            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }
            var grid = await _GC_BlackListService.GetByFilter(param.FromDate, param.ToDate, param.Filter, param.Page, param.PageSize, user.CompanyIndex);

            return ApiOk(grid);
        }

        [Authorize]
        [ActionName("AddBlackList")]
        [HttpPost]
        public async Task<IActionResult> AddBlackList(BlackListParams param)
        {
            UserInfo user = GetUserInfo();

            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }

            var parkingLotAccessed = _GC_BlackListService
                .CheckExistBlackList(param.IsEmployeeSystem, param.EmployeeATID, param.Nric, param.FromDate, param.ToDate, user.CompanyIndex);

            if (parkingLotAccessed != null && parkingLotAccessed.Count > 0)
            {
                return ApiConflict("BlackListDateOverlap");
            }

            var isSuccess = await _GC_BlackListService.AddBlackList(param, user);

            if(isSuccess != null)
            {
                await _GC_BlackListService.CreateCommandBlacklist(new List<long> { isSuccess.Index }, user);
            }

            return ApiOk(isSuccess);
        }

        [Authorize]
        [ActionName("UpdateBlackList")]
        [HttpPut]
        public async Task<IActionResult> UpdateBlackList(BlackListParams param)
        {
            UserInfo user = GetUserInfo();

            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }

            var isExist = _GC_BlackListService
                .GetByFilterAndCompanyIndexExcludeThis(param.IsEmployeeSystem, param.EmployeeATID, param.Nric, param.FromDate, param.ToDate, param.Index, user.CompanyIndex);
            if (isExist)
            {
                return ApiConflict("BlackListDateOverlap");
            }

            var parkingLotAccessed = await _GC_BlackListService.GetDataByIndex(param.Index);
            if (parkingLotAccessed == null)
            {
                return ApiConflict("BlackListNotExist");
            }

            var isSuccess = await _GC_BlackListService.UpdateBlackList(param, user);

            if (isSuccess != null)
            {
                await _GC_BlackListService.CreateCommandBlacklist(new List<long> { param.Index }, user);
            }

            return ApiOk(isSuccess);
        }


        [Authorize]
        [ActionName("DeleteBlackList")]
        [HttpDelete]
        public async Task<IActionResult> DeleteBlackList([FromBody] List<long> listIndex)
        {
            UserInfo user = GetUserInfo();
            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }

            await _GC_BlackListService.DeleteBlackListByCreateCommand(listIndex, user);

            var isSuccess = await _GC_BlackListService.DeleteBlackList(listIndex);

            return ApiOk(isSuccess);
        }


        [Authorize]
        [ActionName("RemoveEmployeeInBlackList")]
        [HttpPost]
        public async Task<IActionResult> RemoveEmployeeInBlackList(RemoveEmployeeInBlackListParam param)
        {
            UserInfo user = GetUserInfo();

            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }

            var parkingLotAccessed = await _GC_BlackListService.GetDataByIndex(param.Index);
            if (parkingLotAccessed == null)
            {
                return ApiConflict("BlackListNotExist");
            }

            var isExist = _GC_BlackListService
                .GetByFilterAndCompanyIndexExcludeThis(parkingLotAccessed.IsEmployeeSystem, parkingLotAccessed.EmployeeATID, parkingLotAccessed.Nric, parkingLotAccessed.FromDate, param.ToDate, param.Index, user.CompanyIndex);
            if (isExist)
            {
                return ApiConflict("BlackListDateOverlap");
            }
            var isSuccess = await _GC_BlackListService.RemoveEmployeeInBlackList(param, user);
            await _GC_BlackListService.CreateCommandBlacklist(new List<long> { param.Index },user);

            return ApiOk(isSuccess);
        }



        [Authorize]
        [ActionName("ImportBlackList")]
        [HttpPost]
        public async Task<IActionResult> ImportBlackList(List<BlackListParams> param)
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            var message = "";

            var importResult = await _GC_BlackListService.ImportBlackList(param, user);


            if (importResult != null && importResult.Any(x => !string.IsNullOrWhiteSpace(x.ErrorMessage)))
            {
                message = importResult.Count().ToString();
                string sWebRootFolder = _hostingEnvironment.ContentRootPath;
                string URL = string.Format("{0}://{1}/{2}", Request.Scheme, Request.Host, @"Files/Template_BlackList_Error.xlsx");
                FileInfo file = new FileInfo(Path.Combine(sWebRootFolder, @"Files/Template_BlackList_Error.xlsx"));

                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("DataError");
                    var currentRow = 1;
                    worksheet.Cell(currentRow, 1).Value = "Mã người dùng (*)";
                    worksheet.Cell(currentRow, 2).Value = "CMND/CCCD/Passport (*)";
                    worksheet.Cell(currentRow, 3).Value = "Họ tên";
                    worksheet.Cell(currentRow, 4).Value = "Từ ngày (*)";
                    worksheet.Cell(currentRow, 5).Value = "Đến ngày";
                    worksheet.Cell(currentRow, 6).Value = "Lý do";
                    worksheet.Cell(currentRow, 7).Value = "Lỗi";

                    for (int i = 1; i < 8; i++)
                    {
                        worksheet.Cell(1, i).Style.Fill.BackgroundColor = XLColor.Yellow;
                        worksheet.Cell(1, i).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        worksheet.Column(i).Width = 20;
                    }

                    var errorResult = importResult.Where(x => !string.IsNullOrWhiteSpace(x.ErrorMessage)).ToList();

                    foreach (var importRow in errorResult)
                    {
                        currentRow++;
                        //New template
                        worksheet.Cell(currentRow, 1).Value = "'" + importRow.EmployeeATID;
                        worksheet.Cell(currentRow, 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                        worksheet.Cell(currentRow, 2).Value = "'" + importRow.Nric;
                        worksheet.Cell(currentRow, 2).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                        worksheet.Cell(currentRow, 3).Value = importRow.FullName;
                        worksheet.Cell(currentRow, 3).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                        worksheet.Cell(currentRow, 4).Value = importRow.FromDateString;
                        worksheet.Cell(currentRow, 4).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                        worksheet.Cell(currentRow, 5).Value = importRow.ToDateString;
                        worksheet.Cell(currentRow, 5).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                        worksheet.Cell(currentRow, 6).Value = importRow.Reason;
                        worksheet.Cell(currentRow, 6).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                        worksheet.Cell(currentRow, 7).Value = importRow.ErrorMessage;
                        worksheet.Cell(currentRow, 7).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    }
                    workbook.SaveAs(file.FullName);
                }

            }

            return Ok(message);
        }
    }
}