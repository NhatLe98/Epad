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
using NPOI.SS.Formula.Functions;
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
    public class GC_Department_AccessedGroupController : ApiControllerBase
    {
        protected readonly IGC_Department_AccessedGroupService _GC_Department_AccessedGroupService;
        protected readonly IGC_AccessedGroupService _GC_AccessedGroupService;
        protected readonly IIC_DepartmentService _IC_DepartmentService;
        protected readonly IHR_UserService _HR_UserService;
        private readonly IHostingEnvironment _hostingEnvironment;
        public GC_Department_AccessedGroupController(IServiceProvider pServiceProvider) : base(pServiceProvider)
        {
            _GC_Department_AccessedGroupService = TryResolve<IGC_Department_AccessedGroupService>();
            _GC_AccessedGroupService = TryResolve<IGC_AccessedGroupService>();
            _HR_UserService = TryResolve<IHR_UserService>();
            _IC_DepartmentService = TryResolve<IIC_DepartmentService>();
            _hostingEnvironment = TryResolve<IHostingEnvironment>();
        }

        [Authorize]
        [ActionName("GetDepartmentAccessedGroup")]
        [HttpGet]
        public async Task<IActionResult> GetDepartmentAccessedGroup(string filter, int pageIndex, int pageSize)
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();
            var data = await _GC_Department_AccessedGroupService.GetDepartmentAccessedGroup(pageIndex, pageSize, filter, user);
            return ApiOk(data);
        }

        [Authorize]
        [ActionName("GetDepartmentAccessedGroupByFilter")]
        [HttpPost]
        public async Task<IActionResult> GetDepartmentAccessedGroupByFilter([FromBody] Department_AccessedGroupRequest request)
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();
            DateTime? fromDate = !string.IsNullOrWhiteSpace(request.FromDateString) ? DateTime.Parse(request.FromDateString) : null;

            DateTime? toDate = !string.IsNullOrWhiteSpace(request.ToDateString) ? DateTime.Parse(request.ToDateString) : null;
            var data = await _GC_Department_AccessedGroupService.GetDepartmentAccessedGroupByFilter(request.pageIndex, request.pageSize,
                request.filter, fromDate, toDate, request.DepartmentFilter, user);

            return ApiOk(data);
        }

        //[Authorize]
        //[ActionName("GetDepartmentAccessedGroupAll")]
        //[HttpGet]
        //public IActionResult GetDepartmentAccessedGroupAll()
        //{
        //    var user = GetUserInfo();
        //    if (user == null) return ApiUnauthorized();
        //    var grid = _GC_Department_AccessedGroupService.GetAllByCurrentCompanyIndex();
        //    return ApiOk(grid);
        //}

        [Authorize]
        [ActionName("AddDepartmentAccessedGroup")]
        [HttpPost]
        public IActionResult AddDepartmentAccessedGroup([FromBody] Department_AccessedGroupRequest request)
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();
            var listDepartmentAccessGroupExist = _GC_Department_AccessedGroupService.GetByListDepartmentAndFromToDate(request.DepartmentIDs, request.FromDate, request.ToDate, user);
            var departmentList = _IC_DepartmentService.Where(x => listDepartmentAccessGroupExist.Select(x => x.DepartmentIndex).Contains(x.Index)).ToList();
            var departmentExist = "";

            foreach (var departmentIndex in request.DepartmentIDs)
            {
                var checkExist = listDepartmentAccessGroupExist.Any(x => x.DepartmentIndex == departmentIndex);
                if (checkExist)
                {
                    departmentExist += "<p>  - " + departmentList.FirstOrDefault(x => x.Index == departmentIndex)?.Name + "</p>" + "<p class=\"\" style=\"margin: 4px;\"></p>";
                }
                else
                {
                    var data = new GC_Department_AccessedGroup()
                    {
                        DepartmentIndex = departmentIndex,
                        FromDate = request.FromDate,
                        ToDate = request.ToDate,
                        AccessedGroupIndex = request.AccessedGroupIndex,
                        UpdatedDate = DateTime.Now,
                        UpdatedUser = user.UserName,
                        CompanyIndex = user.CompanyIndex
                    };

                    _GC_Department_AccessedGroupService.Insert(data);
                }
            }
            SaveChange();
            return ApiOk(departmentExist);
        }

        [Authorize]
        [ActionName("UpdateDepartmentAccessedGroup")]
        [HttpPut]
        public IActionResult UpdateDepartmentAccessedGroup([FromBody] GC_Department_AccessedGroup request)
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            var checkExist = _GC_Department_AccessedGroupService.Any(x => x.CompanyIndex == request.CompanyIndex
                && x.Index != request.Index
                && x.DepartmentIndex == request.DepartmentIndex
                && ((x.ToDate.HasValue && ((request.ToDate.HasValue && ((request.ToDate.Value.Date >= x.FromDate.Date && request.FromDate.Date <= x.FromDate.Date)
                    || (request.FromDate.Date >= x.FromDate.Date && request.ToDate.Value.Date <= x.ToDate.Value.Date)
                    || (request.FromDate.Date <= x.ToDate.Value.Date && request.ToDate.Value.Date >= x.ToDate.Value.Date)))
                            || (!request.ToDate.HasValue && (request.FromDate.Date <= x.FromDate.Date
                            || (request.FromDate.Date >= x.FromDate.Date && request.FromDate.Date <= x.ToDate.Value.Date)))))
                        || (!x.ToDate.HasValue && (!request.ToDate.HasValue
                            || (request.ToDate.HasValue && request.ToDate.Value.Date >= x.FromDate.Date)))));

            if (checkExist) return ApiError("DepartmentAccessedGroupExist");

            var data = _GC_Department_AccessedGroupService.FirstOrDefault(x => x.Index == request.Index);
            if (data == null)
            {
                return ApiError("MSG_ObjectNotExisted");
            }
            data.ToDate = request.ToDate;
            data.AccessedGroupIndex = request.AccessedGroupIndex;
            data.UpdatedDate = DateTime.Now;
            data.UpdatedUser = user.UserName;

            var rs = _GC_Department_AccessedGroupService.Update(data);
            SaveChange();
            return ApiOk(rs);
        }

        [Authorize]
        [ActionName("DeleteDepartmentAccessedGroup")]
        [HttpDelete]
        public IActionResult DeleteDepartmentAccessedGroup([FromBody] List<int> listItem)
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();
            try
            {
                var dataDelete = _DbContext.GC_Department_AccessedGroup.Where(x => listItem.Contains(x.Index)).ToList();
                _DbContext.GC_Department_AccessedGroup.RemoveRange(dataDelete);
                _DbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                return ApiError(ex.Message);
            }
            return ApiOk();
        }

        [Authorize]
        [ActionName("ImportDepartmentAccessedGroup")]
        [HttpPost]
        public async Task<IActionResult> ImportDepartmentAccessedGroup(List<DepartmentAccessedGroupModel> param)
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            var result = true;

            var importResult = await _GC_Department_AccessedGroupService.ImportDepartmentAccessedGroup(param, user);
            var message = "";

            var dataImportError = importResult.Where(x => !string.IsNullOrWhiteSpace(x.ErrorMessage)).ToList();
            if (dataImportError != null && dataImportError.Any(x => !string.IsNullOrWhiteSpace(x.ErrorMessage)))
            {
                message = dataImportError.Count().ToString();

                string sWebRootFolder = _hostingEnvironment.ContentRootPath;
                string URL = string.Format("{0}://{1}/{2}", Request.Scheme, Request.Host, @"Files/Template_DepartmentAccessedGroup_Error.xlsx");
                FileInfo file = new FileInfo(Path.Combine(sWebRootFolder, @"Files/Template_DepartmentAccessedGroup_Error.xlsx"));

                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("DataError");
                    var currentRow = 1;
                    worksheet.Cell(currentRow, 1).Value = "Phòng ban (*)";
                    worksheet.Cell(currentRow, 2).Value = "Từ ngày (*)";
                    worksheet.Cell(currentRow, 3).Value = "Đến ngày";
                    worksheet.Cell(currentRow, 4).Value = "Nhóm truy cập (*)";
                    worksheet.Cell(currentRow, 5).Value = "Lỗi";

                    for (int i = 1; i < 6; i++)
                    {
                        worksheet.Cell(1, i).Style.Fill.BackgroundColor = XLColor.Yellow;
                        worksheet.Cell(1, i).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        worksheet.Column(i).Width = 20;
                    }

                    foreach (var importRow in dataImportError)
                    {
                        currentRow++;
                        //New template

                        worksheet.Cell(currentRow, 1).Value = importRow.DepartmentName;
                        worksheet.Cell(currentRow, 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                        worksheet.Cell(currentRow, 2).Value = importRow.FromDateFormat;
                        worksheet.Cell(currentRow, 2).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                        worksheet.Cell(currentRow, 3).Value = importRow.ToDateFormat;
                        worksheet.Cell(currentRow, 3).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                        worksheet.Cell(currentRow, 4).Value = importRow.AccessedGroupName;
                        worksheet.Cell(currentRow, 4).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                        worksheet.Cell(currentRow, 5).Value = importRow.ErrorMessage;
                        worksheet.Cell(currentRow, 5).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    }
                    workbook.SaveAs(file.FullName);
                }

                result = false;
            }

            return Ok(message);
        }

    }

    public class Department_AccessedGroupRequest
    {
        public long DepartmentIndex { get; set; }
        public List<int> DepartmentIDs { get; set; }
        public List<long> DepartmentFilter { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string FromDateString { get; set; }
        public string ToDateString { get; set; }
        public int? AccessedGroupIndex { get; set; }
        public string filter { get; set; }
        public int pageIndex { get; set; }
        public int pageSize { get; set; }

    }

    public class Delete_Department_AccessedGroupRequest
    {
        public string DepartmentIndex { get; set; }
        public DateTime FromDate { get; set; }
    }

    //public class DepartmentAccessedGroupImportData : GC_Department_AccessedGroup
    //{
    //    public string AccessedGroup { get; set; }
    //}
}
