using EPAD_Common.Services;
using EPAD_Data.Entities;
using EPAD_Data;
using EPAD_Services.Interface;
using System;
using System.Collections.Generic;
using EPAD_Common.Types;
using System.Linq;
using EPAD_Data.Models;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EPAD_Common.Extensions;
using AutoMapper;
using System.Globalization;
using Microsoft.Extensions.DependencyInjection;

namespace EPAD_Services.Impl
{
    public class TA_AjustAttendanceLogService : BaseServices<TA_AjustAttendanceLog, EPAD_Context>, ITA_AjustAttendanceLogService
    {
        public EPAD_Context _dbContext;
        private readonly IMapper _mapper;
        private readonly ITA_BusinessRegistrationService _TA_BusinessRegistrationService;
        public TA_AjustAttendanceLogService(IServiceProvider serviceProvider, EPAD_Context context, IMapper mapper) : base(serviceProvider)
        {
            _dbContext = context;
            _mapper = mapper;
            _TA_BusinessRegistrationService = serviceProvider.GetService<ITA_BusinessRegistrationService>();
        }

        public DataGridClass GetDataGrid(int pCompanyIndex, int pPage, int pLimit, string filter, List<long> departments, List<string> employeeatids, DateTime fromDate, DateTime toDate)
        {
            DataGridClass dataGrid = null;
            int countPage = 0;
            var filterBy = new List<string>();
            if (filter != null)
            {
                filterBy = filter.Split(" ").Select(x => x.ToLower()).ToList();
            }

            var devices = _dbContext.IC_Device.Where(x => x.CompanyIndex == pCompanyIndex).ToList();
            var logsQuery = (from attendanceLog in _dbContext.IC_AttendanceLog.Where(t => t.CompanyIndex == pCompanyIndex)
                             join log in _dbContext.TA_AjustAttendanceLog.Where(x => x.CompanyIndex == pCompanyIndex) on
                              new { attendanceLog.EmployeeATID, attendanceLog.SerialNumber, CheckTime = attendanceLog.CheckTime } equals
                              new { log.EmployeeATID, log.SerialNumber, CheckTime = log.RawCheckTime }
                            into tempLog
                             from logDump in tempLog.DefaultIfEmpty()

                             join user in _dbContext.HR_User.Where(x => x.CompanyIndex == pCompanyIndex)
                             on attendanceLog.EmployeeATID equals user.EmployeeATID into temp
                             from dummy in temp.DefaultIfEmpty()

                             join wk in _dbContext.IC_WorkingInfo.Where(x => x.CompanyIndex == pCompanyIndex)
                                 on dummy.EmployeeATID equals wk.EmployeeATID into wCheck
                             from wResult in wCheck.DefaultIfEmpty()

                             join de in _dbContext.IC_Department.Where(x => x.CompanyIndex == pCompanyIndex && x.IsInactive != true)
                                 on wResult.DepartmentIndex equals de.Index into deCheck
                             from deResult in deCheck.DefaultIfEmpty()

                             join dev in _dbContext.IC_Device.Where(x => x.CompanyIndex == pCompanyIndex)
                               on attendanceLog.SerialNumber equals dev.SerialNumber into devCheck
                             from devResult in devCheck.DefaultIfEmpty()

                             join dev1 in _dbContext.IC_Device.Where(x => x.CompanyIndex == pCompanyIndex)
                               on logDump.SerialNumber equals dev1.SerialNumber into dev1Check
                             from dev1Result in dev1Check.DefaultIfEmpty()

                             where
                          //        (string.IsNullOrEmpty(filter)
                          //      ? attendanceLog.EmployeeATID.Contains("")
                          //      : (attendanceLog.EmployeeATID.Contains(filter)
                          //          || filterBy.Contains(attendanceLog.EmployeeATID)
                          //          || (!string.IsNullOrWhiteSpace(dummy.EmployeeCode) && (dummy.EmployeeCode == filter || dummy.EmployeeCode.Contains(filter) || filterBy.Contains(dummy.EmployeeCode)))
                          //          || (!string.IsNullOrWhiteSpace(dummy.FullName) && (dummy.FullName == filter || dummy.FullName.Contains(filter) || filterBy.Contains(dummy.FullName)))
                          //      ))
                          //&& 
                          ((departments != null && departments.Count > 0) ? departments.Contains(wResult.DepartmentIndex) : true)
                           && ((employeeatids != null && employeeatids.Count > 0) ? employeeatids.Contains(attendanceLog.EmployeeATID) : true)
                            && wResult.Status == 1 && (!wResult.ToDate.HasValue
                            || (wResult.ToDate.HasValue && wResult.ToDate.Value.Date >= DateTime.Now.Date))
                            && (logDump == null || logDump.Operate != (int)AjustAttendanceLogOperator.Delete)
                            && ((logDump == null && attendanceLog.CheckTime.Date >= fromDate.Date && attendanceLog.CheckTime.Date <= toDate.Date)
                            || (logDump != null && logDump.ProcessedCheckTime.Date >= fromDate.Date && logDump.ProcessedCheckTime.Date <= toDate.Date))
                             select new TA_AjustAttendanceLogDTO
                             {
                                 EmployeeATID = attendanceLog.EmployeeATID,
                                 FullName = dummy.FullName,
                                 DepartmentName = deResult.Name,
                                 EmployeeCode = dummy.EmployeeCode,
                                 Index = logDump != null ? logDump.Index : 0,
                                 DepartmentIndex = wResult.DepartmentIndex,
                                 Operate = logDump != null ? logDump.Operate : (int)AjustAttendanceLogOperator.NoProcess,
                                 ProcessedCheckTime = logDump != null ? logDump.ProcessedCheckTime : attendanceLog.CheckTime,
                                 RawCheckTime = logDump != null ? logDump.RawCheckTime : attendanceLog.CheckTime,
                                 SerialNumber = logDump != null ? logDump.SerialNumber : attendanceLog.SerialNumber,
                                 CompanyIndex = attendanceLog.CompanyIndex,
                                 InOutMode = attendanceLog.InOutMode,
                                 VerifyMode = attendanceLog.VerifyMode,
                                 InOutModeString = attendanceLog.InOutMode.GetInOutModeString() == "In" ? "Vào" : "Ra",
                                 VerifyModeString = attendanceLog.VerifyMode.GetVerifyModeString(),
                                 IsError = logDump != null ? logDump.Operate == (int)AjustAttendanceLogOperator.Update : false,
                                 Day = logDump != null ? logDump.ProcessedCheckTime.ToString("dd-MM-yyyy") : attendanceLog.CheckTime.ToString("dd-MM-yyyy"),
                                 DeviceName = logDump != null ? dev1Result.AliasName : devResult.AliasName,
                                 Note = logDump != null ? logDump.Note : ""
                             });

            var logs = logsQuery.ToList();

            var logAdd = (from attendanceLog in _dbContext.TA_AjustAttendanceLog.Where(t => t.CompanyIndex == pCompanyIndex)
                          join user in _dbContext.HR_User.Where(x => x.CompanyIndex == pCompanyIndex)
                          on attendanceLog.EmployeeATID equals user.EmployeeATID into temp
                          from dummy in temp.DefaultIfEmpty()

                          join wk in _dbContext.IC_WorkingInfo.Where(x => x.CompanyIndex == pCompanyIndex)
                              on dummy.EmployeeATID equals wk.EmployeeATID into wCheck
                          from wResult in wCheck.DefaultIfEmpty()

                          join de in _dbContext.IC_Department.Where(x => x.CompanyIndex == pCompanyIndex && x.IsInactive != true)
                              on wResult.DepartmentIndex equals de.Index into deCheck
                          from deResult in deCheck.DefaultIfEmpty()

                          join dev in _dbContext.IC_Device.Where(x => x.CompanyIndex == pCompanyIndex)
                             on attendanceLog.SerialNumber equals dev.SerialNumber into devCheck
                          from devResult in devCheck.DefaultIfEmpty()
                          where
                       //        (string.IsNullOrEmpty(filter)
                       //      ? attendanceLog.EmployeeATID.Contains("")
                       //      : (
                       //             attendanceLog.EmployeeATID.Contains(filter)
                       //          || filterBy.Contains(attendanceLog.EmployeeATID)
                       //        || (!string.IsNullOrWhiteSpace(dummy.EmployeeCode) && (dummy.EmployeeCode == filter || dummy.EmployeeCode.Contains(filter) || filterBy.Contains(dummy.EmployeeCode)))
                       //        || (!string.IsNullOrWhiteSpace(dummy.FullName) && (dummy.FullName == filter || dummy.FullName.Contains(filter) || filterBy.Contains(dummy.FullName)))
                       //      ))
                       //&& 
                       ((departments != null && departments.Count > 0) ? departments.Contains(wResult.DepartmentIndex) : true)
                        && ((employeeatids != null && employeeatids.Count > 0) ? employeeatids.Contains(attendanceLog.EmployeeATID) : true)
                         && wResult.Status == 1 && (!wResult.ToDate.HasValue
                         || (wResult.ToDate.HasValue && wResult.ToDate.Value.Date >= DateTime.Now.Date))
                         && (attendanceLog.Operate == (int)AjustAttendanceLogOperator.Add || attendanceLog.Operate == (int)AjustAttendanceLogOperator.Import)
                         && attendanceLog.ProcessedCheckTime.Date >= fromDate.Date && attendanceLog.ProcessedCheckTime.Date <= toDate.Date
                          select new TA_AjustAttendanceLogDTO
                          {
                              EmployeeATID = attendanceLog.EmployeeATID,
                              FullName = dummy.FullName,
                              DepartmentName = deResult.Name,
                              EmployeeCode = dummy.EmployeeCode,
                              Index = attendanceLog.Index,
                              DepartmentIndex = wResult.DepartmentIndex,
                              Operate = attendanceLog.Operate,
                              ProcessedCheckTime = attendanceLog.ProcessedCheckTime,
                              RawCheckTime = attendanceLog.RawCheckTime,
                              SerialNumber = attendanceLog.SerialNumber,
                              CompanyIndex = attendanceLog.CompanyIndex,
                              InOutMode = attendanceLog.InOutMode,
                              VerifyMode = attendanceLog.VerifyMode,
                              InOutModeString = attendanceLog.InOutMode.GetInOutModeString() == "In" ? "Vào" : "Ra",
                              VerifyModeString = attendanceLog.VerifyMode.GetVerifyModeString(),
                              IsWarning = true,
                              Day = attendanceLog.ProcessedCheckTime.ToString("dd-MM-yyyy"),
                              DeviceName = devResult.AliasName,
                              Note = attendanceLog.Note

                          }).ToList();
            logs.AddRange(logAdd);

            if (!string.IsNullOrWhiteSpace(filter))
            {
                logs = logs.Where(x => filterBy.Contains(x.EmployeeATID) || filter.Contains(x.EmployeeATID)
                || (!string.IsNullOrWhiteSpace(x.EmployeeCode) && (filter.ToLower().Contains(x.EmployeeCode.ToLower())
                || filter.ToLower() == x.EmployeeCode.ToLower() || x.EmployeeCode.ToLower().Contains(filter.ToLower())))
                || (!string.IsNullOrWhiteSpace(x.FullName) && (filter.ToLower().Contains(x.FullName.ToLower())
                || filter.ToLower() == x.FullName.ToLower() || x.FullName.ToLower().Contains(filter.ToLower())))).ToList();
            }

            countPage = logs.Count();
            dataGrid = new DataGridClass(countPage, logs);
            if (pPage <= 1)
            {
                var lstAnnualLeave = logs.OrderBy(t => t.ProcessedCheckTime).Take(pLimit).ToList();
                dataGrid = new DataGridClass(countPage, lstAnnualLeave);
            }
            else
            {
                int fromRow = pLimit * (pPage - 1);
                var lstAnnualLeave = logs.OrderBy(t => t.ProcessedCheckTime).Skip(fromRow).Take(pLimit).ToList();
                dataGrid = new DataGridClass(countPage, lstAnnualLeave);
            }
            return dataGrid;
        }

        public async Task<string> InsertAjustAttendanceLog(TA_AjustAttendanceLogInsertDTO log, UserInfo user)
        {
            if (log.ProcessedCheckTime == DateTime.MinValue)
            {
                var attendanceTime = Convert.ToDateTime(log.AttendanceTime);
                var attendanceDate = Convert.ToDateTime(log.AttendanceDate);
                log.ProcessedCheckTime = attendanceDate.ChangeTime(attendanceTime.Hour, attendanceTime.Minute, attendanceTime.Second, 0);
            }

            var lockTime = await _TA_BusinessRegistrationService.GetLockAttendanceTimeValidDate(user);

            var fromLog = log.ProcessedCheckTime.ChangeTime(log.ProcessedCheckTime.Hour, log.ProcessedCheckTime.Minute, log.ProcessedCheckTime.Second, 0);
            var toLog = fromLog.AddSeconds(1).AddMilliseconds(-1);

            var checkExistRawLog = await _dbContext.IC_AttendanceLog.FirstOrDefaultAsync(x => x.EmployeeATID == log.EmployeeATID && x.CompanyIndex == user.CompanyIndex
                    && x.CheckTime >= fromLog && x.CheckTime <= toLog && x.SerialNumber == log.SerialNumber);

            var checkExistLog = await _dbContext.TA_AjustAttendanceLog.FirstOrDefaultAsync(x => x.EmployeeATID == log.EmployeeATID && x.CompanyIndex == user.CompanyIndex
                                           && x.ProcessedCheckTime >= fromLog && x.ProcessedCheckTime <= toLog && x.SerialNumber == log.SerialNumber && x.Operate != (int)AjustAttendanceLogOperator.Delete);

            if (lockTime.Date > log.ProcessedCheckTime.Date)
            {
                return "PassedLockAttendanceTime";
            }
            else if (checkExistRawLog == null && checkExistLog == null)
            {
                var newLog = new TA_AjustAttendanceLog()
                {
                    EmployeeATID = log.EmployeeATID,
                    CompanyIndex = user.CompanyIndex,
                    SerialNumber = log.SerialNumber,
                    CreatedDate = DateTime.Now,
                    Operate = (int)AjustAttendanceLogOperator.Add,
                    ProcessedCheckTime = log.ProcessedCheckTime,
                    RawCheckTime = log.ProcessedCheckTime,
                    UpdatedUser = user.FullName,
                    UpdatedDate = DateTime.Now,
                    Note = log.Note,
                    InOutMode = log.InOutMode,
                    VerifyMode = log.VerifyMode
                };

                var history = new TA_AjustAttendanceLogHistory()
                {
                    EmployeeATID = log.EmployeeATID,
                    CompanyIndex = user.CompanyIndex,
                    SerialNumber = log.SerialNumber,
                    Operate = (int)AjustAttendanceLogOperator.Add,
                    ProcessedCheckTime = log.ProcessedCheckTime,
                    UpdatedUser = user.FullName,
                    UpdatedDate = DateTime.Now,
                    Note = log.Note,
                    InOutMode = log.InOutMode,
                    VerifyMode = log.VerifyMode
                };

                await _dbContext.TA_AjustAttendanceLog.AddAsync(newLog);
                await _dbContext.TA_AjustAttendanceLogHistory.AddAsync(history);
                await _dbContext.SaveChangesAsync();
                return "";
            }
            return "AttendanceLogIsExist";
        }

        public async Task<string> UpdateAjustAttendanceLog(TA_AjustAttendanceLogInsertDTO log, UserInfo user)
        {
            var attendanceTime = Convert.ToDateTime(log.AttendanceTime);
            var attendanceDate = Convert.ToDateTime(log.AttendanceDate);
            log.ProcessedCheckTime = attendanceDate.ChangeTime(attendanceTime.Hour, attendanceTime.Minute, attendanceTime.Second, 0);
            var lockTime = await _TA_BusinessRegistrationService.GetLockAttendanceTimeValidDate(user);
            if (log.Index != 0)
            {
                var updateLog = await _dbContext.TA_AjustAttendanceLog.FirstOrDefaultAsync(x => x.Index == log.Index);
                var fromLog = log.RawCheckTime.ChangeTime(log.ProcessedCheckTime.Hour, log.ProcessedCheckTime.Minute, log.ProcessedCheckTime.Second, 0);
                var toLog = fromLog.AddSeconds(1).AddMilliseconds(-1);


                var checkExistRawLog = await _dbContext.IC_AttendanceLog.FirstOrDefaultAsync(x => x.EmployeeATID == log.EmployeeATID && x.CompanyIndex == user.CompanyIndex
                        && x.CheckTime >= fromLog && x.CheckTime <= toLog && x.CheckTime != log.RawCheckTime && x.SerialNumber == log.SerialNumber);

                var checkExistLog = await _dbContext.TA_AjustAttendanceLog.FirstOrDefaultAsync(x => x.EmployeeATID == log.EmployeeATID && x.CompanyIndex == user.CompanyIndex && x.Operate != (int)AjustAttendanceLogOperator.Delete
                                               && x.ProcessedCheckTime >= fromLog && x.ProcessedCheckTime <= toLog && x.SerialNumber == log.SerialNumber && x.Index != log.Index);
                if (lockTime.Date > log.ProcessedCheckTime.Date)
                {
                    return "PassedLockAttendanceTime";
                }
                else if (checkExistRawLog == null && checkExistLog == null)
                {
                    var rawCheckTime = updateLog.ProcessedCheckTime;
                    updateLog.ProcessedCheckTime = log.ProcessedCheckTime;
                    if (updateLog.Operate != (int)AjustAttendanceLogOperator.Add && updateLog.Operate != (int)AjustAttendanceLogOperator.Import)
                    {
                        updateLog.Operate = (int)AjustAttendanceLogOperator.Update;
                    }
                    else
                    {
                        updateLog.RawCheckTime = log.ProcessedCheckTime;
                    }

                    updateLog.UpdatedDate = DateTime.Now;
                    updateLog.UpdatedUser = user.FullName;
                    updateLog.Note = log.Note;
                    _dbContext.Update(updateLog);
                    var history = new TA_AjustAttendanceLogHistory()
                    {
                        EmployeeATID = log.EmployeeATID,
                        CompanyIndex = user.CompanyIndex,
                        SerialNumber = log.SerialNumber,
                        Operate = updateLog.Operate,
                        ProcessedCheckTime = log.ProcessedCheckTime,
                        UpdatedUser = user.FullName,
                        UpdatedDate = DateTime.Now,
                        Note = log.Note,
                        InOutMode = log.InOutMode,
                        VerifyMode = log.VerifyMode,
                        RawCheckTime = rawCheckTime
                    };
                    await _dbContext.AddAsync(history);
                    await _dbContext.SaveChangesAsync();
                    return "";
                }
            }
            else
            {
                var fromLog = log.RawCheckTime.ChangeTime(log.ProcessedCheckTime.Hour, log.ProcessedCheckTime.Minute, log.ProcessedCheckTime.Second, 0);
                var toLog = fromLog.AddSeconds(1).AddMilliseconds(-1);


                var checkExistRawLog = await _dbContext.IC_AttendanceLog.FirstOrDefaultAsync(x => x.EmployeeATID == log.EmployeeATID && x.CompanyIndex == user.CompanyIndex
                        && x.CheckTime >= fromLog && x.CheckTime <= toLog && x.SerialNumber == log.SerialNumber);

                var checkExistLog = await _dbContext.TA_AjustAttendanceLog.FirstOrDefaultAsync(x => x.EmployeeATID == log.EmployeeATID && x.CompanyIndex == user.CompanyIndex && x.Operate != (int)AjustAttendanceLogOperator.Delete
                                               && x.ProcessedCheckTime >= fromLog && x.ProcessedCheckTime <= toLog && x.SerialNumber == log.SerialNumber);

                if (lockTime.Date > log.ProcessedCheckTime.Date)
                {
                    return "PassedLockAttendanceTime";
                }
                else if (checkExistRawLog == null && checkExistLog == null)
                {
                    var newLog = new TA_AjustAttendanceLog()
                    {
                        EmployeeATID = log.EmployeeATID,
                        CompanyIndex = user.CompanyIndex,
                        SerialNumber = log.SerialNumber,
                        CreatedDate = DateTime.Now,
                        Operate = (int)AjustAttendanceLogOperator.Update,
                        ProcessedCheckTime = fromLog,
                        RawCheckTime = log.RawCheckTime,
                        UpdatedUser = user.FullName,
                        UpdatedDate = DateTime.Now,
                        Note = log.Note
                    };
                    var history = new TA_AjustAttendanceLogHistory()
                    {
                        EmployeeATID = log.EmployeeATID,
                        CompanyIndex = user.CompanyIndex,
                        SerialNumber = log.SerialNumber,
                        Operate = (int)AjustAttendanceLogOperator.Update,
                        ProcessedCheckTime = log.ProcessedCheckTime,
                        UpdatedUser = user.FullName,
                        UpdatedDate = DateTime.Now,
                        Note = log.Note,
                        InOutMode = log.InOutMode,
                        VerifyMode = log.VerifyMode,
                        RawCheckTime = log.RawCheckTime
                    };
                    await _dbContext.AddAsync(history);
                    await _dbContext.TA_AjustAttendanceLog.AddAsync(newLog);
                    await _dbContext.SaveChangesAsync();
                    return "";
                }
            }
            return "AttendanceLogIsExist";
        }

        public async Task<string> DeleteAjustAttendanceLog(List<TA_AjustAttendanceLogDTO> logs, UserInfo user)
        {
            var lockTime = await _TA_BusinessRegistrationService.GetLockAttendanceTimeValidDate(user);
            var check = true;
            foreach (var log in logs)
            {
                var updateLog = await _dbContext.TA_AjustAttendanceLog.FirstOrDefaultAsync(x => x.Index == log.Index);

                if (updateLog == null)
                {
                    if (lockTime.Date > log.ProcessedCheckTime.Date)
                    {
                        check = false;
                        return "PassedLockAttendanceTime";
                    }
                    else
                    {
                        var newLog = new TA_AjustAttendanceLog()
                        {
                            EmployeeATID = log.EmployeeATID,
                            CompanyIndex = user.CompanyIndex,
                            SerialNumber = log.SerialNumber,
                            CreatedDate = DateTime.Now,
                            Operate = (int)AjustAttendanceLogOperator.Delete,
                            ProcessedCheckTime = log.ProcessedCheckTime,
                            RawCheckTime = log.ProcessedCheckTime,
                            UpdatedUser = user.FullName,
                            UpdatedDate = DateTime.Now,
                            Note = log.Note
                        };
                        var history = new TA_AjustAttendanceLogHistory()
                        {
                            EmployeeATID = log.EmployeeATID,
                            CompanyIndex = user.CompanyIndex,
                            SerialNumber = log.SerialNumber,
                            Operate = (int)AjustAttendanceLogOperator.Delete,
                            ProcessedCheckTime = log.ProcessedCheckTime,
                            UpdatedUser = user.FullName,
                            UpdatedDate = DateTime.Now,
                            Note = log.Note,
                            InOutMode = log.InOutMode,
                            VerifyMode = log.VerifyMode,
                            RawCheckTime = log.ProcessedCheckTime
                        };
                        await _dbContext.AddAsync(history);
                        await _dbContext.TA_AjustAttendanceLog.AddAsync(newLog);
                    }
                }
                else
                {
                    if (lockTime.Date > log.ProcessedCheckTime.Date)
                    {
                        check = false;
                        return "PassedLockAttendanceTime";
                    }
                    else
                    {
                        updateLog.Operate = (int)AjustAttendanceLogOperator.Delete;
                        updateLog.UpdatedDate = DateTime.Now;
                        updateLog.UpdatedUser = user.FullName;
                        var history = new TA_AjustAttendanceLogHistory()
                        {
                            EmployeeATID = log.EmployeeATID,
                            CompanyIndex = user.CompanyIndex,
                            SerialNumber = updateLog.SerialNumber,
                            Operate = (int)AjustAttendanceLogOperator.Delete,
                            ProcessedCheckTime = updateLog.ProcessedCheckTime,
                            UpdatedUser = user.FullName,
                            UpdatedDate = DateTime.Now,
                            Note = updateLog.Note,
                            InOutMode = updateLog.InOutMode,
                            VerifyMode = updateLog.VerifyMode,
                            RawCheckTime = updateLog.ProcessedCheckTime
                        };
                        await _dbContext.AddAsync(history);
                        _dbContext.Update(updateLog);
                    }
                }
            }
            if (check)
            {
                await _dbContext.SaveChangesAsync();
                return "";
            }
            else
            {
                return "PassedLockAttendanceTime";
            }

        }

        public async Task<string> UpdateAjustAttendanceLogLst(List<TA_AjustAttendanceLogDTO> logs, UserInfo user)
        {
            var lockTime = await _TA_BusinessRegistrationService.GetLockAttendanceTimeValidDate(user);
            var check = true;
            var employeeExist = "";

            foreach (var log in logs)
            {
                var processedCheckTime = DateTime.Now;
                try
                {
                    processedCheckTime = Convert.ToDateTime(log.ProcessedCheckTimeString);
                }
                catch
                {
                    employeeExist += "<p>Dòng " + log.indexStt + ":</p>";
                    employeeExist += "<p>- Giờ chấm công không hợp lệ. Vui lòng kiểm tra lại</p>" + "<br>";
                    check = false;
                    continue;
                }

                log.ProcessedCheckTime = log.RawCheckTime;
                log.ProcessedCheckTime = log.ProcessedCheckTime.ChangeTime(processedCheckTime.Hour, processedCheckTime.Minute, processedCheckTime.Second, 0);
                if (lockTime.Date > log.ProcessedCheckTime.Date)
                {
                    employeeExist += "<p>Dòng " + log.indexStt + ":</p>";
                    employeeExist += "<p>- Quá thời gian chốt công. Vui lòng kiểm tra lại</p>" + "<br>";
                    check = false;
                }
                else
                {
                    if (log.Index != 0)
                    {
                        var updateLog = await _dbContext.TA_AjustAttendanceLog.FirstOrDefaultAsync(x => x.Index == log.Index);
                        var fromLog = log.RawCheckTime.ChangeTime(log.ProcessedCheckTime.Hour, log.ProcessedCheckTime.Minute, log.ProcessedCheckTime.Second, 0);
                        var toLog = fromLog.AddSeconds(1).AddMilliseconds(-1);


                        var checkExistRawLog = await _dbContext.IC_AttendanceLog.FirstOrDefaultAsync(x => x.EmployeeATID == log.EmployeeATID && x.CompanyIndex == user.CompanyIndex
                                && x.CheckTime >= fromLog && x.CheckTime <= toLog && x.CheckTime != log.RawCheckTime && x.SerialNumber == log.SerialNumber);

                        var checkExistLog = await _dbContext.TA_AjustAttendanceLog.FirstOrDefaultAsync(x => x.EmployeeATID == log.EmployeeATID && x.CompanyIndex == user.CompanyIndex && x.Operate != (int)AjustAttendanceLogOperator.Delete
                                                       && x.ProcessedCheckTime >= fromLog && x.ProcessedCheckTime <= toLog && x.SerialNumber == log.SerialNumber && x.Index != log.Index);

                        if (checkExistRawLog == null && checkExistLog == null)
                        {
                            var rawCheckTime = updateLog.ProcessedCheckTime;
                            updateLog.ProcessedCheckTime = log.ProcessedCheckTime;
                            if (updateLog.Operate != (int)AjustAttendanceLogOperator.Add && updateLog.Operate != (int)AjustAttendanceLogOperator.Import)
                            {
                                updateLog.Operate = (int)AjustAttendanceLogOperator.Update;
                            }
                            else
                            {
                                updateLog.RawCheckTime = log.ProcessedCheckTime;
                            }
                            updateLog.Note = log.Note;
                            updateLog.UpdatedDate = DateTime.Now;
                            updateLog.UpdatedUser = user.FullName;
                            var history = new TA_AjustAttendanceLogHistory()
                            {
                                EmployeeATID = log.EmployeeATID,
                                CompanyIndex = user.CompanyIndex,
                                SerialNumber = updateLog.SerialNumber,
                                Operate = updateLog.Operate,
                                ProcessedCheckTime = updateLog.ProcessedCheckTime,
                                UpdatedUser = user.FullName,
                                UpdatedDate = DateTime.Now,
                                Note = log.Note,
                                InOutMode = updateLog.InOutMode,
                                VerifyMode = updateLog.VerifyMode,
                                RawCheckTime = rawCheckTime
                            };
                            await _dbContext.AddAsync(history);
                            _dbContext.Update(updateLog);
                        }
                        else
                        {
                            check = false;
                            employeeExist += "<p>Dòng " + log.indexStt + ":</p>" + "<br>";
                            employeeExist += "<p>  - Trùng dữ liệu điểm danh. Vui lòng kiểm tra lại</p>" + "<br>";
                        }
                    }
                    else
                    {
                        var fromLog = log.RawCheckTime.ChangeTime(log.ProcessedCheckTime.Hour, log.ProcessedCheckTime.Minute, log.ProcessedCheckTime.Second, 0);
                        var toLog = fromLog.AddSeconds(1).AddMilliseconds(-1);


                        var checkExistRawLog = await _dbContext.IC_AttendanceLog.FirstOrDefaultAsync(x => x.EmployeeATID == log.EmployeeATID && x.CompanyIndex == user.CompanyIndex
                                && x.CheckTime >= fromLog && x.CheckTime <= toLog && x.SerialNumber == log.SerialNumber);

                        var checkExistLog = await _dbContext.TA_AjustAttendanceLog.FirstOrDefaultAsync(x => x.EmployeeATID == log.EmployeeATID && x.CompanyIndex == user.CompanyIndex && x.Operate != (int)AjustAttendanceLogOperator.Delete
                                                       && x.ProcessedCheckTime >= fromLog && x.ProcessedCheckTime <= toLog && x.SerialNumber == log.SerialNumber);


                        if (checkExistRawLog == null && checkExistLog == null)
                        {
                            var newLog = new TA_AjustAttendanceLog()
                            {
                                EmployeeATID = log.EmployeeATID,
                                CompanyIndex = user.CompanyIndex,
                                SerialNumber = log.SerialNumber,
                                CreatedDate = DateTime.Now,
                                Operate = (int)AjustAttendanceLogOperator.Update,
                                ProcessedCheckTime = fromLog,
                                RawCheckTime = log.RawCheckTime,
                                UpdatedUser = user.FullName,
                                UpdatedDate = DateTime.Now,
                                Note = log.Note
                            };
                            var history = new TA_AjustAttendanceLogHistory()
                            {
                                EmployeeATID = log.EmployeeATID,
                                CompanyIndex = user.CompanyIndex,
                                SerialNumber = log.SerialNumber,
                                Operate = (int)AjustAttendanceLogOperator.Update,
                                ProcessedCheckTime = log.ProcessedCheckTime,
                                UpdatedUser = user.FullName,
                                UpdatedDate = DateTime.Now,
                                Note = log.Note,
                                InOutMode = log.InOutMode,
                                VerifyMode = log.VerifyMode,
                                RawCheckTime = log.RawCheckTime
                            };
                            await _dbContext.AddAsync(history);
                            await _dbContext.TA_AjustAttendanceLog.AddAsync(newLog);
                        }
                        else
                        {
                            check = false;
                            employeeExist += "<p>Dòng " + log.indexStt + ":</p>" + "<br>";
                            employeeExist += "<p>  - Trùng dữ liệu điểm danh. Vui lòng kiểm tra lại</p>" + "<br>";
                        }
                    }
                }
            }
            if (check)
            {
                await _dbContext.SaveChangesAsync();
                return "";
            }
            else
            {
                return employeeExist;
            }

        }

        public async Task<List<TA_AjustAttendanceLogImport>> ValidationImportAjustAttendanceLog(List<TA_AjustAttendanceLogImport> logs, UserInfo user)
        {
            var errorList = new List<TA_AjustAttendanceLogImport>();
            var checkIsNull = logs.Where(e => string.IsNullOrWhiteSpace(e.EmployeeATID)
                || (string.IsNullOrWhiteSpace(e.Date))
                || (string.IsNullOrWhiteSpace(e.In) && string.IsNullOrWhiteSpace(e.Out))).ToList();


            if (checkIsNull != null && checkIsNull.Count() > 0)
            {
                foreach (var item in checkIsNull)
                {
                    if (string.IsNullOrWhiteSpace(item.EmployeeATID)) item.Error += "Mã chấm công không được để trống\r\n";
                    if (string.IsNullOrWhiteSpace(item.In) && string.IsNullOrWhiteSpace(item.Out)) item.Error += "Giờ vào và giờ ra không được bỏ trống\r\n";
                    if (string.IsNullOrWhiteSpace(item.Date)) item.Error += "Ngày không được để trống\r\n";
                }
            }
            var employeeATIDs = logs.Select(x => x.EmployeeATID).Distinct().ToList();

            var lstEmployeeATIDs = await _dbContext.HR_User.Where(x => x.CompanyIndex == user.CompanyIndex && employeeATIDs.Contains(x.EmployeeATID)).Select(x => x.EmployeeATID).ToListAsync();
            var employeeATIDLst = logs.Select(x => x.EmployeeATID).Distinct().ToList();
            var minLogs = logs.Select(x => x.DateFormat).OrderBy(x => x).FirstOrDefault();
            var maxLogs = logs.Select(x => x.DateFormat).OrderByDescending(x => x).FirstOrDefault();
            var checkExistLogs = await _dbContext.TA_AjustAttendanceLog.Where(x => x.ProcessedCheckTime.Date >= minLogs && x.ProcessedCheckTime.Date <= maxLogs && x.Operate != (int)AjustAttendanceLogOperator.Delete &&
                employeeATIDLst.Contains(x.EmployeeATID)).ToListAsync();
            var checkExistRawLogs = await _dbContext.IC_AttendanceLog.Where(x => x.CheckTime.Date >= minLogs && x.CheckTime.Date <= maxLogs &&
                employeeATIDLst.Contains(x.EmployeeATID)).ToListAsync();

            var format = "dd/MM/yyyy";
            foreach (var item in logs)
            {
                var checkDate = new DateTime();
                var time1 = new DateTime();
                var time2 = new DateTime();


                var checkExistEmployee = lstEmployeeATIDs.FirstOrDefault(x => x == item.EmployeeATID);
                if (checkExistEmployee == null)
                {
                    item.Error += "Mã chấm công không tồn tại\r\n";
                }

                if (!string.IsNullOrWhiteSpace(item.Date)
                      && !DateTime.TryParseExact(item.Date, format, null, System.Globalization.DateTimeStyles.None,
                      out checkDate))
                {
                    item.Error += "Ngày không đúng định dạng\r\n";
                }
                else
                {
                    item.DateFormat = checkDate;
                }

                if (!string.IsNullOrWhiteSpace(item.In)
               && !DateTime.TryParseExact(item.In, "HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out time1))
                {
                    item.Error += "Giờ vào không đúng định dạng\r\n";
                }
                else
                {
                    item.TimeIn = time1;
                }
                if (!string.IsNullOrWhiteSpace(item.Out)
                   && !DateTime.TryParseExact(item.Out, "HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out time2))
                {
                    item.Error += "Giờ ra không đúng định dạng\r\n";
                }
                else
                {
                    item.TimeOut = time2;
                }

                if (!string.IsNullOrEmpty(item.VerifyMode))
                {
                    item.VerifyModeFormat = item.VerifyMode.GetVerifyModeFromString();
                }
                if (item.TimeIn == item.TimeOut && item.TimeIn > DateTime.MinValue && item.TimeOut > DateTime.MinValue)
                {
                    item.Error += "Trùng thời gian vào và thời gian ra\r\n";
                }
                else
                {
                    if (item.TimeIn > DateTime.MinValue)
                    {
                        var checkDuplicateIn = logs.Where(x => x.EmployeeATID == item.EmployeeATID && x.Date == item.Date && (x.In == item.In || x.Out == item.In)).ToList();
                        if (checkDuplicateIn.Count > 1)
                        {
                            item.Error += "Trùng thời gian vào trong file excel\r\n";
                        }
                    }
                    if (item.TimeIn > DateTime.MinValue)
                    {
                        var checkDuplicateOut = logs.Where(x => x.EmployeeATID == item.EmployeeATID && x.Date == item.Date && (x.Out == item.Out || x.In == item.Out)).ToList();
                        if (checkDuplicateOut.Count > 1)
                        {
                            item.Error += "Trùng thời gian ra trong file excel\r\n";
                        }
                    }
                }

            }
            return logs;

        }
        public async Task<List<TA_AjustAttendanceLogImport>> CheckAjustAttendanceLogInDatabase(List<TA_AjustAttendanceLogImport> logs, UserInfo user)
        {
            var employeeATIDLst = logs.Select(x => x.EmployeeATID).Distinct().ToList();
            var minLogs = logs.Select(x => x.DateFormat).OrderBy(x => x).FirstOrDefault();
            var maxLogs = logs.Select(x => x.DateFormat).OrderByDescending(x => x).FirstOrDefault();
            var checkExistLogs = await _dbContext.TA_AjustAttendanceLog.Where(x => x.ProcessedCheckTime.Date >= minLogs && x.ProcessedCheckTime.Date <= maxLogs && x.Operate != (int)AjustAttendanceLogOperator.Delete &&
                employeeATIDLst.Contains(x.EmployeeATID)).ToListAsync();
            var checkExistRawLogs = await _dbContext.IC_AttendanceLog.Where(x => x.CheckTime.Date >= minLogs && x.CheckTime.Date <= maxLogs &&
                employeeATIDLst.Contains(x.EmployeeATID)).ToListAsync();
            var lockTime = await _TA_BusinessRegistrationService.GetLockAttendanceTimeValidDate(user);
            foreach (var log in logs)
            {
                if (!string.IsNullOrEmpty(log.Error)) { continue; }
                if (lockTime.Date > log.DateFormat.Date)
                {
                    log.Error += "Đã qua thời gian khóa công hàng tháng\r\n";
                }
                else
                {


                    if (log.TimeIn > DateTime.MinValue)
                    {
                        var processedCheckTime = log.DateFormat.ChangeTime(log.TimeIn.Hour, log.TimeIn.Minute, log.TimeIn.Second, 0);

                        var fromLog = processedCheckTime.ChangeTime(processedCheckTime.Hour, processedCheckTime.Minute, processedCheckTime.Second, 0);
                        var toLog = fromLog.AddSeconds(1).AddMilliseconds(-1);
                        var checkExistRawLog = checkExistRawLogs.FirstOrDefault(x => x.EmployeeATID == log.EmployeeATID && x.CompanyIndex == user.CompanyIndex
                        && x.CheckTime >= fromLog && x.CheckTime <= toLog && x.SerialNumber == log.SerialNumber);

                        var checkExistLog = checkExistLogs.FirstOrDefault(x => x.EmployeeATID == log.EmployeeATID && x.CompanyIndex == user.CompanyIndex
                                                       && x.ProcessedCheckTime >= fromLog && x.ProcessedCheckTime <= toLog && x.SerialNumber == log.SerialNumber);

                        if (checkExistRawLog != null || checkExistLog != null)
                        {
                            log.Error += "Giờ vào đã tồn tại\r\n";
                        }
                    }

                    if (log.TimeOut > DateTime.MinValue)
                    {
                        var processedCheckTime = log.DateFormat.ChangeTime(log.TimeOut.Hour, log.TimeOut.Minute, log.TimeOut.Second, 0);

                        var fromLog = processedCheckTime.ChangeTime(processedCheckTime.Hour, processedCheckTime.Minute, processedCheckTime.Second, 0);
                        var toLog = fromLog.AddSeconds(1).AddMilliseconds(-1);
                        var checkExistRawLog = checkExistRawLogs.FirstOrDefault(x => x.EmployeeATID == log.EmployeeATID && x.CompanyIndex == user.CompanyIndex
                        && x.CheckTime >= fromLog && x.CheckTime <= toLog && x.SerialNumber == log.SerialNumber);

                        var checkExistLog = checkExistLogs.FirstOrDefault(x => x.EmployeeATID == log.EmployeeATID && x.CompanyIndex == user.CompanyIndex
                                                       && x.ProcessedCheckTime >= fromLog && x.ProcessedCheckTime <= toLog && x.SerialNumber == log.SerialNumber);

                        if (checkExistRawLog != null || checkExistLog != null)
                        {
                            log.Error += "Giờ ra đã tồn tại\r\n";
                        }
                    }
                }
            }
            return logs;
        }

        public async Task AddOrUpdateImportAjustAttendanceLog(List<TA_AjustAttendanceLogImport> logs, UserInfo user)
        {
            var employeeATIDLst = logs.Select(x => x.EmployeeATID).Distinct().ToList();
            var minLogs = logs.Select(x => x.DateFormat).OrderBy(x => x).FirstOrDefault();
            var maxLogs = logs.Select(x => x.DateFormat).OrderByDescending(x => x).FirstOrDefault();
            var checkExistLogs = await _dbContext.TA_AjustAttendanceLog.Where(x => x.ProcessedCheckTime.Date >= minLogs && x.ProcessedCheckTime.Date <= maxLogs && x.Operate != (int)AjustAttendanceLogOperator.Delete &&
                employeeATIDLst.Contains(x.EmployeeATID)).ToListAsync();
            var checkExistRawLogs = await _dbContext.IC_AttendanceLog.Where(x => x.CheckTime.Date >= minLogs && x.CheckTime.Date <= maxLogs &&
                employeeATIDLst.Contains(x.EmployeeATID)).ToListAsync();

            foreach (var log in logs)
            {
                if (log.TimeIn > DateTime.MinValue)
                {
                    var processedCheckTime = log.DateFormat.ChangeTime(log.TimeIn.Hour, log.TimeIn.Minute, log.TimeIn.Second, 0);

                    var fromLog = processedCheckTime.ChangeTime(processedCheckTime.Hour, processedCheckTime.Minute, processedCheckTime.Second, 0);
                    var toLog = fromLog.AddSeconds(1).AddMilliseconds(-1);
                    var checkExistRawLog = checkExistRawLogs.FirstOrDefault(x => x.EmployeeATID == log.EmployeeATID && x.CompanyIndex == user.CompanyIndex
                    && x.CheckTime >= fromLog && x.CheckTime <= toLog && x.SerialNumber == log.SerialNumber);

                    var checkExistLog = checkExistLogs.FirstOrDefault(x => x.EmployeeATID == log.EmployeeATID && x.CompanyIndex == user.CompanyIndex
                                                   && x.ProcessedCheckTime >= fromLog && x.ProcessedCheckTime <= toLog && x.SerialNumber == log.SerialNumber);

                    if (checkExistRawLog == null && checkExistLog == null)
                    {
                        var newLog = new TA_AjustAttendanceLog()
                        {
                            EmployeeATID = log.EmployeeATID,
                            CompanyIndex = user.CompanyIndex,
                            SerialNumber = log.SerialNumber,
                            CreatedDate = DateTime.Now,
                            Operate = (int)AjustAttendanceLogOperator.Import,
                            ProcessedCheckTime = processedCheckTime,
                            RawCheckTime = processedCheckTime,
                            UpdatedUser = user.FullName,
                            UpdatedDate = DateTime.Now,
                            Note = log.Note,
                            InOutMode = 0,
                            VerifyMode = log.VerifyModeFormat
                        };
                        var history = new TA_AjustAttendanceLogHistory()
                        {
                            EmployeeATID = log.EmployeeATID,
                            CompanyIndex = user.CompanyIndex,
                            SerialNumber = log.SerialNumber,
                            Operate = (int)AjustAttendanceLogOperator.Import,
                            ProcessedCheckTime = processedCheckTime,
                            UpdatedUser = user.FullName,
                            UpdatedDate = DateTime.Now,
                            Note = log.Note,
                            InOutMode = 0,
                            VerifyMode = log.VerifyModeFormat
                        };
                        await _dbContext.AddAsync(history);
                        await _dbContext.TA_AjustAttendanceLog.AddAsync(newLog);
                    }
                }

                if (log.TimeOut > DateTime.MinValue)
                {
                    var processedCheckTime = log.DateFormat.ChangeTime(log.TimeOut.Hour, log.TimeOut.Minute, log.TimeOut.Second, 0);

                    var fromLog = processedCheckTime.ChangeTime(processedCheckTime.Hour, processedCheckTime.Minute, processedCheckTime.Second, 0);
                    var toLog = fromLog.AddSeconds(1).AddMilliseconds(-1);
                    var checkExistRawLog = checkExistRawLogs.FirstOrDefault(x => x.EmployeeATID == log.EmployeeATID && x.CompanyIndex == user.CompanyIndex
                    && x.CheckTime >= fromLog && x.CheckTime <= toLog && x.SerialNumber == log.SerialNumber);

                    var checkExistLog = checkExistLogs.FirstOrDefault(x => x.EmployeeATID == log.EmployeeATID && x.CompanyIndex == user.CompanyIndex
                                                   && x.ProcessedCheckTime >= fromLog && x.ProcessedCheckTime <= toLog && x.SerialNumber == log.SerialNumber);

                    if (checkExistRawLog == null && checkExistLog == null)
                    {
                        var newLog = new TA_AjustAttendanceLog()
                        {
                            EmployeeATID = log.EmployeeATID,
                            CompanyIndex = user.CompanyIndex,
                            SerialNumber = log.SerialNumber,
                            CreatedDate = DateTime.Now,
                            Operate = (int)AjustAttendanceLogOperator.Import,
                            ProcessedCheckTime = processedCheckTime,
                            RawCheckTime = processedCheckTime,
                            UpdatedUser = user.FullName,
                            UpdatedDate = DateTime.Now,
                            Note = log.Note,
                            InOutMode = 1,
                            VerifyMode = log.VerifyModeFormat
                        };
                        var history = new TA_AjustAttendanceLogHistory()
                        {
                            EmployeeATID = log.EmployeeATID,
                            CompanyIndex = user.CompanyIndex,
                            SerialNumber = log.SerialNumber,
                            Operate = (int)AjustAttendanceLogOperator.Import,
                            ProcessedCheckTime = processedCheckTime,
                            UpdatedUser = user.FullName,
                            UpdatedDate = DateTime.Now,
                            Note = log.Note,
                            InOutMode = 1,
                            VerifyMode = log.VerifyModeFormat
                        };
                        await _dbContext.AddAsync(history);
                        await _dbContext.TA_AjustAttendanceLog.AddAsync(newLog);
                    }
                }
            }
            await _dbContext.SaveChangesAsync();
        }


    }
}
