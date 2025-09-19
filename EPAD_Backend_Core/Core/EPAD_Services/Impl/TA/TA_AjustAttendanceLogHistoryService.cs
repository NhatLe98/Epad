using EPAD_Common.Services;
using EPAD_Data.Entities;
using EPAD_Data;
using EPAD_Services.Interface;
using System;
using System.Collections.Generic;
using System.Text;
using EPAD_Common.Types;
using System.Threading.Tasks;
using EPAD_Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using EPAD_Common.Extensions;

namespace EPAD_Services.Impl
{
    public class TA_AjustAttendanceLogHistoryService : BaseServices<TA_AjustAttendanceLogHistory, EPAD_Context>, ITA_AjustAttendanceLogHistoryService
    {
        private readonly EPAD_Context _dbContext;
        public TA_AjustAttendanceLogHistoryService(IServiceProvider serviceProvider, EPAD_Context context) : base(serviceProvider)
        {
            _dbContext = context;
        }

        public DataGridClass GetDataGrid(int pCompanyIndex, int pPage, int pLimit, string filter, List<long> departments, List<string> employeeatids, DateTime fromTime, DateTime toTime, List<int> operators)
        {
            DataGridClass dataGrid = null;
            int countPage = 0;
            var filterBy = new List<string>();
            if (filter != null)
            {
                filterBy = filter.Split(" ").Select(x => x.ToLower()).ToList();
            }

            var devices = _dbContext.IC_Device.Where(x => x.CompanyIndex == pCompanyIndex).ToList();
            var logs = (from attendanceLog in _dbContext.TA_AjustAttendanceLogHistory.Where(t => t.CompanyIndex == pCompanyIndex)     
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
                     //          || (!string.IsNullOrWhiteSpace(dummy.EmployeeCode) && (dummy.EmployeeCode == filter || dummy.EmployeeCode.Contains(filter) || filterBy.Contains(dummy.EmployeeCode)))
                     //          || (!string.IsNullOrWhiteSpace(dummy.FullName) && (dummy.FullName == filter || dummy.FullName.Contains(filter) || filterBy.Contains(dummy.FullName)))
                     //      ))
                     //&& 
                     ((departments != null && departments.Count > 0) ? departments.Contains(wResult.DepartmentIndex) : true)
                      && ((employeeatids != null && employeeatids.Count > 0) ? employeeatids.Contains(attendanceLog.EmployeeATID) : true)
                       && ((operators != null && operators.Count > 0) ? operators.Contains(attendanceLog.Operate) : true)
                       && wResult.Status == 1 && (!wResult.ToDate.HasValue
                       || (wResult.ToDate.HasValue && wResult.ToDate.Value.Date >= DateTime.Now.Date))
                       && ( attendanceLog.UpdatedDate >= fromTime && attendanceLog.UpdatedDate <= toTime)
                        select new TA_AjustAttendanceLogHistoryDTO
                        {
                            EmployeeATID = attendanceLog.EmployeeATID,
                            FullName = dummy.FullName,
                            DepartmentName = deResult.Name,
                            EmployeeCode = dummy.EmployeeCode,
                            CompanyIndex = attendanceLog.CompanyIndex,
                            InOutMode = attendanceLog.InOutMode,
                            VerifyMode = attendanceLog.VerifyMode,
                            DeviceName = devResult != null ? devResult.AliasName : "",
                            Note = attendanceLog.Note,
                            UpdatedUser = attendanceLog.UpdatedUser,
                            UpdatedDate = attendanceLog.UpdatedDate,
                            Operate = attendanceLog.Operate,
                            ProcessedCheckTime = attendanceLog.ProcessedCheckTime,
                            RawCheckTime = attendanceLog.RawCheckTime
                        }).ToList();

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
                var lstAnnualLeave = logs.OrderByDescending(t => t.UpdatedDate).Take(pLimit).ToList();
                lstAnnualLeave = lstAnnualLeave.Select(x =>
                {
                    x.InOutModeString = x.InOutMode.GetInOutModeString() == "In" ? "Vào" : "Ra";
                    x.VerifyModeString = x.VerifyMode.GetVerifyModeString();
                    x.Hour = x.ProcessedCheckTime.ToString("HH:mm:ss");
                    x.Day = x.ProcessedCheckTime.ToString("dd/MM/yyyy");
                    x.UpdatedDateString = x.UpdatedDate.ToddMMyyyyHHmmss();
                    x.OperatorString = ((AjustAttendanceLogOperator)x.Operate).ToString();
                    x.OriginalData = x.RawCheckTime.ToString("HH:mm:ss");
                    x.ModifiedData = x.ProcessedCheckTime.ToString("HH:mm:ss");
                    return x;
                }).ToList();
                dataGrid = new DataGridClass(countPage, lstAnnualLeave);
            }
            else
            {
                int fromRow = pLimit * (pPage - 1);
                var lstAnnualLeave = logs.OrderByDescending(t => t.UpdatedDate).Skip(fromRow).Take(pLimit).ToList();
                lstAnnualLeave = lstAnnualLeave.Select(x =>
                {
                    x.InOutModeString = x.InOutMode.GetInOutModeString() == "In" ? "Vào" : "Ra";
                    x.VerifyModeString = x.VerifyMode.GetVerifyModeString();
                    x.Hour = x.ProcessedCheckTime.ToString("HH:mm:ss");
                    x.Day = x.ProcessedCheckTime.ToString("dd/MM/yyyy");
                    x.UpdatedDateString = x.UpdatedDate.ToddMMyyyyHHmmss();
                    x.OperatorString = ((AjustAttendanceLogOperator)x.Operate).ToString();
                    x.OriginalData = x.RawCheckTime.ToString("HH:mm:ss");
                    x.ModifiedData = x.ProcessedCheckTime.ToString("HH:mm:ss");
                    return x;
                }).ToList();
                dataGrid = new DataGridClass(countPage, lstAnnualLeave);
            }
            return dataGrid;
        }

        public async Task InsertAjustAttendanceLogHistory(TA_AjustAttendanceLogHistory log, UserInfo user)
        {
            await _dbContext.AddAsync(log);
            await _dbContext.SaveChangesAsync();
        }
    }
}
