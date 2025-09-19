using ClosedXML.Excel;
using DocumentFormat.OpenXml.VariantTypes;
using EPAD_Backend_Core.Base;
using EPAD_Common.Types;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using EPAD_Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
namespace GCS_API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class GC_Employee_AccessedGroupController : ApiControllerBase
    {
        protected readonly IGC_Employee_AccessedGroupService _GC_Employee_AccessedGroupService;
        protected readonly IGC_AccessedGroupService _GC_AccessedGroupService;
        protected readonly IHR_UserService _HR_UserService;
        private readonly IHostingEnvironment _hostingEnvironment;
        public GC_Employee_AccessedGroupController(IServiceProvider pServiceProvider) : base(pServiceProvider)
        {
            _GC_Employee_AccessedGroupService = TryResolve<IGC_Employee_AccessedGroupService>();
            _GC_AccessedGroupService = TryResolve<IGC_AccessedGroupService>();
            _HR_UserService = TryResolve<IHR_UserService>();
            _hostingEnvironment = TryResolve<IHostingEnvironment>();
        }

        [Authorize]
        [ActionName("GetEmployeeAccessedGroup")]
        [HttpGet]
        public async Task<IActionResult> GetEmployeeAccessedGroup(string filter, int pageIndex, int pageSize)
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();
            var data = await _GC_Employee_AccessedGroupService.GetEmployeeAccessedGroup(pageIndex, pageSize, filter, user);
            //var resultEmpIDs = ((List<GC_Employee_AccessedGroup>)data.data).Select(x => x.TryGetValue("EmployeeATID").ToString()).ToList();

            return ApiOk(data);
        }

        [Authorize]
        [ActionName("GetEmployeeAccessedGroupByFilter")]
        [HttpPost]
        public async Task<IActionResult> GetEmployeeAccessedGroupByFilter([FromBody] Employee_AccessedGroupRequest request)
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();
            var data = await _GC_Employee_AccessedGroupService.GetEmployeeAccessedGroupByFilter(request.pageIndex, request.pageSize,
                request.filter, request.FromDate, request.ToDate, request.DepartmentIDs, request.EmployeeATIDs, user);

            return ApiOk(data);
        }

        //[Authorize]
        //[ActionName("GetEmployeeAccessedGroupAll")]
        //[HttpGet]
        //public IActionResult GetEmployeeAccessedGroupAll()
        //{
        //    var user = GetUserInfo();
        //    if (user == null) return ApiUnauthorized();
        //    var grid = _GC_Employee_AccessedGroupService.GetAllByCurrentCompanyIndex();
        //    return ApiOk(grid);
        //}

        [Authorize]
        [ActionName("AddEmployeeAccessedGroup")]
        [HttpPost]
        public async Task<IActionResult> AddEmployeeAccessedGroup([FromBody] Employee_AccessedGroupRequest request)
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            var userInfoList = _HR_UserService.Where(x => request.EmployeeATIDs.Contains(x.EmployeeATID)).ToList();
            var listEmployeeExist = await _GC_Employee_AccessedGroupService.GetByEmployeeAndFromToDate(request.EmployeeATIDs, request.FromDate, request.ToDate, user);
            var employeeExist = "";
            var isAdd = true;
            foreach (string employeeATID in request.EmployeeATIDs)
            {
                var check = listEmployeeExist.FirstOrDefault(x => x.EmployeeATID == employeeATID);
                if (check != null)
                {
                    employeeExist += "<p>  - " + employeeATID + " " + userInfoList.FirstOrDefault(x => x.EmployeeATID == employeeATID)?.FullName + "</p>" + "<p class=\"\" style=\"margin: 4px;\"></p>";
                    isAdd = false;
                    //return ApiOk(employeeExist);
                }
                else
                {
                    var data = new GC_Employee_AccessedGroup()
                    {
                        EmployeeATID = employeeATID,
                        FromDate = request.FromDate,
                        ToDate = request.ToDate,
                        AccessedGroupIndex = request.AccessedGroupIndex,
                        UpdatedDate = DateTime.Now,
                        UpdatedUser = user.UserName,
                        CompanyIndex = user.CompanyIndex
                    };

                    _GC_Employee_AccessedGroupService.Insert(data);
                }
            }
            if (isAdd)
            {
                SaveChange();
            }
            return ApiOk(employeeExist);
        }

        [Authorize]
        [ActionName("UpdateEmployeeAccessedGroup")]
        [HttpPut]
        public IActionResult UpdateEmployeeAccessedGroup([FromBody] GC_Employee_AccessedGroup request)
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            var checkExist = _GC_Employee_AccessedGroupService.Any(x => x.CompanyIndex == request.CompanyIndex
                && x.Index != request.Index
                && x.EmployeeATID == request.EmployeeATID
                && ((x.ToDate.HasValue && ((request.ToDate.HasValue && ((request.ToDate.Value.Date >= x.FromDate.Date && request.FromDate.Date <= x.FromDate.Date)
                    || (request.FromDate.Date >= x.FromDate.Date && request.ToDate.Value.Date <= x.ToDate.Value.Date)
                    || (request.FromDate.Date <= x.ToDate.Value.Date && request.ToDate.Value.Date >= x.ToDate.Value.Date)))
                            || (!request.ToDate.HasValue && (request.FromDate.Date <= x.FromDate.Date
                            || (request.FromDate.Date >= x.FromDate.Date && request.FromDate.Date <= x.ToDate.Value.Date)))))
                        || (!x.ToDate.HasValue && (!request.ToDate.HasValue
                            || (request.ToDate.HasValue && request.ToDate.Value.Date >= x.FromDate.Date)))));

            if (checkExist) return ApiError("ParkingLotAccessedDateOverlap");

            var data = _GC_Employee_AccessedGroupService.FirstOrDefault(x => x.Index == request.Index);
            if (data == null)
            {
                return ApiError("MSG_ObjectNotExisted");
            }
            data.ToDate = request.ToDate;
            data.AccessedGroupIndex = request.AccessedGroupIndex;
            data.UpdatedDate = DateTime.Now;
            data.UpdatedUser = user.UserName;

            var rs = _GC_Employee_AccessedGroupService.Update(data);
            SaveChange();
            return ApiOk(rs);
        }

        [Authorize]
        [ActionName("DeleteEmployeeAccessedGroup")]
        [HttpDelete]
        public IActionResult DeleteEmployeeAccessedGroup([FromBody] List<int> listItem)
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();
            try
            {
                var dataDelete = _DbContext.GC_Employee_AccessedGroup.Where(x => listItem.Contains(x.Index)).ToList();
                _DbContext.GC_Employee_AccessedGroup.RemoveRange(dataDelete);
                _DbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                return ApiError(ex.Message);
            }
            return ApiOk();
        }

        [Authorize]
        [ActionName("ImportEmployeeAccessedGroup")]
        [HttpPost]
        public async Task<IActionResult> ImportEmployeeAccessedGroup(List<EmployeeAccessedGroupModel> param)
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            var result = true;

            var importResult = await _GC_Employee_AccessedGroupService.ImportEmployeeAccessedGroup(param, user);

            if (importResult != null && importResult.Any(x => !string.IsNullOrWhiteSpace(x.ErrorMessage)))
            {
                string sWebRootFolder = _hostingEnvironment.ContentRootPath;
                string URL = string.Format("{0}://{1}/{2}", Request.Scheme, Request.Host, @"Files/Template_EmployeeAccessedGroup_Error.xlsx");
                FileInfo file = new FileInfo(Path.Combine(sWebRootFolder, @"Files/Template_EmployeeAccessedGroup_Error.xlsx"));

                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("DataError");
                    var currentRow = 1;
                    worksheet.Cell(currentRow, 1).Value = "MCC (*)";
                    worksheet.Cell(currentRow, 2).Value = "Nhân viên";
                    worksheet.Cell(currentRow, 3).Value = "Từ ngày (*)";
                    worksheet.Cell(currentRow, 4).Value = "Đến ngày";
                    worksheet.Cell(currentRow, 5).Value = "Nhóm truy cập";
                    worksheet.Cell(currentRow, 6).Value = "Lỗi";

                    for (int i = 1; i <= 6; i++)
                    {
                        worksheet.Cell(1, i).Style.Fill.BackgroundColor = XLColor.Yellow;
                        worksheet.Cell(1, i).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        worksheet.Column(i).Width = 20;
                    }

                    foreach (var importRow in importResult)
                    {
                        currentRow++;
                        //New template
                        worksheet.Cell(currentRow, 1).Value = "'" + importRow.EmployeeATID;
                        worksheet.Cell(currentRow, 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                        worksheet.Cell(currentRow, 2).Value = importRow.EmployeeName;
                        worksheet.Cell(currentRow, 2).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                        worksheet.Cell(currentRow, 3).Value = importRow.FromDateFormat;
                        worksheet.Cell(currentRow, 3).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                        worksheet.Cell(currentRow, 4).Value = importRow.ToDateFormat;
                        worksheet.Cell(currentRow, 4).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                        worksheet.Cell(currentRow, 5).Value = importRow.AccessedGroupName;
                        worksheet.Cell(currentRow, 5).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                        worksheet.Cell(currentRow, 6).Value = importRow.ErrorMessage;
                        worksheet.Cell(currentRow, 6).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    }
                    workbook.SaveAs(file.FullName);
                }

                result = false;
            }

            return ApiOk(result);
        }

    }

    public class Employee_AccessedGroupRequest
    {
        public List<long> DepartmentIDs { get; set; }
        public List<string> EmployeeATIDs { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string FromDateString { get; set; }
        public string ToDateString { get; set; }
        public int? AccessedGroupIndex { get; set; }
        public string filter { get; set; }
        public int pageIndex { get; set; }
        public int pageSize { get; set; }

    }

    public class Delete_Employee_AccessedGroupRequest
    {
        public string EmployeeATID { get; set; }
        public DateTime FromDate { get; set; }
    }

    public class EmployeeAccessedGroupImportData : GC_Employee_AccessedGroup
    {
        public string AccessedGroup { get; set; }
    }
}
