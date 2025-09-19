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
using Microsoft.AspNetCore.Mvc;
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
    public class TA_EmployeeShiftService : BaseServices<TA_EmployeeShift, EPAD_Context>, ITA_EmployeeShiftService
    {
        public EPAD_Context _dbContext;
        ConfigObject _Config;
        IMemoryCache _Cache;
        private ILogger _logger;
        private IConfiguration _configuration;
        private readonly IHR_UserService _HR_UserService;
        public TA_EmployeeShiftService(IServiceProvider serviceProvider, EPAD_Context context,
            ILoggerFactory loggerFactory, IConfiguration configuration) : base(serviceProvider)
        {
            _dbContext = context;
            _Cache = serviceProvider.GetService<IMemoryCache>();
            _Config = ConfigObject.GetConfig(_Cache);
            _logger = loggerFactory.CreateLogger<GC_GatesService>();
            _configuration = configuration;
            _HR_UserService = serviceProvider.GetService<IHR_UserService>();
        }
        public async Task<List<TA_Rules_Shift>> GetEmployee(int companyIndex)
        {
            return await _dbContext.TA_Rules_Shift.AsNoTracking().Where(x => x.CompanyIndex == companyIndex).ToListAsync();
        }


        public List<TA_EmployeeShift> GetListEmpoyeeShiftByDateAndEmps(DateTime from, DateTime to, List<string> employeeATIDs, int companyIndex)
        {
            try
            {
                var result = _dbContext.TA_EmployeeShift.Where(e => e.CompanyIndex == companyIndex
                && employeeATIDs.Contains(e.EmployeeATID)
                && from.Date <= e.Date.Date && to.Date >= e.Date.Date).ToList();
                return result;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public bool DeleteEmployeeShift(List<CM_EmployeeShiftModel> param)
        {
            using (var transaction = _dbContext.Database.BeginTransaction())
            {
                try
                {
                    var listDate = param.Select(x => DateTime.ParseExact(x.DateStr, "dd/MM/yyyy", null)).ToList();
                    var minDate = new DateTime(listDate.Min().Year, listDate.Min().Month, listDate.Min().Day);
                    var maxDate = new DateTime(listDate.Max().Year, listDate.Max().Month, listDate.Max().Day);
                    var employeeIDs = param.Select(e => e.EmployeeATID).Distinct().ToList();
                    var employeeShiftList = _dbContext.TA_EmployeeShift.Where(e => employeeIDs.Contains(e.EmployeeATID) && e.Date.Date >= minDate.Date && e.Date.Date <= maxDate.Date).ToList();
                    _dbContext.RemoveRange(employeeShiftList);
                    _dbContext.SaveChanges();
                    transaction.Commit();
                    return true;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    _logger.LogError("DeleteEmployeeShiftFail: ", ex);
                    return false;
                }
            }
        }

        public async Task<List<EmployeeShiftTableRequest>> ImportShiftTable(List<EmployeeShiftTableRequest> shiftTableData, UserInfo user)
        {
            try
            {
                var shiftList = _dbContext.TA_Shift.ToList();

                var employeeIds = shiftTableData.Select(x => x.EmployeeATID).Distinct().ToList();
                var employeePermissonDic = new Dictionary<string, string>();

                var employeeShiftList = _dbContext.TA_EmployeeShift.Where(x => employeeIds.Contains(x.EmployeeATID)).ToList();

                var employeeList = await _HR_UserService.GetEmployeeCompactInfoByListEmpATID(shiftTableData.Select(x => x.EmployeeATID).ToList(), user.CompanyIndex);
                int i = 0;
                foreach (var infoShift in shiftTableData)
                {
                    bool hasError = false;

                    if (string.IsNullOrWhiteSpace(infoShift.EmployeeATID))
                    {
                        infoShift.ErrorMessage += "Mã chấm công không được để trống\r\n";
                    }
                    else if (!string.IsNullOrWhiteSpace(infoShift.EmployeeATID))
                    {
                        var employee = employeeList.FirstOrDefault(y => y.EmployeeATID == infoShift.EmployeeATID);
                        if (employee != null)
                        {
                            infoShift.EmployeeATID = employee.EmployeeATID;
                        }
                        else
                        {
                            infoShift.ErrorMessage += "Nhân viên không tồn tại\r\n";
                        }
                    }

                    foreach (var dailyShifts in infoShift.DailyShifts)
                    {
                        var employeeShift = new TA_EmployeeShift();
                        var shift = new TA_Shift();
                        if (!string.IsNullOrEmpty(dailyShifts.ShiftValue))
                        {
                            if (dailyShifts.ShiftValue == "Không có ca")
                            {
                                shift.Index = 0;
                            }
                            else
                            {
                                shift = shiftList?.FirstOrDefault(x => x.Code.Trim().ToLower() == dailyShifts.ShiftValue.Trim().ToLower());
                                if (shift == null)
                                {
                                    infoShift.ErrorMessage += "Ca làm việc " + dailyShifts.ShiftValue + " ngày " + dailyShifts.Date.ToString("dd/MM/yyyy") + " không tồn tại\r\n";

                                }
                            }
                        }
                        if (string.IsNullOrWhiteSpace(infoShift.ErrorMessage) && !string.IsNullOrEmpty(dailyShifts.ShiftValue))
                        {
                            var checkEmployeeShiftExist = employeeShiftList.FirstOrDefault(x => x.EmployeeATID == infoShift.EmployeeATID && x.Date.Date == dailyShifts.Date.Date);
                            if (shift != null)
                            {
                                if (checkEmployeeShiftExist == null)
                                {
                                    employeeShift.CompanyIndex = user.CompanyIndex;
                                    employeeShift.EmployeeATID = infoShift.EmployeeATID;
                                    employeeShift.ShiftIndex = shift.Index;
                                    employeeShift.Date = dailyShifts.Date;
                                    _dbContext.TA_EmployeeShift.Add(employeeShift);
                                }
                                else
                                {
                                    checkEmployeeShiftExist.CompanyIndex = user.CompanyIndex;
                                    checkEmployeeShiftExist.EmployeeATID = infoShift.EmployeeATID;
                                    checkEmployeeShiftExist.ShiftIndex = shift.Index;
                                    checkEmployeeShiftExist.Date = dailyShifts.Date;
                                    Update(checkEmployeeShiftExist);
                                }
                            }
                        }
                    }
                }
                await _dbContext.SaveChangesAsync();
                var errorList = shiftTableData.Where(x => !string.IsNullOrEmpty(x.ErrorMessage)).ToList();
                return errorList;
            }
            catch (Exception ex)
            {
                _logger.LogError("ImportShiftTable: ", ex);
                throw;
            }
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
                    worksheet3.Range("E2:E10003").SetDataValidation().List(worksheet1.Range(startShiftCell + ":" + endShiftCell), true);
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
    }
}
