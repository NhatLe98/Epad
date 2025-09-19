using ClosedXML.Excel;
using EPAD_Common.Enums;
using EPAD_Common.Extensions;
using EPAD_Common.Services;
using EPAD_Common.Types;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using EPAD_Services.Interface;
using EPAD_Services.Plugins;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPAD_Services.Impl
{
    public class TA_ScheduleFixedByEmployeeService : BaseServices<TA_ScheduleFixedByEmployee, EPAD_Context>, ITA_ScheduleFixedByEmployeeService
    {
        public EPAD_Context _dbContext;
        ConfigObject _Config;
        IMemoryCache _Cache;
        private ILogger _logger;
        private IConfiguration _configuration;
        private readonly IHR_UserService _HR_UserService;
        public TA_ScheduleFixedByEmployeeService(IServiceProvider serviceProvider, EPAD_Context context,
            ILoggerFactory loggerFactory, IConfiguration configuration) : base(serviceProvider)
        {
            _dbContext = context;
            _Cache = serviceProvider.GetService<IMemoryCache>();
            _Config = ConfigObject.GetConfig(_Cache);
            _logger = loggerFactory.CreateLogger<GC_GatesService>();
            _configuration = configuration;
            _HR_UserService = serviceProvider.GetService<IHR_UserService>();
        }
        public async Task<string> CheckScheduleFixedByEmployeeExist(TA_ScheduleFixedByEmployeeDTO data, int companyIndex)
        {
            var dataExist = new List<TA_ScheduleFixedByEmployee>();
            if (data.ToDate == null)
            {
                dataExist = _dbContext.TA_ScheduleFixedByEmployee
               .Where(x => x.Index != data.Id &&
                           data.EmployeeATIDs.Contains(x.EmployeeATID) &&
                           (x.FromDate.Date == data.FromDate.Date || x.ToDate.Value.Date == data.FromDate.Date
                           || x.FromDate >= data.FromDate
                           || (x.ToDate == null)
                           || (x.FromDate.Date > data.FromDate.Date && x.ToDate.Value.Date < data.FromDate.Date))).ToList();
            }
            else
            {
                dataExist = _dbContext.TA_ScheduleFixedByEmployee
               .Where(x => x.Index != data.Id &&
                           data.EmployeeATIDs.Contains(x.EmployeeATID) &&
                           (x.FromDate.Date == data.FromDate.Date || x.ToDate.Value.Date == data.FromDate.Date
                           || ((x.FromDate.Date >= data.FromDate.Date && x.FromDate.Date <= data.ToDate.Value.Date) && x.ToDate == null)
                           || ((x.FromDate.Date <= data.FromDate.Date || x.FromDate.Date <= data.ToDate.Value.Date) && x.ToDate == null)
                           || ((x.FromDate.Date <= data.FromDate.Date && x.ToDate.Value.Date >= data.FromDate.Date) || ((x.FromDate.Date <= data.ToDate.Value.Date && x.ToDate.Value.Date >= data.ToDate.Value.Date))))).ToList();
            }

            if (dataExist != null && dataExist.Count > 0)
            {
                var employeeATIDList = dataExist.Select(x => x.EmployeeATID).Distinct().ToList();
                //var classInfo = await _HR_UserService.GetDepartmentByIds(classIndex.Select(x => x.ToString()).ToList(), companyIndex);
                var errorString = string.Join(",", employeeATIDList.ToList());
                return errorString;
            }
            return string.Empty;
        }

        public async Task<bool> AddScheduleFixedByEmployee(TA_ScheduleFixedByEmployeeDTO data, int companyIndex)
        {
            var result = true;
            try
            {
                var scheduleFixedByEmployeeList = new List<TA_ScheduleFixedByEmployee>();
                foreach (var employee in data.EmployeeATIDs)
                {
                    var scheduleFixedByDepartment = new TA_ScheduleFixedByEmployee()
                    {
                        CompanyIndex = companyIndex,
                        EmployeeATID = employee,
                        FromDate = data.FromDate,
                        ToDate = data.ToDate,
                        Monday = data.Monday,
                        Tuesday = data.Tuesday,
                        Wednesday = data.Wednesday,
                        Thursday = data.Thursday,
                        Friday = data.Friday,
                        Saturday = data.Saturday,
                        Sunday = data.Sunday,
                        UpdateDate = data.UpdateDate,
                        UpdateUser = data.UpdateUser
                    };
                    scheduleFixedByEmployeeList.Add(scheduleFixedByDepartment);
                }

                await _dbContext.TA_ScheduleFixedByEmployee.AddRangeAsync(scheduleFixedByEmployeeList);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"AddScheduleFixedByEmployee: {ex}");
                result = false;
            }
            return result;
        }

        public async Task<bool> UpdateScheduleFixedByEmployee(TA_ScheduleFixedByEmployee dataExist, TA_ScheduleFixedByEmployeeDTO data, int companyIndex)
        {
            var result = true;
            try
            {
                _dbContext.Remove(dataExist);
                _dbContext.SaveChanges();

                var scheduleFixedByDepartment = new TA_ScheduleFixedByEmployee()
                {
                    CompanyIndex = companyIndex,
                    EmployeeATID = data.EmployeeATID,
                    FromDate = data.FromDate,
                    ToDate = data.ToDate,
                    Monday = data.Monday,
                    Tuesday = data.Tuesday,
                    Wednesday = data.Wednesday,
                    Thursday = data.Thursday,
                    Friday = data.Friday,
                    Saturday = data.Saturday,
                    Sunday = data.Sunday,
                    UpdateDate = data.UpdateDate,
                    UpdateUser = data.UpdateUser
                };

                await _dbContext.TA_ScheduleFixedByEmployee.AddAsync(scheduleFixedByDepartment);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"UpdateScheduleFixedByEmployee: {ex}");
                result = false;
            }
            return result;
        }

        public bool DeleteScheduleFixedByEmployee(List<int> dataDelete)
        {
            using (var transaction = DbContext.Database.BeginTransaction())
            {
                try
                {
                    var statusDelete = _dbContext.TA_ScheduleFixedByEmployee.Where(x => dataDelete.Contains(x.Index)).ToList();
                    if (statusDelete != null && statusDelete.Count > 0)
                    {
                        _dbContext.RemoveRange(statusDelete);
                    }
                    _dbContext.SaveChanges();
                    transaction.Commit();
                    return true;
                }
                catch (Exception ex)
                {
                    _logger.LogError($"DeleteScheduleFixedByDepartment: ", ex);
                    transaction.Rollback();
                    return false;
                }
            }
        }

        public async Task<DataGridClass> GetScheduleFixedByEmployee(List<string> employeeFilter, DateTime date, int pCompanyIndex, int pPage, int pLimit)
        {
            try
            {
                DataGridClass dataGrid = null;
                int countPage = 0;
                var scheduleFixedByEmployeeList = _dbContext.TA_ScheduleFixedByEmployee.Where(x => date.Date.Date >= x.FromDate.Date && (x.ToDate == null || (x.ToDate != null && date.Date.Date <= x.ToDate.Value.Date))).ToList();

                if (employeeFilter != null && employeeFilter.Count > 0)
                {
                    scheduleFixedByEmployeeList = scheduleFixedByEmployeeList.Where(x => employeeFilter.Contains(x.EmployeeATID)).ToList();
                }

                var employeeATIDs = scheduleFixedByEmployeeList.Select(x => x.EmployeeATID.ToString()).ToList();

                var employeeInfoList = await _HR_UserService.GetEmployeeCompactInfoByEmployeeATID(employeeATIDs, DateTime.Now, pCompanyIndex);
                var shiftInfo = _dbContext.TA_Shift.ToList();
                var listData = new List<ScheduleFixedByEmployeeReponse>();
                foreach (var item in scheduleFixedByEmployeeList)
                {
                    var departmentInfo = employeeInfoList.FirstOrDefault(x => x.EmployeeATID == item.EmployeeATID);
                    listData.Add(new ScheduleFixedByEmployeeReponse()
                    {
                        Id = item.Index,
                        FromDate = item.FromDate,
                        FromDateFormat = item.FromDate.ToddMMyyyy(),
                        ToDate = item.ToDate,
                        ToDateFormat = item.ToDate != null ? item.ToDate.Value.ToddMMyyyy() : "",
                        EmployeeATID = item.EmployeeATID,
                        EmployeeName = employeeInfoList.FirstOrDefault(x => x.EmployeeATID == item.EmployeeATID)?.FullName,
                        EmployeeCode = employeeInfoList.FirstOrDefault(x => x.EmployeeATID == item.EmployeeATID)?.EmployeeCode,
                        EmployeeATIDs = new List<string>() { item.EmployeeATID },
                        DepartmentName = departmentInfo != null ? departmentInfo.Department : "",
                        DepartmentList = new List<long?>() { departmentInfo != null ? departmentInfo.DepartmentIndex : 0 },
                        Monday = item != null && item.Monday > 0 ? item.Monday : (int?)null,
                        MondayShift = shiftInfo.FirstOrDefault(x => x.Index == item.Monday)?.Code,
                        Tuesday = item != null && item.Tuesday > 0 ? item.Tuesday : (int?)null,
                        TuesdayShift = shiftInfo.FirstOrDefault(x => x.Index == item.Tuesday)?.Code,
                        Wednesday = item != null && item.Wednesday > 0 ? item.Wednesday : (int?)null,
                        WednesdayShift = shiftInfo.FirstOrDefault(x => x.Index == item.Wednesday)?.Code,
                        Thursday = item != null && item.Thursday > 0 ? item.Thursday : (int?)null,
                        ThursdayShift = shiftInfo.FirstOrDefault(x => x.Index == item.Thursday)?.Code,
                        Friday = item != null && item.Friday > 0 ? item.Friday : (int?)null,
                        FridayShift = shiftInfo.FirstOrDefault(x => x.Index == item.Friday)?.Code,
                        Saturday = item != null && item.Saturday > 0 ? item.Saturday : (int?)null,
                        SaturdayShift = shiftInfo.FirstOrDefault(x => x.Index == item.Saturday)?.Code,
                        Sunday = item != null && item.Sunday > 0 ? item.Sunday : (int?)null,
                        SundayShift = shiftInfo.FirstOrDefault(x => x.Index == item.Sunday)?.Code,
                    });
                }


                countPage = listData.Count();
                dataGrid = new DataGridClass(countPage, listData);
                if (pPage <= 1)
                {
                    var lsDevice = listData.OrderBy(t => t.DepartmentName).Take(pLimit).ToList();
                    dataGrid = new DataGridClass(countPage, lsDevice);
                }
                else
                {
                    int fromRow = pLimit * (pPage - 1);
                    var lsDevice = listData.OrderBy(t => t.DepartmentName).Skip(fromRow).Take(pLimit).ToList();
                    dataGrid = new DataGridClass(countPage, lsDevice);
                }
                return dataGrid;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<ScheduleFixedByEmployeeImportExcel>> AddScheduleFixedByEmployeeFromExcel(List<ScheduleFixedByEmployeeImportExcel> param, UserInfo user)
        {
            var result = param;
            try
            {
                var shiftList = _dbContext.TA_Shift.ToList();
                var employeeList = await _HR_UserService.GetEmployeeCompactInfoByListEmpATID(param.Select(x => x.EmployeeATID).ToList(), user.CompanyIndex);
                string[] formats = { "dd/MM/yyyy" };

                foreach (var item in result)
                {
                    if (string.IsNullOrWhiteSpace(item.FromDateFormat))
                    {
                        item.ErrorMessage += "Ngày áp dụng không được để trống\r\n";
                    }
                    else if (!string.IsNullOrWhiteSpace(item.FromDateFormat))
                    {
                        var fromDate = new DateTime();
                        var convertFromDate = DateTime.TryParseExact(item.FromDateFormat, formats,
                            System.Globalization.CultureInfo.InvariantCulture,
                            System.Globalization.DateTimeStyles.None, out fromDate);
                        if (!convertFromDate)
                        {
                            item.ErrorMessage += "Ngày áp dụng không hợp lệ\r\n";
                        }
                        else
                        {
                            item.FromDate = fromDate;
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(item.ToDateFormat))
                    {
                        var toDate = new DateTime();
                        var convertToDate = DateTime.TryParseExact(item.ToDateFormat, formats,
                            System.Globalization.CultureInfo.InvariantCulture,
                            System.Globalization.DateTimeStyles.None, out toDate);
                        if (!convertToDate)
                        {
                            item.ErrorMessage += "Đến ngày không hợp lệ\r\n";
                        }
                        else
                        {
                            item.ToDate = toDate;
                        }
                    }

                    if (item.ToDate.HasValue && item.ToDate.Value.Date < item.FromDate.Date)
                    {
                        item.ErrorMessage += "Ngày áp dụng không được lớn hơn đến ngày\r\n";
                    }

                    if (string.IsNullOrWhiteSpace(item.EmployeeATID))
                    {
                        item.ErrorMessage += "Mã chấm công không được để trống\r\n";
                    }
                    else if (!string.IsNullOrWhiteSpace(item.EmployeeATID))
                    {
                        var employee = employeeList.FirstOrDefault(y => y.EmployeeATID == item.EmployeeATID);
                        if (employee != null)
                        {
                            item.EmployeeATID = employee.EmployeeATID;
                        }
                        else
                        {
                            item.ErrorMessage += "Nhân viên không tồn tại\r\n";
                        }
                    }
                    //----
                    if (!string.IsNullOrWhiteSpace(item.MondayShift))
                    {
                        var shiftInfo = shiftList.FirstOrDefault(y => y.Code == item.MondayShift);
                        if (shiftInfo != null)
                        {
                            item.Monday = shiftInfo.Index;
                        }
                        else
                        {
                            item.ErrorMessage += "Ca làm việc thứ hai không tồn tại\r\n";
                        }
                    }
                    if (!string.IsNullOrWhiteSpace(item.TuesdayShift))
                    {
                        var shiftInfo = shiftList.FirstOrDefault(y => y.Code == item.TuesdayShift);
                        if (shiftInfo != null)
                        {
                            item.Tuesday = shiftInfo.Index;
                        }
                        else
                        {
                            item.ErrorMessage += "Ca làm việc thứ ba không tồn tại\r\n";
                        }
                    }
                    if (!string.IsNullOrWhiteSpace(item.WednesdayShift))
                    {
                        var shiftInfo = shiftList.FirstOrDefault(y => y.Code == item.WednesdayShift);
                        if (shiftInfo != null)
                        {
                            item.Wednesday = shiftInfo.Index;
                        }
                        else
                        {
                            item.ErrorMessage += "Ca làm việc thứ tư không tồn tại\r\n";
                        }
                    }
                    if (!string.IsNullOrWhiteSpace(item.ThursdayShift))
                    {
                        var shiftInfo = shiftList.FirstOrDefault(y => y.Code == item.ThursdayShift);
                        if (shiftInfo != null)
                        {
                            item.Thursday = shiftInfo.Index;
                        }
                        else
                        {
                            item.ErrorMessage += "Ca làm việc thứ năm không tồn tại\r\n";
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(item.FridayShift))
                    {
                        var shiftInfo = shiftList.FirstOrDefault(y => y.Code == item.FridayShift);
                        if (shiftInfo != null)
                        {
                            item.Friday = shiftInfo.Index;
                        }
                        else
                        {
                            item.ErrorMessage += "Ca làm việc thứ sáu không tồn tại\r\n";
                        }

                    }

                    if (!string.IsNullOrWhiteSpace(item.SaturdayShift))
                    {
                        var shiftInfo = shiftList.FirstOrDefault(y => y.Code == item.SaturdayShift);
                        if (shiftInfo != null)
                        {
                            item.Saturday = shiftInfo.Index;
                        }
                        else
                        {
                            item.ErrorMessage += "Ca làm việc thứ bảy không tồn tại\r\n";
                        }
                    }
                    if (!string.IsNullOrWhiteSpace(item.SundayShift))
                    {
                        var shiftInfo = shiftList.FirstOrDefault(y => y.Code == item.SundayShift);
                        if (shiftInfo != null)
                        {
                            item.Sunday = shiftInfo.Index;
                        }
                        else
                        {
                            item.ErrorMessage += "Ca làm việc chủ nhật không tồn tại\r\n";
                        }
                    }
                }


                var dataScheduleFixedByDepartment = _dbContext.TA_ScheduleFixedByEmployee.Where(x => result.Select(z => z.EmployeeATID).Contains(x.EmployeeATID)).ToList();
                var logExistInFile = new List<TA_ScheduleFixedByEmployee>();
                foreach (var scheduleInfo in result)
                {
                    //var dataScheduleFixedByDepartmentExist = CheckScheduleFixedByEmployeeExistImport(dataScheduleFixedByDepartment, scheduleInfo);
                    var dataScheduleFixedByDepartmentExist = new TA_ScheduleFixedByEmployee();

                    if (scheduleInfo.ToDate == null)
                    {
                        dataScheduleFixedByDepartmentExist = dataScheduleFixedByDepartment
                       .FirstOrDefault(x => x.EmployeeATID == scheduleInfo.EmployeeATID &&
                                   (x.FromDate == scheduleInfo.FromDate
                                   || x.ToDate == scheduleInfo.FromDate
                                   || x.FromDate >= scheduleInfo.FromDate
                                   || (x.ToDate == null)
                                   || (x.FromDate > scheduleInfo.FromDate && x.ToDate < scheduleInfo.FromDate)
                                   || (x.FromDate <= scheduleInfo.FromDate && x.ToDate >= scheduleInfo.FromDate)));
                    }
                    else
                    {
                        dataScheduleFixedByDepartmentExist = dataScheduleFixedByDepartment
                       .FirstOrDefault(x => x.EmployeeATID == scheduleInfo.EmployeeATID &&
                                   (x.FromDate == scheduleInfo.FromDate || x.ToDate == scheduleInfo.FromDate
                                   || ((x.FromDate >= scheduleInfo.FromDate && x.FromDate <= scheduleInfo.ToDate) && x.ToDate == null)
                                   || ((x.FromDate <= scheduleInfo.FromDate || x.FromDate <= scheduleInfo.ToDate) && x.ToDate == null)
                                   || ((x.FromDate <= scheduleInfo.FromDate && x.ToDate >= scheduleInfo.FromDate) || ((x.FromDate <= scheduleInfo.ToDate && x.ToDate >= scheduleInfo.ToDate)))));
                    }

                    if (dataScheduleFixedByDepartmentExist != null && scheduleInfo.FromDate.Date != dataScheduleFixedByDepartmentExist.FromDate.Date)
                    {
                        scheduleInfo.ErrorMessage += "Nhân viên đã được khai báo lịch cố định trong cùng khoảng thời gian\r\n";
                    }

                    var dataExist = new TA_ScheduleFixedByEmployee();
                    if (string.IsNullOrWhiteSpace(scheduleInfo.ErrorMessage))
                    {
                        if (scheduleInfo.ToDate == null)
                        {
                            dataExist = logExistInFile
                            .FirstOrDefault(x => x.EmployeeATID == scheduleInfo.EmployeeATID &&
                                       (x.FromDate == scheduleInfo.FromDate || x.ToDate == scheduleInfo.FromDate
                                         || x.FromDate >= scheduleInfo.FromDate
                                       || (x.ToDate == null)
                                       || (x.FromDate > scheduleInfo.FromDate && x.ToDate < scheduleInfo.FromDate)
                                       || (x.FromDate <= scheduleInfo.FromDate && x.ToDate >= scheduleInfo.FromDate)));
                        }
                        else
                        {


                            dataExist = logExistInFile
                            .FirstOrDefault(x => x.EmployeeATID == scheduleInfo.EmployeeATID &&
                                       (x.FromDate == scheduleInfo.FromDate || x.ToDate == scheduleInfo.FromDate
                                       || ((x.FromDate >= scheduleInfo.FromDate && x.FromDate <= scheduleInfo.ToDate) && x.ToDate == null)
                                       || ((x.FromDate <= scheduleInfo.FromDate || x.FromDate <= scheduleInfo.ToDate) && x.ToDate == null)
                                       || ((x.FromDate <= scheduleInfo.FromDate && x.ToDate >= scheduleInfo.FromDate) || ((x.FromDate <= scheduleInfo.ToDate && x.ToDate >= scheduleInfo.ToDate)))));
                        }

                        if (dataExist != null)
                        {
                            scheduleInfo.ErrorMessage += "Dữ liệu bị trùng trong tập tin\r\n";
                        }

                    }


                    //if (scheduleInfo.ToDate.HasValue && scheduleInfo.FromDate.Date <= scheduleInfo.ToDate.Value.Date
                    //       && logExistInFile.Any(x => x.EmployeeATID == scheduleInfo.EmployeeATID
                    //       && DoRangesOverlap(scheduleInfo.FromDate.Date, scheduleInfo.ToDate.Value.Date, x.FromDate.Date, x.ToDate.Value.Date)))
                    //{
                    //    scheduleInfo.ErrorMessage += "Dữ liệu bị trùng trong tập tin\r\n";
                    //}

                    logExistInFile.Add(scheduleInfo);
                }

                var noErrorParam = result.Where(x => string.IsNullOrWhiteSpace(x.ErrorMessage)).ToList();
                if (noErrorParam != null && noErrorParam.Count > 0)
                {
                    foreach (var scheduleFixedEmployee in noErrorParam)
                    {
                        var dataScheduleFixedByEmployeeExist = CheckScheduleFixedByEmployeeExistImport(dataScheduleFixedByDepartment, scheduleFixedEmployee);
                        if (dataScheduleFixedByEmployeeExist != null && scheduleFixedEmployee.FromDate.Date == dataScheduleFixedByEmployeeExist.FromDate.Date)
                        {
                            dataScheduleFixedByEmployeeExist.CompanyIndex = user.CompanyIndex;
                            dataScheduleFixedByEmployeeExist.EmployeeATID = scheduleFixedEmployee.EmployeeATID;
                            dataScheduleFixedByEmployeeExist.FromDate = scheduleFixedEmployee.FromDate;
                            dataScheduleFixedByEmployeeExist.ToDate = scheduleFixedEmployee.ToDate;
                            dataScheduleFixedByEmployeeExist.Monday = scheduleFixedEmployee.Monday;
                            dataScheduleFixedByEmployeeExist.Tuesday = scheduleFixedEmployee.Tuesday;
                            dataScheduleFixedByEmployeeExist.Wednesday = scheduleFixedEmployee.Wednesday;
                            dataScheduleFixedByEmployeeExist.Thursday = scheduleFixedEmployee.Thursday;
                            dataScheduleFixedByEmployeeExist.Friday = scheduleFixedEmployee.Friday;
                            dataScheduleFixedByEmployeeExist.Saturday = scheduleFixedEmployee.Saturday;
                            dataScheduleFixedByEmployeeExist.Sunday = scheduleFixedEmployee.Sunday;
                            dataScheduleFixedByEmployeeExist.UpdateDate = scheduleFixedEmployee.UpdateDate;
                            dataScheduleFixedByEmployeeExist.UpdateUser = scheduleFixedEmployee.UpdateUser;
                            _dbContext.TA_ScheduleFixedByEmployee.Update(dataScheduleFixedByEmployeeExist);
                        }
                        if (dataScheduleFixedByEmployeeExist == null)
                        {
                            var scheduleFixedByEmployee = new TA_ScheduleFixedByEmployee()
                            {
                                CompanyIndex = user.CompanyIndex,
                                EmployeeATID = scheduleFixedEmployee.EmployeeATID,
                                FromDate = scheduleFixedEmployee.FromDate,
                                ToDate = scheduleFixedEmployee.ToDate,
                                Monday = scheduleFixedEmployee.Monday,
                                Tuesday = scheduleFixedEmployee.Tuesday,
                                Wednesday = scheduleFixedEmployee.Wednesday,
                                Thursday = scheduleFixedEmployee.Thursday,
                                Friday = scheduleFixedEmployee.Friday,
                                Saturday = scheduleFixedEmployee.Saturday,
                                Sunday = scheduleFixedEmployee.Sunday,
                                UpdateDate = scheduleFixedEmployee.UpdateDate,
                                UpdateUser = scheduleFixedEmployee.UpdateUser,
                            };

                            _dbContext.TA_ScheduleFixedByEmployee.Add(scheduleFixedByEmployee);
                        }
                    }
                    await _dbContext.SaveChangesAsync();

                }
                var errorList = result.Where(x => !string.IsNullOrEmpty(x.ErrorMessage)).ToList();
                return errorList;
            }
            catch (Exception ex)
            {
                _logger.LogError("AddScheduleFixedByEmployeeFromExcel: " + ex);
            }
            return result;
        }

        public TA_ScheduleFixedByEmployee CheckScheduleFixedByEmployeeExistImport(List<TA_ScheduleFixedByEmployee> listData, ScheduleFixedByEmployeeImportExcel data)
        {
            var dataExist = new TA_ScheduleFixedByEmployee();
            if (data.ToDate == null)
            {
                dataExist = listData
               .FirstOrDefault(x => x.EmployeeATID == data.EmployeeATID &&
                                       (x.FromDate == data.FromDate || x.ToDate == data.FromDate
                                         || x.FromDate >= data.FromDate
                                       || (x.ToDate == null)
                                       || (x.FromDate > data.FromDate && x.ToDate < data.FromDate)
                                       || (x.FromDate <= data.FromDate && x.ToDate >= data.FromDate)));
            }
            else
            {
                dataExist = listData.FirstOrDefault(x => x.EmployeeATID == data.EmployeeATID &&
                                       (x.FromDate == data.FromDate || x.ToDate == data.FromDate
                                       || ((x.FromDate >= data.FromDate && x.FromDate <= data.ToDate) && x.ToDate == null)
                                       || ((x.FromDate <= data.FromDate || x.FromDate <= data.ToDate) && x.ToDate == null)
                                       || ((x.FromDate <= data.FromDate && x.ToDate >= data.FromDate) || ((x.FromDate <= data.ToDate && x.ToDate >= data.ToDate)))));
            }

            return dataExist;
        }

        public bool ExportInfoShift(string folderDetails)
        {
            try
            {
                var shiftList = _dbContext.TA_Shift.Select(x => x.Code).OrderByDescending(x => x).ToList();
                using (var workbook = new XLWorkbook(folderDetails))
                {
                    var worksheet = workbook.Worksheets;
                    IXLWorksheet worksheet1;
                    IXLWorksheet worksheet3;
                    var w1 = worksheet.TryGetWorksheet("ShiftData", out worksheet1);
                    worksheet1.Cells().Clear();
                    string startShiftCell = "A2";
                    string endShiftCell = string.Empty;
                    for (int i = 0; i < shiftList.Count; i++)
                    {
                        if (i == (shiftList.Count - 1))
                        {
                            endShiftCell = "A" + (i + 1);
                        }
                        worksheet1.Cell("A" + (i + 1)).Value = shiftList[i];
                    }

                    var w = worksheet.TryGetWorksheet("Data", out worksheet3);
                    worksheet3.Range("F2:F10003").SetDataValidation().List(worksheet1.Range(startShiftCell + ":" + endShiftCell), true);
                    worksheet3.Range("G2:G10003").SetDataValidation().List(worksheet1.Range(startShiftCell + ":" + endShiftCell), true);
                    worksheet3.Range("H2:H10003").SetDataValidation().List(worksheet1.Range(startShiftCell + ":" + endShiftCell), true);
                    worksheet3.Range("I2:I10003").SetDataValidation().List(worksheet1.Range(startShiftCell + ":" + endShiftCell), true);
                    worksheet3.Range("J2:J10003").SetDataValidation().List(worksheet1.Range(startShiftCell + ":" + endShiftCell), true);
                    worksheet3.Range("K2:K10003").SetDataValidation().List(worksheet1.Range(startShiftCell + ":" + endShiftCell), true);
                    worksheet3.Range("L2:L10003").SetDataValidation().List(worksheet1.Range(startShiftCell + ":" + endShiftCell), true);
                    workbook.Save();
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError("ExportInfoShift: ", ex);
                return false;
            }
        }

        public bool DoRangesOverlap(DateTime obj1FromTime, DateTime obj1ToTime, DateTime obj2FromTime, DateTime obj2ToTime)
        {
            // Check if one range is fully contained within the other range
            if ((obj1FromTime >= obj2FromTime && obj1FromTime <= obj2ToTime) ||
                (obj1ToTime >= obj2FromTime && obj1ToTime <= obj2ToTime) ||
                (obj2FromTime >= obj1FromTime && obj2FromTime <= obj1ToTime) ||
                (obj2ToTime >= obj1FromTime && obj2ToTime <= obj1ToTime))
            {
                return true; // The ranges overlap
            }

            return false; // The ranges do not overlap
        }
    }
}
