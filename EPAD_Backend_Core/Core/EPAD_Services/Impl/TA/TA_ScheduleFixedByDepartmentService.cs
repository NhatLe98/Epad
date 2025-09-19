using ClosedXML.Excel;
using DocumentFormat.OpenXml.Drawing.Charts;
using EPAD_Common.Enums;
using EPAD_Common.Extensions;
using EPAD_Common.Services;
using EPAD_Common.Types;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using EPAD_Data.Sync_Entities;
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
using static EPAD_Common.Utility.AppUtils;

namespace EPAD_Services.Impl
{
    public class TA_ScheduleFixedByDepartmentService : BaseServices<TA_ScheduleFixedByDepartment, EPAD_Context>, ITA_ScheduleFixedByDepartmentService
    {
        public EPAD_Context _dbContext;
        ConfigObject _Config;
        IMemoryCache _Cache;
        private ILogger _logger;
        private IConfiguration _configuration;
        private readonly IHR_UserService _HR_UserService;
        private readonly IIC_DepartmentService _IC_DepartmentService;
        public TA_ScheduleFixedByDepartmentService(IServiceProvider serviceProvider, EPAD_Context context,
            ILoggerFactory loggerFactory, IConfiguration configuration) : base(serviceProvider)
        {

            _dbContext = context;
            _Cache = serviceProvider.GetService<IMemoryCache>();
            _Config = ConfigObject.GetConfig(_Cache);
            _logger = loggerFactory.CreateLogger<GC_GatesService>();
            _configuration = configuration;
            _HR_UserService = serviceProvider.GetService<IHR_UserService>();
            _IC_DepartmentService = serviceProvider.GetService<IIC_DepartmentService>();
        }
        public async Task<string> CheckScheduleFixedByDepartmentExist(TA_ScheduleFixedByDepartmentDTO data, int companyIndex)
        {
            var dataExist = new List<TA_ScheduleFixedByDepartment>();
            if (data.ToDate == null)
            {
                dataExist = _dbContext.TA_ScheduleFixedByDepartment
               .Where(x => x.Index != data.Id &&
                           data.DepartmentList.Contains(x.DepartmentIndex) &&
                           (x.FromDate.Date == data.FromDate.Date || x.ToDate.Value.Date == data.FromDate.Date
                           || x.FromDate >= data.FromDate
                           || (x.ToDate == null)
                           || (x.FromDate.Date > data.FromDate.Date && x.ToDate.Value.Date < data.FromDate.Date))).ToList();
            }
            else
            {
                dataExist = _dbContext.TA_ScheduleFixedByDepartment
               .Where(x => x.Index != data.Id &&
                           data.DepartmentList.Contains(x.DepartmentIndex) &&
                           (x.FromDate.Date == data.FromDate.Date || x.ToDate.Value.Date == data.FromDate.Date
                           || ((x.FromDate.Date >= data.FromDate.Date && x.FromDate.Date <= data.ToDate.Value.Date) && x.ToDate == null)
                           || ((x.FromDate.Date <= data.FromDate.Date || x.FromDate.Date <= data.ToDate.Value.Date) && x.ToDate == null)
                           || ((x.FromDate.Date <= data.FromDate.Date && x.ToDate.Value.Date >= data.FromDate.Date) || ((x.FromDate.Date <= data.ToDate.Value.Date && x.ToDate.Value.Date >= data.ToDate.Value.Date))))).ToList();
            }

            if (dataExist != null && dataExist.Count > 0)
            {
                var classIndex = dataExist.Select(x => x.DepartmentIndex).ToList();
                var classInfo = await _HR_UserService.GetDepartmentByIds(classIndex.Select(x => x.ToString()).ToList(), companyIndex);
                var errorString = string.Join(",", classInfo.Select(x => x.Name).ToList());
                return errorString;
            }
            return string.Empty;
        }

        public async Task<bool> AddScheduleFixedByDepartment(TA_ScheduleFixedByDepartmentDTO data, int companyIndex)
        {
            var result = true;
            try
            {
                var scheduleFixedByDepartmentList = new List<TA_ScheduleFixedByDepartment>();
                foreach (var item in data.DepartmentList)
                {
                    var scheduleFixedByDepartment = new TA_ScheduleFixedByDepartment()
                    {
                        CompanyIndex = companyIndex,
                        DepartmentIndex = item,
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
                    scheduleFixedByDepartmentList.Add(scheduleFixedByDepartment);
                }

                await _dbContext.TA_ScheduleFixedByDepartment.AddRangeAsync(scheduleFixedByDepartmentList);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"AddScheduleFixedByDepartment: {ex}");
                result = false;
            }
            return result;
        }

        public async Task<bool> UpdateScheduleFixedByDepartment(TA_ScheduleFixedByDepartment dataExist, TA_ScheduleFixedByDepartmentDTO data, int companyIndex)
        {
            var result = true;
            try
            {
                _dbContext.Remove(dataExist);
                _dbContext.SaveChanges();

                var scheduleFixedByDepartment = new TA_ScheduleFixedByDepartment()
                {
                    CompanyIndex = companyIndex,
                    DepartmentIndex = data.DepartmentIndex,
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

                await _dbContext.TA_ScheduleFixedByDepartment.AddAsync(scheduleFixedByDepartment);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"UpdateScheduleFixedByDepartment: {ex}");
                result = false;
            }
            return result;
        }

        public bool DeleteScheduleFixedByDepartment(List<int> dataDelete)
        {
            using (var transaction = DbContext.Database.BeginTransaction())
            {
                try
                {
                    var statusDelete = _dbContext.TA_ScheduleFixedByDepartment.Where(x => dataDelete.Contains(x.Index)).ToList();
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
        public async Task<DataGridClass> GetScheduleFixedByDepartment(List<long> departmentFilter, DateTime date, int pCompanyIndex, int pPage, int pLimit)
        {
            try
            {
                DataGridClass dataGrid = null;
                int countPage = 0;
                var scheduleFixedByDepartmentList = _dbContext.TA_ScheduleFixedByDepartment.Where(x => date.Date.Date >= x.FromDate.Date && (x.ToDate == null || (x.ToDate != null && date.Date.Date <= x.ToDate.Value.Date))).ToList();

                if (departmentFilter != null && departmentFilter.Count > 0)
                {
                    scheduleFixedByDepartmentList = scheduleFixedByDepartmentList.Where(x => departmentFilter.Contains(x.DepartmentIndex)).ToList();
                }

                var departmentIndexes = scheduleFixedByDepartmentList.Select(x => x.DepartmentIndex.ToString()).ToList();
                var departmentInfo = await _HR_UserService.GetDepartmentByIds(departmentIndexes, pCompanyIndex);
                var shiftInfo = _dbContext.TA_Shift.ToList();
                var listData = new List<ScheduleFixedByDepartmentReponse>();
                foreach (var item in scheduleFixedByDepartmentList)
                {
                    listData.Add(new ScheduleFixedByDepartmentReponse()
                    {
                        Id = item.Index,
                        FromDate = item.FromDate,
                        FromDateFormat = item.FromDate.ToddMMyyyy(),
                        ToDate = item.ToDate,
                        ToDateFormat = item.ToDate != null ? item.ToDate.Value.ToddMMyyyy() : "",
                        DepartmentIndex = item.DepartmentIndex,
                        DepartmentList = new List<long>() { item.DepartmentIndex },
                        DepartmentName = departmentInfo.FirstOrDefault(x => x.Index == item.DepartmentIndex)?.Name,
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

        public async Task<List<ScheduleFixedByDepartmentImportExcel>> AddScheduleDepartmentFromExcel(List<ScheduleFixedByDepartmentImportExcel> param, UserInfo user)
        {

            try
            {
                var result = param;
                var shiftList = _dbContext.TA_Shift.ToList();
                var departmentList = _IC_DepartmentService.GetAllActiveDepartment(user.CompanyIndex);
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

                    if (string.IsNullOrWhiteSpace(item.DepartmentName))
                    {
                        item.ErrorMessage += "Phòng ban không được để trống\r\n";
                    }
                    else if (!string.IsNullOrWhiteSpace(item.DepartmentName))
                    {
                        var listAllDepartmentNameFromImport = new List<string>();
                        if (item.DepartmentName.Contains("/"))
                        {
                            var listSplitDepartmentName = item.DepartmentName.Split("/").Distinct().ToList();
                            listAllDepartmentNameFromImport.AddRange(listSplitDepartmentName);
                        }
                        else
                        {
                            listAllDepartmentNameFromImport.Add(item.DepartmentName);
                        }
                        listAllDepartmentNameFromImport = listAllDepartmentNameFromImport.Distinct().ToList();
                        var departmentInfo = listAllDepartmentNameFromImport.LastOrDefault();
                        var department = departmentList.FirstOrDefault(y => y.Name == departmentInfo);
                        if (department != null)
                        {
                            item.DepartmentIndex = department.Index;
                        }
                        else
                        {
                            item.ErrorMessage += "Phòng ban không tồn tại\r\n";
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

                var dataScheduleFixedByDepartment = _dbContext.TA_ScheduleFixedByDepartment.Where(x => result.Select(z => z.DepartmentIndex).Contains(x.DepartmentIndex)).ToList();
                var logExistInFile = new List<TA_ScheduleFixedByDepartment>();
                foreach (var scheduleInfo in result)
                {
                    //var dataScheduleFixedByDepartmentExist = CheckScheduleFixedByDepartmentExistImport(dataScheduleFixedByDepartment, scheduleInfo);
                    var dataScheduleFixedByDepartmentExist = new TA_ScheduleFixedByDepartment();
                    if (scheduleInfo.ToDate == null)
                    {
                        dataScheduleFixedByDepartmentExist = dataScheduleFixedByDepartment
                       .FirstOrDefault(x => x.DepartmentIndex == scheduleInfo.DepartmentIndex &&
                                   (x.FromDate == scheduleInfo.FromDate || x.ToDate == scheduleInfo.FromDate
                                     || x.FromDate >= scheduleInfo.FromDate
                                   || (x.ToDate == null)
                                   || (x.FromDate > scheduleInfo.FromDate && x.ToDate < scheduleInfo.FromDate)
                                   || (x.FromDate <= scheduleInfo.FromDate && x.ToDate >= scheduleInfo.FromDate)));
                    }
                    else
                    {
                        dataScheduleFixedByDepartmentExist = dataScheduleFixedByDepartment
                       .FirstOrDefault(x => x.DepartmentIndex == scheduleInfo.DepartmentIndex &&
                                   (x.FromDate == scheduleInfo.FromDate || x.ToDate == scheduleInfo.FromDate
                                   || ((x.FromDate >= scheduleInfo.FromDate && x.FromDate <= scheduleInfo.ToDate) && x.ToDate == null)
                                   || ((x.FromDate <= scheduleInfo.FromDate || x.FromDate <= scheduleInfo.ToDate) && x.ToDate == null)
                                   || ((x.FromDate <= scheduleInfo.FromDate && x.ToDate >= scheduleInfo.FromDate) || ((x.FromDate <= scheduleInfo.ToDate && x.ToDate >= scheduleInfo.ToDate)))));
                    }

                    if (dataScheduleFixedByDepartmentExist != null && scheduleInfo.FromDate.Date != dataScheduleFixedByDepartmentExist.FromDate.Date)
                    {
                        scheduleInfo.ErrorMessage += "Phòng ban đã được khai báo lịch cố định trong cùng khoảng thời gian\r\n";
                    }

                    var dataExist = new TA_ScheduleFixedByDepartment();
                    if (string.IsNullOrWhiteSpace(scheduleInfo.ErrorMessage))
                    {
                        if (scheduleInfo.ToDate == null)
                        {
                            dataExist = logExistInFile
                           .FirstOrDefault(x => scheduleInfo.DepartmentIndex == x.DepartmentIndex &&
                                        (x.FromDate == scheduleInfo.FromDate || x.ToDate == scheduleInfo.FromDate
                                          || x.FromDate >= scheduleInfo.FromDate
                                       || (x.ToDate == null)
                                       || (x.FromDate > scheduleInfo.FromDate && x.ToDate < scheduleInfo.FromDate)
                                       || (x.FromDate <= scheduleInfo.FromDate && x.ToDate >= scheduleInfo.FromDate)));
                        }
                        else
                        {
                            dataExist = logExistInFile
                           .FirstOrDefault(x => scheduleInfo.DepartmentIndex == x.DepartmentIndex &&
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
                    logExistInFile.Add(scheduleInfo);
                }

                var noErrorParam = result.Where(x => string.IsNullOrWhiteSpace(x.ErrorMessage)).ToList();
                if (noErrorParam != null && noErrorParam.Count > 0)
                {
                    foreach (var scheduleFixedDepartment in noErrorParam)
                    {
                        var dataScheduleFixedByDepartmentExist = CheckScheduleFixedByDepartmentExistImport(dataScheduleFixedByDepartment, scheduleFixedDepartment);
                        if (dataScheduleFixedByDepartmentExist != null && scheduleFixedDepartment.FromDate.Date == dataScheduleFixedByDepartmentExist.FromDate.Date)
                        {
                            dataScheduleFixedByDepartmentExist.CompanyIndex = user.CompanyIndex;
                            dataScheduleFixedByDepartmentExist.DepartmentIndex = scheduleFixedDepartment.DepartmentIndex;
                            dataScheduleFixedByDepartmentExist.FromDate = scheduleFixedDepartment.FromDate;
                            dataScheduleFixedByDepartmentExist.ToDate = scheduleFixedDepartment.ToDate;
                            dataScheduleFixedByDepartmentExist.Monday = scheduleFixedDepartment.Monday;
                            dataScheduleFixedByDepartmentExist.Tuesday = scheduleFixedDepartment.Tuesday;
                            dataScheduleFixedByDepartmentExist.Wednesday = scheduleFixedDepartment.Wednesday;
                            dataScheduleFixedByDepartmentExist.Thursday = scheduleFixedDepartment.Thursday;
                            dataScheduleFixedByDepartmentExist.Friday = scheduleFixedDepartment.Friday;
                            dataScheduleFixedByDepartmentExist.Saturday = scheduleFixedDepartment.Saturday;
                            dataScheduleFixedByDepartmentExist.Sunday = scheduleFixedDepartment.Sunday;
                            dataScheduleFixedByDepartmentExist.UpdateDate = scheduleFixedDepartment.UpdateDate;
                            dataScheduleFixedByDepartmentExist.UpdateUser = scheduleFixedDepartment.UpdateUser;
                            _dbContext.TA_ScheduleFixedByDepartment.Update(dataScheduleFixedByDepartmentExist);
                        }
                        if (dataScheduleFixedByDepartmentExist == null)
                        {
                            var scheduleFixedByDepartment = new TA_ScheduleFixedByDepartment()
                            {
                                CompanyIndex = user.CompanyIndex,
                                DepartmentIndex = scheduleFixedDepartment.DepartmentIndex,
                                FromDate = scheduleFixedDepartment.FromDate,
                                ToDate = scheduleFixedDepartment.ToDate,
                                Monday = scheduleFixedDepartment.Monday,
                                Tuesday = scheduleFixedDepartment.Tuesday,
                                Wednesday = scheduleFixedDepartment.Wednesday,
                                Thursday = scheduleFixedDepartment.Thursday,
                                Friday = scheduleFixedDepartment.Friday,
                                Saturday = scheduleFixedDepartment.Saturday,
                                Sunday = scheduleFixedDepartment.Sunday,
                                UpdateDate = scheduleFixedDepartment.UpdateDate,
                                UpdateUser = scheduleFixedDepartment.UpdateUser,
                            };

                            _dbContext.TA_ScheduleFixedByDepartment.Add(scheduleFixedByDepartment);
                        }
                    }
                    await _dbContext.SaveChangesAsync();
                }

                var errorList = result.Where(x => !string.IsNullOrEmpty(x.ErrorMessage)).ToList();
                return errorList;
            }
            catch (Exception ex)
            {
                _logger.LogError("AddScheduleDepartmentFromExcel: " + ex);
            }
            return param;
        }

        public TA_ScheduleFixedByDepartment CheckScheduleFixedByDepartmentExistImport(List<TA_ScheduleFixedByDepartment> listData, ScheduleFixedByDepartmentImportExcel scheduleInfo)
        {
            var dataExist = new TA_ScheduleFixedByDepartment();
            if (scheduleInfo.ToDate == null)
            {
                dataExist = listData
               .FirstOrDefault(x => scheduleInfo.DepartmentIndex == x.DepartmentIndex &&
                                        (x.FromDate == scheduleInfo.FromDate || x.ToDate == scheduleInfo.FromDate
                                          || x.FromDate >= scheduleInfo.FromDate
                                       || (x.ToDate == null)
                                       || (x.FromDate > scheduleInfo.FromDate && x.ToDate < scheduleInfo.FromDate)
                                       || (x.FromDate <= scheduleInfo.FromDate && x.ToDate >= scheduleInfo.FromDate)));
            }
            else
            {
                dataExist = listData
               .FirstOrDefault(x => scheduleInfo.DepartmentIndex == x.DepartmentIndex &&
                                       (x.FromDate == scheduleInfo.FromDate || x.ToDate == scheduleInfo.FromDate
                                       || ((x.FromDate >= scheduleInfo.FromDate && x.FromDate <= scheduleInfo.ToDate) && x.ToDate == null)
                                       || ((x.FromDate <= scheduleInfo.FromDate || x.FromDate <= scheduleInfo.ToDate) && x.ToDate == null)
                                       || ((x.FromDate <= scheduleInfo.FromDate && x.ToDate >= scheduleInfo.FromDate) || ((x.FromDate <= scheduleInfo.ToDate && x.ToDate >= scheduleInfo.ToDate)))));
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
                    worksheet3.Range("D2:D10003").SetDataValidation().List(worksheet1.Range(startShiftCell + ":" + endShiftCell), true);
                    worksheet3.Range("E2:E10003").SetDataValidation().List(worksheet1.Range(startShiftCell + ":" + endShiftCell), true);
                    worksheet3.Range("F2:F10003").SetDataValidation().List(worksheet1.Range(startShiftCell + ":" + endShiftCell), true);
                    worksheet3.Range("G2:G10003").SetDataValidation().List(worksheet1.Range(startShiftCell + ":" + endShiftCell), true);
                    worksheet3.Range("H2:H10003").SetDataValidation().List(worksheet1.Range(startShiftCell + ":" + endShiftCell), true);
                    worksheet3.Range("I2:I10003").SetDataValidation().List(worksheet1.Range(startShiftCell + ":" + endShiftCell), true);
                    worksheet3.Range("J2:J10003").SetDataValidation().List(worksheet1.Range(startShiftCell + ":" + endShiftCell), true);
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
    }
}
