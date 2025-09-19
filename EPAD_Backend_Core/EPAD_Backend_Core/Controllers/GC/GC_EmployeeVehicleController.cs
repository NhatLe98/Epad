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
    public class GC_EmployeeVehicleController : ApiControllerBase
    {
        protected readonly IGC_EmployeeVehicleService _GC_EmployeeVehicleService;
        private readonly IHostingEnvironment _hostingEnvironment;
        public GC_EmployeeVehicleController(IServiceProvider pServiceProvider) : base(pServiceProvider)
        {
            _hostingEnvironment = TryResolve<IHostingEnvironment>();
            _GC_EmployeeVehicleService = TryResolve<IGC_EmployeeVehicleService>();
        }

        [Authorize]
        [ActionName("GetEmployeeVehicleAll")]
        [HttpGet]
        public async Task<IActionResult> GetEmployeeVehicle(string filter, int pageIndex, int pageSize)
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            var data = await _GC_EmployeeVehicleService.GetEmployeeVehicleByFilter(filter, pageIndex, pageSize);
            return ApiOk(data);
        }

        [Authorize]
        [ActionName("GetEmployeeVehicleByFilter")]
        [HttpPost]
        public async Task<IActionResult> GetEmployeeVehicleByFilter([FromBody] EmployeeVehicleRequest param)
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            var data = await _GC_EmployeeVehicleService.GetEmployeeVehicleByFilterAdvance(param);
            return ApiOk(data);
        }

        [Authorize]
        [ActionName("GetEmployeeVehicle")]
        [HttpGet]
        public async Task<IActionResult> GetEmployeeVehicleByEmployee(int index)
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            var data = await _GC_EmployeeVehicleService.FirstOrDefaultAsync(x => x.CompanyIndex == user.CompanyIndex && x.Index == index);
            return ApiOk(data);
        }

        [Authorize]
        [ActionName("AddEmployeeVehicle")]
        [HttpPost]
        public async Task<IActionResult> AddEmployeeVehicleAsync(EmployeeVehicleRequest request)
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            var check = await _GC_EmployeeVehicleService.GetByEmpAndPlate(request.EmployeeATID, request.Plate);
            if (check != null)
            {
                return ApiError("MSG_DataExisted");
            }

            var listItem = new List<GC_EmployeeVehicle>();
            foreach (string employeeATID in request.EmployeeATIDs)
            {
                var data = new GC_EmployeeVehicle()
                {
                    EmployeeATID = employeeATID,
                    FromDate = DateTime.Now,
                    ToDate = request.ToDate,
                    Plate = request.Plate,
                    VehicleImage = request.VehicleImage,
                    RegistrationImage = request.RegistrationImage,
                    Type = request.Type,
                    StatusType = request.StatusType,
                    Branch = request.Branch,
                    Color = request.Color,
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now,
                    UpdatedUser = user.UserName,
                    CompanyIndex = user.CompanyIndex
                };

                var rs = _GC_EmployeeVehicleService.Insert(data);
                listItem.Add(rs);
            }
            SaveChange();
            return ApiOk();
        }

        [Authorize]
        [ActionName("UpdateEmployeeVehicle")]
        [HttpPut]
        public async Task<IActionResult> UpdateEmployeeVehicle([FromBody] GC_EmployeeVehicle request)
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            var checkExist = _GC_EmployeeVehicleService.FirstOrDefault(x => x.Index == request.Index);
            if (checkExist == null) return ApiError("MSG_ObjectNotExisted");

            var checkPlateUsing = await _GC_EmployeeVehicleService.GetByEmpAndPlateByUpdate(request.Index, request.EmployeeATID, request.Plate);
            if (checkPlateUsing != null)
            {
                return ApiError("MSG_DataExisted");
            }

            var listIndexs = new List<int>();
            checkExist.FromDate = DateTime.Now;
            checkExist.ToDate = request.ToDate;
            checkExist.VehicleImage = request.VehicleImage;
            checkExist.RegistrationImage = request.RegistrationImage;
            checkExist.Type = request.Type;
            checkExist.StatusType = request.StatusType;
            checkExist.Branch = request.Branch;
            checkExist.Color = request.Color;
            checkExist.UpdatedDate = DateTime.Now;
            checkExist.UpdatedUser = user.UserName;
            checkExist.Plate = request.Plate;

            _GC_EmployeeVehicleService.Update(checkExist);
            SaveChange();
            return ApiOk();

        }

        [Authorize]
        [ActionName("DeleteEmployeeVehicle")]
        [HttpDelete]
        public IActionResult DeleteEmployeeVehicle([FromBody] List<int> listIndex)
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            var rs = _GC_EmployeeVehicleService.Delete(x => listIndex.Contains(x.Index));
            SaveChange();
            return ApiOk(rs);
        }


        [Authorize]
        [ActionName("ImportEmployeeVehicle")]
        [HttpPost]
        public async Task<IActionResult> ImportEmployeeVehicleAsync([FromBody] List<EmployeeVehicleImportData> request)
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            var result = true;

            var importResult = await _GC_EmployeeVehicleService.ImportEmployeeVehicle(request, user);
            if (importResult != null && importResult.Any(x => !string.IsNullOrWhiteSpace(x.ErrorMessage)))
            {
                string sWebRootFolder = _hostingEnvironment.ContentRootPath;
                string URL = string.Format("{0}://{1}/{2}", Request.Scheme, Request.Host, @"Files/Template_EmployeeVehicle_Error.xlsx");
                FileInfo file = new FileInfo(Path.Combine(sWebRootFolder, @"Files/Template_EmployeeVehicle_Error.xlsx"));

                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("DataError");
                    var currentRow = 1;
                    worksheet.Cell(currentRow, 1).Value = "MCC (*)";
                    worksheet.Cell(currentRow, 2).Value = "Nhân viên";
                    worksheet.Cell(currentRow, 3).Value = "Loại phương tiện";
                    worksheet.Cell(currentRow, 4).Value = "Loại xe (*)";
                    worksheet.Cell(currentRow, 5).Value = "Biển số (*)";
                    worksheet.Cell(currentRow, 6).Value = "Nhãn hiệu";
                    worksheet.Cell(currentRow, 7).Value = "Màu sơn";
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

                        worksheet.Cell(currentRow, 2).Value = importRow.FullName;
                        worksheet.Cell(currentRow, 2).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                        //worksheet.Cell(currentRow, 3).Value = importRow.Type == 0 ? "Xe máy" : "";
                        worksheet.Cell(currentRow, 3).Value = importRow.TypeName;
                        worksheet.Cell(currentRow, 3).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                        worksheet.Cell(currentRow, 4).Value = importRow.StatusTypeName;
                        worksheet.Cell(currentRow, 4).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                        worksheet.Cell(currentRow, 5).Value = importRow.Plate;
                        worksheet.Cell(currentRow, 5).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                        worksheet.Cell(currentRow, 6).Value = importRow.Branch;
                        worksheet.Cell(currentRow, 6).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                        worksheet.Cell(currentRow, 7).Value = importRow.Color;
                        worksheet.Cell(currentRow, 7).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;


                        worksheet.Cell(currentRow, 8).Value = importRow.ErrorMessage;
                        worksheet.Cell(currentRow, 8).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    }
                    workbook.SaveAs(file.FullName);
                }

                result = false;
            }

            return ApiOk(result);
        }
    }
}
