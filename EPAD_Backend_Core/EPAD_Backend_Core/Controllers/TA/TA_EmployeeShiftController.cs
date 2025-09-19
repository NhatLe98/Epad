using ClosedXML.Excel;
using Chilkat;
using EPAD_Backend_Core.Base;
using EPAD_Common.Types;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using EPAD_Services.Impl;
using EPAD_Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static EPAD_Common.Utility.AppUtils;
using Microsoft.AspNetCore.Hosting;
using DocumentFormat.OpenXml.Math;
using NPOI.SS.Formula.Functions;

namespace EPAD_Backend_Core.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class TA_EmployeeShiftController : ApiControllerBase
    {
        ITA_ShiftService _TA_ShiftService;
        private readonly IHR_UserService _HR_UserService;
        private readonly ITA_EmployeeShiftService _TA_EmployeeShiftService;
        private readonly ILogger _logger;
        private readonly IHostingEnvironment _hostingEnvironment;
        ITA_ScheduleFixedByEmployeeService _TA_ScheduleFixedByEmployeeService;
        ITA_ScheduleFixedByDepartmentService _TA_ScheduleFixedByDepartmentService;
        public TA_EmployeeShiftController(IConfiguration configuration, IServiceProvider pServiceProvider, ILoggerFactory loggerFactory, IHostingEnvironment hostingEnvironment) : base(pServiceProvider)
        {
            _TA_ShiftService = TryResolve<ITA_ShiftService>();
            _logger = loggerFactory.CreateLogger<TA_EmployeeShiftController>();
            _HR_UserService = TryResolve<IHR_UserService>();
            _TA_EmployeeShiftService = TryResolve<ITA_EmployeeShiftService>();
            _hostingEnvironment = hostingEnvironment;
            _TA_ScheduleFixedByDepartmentService = TryResolve<ITA_ScheduleFixedByDepartmentService>();
            _TA_ScheduleFixedByEmployeeService = TryResolve<ITA_ScheduleFixedByEmployeeService>();
        }

        [Authorize]
        [ActionName("GetEmployeeShiftByFilter")]
        [HttpPost]
        public async Task<IActionResult> GetEmployeeShiftByFilter(TA_EmployeeShiftParam employeeShiftParam)
        {
            UserInfo user = GetUserInfo();
            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }
            var listDate = Enumerable.Range(0, 1 + employeeShiftParam.ToDate.Date.Subtract(employeeShiftParam.FromDate.Date).Days).Select(offset => employeeShiftParam.FromDate.Date.AddDays(offset)).ToArray();

            #region SetColumn
            var table = new HeaderTableEmployeeShift();
            table.Columns.Add(new ColumnData
            {
                Index = 0,
                Name = "MCC",
                Code = "EmployeeATID"
            });
            table.Columns.Add(new ColumnData
            {
                Index = 1,
                Name = "EmployeeCode",
                Code = "EmployeeCode"
            });
            table.Columns.Add(new ColumnData
            {
                Index = 2,
                Name = "FullName",
                Code = "EmployeeName"
            });
            table.Columns.Add(new ColumnData
            {
                Index = 3,
                Name = "DepartmentName",
                Code = "DepartmentName"
            });
            int index = 4;
            foreach (var date in listDate)
            {
                table.Columns.Add(new ColumnData
                {
                    Index = index,
                    Name = date.ToString("dd/MM/yyyy"),
                    Code = date.ToString("dd/MM/yyyy")
                });
                index++;
            }
            #endregion
            var employeeList = await _HR_UserService.GetEmployeeCompactInfoByEmployeeATID(employeeShiftParam.EmployeeATIDs, DateTime.Now, user.CompanyIndex);
            if (employeeShiftParam.DepartmentIds != null && employeeShiftParam.DepartmentIds.Count > 0)
            {
                employeeList = await _HR_UserService.GetEmployeeByDepartmentIds(employeeShiftParam.DepartmentIds, user.CompanyIndex);
            }

            if (employeeShiftParam.EmployeeATIDs != null && employeeShiftParam.EmployeeATIDs.Count > 0)
            {
                employeeList = await _HR_UserService.GetEmployeeCompactInfoByEmployeeATID(employeeShiftParam.EmployeeATIDs, DateTime.Now, user.CompanyIndex);
            }

            var scheduleDepartmentList = _TA_ScheduleFixedByDepartmentService.Where(x => employeeList.Select(z => z.DepartmentIndex).Contains(x.DepartmentIndex)).ToList();
            var scheduleEmployeeList = _TA_ScheduleFixedByEmployeeService.Where(x => employeeList.Select(z => z.EmployeeATID).Contains(x.EmployeeATID)).ToList();

            var listEmployeeShift = _TA_EmployeeShiftService.GetListEmpoyeeShiftByDateAndEmps(employeeShiftParam.FromDate, employeeShiftParam.ToDate, employeeShiftParam.EmployeeATIDs, user.CompanyIndex);
            if (employeeList != null && employeeList.Any())
            {
                var shiftList = await _TA_ShiftService.GetAllShift(user.CompanyIndex);
                foreach (var info in employeeList)
                {
                    if (info != null && !string.IsNullOrEmpty(info.EmployeeATID))
                    {
                        var listColumnObjectUsed = new List<ColumnObjectUsed>();
                        foreach (var date in listDate)
                        {
                            var dayOfWeek = date.DayOfWeek;
                            var shiftByEmployee = listEmployeeShift.FirstOrDefault(x => x.EmployeeATID == info.EmployeeATID && x.Date.Date == date.Date);
                            var shiftByDepartmentScheduleFixed = scheduleDepartmentList.FirstOrDefault(x => x.DepartmentIndex == info.DepartmentIndex && date.Date.Date >= x.FromDate.Date
                                                                                             && (x.ToDate == null || (x.ToDate != null && date.Date.Date <= x.ToDate.Value.Date)));

                            var shiftByEmployeeScheduleFixed = scheduleEmployeeList.FirstOrDefault(x => x.EmployeeATID == info.EmployeeATID && x.FromDate.Date <= date.Date
                                                                                             && (x.ToDate == null || (x.ToDate != null && date.Date.Date <= x.ToDate.Value.Date)));

                            var columnObjectUsed = new ColumnObjectUsed();
                            if (shiftByEmployee != null)
                            {
                                columnObjectUsed.Index = shiftByEmployee.ShiftIndex ?? 0;
                                columnObjectUsed.ShiftName = shiftList?.FirstOrDefault(x => x.Index == shiftByEmployee.ShiftIndex)?.Name ?? "";
                                columnObjectUsed.KeyMain = date.ToString("dd/MM/yyyy");

                            }
                            else if (shiftByEmployeeScheduleFixed != null)
                            {
                                var dayOfWeekProperty = shiftByEmployeeScheduleFixed.GetType().GetProperty(dayOfWeek.ToString());
                                if (dayOfWeekProperty != null)
                                {
                                    var dayOfWeekValue = (int)dayOfWeekProperty.GetValue(shiftByEmployeeScheduleFixed);
                                    var shift = shiftList.FirstOrDefault(x => x.Index == dayOfWeekValue);
                                    columnObjectUsed.Index = shift != null ? shift.Index : 0;
                                    columnObjectUsed.ShiftName = shift != null ? shift.Name : "Không có ca";
                                    columnObjectUsed.KeyMain = date.ToString("dd/MM/yyyy");
                                    columnObjectUsed.IsSchedule = true;
                                }
                            }
                            else if (shiftByDepartmentScheduleFixed != null)
                            {
                                var dayOfWeekProperty = shiftByDepartmentScheduleFixed.GetType().GetProperty(dayOfWeek.ToString());
                                if (dayOfWeekProperty != null)
                                {
                                    var dayOfWeekValue = (int)dayOfWeekProperty.GetValue(shiftByDepartmentScheduleFixed);
                                    var shift = shiftList.FirstOrDefault(x => x.Index == dayOfWeekValue);
                                    columnObjectUsed.Index = shift != null ? shift.Index : 0;
                                    columnObjectUsed.ShiftName = shift != null ? shift.Name : "Không có ca";
                                    columnObjectUsed.KeyMain = date.ToString("dd/MM/yyyy");
                                    columnObjectUsed.IsSchedule = true;
                                }
                            }
                            else
                            {
                                columnObjectUsed.Index = 0;
                                columnObjectUsed.ShiftName = "Không có ca";
                                columnObjectUsed.KeyMain = date.ToString("dd/MM/yyyy");
                            }
                            listColumnObjectUsed.Add(columnObjectUsed);
                        }

                        //var listUsed = listEmployeeShift.Where(e => e.EmployeeATID == info.EmployeeATID).Select(e => new ColumnObjectUsed
                        //{
                        //    Index = e.ShiftIndex ?? 0,
                        //    ShiftName = shifts?.FirstOrDefault(x => x.Index == e.ShiftIndex)?.Name ?? "",
                        //    KeyMain = e.Date.ToString("dd/MM/yyyy")
                        //});

                        table.Rows.Add(new RowData
                        {
                            EmployeeATID = info.EmployeeATID,
                            EmployeeCode = info.EmployeeCode,
                            EmployeeName = info?.FullName,
                            DepartmentName = info?.Department,
                            DepartmentIndex = info.DepartmentIndex,
                            ListColumnObjectUsed = listColumnObjectUsed
                        });
                    }
                }
            }
            var count = table.Rows.Count;
            table.total = count;
            table.Rows = table.Rows.Skip((employeeShiftParam.page - 1) * employeeShiftParam.pageSize).Take(employeeShiftParam.pageSize).ToList();
            return ApiOk(table);
        }

        [Authorize]
        [ActionName("ExportDataIntoTemplateImport")]
        [HttpPost]
        public async Task<IActionResult> ExportDataIntoTemplateImport(TA_EmployeeShiftParam employeeShiftParam)
        {
            UserInfo user = GetUserInfo();
            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }
            var listDate = Enumerable.Range(0, 1 + employeeShiftParam.ToDate.Date.Subtract(employeeShiftParam.FromDate.Date).Days).Select(offset => employeeShiftParam.FromDate.Date.AddDays(offset)).ToArray();

            var employeeList = await _HR_UserService.GetEmployeeCompactInfoByEmployeeATID(employeeShiftParam.EmployeeATIDs, DateTime.Now, user.CompanyIndex);
            if (employeeShiftParam.DepartmentIds != null && employeeShiftParam.DepartmentIds.Count > 0)
            {
                employeeList = await _HR_UserService.GetEmployeeByDepartmentIds(employeeShiftParam.DepartmentIds, user.CompanyIndex);
            }

            if (employeeShiftParam.EmployeeATIDs != null && employeeShiftParam.EmployeeATIDs.Count > 0)
            {
                employeeList = await _HR_UserService.GetEmployeeCompactInfoByEmployeeATID(employeeShiftParam.EmployeeATIDs, DateTime.Now, user.CompanyIndex);
            }

            var scheduleDepartmentList = _TA_ScheduleFixedByDepartmentService.Where(x => employeeList.Select(z => z.DepartmentIndex).Contains(x.DepartmentIndex)).ToList();
            var scheduleEmployeeList = _TA_ScheduleFixedByEmployeeService.Where(x => employeeList.Select(z => z.EmployeeATID).Contains(x.EmployeeATID)).ToList();

            var listEmployeeShift = _TA_EmployeeShiftService.GetListEmpoyeeShiftByDateAndEmps(employeeShiftParam.FromDate, employeeShiftParam.ToDate, employeeShiftParam.EmployeeATIDs, user.CompanyIndex);
            var table = new HeaderTableEmployeeShift();
            if (employeeList != null && employeeList.Any())
            {
                var shiftList = await _TA_ShiftService.GetAllShift(user.CompanyIndex);

                foreach (var info in employeeList)
                {
                    if (info != null && !string.IsNullOrEmpty(info.EmployeeATID))
                    {
                        var listColumnObjectUsed = new List<ColumnObjectUsed>();
                        foreach (var date in listDate)
                        {
                            var dayOfWeek = date.DayOfWeek;
                            var shiftByEmployee = listEmployeeShift.FirstOrDefault(x => x.EmployeeATID == info.EmployeeATID && x.Date.Date == date.Date);
                            var shiftByDepartmentScheduleFixed = scheduleDepartmentList.FirstOrDefault(x => x.DepartmentIndex == info.DepartmentIndex && date.Date.Date >= x.FromDate.Date
                                                                                             && (x.ToDate == null || (x.ToDate != null && date.Date.Date <= x.ToDate.Value.Date)));

                            var shiftByEmployeeScheduleFixed = scheduleEmployeeList.FirstOrDefault(x => x.EmployeeATID == info.EmployeeATID && x.FromDate.Date <= date.Date
                                                                                             && (x.ToDate == null || (x.ToDate != null && date.Date.Date <= x.ToDate.Value.Date)));

                            var columnObjectUsed = new ColumnObjectUsed();
                            if (shiftByEmployee != null)
                            {
                                columnObjectUsed.Index = shiftByEmployee.ShiftIndex ?? 0;
                                columnObjectUsed.ShiftName = shiftList?.FirstOrDefault(x => x.Index == shiftByEmployee.ShiftIndex)?.Name ?? "";
                                columnObjectUsed.KeyMain = date.ToString("dd/MM/yyyy");

                            }
                            else if (shiftByEmployeeScheduleFixed != null)
                            {
                                var dayOfWeekProperty = shiftByEmployeeScheduleFixed.GetType().GetProperty(dayOfWeek.ToString());
                                if (dayOfWeekProperty != null)
                                {
                                    var dayOfWeekValue = (int)dayOfWeekProperty.GetValue(shiftByEmployeeScheduleFixed);
                                    var shift = shiftList.FirstOrDefault(x => x.Index == dayOfWeekValue);
                                    columnObjectUsed.Index = shift != null ? shift.Index : 0;
                                    columnObjectUsed.ShiftName = shift != null ? shift.Name : "";
                                    columnObjectUsed.KeyMain = date.ToString("dd/MM/yyyy");
                                    columnObjectUsed.IsSchedule = true;
                                }
                            }
                            else if (shiftByDepartmentScheduleFixed != null)
                            {
                                var dayOfWeekProperty = shiftByDepartmentScheduleFixed.GetType().GetProperty(dayOfWeek.ToString());
                                if (dayOfWeekProperty != null)
                                {
                                    var dayOfWeekValue = (int)dayOfWeekProperty.GetValue(shiftByDepartmentScheduleFixed);
                                    var shift = shiftList.FirstOrDefault(x => x.Index == dayOfWeekValue);
                                    columnObjectUsed.Index = shift != null ? shift.Index : 0;
                                    columnObjectUsed.ShiftName = shift != null ? shift.Name : "";
                                    columnObjectUsed.KeyMain = date.ToString("dd/MM/yyyy");
                                    columnObjectUsed.IsSchedule = true;
                                }
                            }
                            else
                            {
                                columnObjectUsed.Index = 0;
                                columnObjectUsed.ShiftName = "";
                                columnObjectUsed.KeyMain = date.ToString("dd/MM/yyyy");
                            }
                            listColumnObjectUsed.Add(columnObjectUsed);
                        }


                        table.Rows.Add(new RowData
                        {
                            EmployeeATID = info.EmployeeATID,
                            EmployeeCode = info.EmployeeCode,
                            EmployeeName = info?.FullName,
                            DepartmentName = info?.Department,
                            DepartmentIndex = info.DepartmentIndex,
                            ListColumnObjectUsed = listColumnObjectUsed
                        });
                    }
                }
            }

            //Export
            string sWebRootFolder = _hostingEnvironment.ContentRootPath;
            string URL = string.Format("{0}://{1}/{2}", Request.Scheme, Request.Host, @"Files/Data_Employee_Shift.xlsx");
            FileInfo file = new FileInfo(Path.Combine(sWebRootFolder, @"Files/Data_Employee_Shift.xlsx"));

            employeeShiftParam.ToDate = employeeShiftParam.FromDate.AddDays(30);
            listDate = Enumerable.Range(0, 1 + employeeShiftParam.ToDate.Date.Subtract(employeeShiftParam.FromDate.Date).Days).Select(offset => employeeShiftParam.FromDate.Date.AddDays(offset)).ToArray();
            using (var workbook = new XLWorkbook(file.FullName))
            {
                var worksheet = workbook.Worksheet(1);
                var currentRow = 4;
                worksheet.Cell(1, 1).Value = "Tháng";
                worksheet.Cell(1, 2).Value = "Năm";
                worksheet.Cell(1, 3).Value = "Ngày";

                worksheet.Cell(2, 1).Value = employeeShiftParam.FromDate.Month.ToString();
                worksheet.Cell(2, 2).Value = employeeShiftParam.FromDate.Year.ToString();
                worksheet.Cell(2, 3).Value = employeeShiftParam.FromDate.Day.ToString();


                worksheet.Cell(currentRow, 1).Value = "MCC(*)";
                worksheet.Cell(currentRow, 2).Value = "Mã nhân viên";
                worksheet.Cell(currentRow, 3).Value = "Nhân viên";
                worksheet.Cell(currentRow, 4).Value = "Phòng ban";

                var numberRow = 5;
                foreach (var item in listDate)
                {
                    worksheet.Cell(currentRow, numberRow).Value = item.Day;
                    numberRow++;
                }

                for (int i = 1; i < numberRow; i++)
                {
                    worksheet.Cell(currentRow, i).Style.Fill.BackgroundColor = XLColor.Yellow;
                    worksheet.Cell(currentRow, i).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                }

                for (int i = (currentRow + 1); i <= worksheet.LastRowUsed().RowNumber(); i++)
                {
                    worksheet.Row(i).Clear();
                }

                //Dem
                currentRow++;
                for (int i = 1; i < 36; i++)
                {
                    worksheet.Cell(currentRow, i).Value = i;
                    worksheet.Cell(currentRow, i).Style.Fill.BackgroundColor = XLColor.LightCyan;
                    worksheet.Cell(currentRow, i).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                }

                for (int i = (currentRow + 1); i <= worksheet.LastRowUsed().RowNumber(); i++)
                {
                    worksheet.Row(i).Clear();
                }

                foreach (var dataShift in table.Rows)
                {
                    currentRow++;
                    //New template
                    worksheet.Cell(currentRow, 1).Value = "'" + dataShift.EmployeeATID;
                    worksheet.Cell(currentRow, 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    worksheet.Cell(currentRow, 2).Value = "'" + dataShift.EmployeeCode;
                    worksheet.Cell(currentRow, 2).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    worksheet.Cell(currentRow, 3).Value = dataShift.EmployeeName;
                    worksheet.Cell(currentRow, 3).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    worksheet.Cell(currentRow, 4).Value = "'" + dataShift.DepartmentName;
                    worksheet.Cell(currentRow, 4).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    var numberCell = 5;
                    foreach (var date in listDate)
                    {
                        worksheet.Cell(currentRow, numberCell).Value = dataShift.ListColumnObjectUsed.FirstOrDefault(x => x.KeyMain == date.Date.ToString("dd/MM/yyyy"))?.ShiftName;
                        worksheet.Cell(currentRow, numberCell).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        numberCell++;
                    }

                    worksheet.Cell(currentRow, numberCell).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(currentRow, numberCell).Style.Alignment.SetWrapText(true);
                }


                MemoryStream stream = new MemoryStream();
                workbook.SaveAs(stream);

                return new FileContentResult(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                {
                    FileDownloadName = $"Data_Employee_Shift.xlsx"
                };
            }
        }

        [Authorize]
        [ActionName("AddEmployeeShift")]
        [HttpPost]
        public IActionResult AddEmployeeShift(List<CM_EmployeeShiftModel> param)
        {
            try
            {
                var deleteData = _TA_EmployeeShiftService.DeleteEmployeeShift(param);
                if (!deleteData)
                {
                    return ApiError("Failed");
                }
                var scheduleDepartmentList = _TA_ScheduleFixedByDepartmentService.Where(x => param.Select(z => z.DepartmentIndex).Contains(x.DepartmentIndex)).ToList();
                var scheduleEmployeeList = _TA_ScheduleFixedByEmployeeService.Where(x => param.Select(z => z.EmployeeATID).Contains(x.EmployeeATID)).ToList();



                foreach (var item in param)
                {
                    var dateConvert = DateTime.ParseExact(item.DateStr, "dd/MM/yyyy", null);
                    var dayOfWeek = dateConvert.DayOfWeek;
                    var shiftByDepartmentScheduleFixed = scheduleDepartmentList.FirstOrDefault(x => x.DepartmentIndex == item.DepartmentIndex && dateConvert.Date >= x.FromDate.Date
                                                                                           && (x.ToDate == null || (x.ToDate != null && dateConvert.Date <= x.ToDate.Value.Date)));

                    var shiftByEmployeeScheduleFixed = scheduleEmployeeList.FirstOrDefault(x => x.EmployeeATID == item.EmployeeATID && x.FromDate.Date <= dateConvert
                                                                                     && (x.ToDate == null || (x.ToDate != null && dateConvert.Date <= x.ToDate.Value.Date)));
                    if (shiftByEmployeeScheduleFixed != null)
                    {
                        var dayOfWeekProperty = shiftByEmployeeScheduleFixed.GetType().GetProperty(dayOfWeek.ToString());
                        if (dayOfWeekProperty != null)
                        {
                            var dayOfWeekValue = (int)dayOfWeekProperty.GetValue(shiftByEmployeeScheduleFixed);
                            if (dayOfWeekValue == item.ShiftIndex)
                            {
                                continue;
                            }
                        }
                    }

                    if (shiftByDepartmentScheduleFixed != null)
                    {
                        var dayOfWeekProperty = shiftByDepartmentScheduleFixed.GetType().GetProperty(dayOfWeek.ToString());
                        if (dayOfWeekProperty != null)
                        {
                            var dayOfWeekValue = (int)dayOfWeekProperty.GetValue(shiftByDepartmentScheduleFixed);
                            if (dayOfWeekValue == item.ShiftIndex)
                            {
                                continue;
                            }
                        }

                    }

                    var entity = new TA_EmployeeShift();
                    entity.CompanyIndex = item.CompanyIndex;
                    entity.EmployeeATID = item.EmployeeATID;
                    entity.ShiftIndex = item.ShiftIndex;
                    entity.Date = DateTime.ParseExact(item.DateStr, "dd/MM/yyyy", null);
                    _TA_EmployeeShiftService.Insert(entity);

                }
                SaveChange();
                return ApiOk();
            }
            catch (Exception ex)
            {
                _logger.LogError($"AddEmployeeShift: {ex}");
                return ApiOk("Failed");
            }
        }

        [ActionName("ImportExcelEmployeeShift")]
        [HttpPost]
        public async Task<IActionResult> ImportExcelEmployeeShift([FromBody] ImportShiftTableRequest<EmployeeShiftTableRequest> request)
        {
            UserInfo user = GetUserInfo();
            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }
            var dataImport = request.Data.ToList();

            // validation data
            var listError = new List<EmployeeShiftTableRequest>();

            listError = await _TA_EmployeeShiftService.ImportShiftTable(dataImport, user);
            var message = "";
            string sWebRootFolder = _hostingEnvironment.ContentRootPath;
            string URL = string.Format("{0}://{1}/{2}", Request.Scheme, Request.Host, @"Files/Template_EmployeeShift_Error.xlsx");
            FileInfo file = new FileInfo(Path.Combine(sWebRootFolder, @"Files/Template_EmployeeShift_Error.xlsx"));

            if (listError != null && listError.Count() > 0)
            {
                message = listError.Count().ToString();

                var listShift = dataImport.FirstOrDefault().DailyShifts;
                var minDate = listShift.Select(x => x.Date).Min();
                var maxDate = listShift.Select(x => x.Date).Max();

                var listDate = Enumerable.Range(0, 1 + maxDate.Date.Subtract(minDate.Date).Days)
                            .Select(offset => minDate.Date.AddDays(offset)).ToList();

                using (var workbook = new XLWorkbook(file.FullName))
                {
                    var worksheet = workbook.Worksheet(1);
                    var currentRow = 1;
                    worksheet.Cell(currentRow, 1).Value = "MCC (*)";
                    worksheet.Cell(currentRow, 2).Value = "Mã nhân viên";
                    worksheet.Cell(currentRow, 3).Value = "Nhân viên";
                    worksheet.Cell(currentRow, 4).Value = "Phòng ban";
                    var numberRow = 5;
                    foreach (var item in listDate)
                    {
                        worksheet.Cell(currentRow, numberRow).Value = item.Day;
                        numberRow++;
                    }
                    worksheet.Cell(currentRow, numberRow).Value = "Lỗi";

                    for (int i = 1; i <= numberRow; i++)
                    {
                        worksheet.Cell(currentRow, i).Style.Fill.BackgroundColor = XLColor.Yellow;
                        worksheet.Cell(currentRow, i).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    }

                    for (int i = (currentRow + 1); i <= worksheet.LastRowUsed().RowNumber(); i++)
                    {
                        worksheet.Row(i).Clear();
                    }

                    foreach (var department in listError)
                    {
                        currentRow++;
                        //New template
                        worksheet.Cell(currentRow, 1).Value = "'" + department.EmployeeATID;
                        worksheet.Cell(currentRow, 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                        worksheet.Cell(currentRow, 2).Value = "'" + department.EmployeeCode;
                        worksheet.Cell(currentRow, 2).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                        worksheet.Cell(currentRow, 3).Value = department.Employee;
                        worksheet.Cell(currentRow, 3).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                        worksheet.Cell(currentRow, 4).Value = "'" + department.Department;
                        worksheet.Cell(currentRow, 4).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                        var numberCell = 5;
                        foreach (var date in listDate)
                        {
                            worksheet.Cell(currentRow, numberCell).Value = department.DailyShifts.FirstOrDefault(x => x.Date.Date == date.Date)?.ShiftValue;
                            worksheet.Cell(currentRow, numberCell).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                            numberCell++;
                        }

                        worksheet.Cell(currentRow, numberCell).Value = department.ErrorMessage;
                        worksheet.Cell(currentRow, numberCell).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        worksheet.Cell(currentRow, numberCell).Style.Alignment.SetWrapText(true);
                    }
                    worksheet.Columns().AdjustToContents();
                    worksheet.Rows().AdjustToContents();
                    workbook.SaveAs(file.FullName);
                }
            }

            result = Ok(message);
            return result;
        }
    }
}
