using EPAD_Common.Services;
using EPAD_Common.Types;
using EPAD_Common.Utility;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using EPAD_Data.Models.TimeLog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EPAD_Services.Interface
{
    public interface IIC_AttendanceLogService : IBaseServices<IC_AttendanceLog, EPAD_Context>
    {
        Task<List<AttendanceLogInfoResult>> GetMany(List<AddedParam> addedParams);
        Task<DataGridClass> GetDataGrid(string pFilter, DateTime pFromDate, DateTime pToDate, List<string> pEmpATIDs, int pCompanyIndex, int pPageIndex, int pPageSize, UserInfo user);
        void AddAttendanceLog(PostAttendanceLog req, ref TimeLogRequest timeLogRequest, ref List<OverBodyTemparatureEmployeesList> obtEmployeeList, UserInfo user);
        TimeLog CreateTimeLogForRequest(IC_AttendanceLog attendanceLog);
        Task SendTimeLogToAPIAsync(IC_Config cfg, TimeLogRequest timeLogRequest);
        void SendMailWhenHaveEmployeeOverTemp(List<OverBodyTemparatureEmployeesList> obtEmployeeList);
        Task AddAttendanceLogByDevice(PostAttendanceLog req, UserInfo user);
        Task<DataGridClass> GetAttendanceLog(string pFilter, DateTime fromDate, DateTime toDate, List<string> pEmpATIDs, int pCompanyIndex, int pPageIndex, int pPageSize, List<string> listDevice);

        List<IC_AttendanceLog> GetAttendanceLogByEmployeeATIDsAndDateTime(List<string> employeeATIDs, DateTime dateTime, UserInfo user);
        object GetLastedRealtimeAttendanceLog(ConfigObject config, UserInfo user);
        List<AttendanceLogInfoResult> GetAttendanceLogByDeviceInCanteen(DateTime fromDate, DateTime toDate, int pCompanyIndex);
        Task<Dictionary<string, object>> GetLogLast7Days();
        Task<object> GetRemainInLogs();
        Task<Tuple<object, object, object>> GetLogsByDoor();
        Task<List<WorkingLogResult>> GetWorkingEmployeeByDepartment();
        Task<List<WorkingLogResult>> GetFullWorkingEmployeeByRootDepartment(UserInfo user);
        Task<List<WorkingLogResult>> GetFullWorkingEmployeeByDepartment(UserInfo user);
        Task<Tuple<List<WorkingLogResult>, List<WorkingLogResult>, List<WorkingLogResult>>> 
            GetTupleFullWorkingEmployeeByDepartment(UserInfo user);
        Task<Tuple<List<WorkingLogResult>, List<WorkingLogResult>, List<WorkingLogResult>>>
            GetTupleFullVehicleEmployeeByDepartment(UserInfo user);
        void UpdateLatestEmergencyAttendance();
        string GetLatestEmergencyAttendance();
        Task<Tuple<List<WorkingLogResult>, List<WorkingLogResult>, List<WorkingLogResult>, List<WorkingLogResult>>>
            GetIntegratedVehicleLog(UserInfo user);
        Task<Tuple<List<WorkingLogResult>, List<WorkingLogResult>, List<WorkingLogResult>, List<WorkingLogResult>>>
            GetTruckDriverLog(UserInfo user);
        Task<List<WorkingLogResult>>
            GetEmergencyLog(UserInfo user);
        Task<dynamic> GetTupleFullWorkingEmployeeByUserType(UserInfo user);
        Task<Tuple<List<WorkingLogResult>, List<WorkingLogResult>, List<WorkingLogResult>>>
            GetTupleFullWorkingEmployeeByRootDepartment(UserInfo user);
        Task AddAttendanceLogByDeviceFR05M(PostAttendanceLog req);
        List<IC_AttendanceLog> GetAttendanceLogByEmployeeATIDsAndTime(List<string> employeeATIDs, DateTime dateTime, UserInfo user, List<GetAttendanceLogByTime> contractorTime, List<GetAttendanceLogByTime> securityTime);
        Task<DataGridClass> GetACAttendanceLog(string pFilter, DateTime fromDate, DateTime toDate, List<long> pDepartmentIds, int pCompanyIndex, int pPageIndex, int pPageSize, List<int> listArea, List<int> listDoor);
        object GetLastedACOpenDoor(ConfigObject config, UserInfo user);
        Task RunIntegrateLogManual(int previousDay);
        Task<List<AC_DoorStatus>> GetDoorStatus();
        Task<object> GetEmergencyAndEvacuation(UserInfo user);
    }
}
