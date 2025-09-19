using EPAD_Common.Enums;
using EPAD_Common.Extensions;
using EPAD_Common.Services;
using EPAD_Common.Types;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Migrations;
using EPAD_Data.Models;
using EPAD_Services.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace EPAD_Services.Impl
{
    public class TA_AjustTimeAttendanceLogService : BaseServices<TA_AjustTimeAttendanceLog, EPAD_Context>, ITA_AjustTimeAttendanceLogService
    {
        private readonly EPAD_Context _dbContext;
        public TA_AjustTimeAttendanceLogService(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _dbContext = serviceProvider.GetService<EPAD_Context>();
        }

        public List<dynamic> GetDataGrid(int pCompanyIndex, int pPage, int pLimit, string filter, List<long> departments, List<string> employeeatids, DateTime fromDate, DateTime toDate)
        {
            var filterBy = new List<string>();
            if (filter != null)
            {
                filterBy = filter.Split(" ").Select(x => x.ToLower()).ToList();
            }

            var leaveDateType = _dbContext.TA_LeaveDateType.Where(x => x.CompanyIndex == pCompanyIndex).ToList();

            var logs = (from attendanceLog in _dbContext.TA_TimeAttendanceLog.Where(t => t.CompanyIndex == pCompanyIndex)
                        join log in _dbContext.TA_AjustTimeAttendanceLog.Where(x => x.CompanyIndex == pCompanyIndex) on
                         new { attendanceLog.EmployeeATID, attendanceLog.Date } equals
                         new { log.EmployeeATID, log.Date }
                       into tempLog
                        from logDump in tempLog.DefaultIfEmpty()

                        join ho in _dbContext.TA_Holiday.Where(x => x.HolidayDate.Date >= fromDate.Date && x.HolidayDate.Date <= toDate.Date)
                        on attendanceLog.Date.Date equals ho.HolidayDate.Date into tempHo
                        from hoDump in tempHo.DefaultIfEmpty()

                        join leave in _dbContext.TA_LeaveRegistration.Where(x => x.LeaveDate.Date >= fromDate.Date && x.LeaveDate.Date <= toDate.Date) on
                       new { attendanceLog.EmployeeATID, attendanceLog.Date.Date } equals
                         new { leave.EmployeeATID, leave.LeaveDate.Date } into tempLeave
                        from leaveDump in tempLeave.DefaultIfEmpty()

                        join leaveType in _dbContext.TA_LeaveDateType.Where(x => x.CompanyIndex == pCompanyIndex)
                        on leaveDump.LeaveDateType equals leaveType.Index into tempLeaveType
                        from leaveTypeDump in tempLeaveType.DefaultIfEmpty()

                        join user in _dbContext.HR_User.Where(x => x.CompanyIndex == pCompanyIndex)
                        on attendanceLog.EmployeeATID equals user.EmployeeATID into temp
                        from dummy in temp.DefaultIfEmpty()

                        join wk in _dbContext.IC_WorkingInfo.Where(x => x.CompanyIndex == pCompanyIndex)
                            on dummy.EmployeeATID equals wk.EmployeeATID into wCheck
                        from wResult in wCheck.DefaultIfEmpty()

                        join de in _dbContext.IC_Department.Where(x => x.CompanyIndex == pCompanyIndex && x.IsInactive != true)
                            on wResult.DepartmentIndex equals de.Index into deCheck
                        from deResult in deCheck.DefaultIfEmpty()

                        join shift in _dbContext.TA_Shift.Where(x => x.CompanyIndex == pCompanyIndex)
                           on attendanceLog.ShiftIndex equals shift.Index into tempShift
                        from shiftDump in tempShift.DefaultIfEmpty()
                        where
                     //        (string.IsNullOrEmpty(filter)
                     //      ? attendanceLog.EmployeeATID.Contains("")
                     //      : (
                     //             attendanceLog.EmployeeATID.Contains(filter)
                     //          || filterBy.Contains(attendanceLog.EmployeeATID)
                     //          || (!string.IsNullOrWhiteSpace(dummy.EmployeeCode) && (dummy.EmployeeCode == filter || dummy.EmployeeCode.Contains(filter) || filterBy.Contains(dummy.EmployeeCode)))
                     //          || (!string.IsNullOrWhiteSpace(dummy.FullName) && (dummy.FullName == filter || dummy.FullName.Contains(filter) || filterBy.Contains(dummy.FullName)))
                     //      ))
                     //&& 
                     ((departments != null && departments.Count > 0) ? departments.Contains(wResult.DepartmentIndex) : true)
                      && ((employeeatids != null && employeeatids.Count > 0) ? employeeatids.Contains(attendanceLog.EmployeeATID) : true)
                       && wResult.Status == 1 && (!wResult.ToDate.HasValue
                       || (wResult.ToDate.HasValue && wResult.ToDate.Value.Date >= DateTime.Now.Date))
                        && ((attendanceLog.Date.Date >= fromDate.Date && attendanceLog.Date.Date <= toDate.Date))
                        select new TA_AjustTimeAttendanceLogDTO
                        {
                            EmployeeATID = attendanceLog.EmployeeATID,
                            FullName = dummy.FullName,
                            DepartmentName = deResult.Name,
                            EmployeeCode = dummy.EmployeeCode,
                            Index = logDump != null ? logDump.Index : 0,
                            DepartmentIndex = wResult.DepartmentIndex,
                            CompanyIndex = attendanceLog.CompanyIndex,
                            Date = attendanceLog.Date,
                            TotalWorkingDay = attendanceLog.TotalWorkingDay,
                            TotalDayOff = attendanceLog.TotalDayOff,
                            TotalHoliday = attendanceLog.TotalHoliday,
                            TotalBusinessTrip = attendanceLog.TotalBusinessTrip,
                            Status = logDump != null ? logDump.Status : "",
                            LeaveType = leaveTypeDump != null ? leaveTypeDump.Code : "",
                            IsHoliday = hoDump != null,
                            TheoryWorkedTimeByShift = shiftDump != null ? shiftDump.TheoryWorkedTimeByShift : (float?)null,
                            CheckIn = attendanceLog.CheckIn,
                            CheckOut = attendanceLog.CheckOut,
                            IsHolidayPaid = hoDump != null ? hoDump.IsPaidWhenNotWorking : false,
                            IsPaidLeave = leaveTypeDump != null ? leaveTypeDump.IsPaidLeave : false,
                            IsWorkedTimeHoliday = leaveTypeDump != null ? leaveTypeDump.IsWorkedTimeHoliday : false,
                            LeaveDurationType = leaveDump != null ? leaveDump.LeaveDurationType : 0
                        }).ToList();

            if (!string.IsNullOrWhiteSpace(filter))
            {
                logs = logs.Where(x => filterBy.Contains(x.EmployeeATID) || filter.Contains(x.EmployeeATID)
                || (!string.IsNullOrWhiteSpace(x.EmployeeCode) && (filter.ToLower().Contains(x.EmployeeCode.ToLower())
                || filter.ToLower() == x.EmployeeCode.ToLower() || x.EmployeeCode.ToLower().Contains(filter.ToLower())))
                || (!string.IsNullOrWhiteSpace(x.FullName) && (filter.ToLower().Contains(x.FullName.ToLower())
                || filter.ToLower() == x.FullName.ToLower() || x.FullName.ToLower().Contains(filter.ToLower())))).ToList();
            }

            var logRe = new List<dynamic>();
            var logRe2 = new List<ObjectT>();
            foreach (var log in logs)
            {
                var theoryWorkedTimeByShift = log.TheoryWorkedTimeByShift ?? 0;
                dynamic logAdd = new ExpandoObject();
                logAdd.EmployeeATID = log.EmployeeATID;
                logAdd.FullName = log.FullName;
                logAdd.DepartmentName = log.DepartmentName;
                logAdd.EmployeeCode = log.EmployeeCode;
                logAdd.TotalWorkingDay = (double)0;
                logAdd.WorkingDay = (double)0;
                logAdd.AnnualLeave = (double)0;
                logAdd.Leave = (double)0;
                logAdd.NoSalaryLeave = (double)0;
                logAdd.BusinessTrip = (double)0;

                var day = log.Date.ToString("yyyy/MM/dd");
                var status = "";
                if (!string.IsNullOrEmpty(log.Status))
                {
                    status = log.Status;
                    if (status == "L")
                    {
                        logAdd.TotalWorkingDay += theoryWorkedTimeByShift;
                    }
                    else if (status == "X")
                    {
                        logAdd.WorkingDay += theoryWorkedTimeByShift;
                        logAdd.TotalWorkingDay += theoryWorkedTimeByShift;
                    }
                    else if (status == "CT")
                    {
                        logAdd.BusinessTrip += theoryWorkedTimeByShift;
                        logAdd.TotalWorkingDay += theoryWorkedTimeByShift;
                    }
                    else if (status == "V")
                    {
                        logAdd.Leave += theoryWorkedTimeByShift;
                    }
                    else if (status != null && status.Contains("/2"))
                    {
                        var statusReplace = status.Replace("/2", "");
                        var statusDb = leaveDateType.FirstOrDefault(x => x.Code == statusReplace);
                        if (statusDb != null)
                        {
                            if (statusDb.IsWorkedTimeHoliday)
                            {
                                logAdd.TotalWorkingDay += theoryWorkedTimeByShift / 2;
                                if (statusDb.IsPaidLeave)
                                {
                                    logAdd.AnnualLeave += theoryWorkedTimeByShift / 2;
                                }
                            }
                            else
                            {
                                logAdd.NoSalaryLeave += theoryWorkedTimeByShift / 2;
                            }
                        }
                    }
                    else
                    {
                        var statusDb = leaveDateType.FirstOrDefault(x => x.Code == status);
                        if (statusDb != null)
                        {
                            if (statusDb.IsWorkedTimeHoliday)
                            {
                                logAdd.TotalWorkingDay += theoryWorkedTimeByShift;
                                if (statusDb.IsPaidLeave)
                                {
                                    logAdd.AnnualLeave += theoryWorkedTimeByShift;
                                }
                            }
                            else
                            {
                                logAdd.NoSalaryLeave += theoryWorkedTimeByShift;
                            }
                        }
                    }


                }
                else if (log.TotalBusinessTrip > 0 && log.TheoryWorkedTimeByShift != null && log.TotalBusinessTrip == log.TheoryWorkedTimeByShift)
                {
                    status = "CT";
                    logAdd.BusinessTrip += theoryWorkedTimeByShift;
                    logAdd.TotalWorkingDay += theoryWorkedTimeByShift;
                }
                else if (log.IsHoliday)
                {
                    status = "L";
                    if (log.IsHolidayPaid)
                    {
                        logAdd.TotalWorkingDay += theoryWorkedTimeByShift;
                    }
                }
                else if (!string.IsNullOrEmpty(log.LeaveType))
                {
                    status = log.LeaveType;
                    var theoryWorkedTime = theoryWorkedTimeByShift;
                    if (log.LeaveDurationType == (int)LeaveDurationType.LeaveHaftShift)
                    {
                        theoryWorkedTime = theoryWorkedTime / 2;
                        status = status + "/2";
                    }

                    //0.5
                    if (log.IsWorkedTimeHoliday)
                    {
                        logAdd.TotalWorkingDay += theoryWorkedTime;
                        if (log.IsPaidLeave)
                        {
                            logAdd.AnnualLeave += theoryWorkedTime;
                        }
                    }
                    else
                    {
                        logAdd.NoSalaryLeave += theoryWorkedTime;
                    }

                }
                else if (string.IsNullOrEmpty(log.Status) && log.Index == 0)
                {

                    if (log.TheoryWorkedTimeByShift != null && log.TotalWorkingDay != 0 && log.TotalWorkingDay == log.TheoryWorkedTimeByShift)
                    {
                        status = "X";
                        logAdd.TotalWorkingDay += theoryWorkedTimeByShift;
                        logAdd.WorkingDay += theoryWorkedTimeByShift;
                    }
                    else if (log.TheoryWorkedTimeByShift == null && log.TotalWorkingDay == 0)
                    {
                        status = null;
                    }
                    else if (log.TheoryWorkedTimeByShift != null && (log.CheckOut == DateTime.MinValue || log.CheckOut == null || log.CheckIn == null || log.CheckIn == DateTime.MinValue) && log.TotalWorkingDay == 0)
                    {
                        status = "V";
                        logAdd.Leave += theoryWorkedTimeByShift;
                    }
                    else
                    {
                        status = log.TotalWorkingDay.ToString();
                        logAdd.TotalWorkingDay += log.TotalWorkingDay;
                        logAdd.WorkingDay += log.TotalWorkingDay;
                    }
                }

                var logAdds = logRe.FirstOrDefault(x => x.EmployeeATID == logAdd.EmployeeATID);
                logRe2.Add(new ObjectT { Day = day, EmployeeATID = logAdd.EmployeeATID, Status = status });
                if (logAdds != null)
                {
                    logAdds.TotalWorkingDay = (decimal)logAdds.TotalWorkingDay + (decimal)logAdd.TotalWorkingDay;
                    logAdds.WorkingDay = (decimal)logAdds.WorkingDay + (decimal)logAdd.WorkingDay;
                    logAdds.AnnualLeave = (decimal)logAdds.AnnualLeave + (decimal)logAdd.AnnualLeave;
                    logAdds.Leave = (decimal)logAdds.Leave + (decimal)logAdd.Leave;
                    logAdds.NoSalaryLeave = (decimal)logAdds.NoSalaryLeave + (decimal)logAdd.NoSalaryLeave;
                    logAdds.BusinessTrip = (decimal)logAdds.BusinessTrip + (decimal)logAdd.BusinessTrip;

                }
                else
                {

                    logRe.Add(logAdd);
                }

            }
            var s = SetObject(logRe2, logRe);
            return s;
        }

        public List<dynamic> GetSyntheticDataGrid(int pCompanyIndex, int pPage, int pLimit, string filter, List<long> departments, List<string> employeeatids, DateTime fromDate, DateTime toDate, int filterByType)
        {
            var filterBy = new List<string>();
            if (filter != null)
            {
                filterBy = filter.Split(" ").Select(x => x.ToLower()).ToList();
            }

            var leaveDateType = _dbContext.TA_LeaveDateType.Where(x => x.CompanyIndex == pCompanyIndex).ToList();

            var logs = (from attendanceLog in _dbContext.TA_TimeAttendanceLog.Where(t => t.CompanyIndex == pCompanyIndex)
                        join ho in _dbContext.TA_Holiday.Where(x => x.HolidayDate.Date >= fromDate.Date && x.HolidayDate.Date <= toDate.Date)
                        on attendanceLog.Date.Date equals ho.HolidayDate.Date into tempHo
                        from hoDump in tempHo.DefaultIfEmpty()

                        join leave in _dbContext.TA_LeaveRegistration.Where(x => x.LeaveDate.Date >= fromDate.Date && x.LeaveDate.Date <= toDate.Date) on
                       new { attendanceLog.EmployeeATID, attendanceLog.Date.Date } equals
                         new { leave.EmployeeATID, leave.LeaveDate.Date } into tempLeave
                        from leaveDump in tempLeave.DefaultIfEmpty()

                        join leaveType in _dbContext.TA_LeaveDateType.Where(x => x.CompanyIndex == pCompanyIndex)
                        on leaveDump.LeaveDateType equals leaveType.Index into tempLeaveType
                        from leaveTypeDump in tempLeaveType.DefaultIfEmpty()

                        join user in _dbContext.HR_User.Where(x => x.CompanyIndex == pCompanyIndex)
                        on attendanceLog.EmployeeATID equals user.EmployeeATID into temp
                        from dummy in temp.DefaultIfEmpty()

                        join wk in _dbContext.IC_WorkingInfo.Where(x => x.CompanyIndex == pCompanyIndex)
                            on dummy.EmployeeATID equals wk.EmployeeATID into wCheck
                        from wResult in wCheck.DefaultIfEmpty()

                        join de in _dbContext.IC_Department.Where(x => x.CompanyIndex == pCompanyIndex && x.IsInactive != true)
                            on wResult.DepartmentIndex equals de.Index into deCheck
                        from deResult in deCheck.DefaultIfEmpty()

                        join shift in _dbContext.TA_Shift.Where(x => x.CompanyIndex == pCompanyIndex)
                           on attendanceLog.ShiftIndex equals shift.Index into tempShift
                        from shiftDump in tempShift.DefaultIfEmpty()
                        where
                     //        (string.IsNullOrEmpty(filter)
                     //      ? attendanceLog.EmployeeATID.Contains("")
                     //      : (
                     //             attendanceLog.EmployeeATID.Contains(filter)
                     //          || filterBy.Contains(attendanceLog.EmployeeATID)
                     //          || (!string.IsNullOrWhiteSpace(dummy.EmployeeCode) && (dummy.EmployeeCode == filter || dummy.EmployeeCode.Contains(filter) || filterBy.Contains(dummy.EmployeeCode)))
                     //          || (!string.IsNullOrWhiteSpace(dummy.FullName) && (dummy.FullName == filter || dummy.FullName.Contains(filter) || filterBy.Contains(dummy.FullName)))
                     //      ))
                     //&& 
                     ((departments != null && departments.Count > 0) ? departments.Contains(wResult.DepartmentIndex) : true)
                      && ((employeeatids != null && employeeatids.Count > 0) ? employeeatids.Contains(attendanceLog.EmployeeATID) : true)
                       && wResult.Status == 1 && (!wResult.ToDate.HasValue
                       || (wResult.ToDate.HasValue && wResult.ToDate.Value.Date >= DateTime.Now.Date))
                        && ((attendanceLog.Date.Date >= fromDate.Date && attendanceLog.Date.Date <= toDate.Date))
                        select new TA_AjustTimeAttendanceLogDTO
                        {
                            EmployeeATID = attendanceLog.EmployeeATID,
                            FullName = dummy.FullName,
                            DepartmentName = deResult.Name,
                            EmployeeCode = dummy.EmployeeCode,
                            Index = 0,
                            DepartmentIndex = wResult.DepartmentIndex,
                            CompanyIndex = attendanceLog.CompanyIndex,
                            Date = attendanceLog.Date,
                            TotalWorkingDay = attendanceLog.TotalWorkingDay,
                            TotalDayOff = attendanceLog.TotalDayOff,
                            TotalHoliday = attendanceLog.TotalHoliday,
                            TotalBusinessTrip = attendanceLog.TotalBusinessTrip,
                            Status = "",
                            LeaveType = leaveTypeDump != null ? leaveTypeDump.Code : "",
                            IsHoliday = hoDump != null,
                            TheoryWorkedTimeByShift = shiftDump != null ? shiftDump.TheoryWorkedTimeByShift : (float?)null,
                            CheckIn = attendanceLog.CheckIn,
                            CheckOut = attendanceLog.CheckOut,
                            IsHolidayPaid = hoDump != null ? hoDump.IsPaidWhenNotWorking : false,
                            IsPaidLeave = leaveTypeDump != null ? leaveTypeDump.IsPaidLeave : false,
                            IsWorkedTimeHoliday = leaveTypeDump != null ? leaveTypeDump.IsWorkedTimeHoliday : false,
                            LeaveDurationType = leaveDump != null ? leaveDump.LeaveDurationType : 0,
                            TotalWorkingTime = attendanceLog.TotalWorkingTime,
                            TotalWorkingTimeNormal = attendanceLog.TotalWorkingTimeNormal,
                            TotalOverTime = attendanceLog.TotalOverTime,
                            CheckInLate = attendanceLog.CheckInLate,
                            CheckOutEarly = attendanceLog.CheckOutEarly,
                            TotalOverTimeNormal = attendanceLog.TotalOverTimeNormal,
                            TotalOverTimeNightNormal = attendanceLog.TotalOverTimeNightNormal,
                            TotalOverTimeDayOff = attendanceLog.TotalOverTimeDayOff,
                            TotalOverTimeNightDayOff = attendanceLog.TotalOverTimeNightDayOff,
                            TotalOverTimeHoliday = attendanceLog.TotalOverTimeHoliday,
                            TotalOverTimeNightHoliday = attendanceLog.TotalOverTimeNightHoliday

                        }).ToList();

            if (!string.IsNullOrWhiteSpace(filter))
            {
                logs = logs.Where(x => filterBy.Contains(x.EmployeeATID) || filter.Contains(x.EmployeeATID)
                || (!string.IsNullOrWhiteSpace(x.EmployeeCode) && (filter.ToLower().Contains(x.EmployeeCode.ToLower())
                || filter.ToLower() == x.EmployeeCode.ToLower() || x.EmployeeCode.ToLower().Contains(filter.ToLower())))
                || (!string.IsNullOrWhiteSpace(x.FullName) && (filter.ToLower().Contains(x.FullName.ToLower())
                || filter.ToLower() == x.FullName.ToLower() || x.FullName.ToLower().Contains(filter.ToLower())))).ToList();
            }

            var logRe = new List<dynamic>();
            var logRe2 = new List<ObjectT>();
            foreach (var log in logs)
            {
                var clt = log.TheoryWorkedTimeByShift ?? 0;
                var theoryWorkedTimeByShift = double.Parse(clt.ToString());
                dynamic logAdd = new ExpandoObject();
                logAdd.EmployeeATID = log.EmployeeATID;
                logAdd.FullName = log.FullName;
                logAdd.DepartmentName = log.DepartmentName;
                logAdd.EmployeeCode = log.EmployeeCode;
                logAdd.TotalWorkingDay = (double)0;
                logAdd.WorkingDay = (double)0;
                logAdd.AnnualLeave = (double)0;
                logAdd.Leave = (double)0;
                logAdd.NoSalaryLeave = (double)0;
                logAdd.BusinessTrip = (double)0;

                var day = log.Date.ToString("yyyy/MM/dd");
                var status = "";
                if (filterByType > 0)
                {
                    if (filterByType == (int)OptionFilterLog.TotalHour)
                    {
                        status = log.TotalWorkingTime.ToString();
                    }
                    else if (filterByType == (int)OptionFilterLog.TotalHourNormal)
                    {
                        status = log.TotalWorkingTimeNormal.ToString();
                    }
                    else if (filterByType == (int)OptionFilterLog.OverTime)
                    {
                        var totalOverTime = log.TotalOverTimeNormal + log.TotalOverTimeNightNormal + log.TotalOverTimeDayOff + log.TotalOverTimeNightDayOff + log.TotalOverTimeHoliday + log.TotalOverTimeNightHoliday;
                        status = totalOverTime.ToString();
                    }
                    else if (filterByType == (int)OptionFilterLog.TotalHourNormalAndOverTime)
                    {
                        var totalOverTime = log.TotalOverTimeNormal + log.TotalOverTimeNightNormal + log.TotalOverTimeDayOff + log.TotalOverTimeNightDayOff + log.TotalOverTimeHoliday + log.TotalOverTimeNightHoliday;
                        status = log.TotalWorkingTimeNormal.ToString() + " + " + totalOverTime.ToString();
                    }
                    else if (filterByType == (int)OptionFilterLog.CheckInLate)
                    {
                        var timeSpan = TimeSpan.FromMinutes(log.CheckInLate);
                        // Format the TimeSpan to HH:mm:ss
                        string formattedTime = timeSpan.ToString(@"hh\:mm\:ss");
                        status = formattedTime;
                    }
                    else if (filterByType == (int)OptionFilterLog.CheckOutEarly)
                    {
                        var timeSpan = TimeSpan.FromMinutes(log.CheckOutEarly);
                        // Format the TimeSpan to HH:mm:ss
                        string formattedTime = timeSpan.ToString(@"hh\:mm\:ss");
                        status = formattedTime;
                    }
                    else if (filterByType == (int)OptionFilterLog.HourByLogInOut)
                    {
                        var checkIn = log.CheckIn != null ? log.CheckIn.Value.ToString("HH:mm") : "";
                        var checkOut = log.CheckOut != null ? log.CheckOut.Value.ToString("HH:mm") : "";
                        status = checkIn + " | " + checkOut;
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(log.Status))
                    {
                        status = log.Status;
                        if (status == "L")
                        {
                            logAdd.TotalWorkingDay += theoryWorkedTimeByShift;
                        }
                        else if (status == "X")
                        {
                            logAdd.WorkingDay += theoryWorkedTimeByShift;
                            logAdd.TotalWorkingDay += theoryWorkedTimeByShift;
                        }
                        else if (status == "CT")
                        {
                            logAdd.BusinessTrip += theoryWorkedTimeByShift;
                            logAdd.TotalWorkingDay += theoryWorkedTimeByShift;
                        }
                        else if (status == "V")
                        {
                            logAdd.Leave += theoryWorkedTimeByShift;
                        }
                        else if (status != null && status.Contains("/2"))
                        {
                            var statusReplace = status.Replace("/2", "");
                            var statusDb = leaveDateType.FirstOrDefault(x => x.Code == statusReplace);
                            if (statusDb != null)
                            {
                                if (statusDb.IsWorkedTimeHoliday)
                                {
                                    logAdd.TotalWorkingDay += theoryWorkedTimeByShift / 2;
                                    if (statusDb.IsPaidLeave)
                                    {
                                        logAdd.AnnualLeave += theoryWorkedTimeByShift / 2;
                                    }
                                }
                                else
                                {
                                    logAdd.NoSalaryLeave += theoryWorkedTimeByShift / 2;
                                }
                            }
                        }
                        else
                        {
                            var statusDb = leaveDateType.FirstOrDefault(x => x.Code == status);
                            if (statusDb != null)
                            {
                                if (statusDb.IsWorkedTimeHoliday)
                                {
                                    logAdd.TotalWorkingDay += theoryWorkedTimeByShift;
                                    if (statusDb.IsPaidLeave)
                                    {
                                        logAdd.AnnualLeave += theoryWorkedTimeByShift;
                                    }
                                }
                                else
                                {
                                    logAdd.NoSalaryLeave += theoryWorkedTimeByShift;
                                }
                            }
                        }


                    }
                    else if (log.TotalBusinessTrip > 0 && log.TheoryWorkedTimeByShift != null && log.TotalBusinessTrip == log.TheoryWorkedTimeByShift)
                    {
                        status = "CT";
                        logAdd.BusinessTrip += theoryWorkedTimeByShift;
                        logAdd.TotalWorkingDay += theoryWorkedTimeByShift;
                    }
                    else if (log.IsHoliday)
                    {
                        status = "L";
                        if (log.IsHolidayPaid)
                        {
                            logAdd.TotalWorkingDay += theoryWorkedTimeByShift;
                        }
                    }
                    else if (!string.IsNullOrEmpty(log.LeaveType))
                    {
                        status = log.LeaveType;
                        var theoryWorkedTime = theoryWorkedTimeByShift;
                        if (log.LeaveDurationType == (int)LeaveDurationType.LeaveHaftShift)
                        {
                            theoryWorkedTime = theoryWorkedTime / 2;
                            status = status + "/2";
                        }

                        //0.5
                        if (log.IsWorkedTimeHoliday)
                        {
                            logAdd.TotalWorkingDay += theoryWorkedTime;
                            if (log.IsPaidLeave)
                            {
                                logAdd.AnnualLeave += theoryWorkedTime;
                            }
                        }
                        else
                        {
                            logAdd.NoSalaryLeave += theoryWorkedTime;
                        }

                    }
                    else if (string.IsNullOrEmpty(log.Status) && log.Index == 0)
                    {

                        if (log.TheoryWorkedTimeByShift != null && log.TotalWorkingDay != 0 && log.TotalWorkingDay == log.TheoryWorkedTimeByShift)
                        {
                            status = "X";
                            logAdd.TotalWorkingDay += theoryWorkedTimeByShift;
                            logAdd.WorkingDay += theoryWorkedTimeByShift;
                        }
                        else if (log.TheoryWorkedTimeByShift == null && log.TotalWorkingDay == 0)
                        {
                            status = null;
                        }
                        else if (log.TheoryWorkedTimeByShift != null && (log.CheckOut == DateTime.MinValue || log.CheckOut == null || log.CheckIn == null || log.CheckIn == DateTime.MinValue) && log.TotalWorkingDay == 0)
                        {
                            status = "V";
                            logAdd.Leave += theoryWorkedTimeByShift;
                        }
                        else
                        {
                            status = log.TotalWorkingDay.ToString();
                            logAdd.TotalWorkingDay += log.TotalWorkingDay;
                            logAdd.WorkingDay += log.TotalWorkingDay;
                        }
                    }
                }


                var logAdds = logRe.FirstOrDefault(x => x.EmployeeATID == logAdd.EmployeeATID);
                logRe2.Add(new ObjectT { Day = day, EmployeeATID = logAdd.EmployeeATID, Status = status });
                if (logAdds != null)
                {

                    logAdds.TotalWorkingDay = (decimal)logAdds.TotalWorkingDay + (decimal)logAdd.TotalWorkingDay;
                    logAdds.WorkingDay = (decimal)logAdds.WorkingDay + (decimal)logAdd.WorkingDay;
                    logAdds.AnnualLeave = (decimal)logAdds.AnnualLeave + (decimal)logAdd.AnnualLeave;
                    logAdds.Leave = (decimal)logAdds.Leave + (decimal)logAdd.Leave;
                    logAdds.NoSalaryLeave = (decimal)logAdds.NoSalaryLeave + (decimal)logAdd.NoSalaryLeave;
                    logAdds.BusinessTrip = (decimal)logAdds.BusinessTrip + (decimal)logAdd.BusinessTrip;

                }
                else
                {

                    logRe.Add(logAdd);
                }

            }
            var s = SetObject(logRe2, logRe);
            return s;
        }

        public class ObjectT
        {
            public string Status { get; set; }
            public string Day { get; set; }
            public string EmployeeATID { get; set; }
        }

        private List<dynamic> SetObject(List<ObjectT> list, List<dynamic> obj)
        {
            var s = list.GroupBy(x => x.EmployeeATID).Select(x => new { x.Key, Data = x.ToList() }).ToList();
            foreach (var item in s)
            {
                var data = obj.FirstOrDefault(x => x.EmployeeATID == item.Key);
                var expandoDict = data as IDictionary<string, object>;
                foreach (var item1 in item.Data)
                {
                    try
                    {
                        expandoDict.Add(item1.Day, item1.Status);
                    }
                    catch { }

                }
            }
            return obj;
        }

        private void SetObjectProperty(string propertyName, string value, object obj)
        {
            PropertyInfo propertyInfo = obj.GetType().GetProperty(propertyName);
            // make sure object has the property we are after
            if (propertyInfo != null)
            {
                propertyInfo.SetValue(obj, value, null);
            }
        }

        public void AddProperty(ExpandoObject expando, string propertyName, object propertyValue)
        {
            // ExpandoObject supports IDictionary so we can extend it like this
            var expandoDict = expando as IDictionary<string, object>;
            if (expandoDict.ContainsKey(propertyName))
                expandoDict[propertyName] = propertyValue;
            else
                expandoDict.Add(propertyName, propertyValue);
        }
        public void UpdateAjustTimeAttendanceLog(AjustTimeAttendanceLogInsertParam logs, UserInfo user)
        {
            var model = JsonConvert.DeserializeObject<List<dynamic>>(logs.Json);

            var listName = new List<string>() { "FullName", "DepartmentName", "EmployeeCode", "TotalWorkingDay", "WorkingDay", "AnnualLeave", "Leave", "NoSalaryLeave", "BusinessTrip" };
            var listAdd = new List<TA_AjustTimeAttendanceLogInsertDTO>();
            foreach (var result in model)
            {
                var timeAdd = new TA_AjustTimeAttendanceLogInsertDTO();
                var listTime = new List<TA_AjustTimeList>();
                foreach (string propertyName in GetPropertyKeysForDynamic(result))
                {
                    if (!listName.Contains(propertyName))
                    {
                        var value = result[propertyName];
                        if (propertyName == "EmployeeATID")
                        {
                            timeAdd.EmployeeATID = value.ToString();
                        }
                        else
                        {
                            var time = new TA_AjustTimeList();
                            time.Date = Convert.ToDateTime(propertyName);
                            time.WorkingType = value != null ? value.ToString() : null;

                            listTime.Add(time);
                        }

                    }
                }
                timeAdd.AjustTimeLists = listTime;
                listAdd.Add(timeAdd);
            }

            var lstValue = listAdd.Select(x => x.AjustTimeLists).SelectMany(x => x).Select(x => x.Date);
            var minValue = lstValue.Min();
            var maxValue = lstValue.Max();
            var lstEmployeeATID = listAdd.Select(x => x.EmployeeATID).Distinct().ToList();

            var lstTimeAttendance = _dbContext.TA_AjustTimeAttendanceLog.Where(x => lstEmployeeATID.Contains(x.EmployeeATID) && x.Date.Date >= minValue.Date && x.Date.Date <= maxValue.Date).ToList();
            foreach (var item in listAdd)
            {
                foreach (var log in item.AjustTimeLists)
                {
                    var time = lstTimeAttendance.FirstOrDefault(x => x.EmployeeATID == item.EmployeeATID && x.Date.Date == log.Date.Date);
                    if (time == null)
                    {
                        var logAdd = new TA_AjustTimeAttendanceLog
                        {
                            EmployeeATID = item.EmployeeATID,
                            CompanyIndex = user.CompanyIndex,
                            Date = log.Date.Date,
                            Status = log.WorkingType,
                            UpdatedDate = DateTime.Now,
                            UpdatedUser = user.FullName
                        };

                        _dbContext.TA_AjustTimeAttendanceLog.Add(logAdd);
                    }
                    else
                    {
                        time.Status = log.WorkingType;
                        time.UpdatedDate = DateTime.Now;
                        time.UpdatedUser = user.FullName;
                        _dbContext.TA_AjustTimeAttendanceLog.Update(time);
                    }
                }
            }
            _dbContext.SaveChanges();

        }
        public List<string> GetPropertyKeysForDynamic(dynamic dynamicToGetPropertiesFor)
        {
            JObject attributesAsJObject = dynamicToGetPropertiesFor;
            Dictionary<string, object> values = attributesAsJObject.ToObject<Dictionary<string, object>>();
            List<string> toReturn = new List<string>();
            foreach (string key in values.Keys)
            {
                toReturn.Add(key);
            }
            return toReturn;
        }


        public List<ComboboxItem> GetAllRegistrationType()
        {
            var result = _dbContext.TA_LeaveDateType.Select(x => new ComboboxItem
            {
                value = x.Code,
                label = x.Code
            }).ToList();

            result.Add(new ComboboxItem { label = "X", value = "X" });
            result.Add(new ComboboxItem { label = "CT", value = "CT" });
            result.Add(new ComboboxItem { label = "V", value = "V" });
            result.Add(new ComboboxItem { label = "L", value = "L" });

            var resultHalft = _dbContext.TA_LeaveDateType.Select(x => new ComboboxItem
            {
                value = x.Code + "/2",
                label = x.Code + "/2"
            }).ToList();
            result.AddRange(resultHalft);
            return result;
        }

    }


}
