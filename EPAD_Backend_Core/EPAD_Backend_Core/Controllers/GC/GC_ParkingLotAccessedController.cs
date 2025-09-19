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
    public class GC_ParkingLotAccessedController : ApiControllerBase
    {
        private IMemoryCache cache;
        private readonly IHostingEnvironment _hostingEnvironment;
        IHR_UserService _HR_UserService;
        IGC_ParkingLotService _GC_ParkingLotService;
        IGC_ParkingLotAccessedService _GC_ParkingLotAccessedService;
        public GC_ParkingLotAccessedController(IServiceProvider pProvider) : base(pProvider)
        {
            cache = TryResolve<IMemoryCache>();
            _hostingEnvironment = TryResolve<IHostingEnvironment>();
            _HR_UserService = TryResolve<IHR_UserService>();
            _GC_ParkingLotService = TryResolve<IGC_ParkingLotService>();
            _GC_ParkingLotAccessedService = TryResolve<IGC_ParkingLotAccessedService>();
        }

        [Authorize]
        [ActionName("GetByFilter")]
        [HttpPost]
        public async Task<IActionResult> GetByFilter(ParkingLotAccessedRequest param)
        {
            UserInfo user = GetUserInfo();

            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }
            var grid = await _GC_ParkingLotAccessedService.GetByFilter(param.accessType, param.parkingLotIndex,
                param.fromDate, param.toDate, param.filter, param.page, param.pageSize);

            return ApiOk(grid);
        }

        [Authorize]
        [ActionName("AddParkingLotAccessed")]
        [HttpPost]
        public async Task<IActionResult> AddParkingLotAccessed(ParkingLotAccessedParams param)
        {
            UserInfo user = GetUserInfo();

            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }

            if (!string.IsNullOrEmpty(param.CustomerIndex) && param.CustomerIndex != "empty")
            {
                var parkingLotAccessed = await _GC_ParkingLotAccessedService.GetByFilterCustomerAndCompanyIndex(param.AccessType, 
                    param.ParkingLotIndex,
                    param.CustomerIndex, param.FromDate, param.ToDate, user.CompanyIndex);
                if (parkingLotAccessed != null)
                {
                    return ApiConflict("ParkingLotAccessedDateOverlap");
                }
            }
            else
            {
                //var listEmployeeATID = param.EmployeeATID.Split(",").ToList();
                var listEmployeeATID = param.EmployeeATIDs;
                var parkingLotAccessed = _GC_ParkingLotAccessedService
                    .GetByFilterEmployeeAndCompanyIndex(param.AccessType, param.ParkingLotIndex,
                        listEmployeeATID, param.FromDate, param.ToDate, user.CompanyIndex).Result;
                if (parkingLotAccessed != null && parkingLotAccessed.Count > 0)
                {
                    return ApiConflict("ParkingLotAccessedDateOverlap");
                }
            }

            var isSuccess = await _GC_ParkingLotAccessedService.AddParkingLotAccessed(param, user);

            return ApiOk(isSuccess);
        }

        [Authorize]
        [ActionName("UpdateParkingLotAccessed")]
        [HttpPut]
        public async Task<IActionResult> UpdateParkingLotAccessed(ParkingLotAccessedParams param)
        {
            UserInfo user = GetUserInfo();

            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }

            var isExist = await _GC_ParkingLotAccessedService
                .GetByFilterAndCompanyIndexExcludeThis(param.AccessType, param.ParkingLotIndex,
                param.EmployeeATIDs[0], param.FromDate, param.ToDate, param.Index, user.CompanyIndex);
            if (isExist)
            {
                return ApiConflict("ParkingLotAccessedDateOverlap");
            }

            var parkingLotAccessed = await _GC_ParkingLotAccessedService.GetDataByIndex(param.Index);
            if (parkingLotAccessed == null)
            {
                return ApiConflict("ParkingLotAccessedNotExist");
            }

            var isSuccess = await _GC_ParkingLotAccessedService.UpdateParkingLotAccessed(param, user);

            return ApiOk(isSuccess);
        }


        [Authorize]
        [ActionName("DeleteParkingLotAccessed")]
        [HttpDelete]
        public async Task<IActionResult> DeleteParkingLotAccessed([FromBody] List<long> listIndex)
        {
            UserInfo user = GetUserInfo();
            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }

            var isSuccess = await _GC_ParkingLotAccessedService.DeleteParkingLotAccessed(listIndex);

            return ApiOk(isSuccess);
        }

        [Authorize]
        [ActionName("ImportParkingLotAccessed")]
        [HttpPost]
        public async Task<IActionResult> ImportParkingLotAccessed(List<ParkingLotAccessedParams> param)
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            var result = true;

            var importResult = await _GC_ParkingLotAccessedService.ImportParkingLotAccessed(param, user);

            if (importResult != null && importResult.Any(x => !string.IsNullOrWhiteSpace(x.ErrorMessage)))
            {
                string sWebRootFolder = _hostingEnvironment.ContentRootPath;
                string URL = string.Format("{0}://{1}/{2}", Request.Scheme, Request.Host, @"Files/Template_EmployeeParkingLot_Error.xlsx");
                FileInfo file = new FileInfo(Path.Combine(sWebRootFolder, @"Files/Template_EmployeeParkingLot_Error.xlsx"));

                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("DataError");
                    var currentRow = 1;
                    worksheet.Cell(currentRow, 1).Value = "MCC (*)";
                    worksheet.Cell(currentRow, 2).Value = "Loại nhân viên (*)";
                    worksheet.Cell(currentRow, 3).Value = "Nhân viên";
                    worksheet.Cell(currentRow, 4).Value = "Nhà xe (*)";
                    worksheet.Cell(currentRow, 5).Value = "Từ ngày (*)";
                    worksheet.Cell(currentRow, 6).Value = "Đến ngày";
                    worksheet.Cell(currentRow, 7).Value = "Mô tả";
                    worksheet.Cell(currentRow, 8).Value = "Lỗi";

                    for (int i = 1; i <= 8; i++)
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

                        worksheet.Cell(currentRow, 2).Value = importRow.AccessType == 0 ? "Nhân viên" : "Khách";
                        worksheet.Cell(currentRow, 2).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                        worksheet.Cell(currentRow, 3).Value = importRow.FullName;
                        worksheet.Cell(currentRow, 3).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                        worksheet.Cell(currentRow, 4).Value = importRow.ParkingLot;
                        worksheet.Cell(currentRow, 4).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                        worksheet.Cell(currentRow, 5).Value = importRow.FromDateString;
                        worksheet.Cell(currentRow, 5).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                        worksheet.Cell(currentRow, 6).Value = importRow.ToDateString;
                        worksheet.Cell(currentRow, 6).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                        worksheet.Cell(currentRow, 7).Value = importRow.Description;
                        worksheet.Cell(currentRow, 7).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                        worksheet.Cell(currentRow, 8).Value = importRow.ErrorMessage;
                        worksheet.Cell(currentRow, 8).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    }
                    workbook.SaveAs(file.FullName);
                }

                result = false;
            }

            return ApiOk(result);

            //var errors = new ImportErrors();
            //this._GC_DbContext.Database.BeginTransaction();
            //var dataImport = request.Data;
            //var employeeIDs = dataImport.Select(x => x.EmployeeATID).ToList();
            //var employeeList = await _epadClient.GetEmployeeCompactInfoByEmployeeATID(employeeIDs);
            //var parkingLotList = _GC_DbContext.GC_ParkingLot.ToList();
            //var parkingLotAccessedList = _GC_DbContext.GC_ParkingLotAccessed.Where(x => employeeIDs.Contains(x.EmployeeATID)).ToList();
            //foreach (var item in dataImport)
            //{
            //    int i = 0;
            //    bool isAdd = true;
            //    bool hasError = false;
            //    var parkingName = item.ParkingLotName.Trim();
            //    var employee = employeeList.FirstOrDefault(x => x.EmployeeATID == item.EmployeeATID);
            //    if (employee == null)
            //    {
            //        errors.AddMeta(i, "Not-Found", "EmployeeATID", item.EmployeeATID, "EmployeeNotExist");
            //        hasError = true;
            //    }
            //    if (item.ToDate.HasValue && item.ToDate.Value <= item.FromDate)
            //    {
            //        errors.AddMeta(i, "Compare-Date", "ToDate", item.ToDate.Value.ToShortDateString(), "MSG_FromDateLessThanToDate");
            //        hasError = true;
            //    }
            //    var parkingLot = parkingLotList.FirstOrDefault(x => x.Name.Trim().ToLower() == parkingName.ToLower());
            //    if (parkingLot == null)
            //    {
            //        errors.AddMeta(i, "Not-Found", "ParkingLotName", item.ParkingLotName, "ParkingLotNameNotExist");
            //        hasError = true;
            //    }

            //    if (hasError) continue;

            //    var parkingLotAccessed = _GC_ParkingLotAccessedService
            //        .GetParkingLotAccessed(parkingLotAccessedList, item.AccessType, parkingLot.Index, employee.EmployeeATID, item.FromDate, item.ToDate, user.CompanyIndex);



            //    if (parkingLotAccessed != null)
            //    {
            //        isAdd = false;
            //        if (!request.OverwriteExist)
            //        {
            //            continue;
            //        }
            //        if (request.OverwriteExist && !request.IgnoreEmpty)
            //        {
            //            parkingLotAccessed.FromDate = item.FromDate;
            //            parkingLotAccessed.ToDate = item.ToDate;
            //            parkingLotAccessed.Description = item.Description;
            //        }
            //    }
            //    else
            //    {
            //        parkingLotAccessed = new GC_ParkingLotAccessed
            //        {
            //            AccessType = item.AccessType,
            //            EmployeeATID = employee.EmployeeATID,
            //            CustomerIndex = "",
            //            ParkingLotIndex = parkingLot.Index,
            //            FromDate = item.FromDate,
            //            ToDate = item.ToDate,
            //            Description = item.Description,
            //            CompanyIndex = user.CompanyIndex,
            //            UpdatedDate = DateTime.Now,
            //            UpdatedUser = user.UserName
            //        };
            //    }

            //    if (isAdd)
            //    {
            //        _GC_ParkingLotAccessedService.InsertWithDefaultValue(parkingLotAccessed);
            //    }
            //    else
            //    {
            //        _GC_ParkingLotAccessedService.Update(parkingLotAccessed);
            //    }
            //    await SaveChangeAsync();

            //}
            //if (errors.Errors.Count > 0)
            //{
            //    this._GC_DbContext.Database.RollbackTransaction();
            //    return ApiError("ImportError", errors.Errors);
            //}
            //else
            //{
            //    this._GC_DbContext.Database.CommitTransaction();
            //    return ApiOk();
            //}
        }
    }
}