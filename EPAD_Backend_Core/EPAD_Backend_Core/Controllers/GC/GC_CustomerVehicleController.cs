using ClosedXML.Excel;
using EPAD_Backend_Core.Base;
using EPAD_Common.Enums;
using EPAD_Common.Types;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using EPAD_Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Hosting;
using ServiceStack.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace EPAD_Backend_Core.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class GC_CustomerVehicleController : ApiControllerBase
    {
        protected readonly IGC_CustomerVehicleService _GC_CustomerVehicleService;
        private readonly IHostingEnvironment _hostingEnvironment;
        public GC_CustomerVehicleController(IServiceProvider pServiceProvider) : base(pServiceProvider)
        {
            _hostingEnvironment = TryResolve<IHostingEnvironment>();
            _GC_CustomerVehicleService = TryResolve<IGC_CustomerVehicleService>();
        }

        [Authorize]
        [ActionName("GetCustomerVehicleAll")]
        [HttpGet]
        public async Task<IActionResult> GetCustomerVehicle(string filter, int pageIndex, int pageSize)
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            var data = await _GC_CustomerVehicleService.GetCustomerVehicleByFilter(filter, pageIndex, pageSize);
            return ApiOk(data);
        }

        [Authorize]
        [ActionName("GetCustomerVehicleByFilter")]
        [HttpPost]
        public async Task<IActionResult> GetCustomerVehicleByFilter([FromBody] CustomerVehicleRequest param)
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            var data = await _GC_CustomerVehicleService.GetCustomerVehicleByFilterAdvance(param);
            return ApiOk(data);
        }

        [Authorize]
        [ActionName("GetCustomerVehicle")]
        [HttpGet]
        public async Task<IActionResult> GetCustomerVehicleByEmployee(int index)
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            var data = await _GC_CustomerVehicleService.FirstOrDefaultAsync(x => x.CompanyIndex == user.CompanyIndex && x.Index == index);
            return ApiOk(data);
        }

        [Authorize]
        [ActionName("AddCustomerVehicle")]
        [HttpPost]
        public async Task<IActionResult> AddCustomerVehicleAsync(CustomerVehicleRequest request)
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            var check = await _GC_CustomerVehicleService.GetByEmpAndPlate(request.EmployeeATID, request.Plate);
            if (check != null)
            {
                return ApiError("MSG_DataExisted");
            }

            var result = await _GC_CustomerVehicleService.AddCustomerVehicle(request, user);
            return ApiOk(result);
        }

        [Authorize]
        [ActionName("UpdateCustomerVehicle")]
        [HttpPut]
        public async Task<IActionResult> UpdateCustomerVehicle([FromBody] GC_CustomerVehicle request)
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            var checkExist = await _GC_CustomerVehicleService.GetByIndex(request.Index);
            if (checkExist == null) return ApiError("MSG_ObjectNotExisted");

            var checkPlateUsing = await _GC_CustomerVehicleService.GetByEmpAndPlateByUpdate(request.Index, request.EmployeeATID, request.Plate);
            if (checkPlateUsing != null)
            {
                return ApiError("MSG_DataExisted");
            }

            var result = await _GC_CustomerVehicleService.UpdateCustomerVehicle(request, user);
            return ApiOk(result);

        }

        [Authorize]
        [ActionName("DeleteCustomerVehicle")]
        [HttpDelete]
        public async Task<IActionResult> DeleteCustomerVehicle([FromBody] List<int> listIndex)
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            var result = await _GC_CustomerVehicleService.DeleteCustomerVehicle(listIndex);
            return ApiOk(result);
        }


        [Authorize]
        [ActionName("ImportCustomerVehicle")]
        [HttpPost]
        public async Task<IActionResult> ImportCustomerVehicleAsync([FromBody] List<CustomerVehicleImportData> request)
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            var result = true;

            var importResult = await _GC_CustomerVehicleService.ImportCustomerVehicle(request, user);
            if (importResult != null && importResult.Any(x => !string.IsNullOrWhiteSpace(x.ErrorMessage)))
            {
                string sWebRootFolder = _hostingEnvironment.ContentRootPath;
                string URL = string.Format("{0}://{1}/{2}", Request.Scheme, Request.Host, @"Files/Template_CustomerVehicle_Error.xlsx");
                FileInfo file = new FileInfo(Path.Combine(sWebRootFolder, @"Files/Template_CustomerVehicle_Error.xlsx"));

                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("DataError");
                    var currentRow = 1;
                    worksheet.Cell(currentRow, 1).Value = "Mã khách (*)";
                    worksheet.Cell(currentRow, 2).Value = "Họ tên";
                    worksheet.Cell(currentRow, 3).Value = "Loại phương tiện";
                    worksheet.Cell(currentRow, 4).Value = "Biển số";
                    worksheet.Cell(currentRow, 5).Value = "Nhãn hiệu";
                    worksheet.Cell(currentRow, 6).Value = "Màu sơn";
                    worksheet.Cell(currentRow, 7).Value = "Lỗi";

                    for (int i = 1; i <= 7; i++)
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

                        worksheet.Cell(currentRow, 2).Value = importRow.FullName;
                        worksheet.Cell(currentRow, 2).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                        //worksheet.Cell(currentRow, 3).Value = importRow.Type == 0 ? "Xe máy" : "";
                        worksheet.Cell(currentRow, 3).Value = importRow.TypeName;
                        worksheet.Cell(currentRow, 3).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;


                        worksheet.Cell(currentRow, 4).Value = importRow.Plate;
                        worksheet.Cell(currentRow, 4).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                        worksheet.Cell(currentRow, 5).Value = importRow.Branch;
                        worksheet.Cell(currentRow, 5).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                        worksheet.Cell(currentRow, 6).Value = importRow.Color;
                        worksheet.Cell(currentRow, 6).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                        worksheet.Cell(currentRow, 7).Value = importRow.ErrorMessage;
                        worksheet.Cell(currentRow, 7).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    }
                    workbook.SaveAs(file.FullName);
                }

                result = false;
            }

            return ApiOk(result);
        }
    }
}
