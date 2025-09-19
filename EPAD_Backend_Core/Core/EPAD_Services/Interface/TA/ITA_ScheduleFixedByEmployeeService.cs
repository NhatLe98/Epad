using EPAD_Common.Services;
using EPAD_Common.Types;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EPAD_Services.Interface
{
    public interface ITA_ScheduleFixedByEmployeeService : IBaseServices<TA_ScheduleFixedByEmployee, EPAD_Context>
    {
        Task<bool> AddScheduleFixedByEmployee(TA_ScheduleFixedByEmployeeDTO data, int companyIndex);
        Task<bool> UpdateScheduleFixedByEmployee(TA_ScheduleFixedByEmployee dataExist, TA_ScheduleFixedByEmployeeDTO data, int companyIndex);
        bool DeleteScheduleFixedByEmployee(List<int> dataDelete);
        Task<string> CheckScheduleFixedByEmployeeExist(TA_ScheduleFixedByEmployeeDTO data, int companyIndex);
        Task<DataGridClass> GetScheduleFixedByEmployee(List<string> employeeFilter, DateTime fromDate, int pCompanyIndex, int pPage, int pLimit);
        Task<List<ScheduleFixedByEmployeeImportExcel>> AddScheduleFixedByEmployeeFromExcel(List<ScheduleFixedByEmployeeImportExcel> param, UserInfo user);
        bool ExportInfoShift(string folderDetails);
    }
}
