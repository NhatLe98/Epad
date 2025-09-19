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
    public interface ITA_ScheduleFixedByDepartmentService : IBaseServices<TA_ScheduleFixedByDepartment, EPAD_Context>
    {
        Task<bool> AddScheduleFixedByDepartment(TA_ScheduleFixedByDepartmentDTO data, int companyIndex);
        Task<bool> UpdateScheduleFixedByDepartment(TA_ScheduleFixedByDepartment dataExist, TA_ScheduleFixedByDepartmentDTO data, int companyIndex);
        bool DeleteScheduleFixedByDepartment(List<int> dataDelete);
        Task<string> CheckScheduleFixedByDepartmentExist(TA_ScheduleFixedByDepartmentDTO data, int companyIndex);
        Task<DataGridClass> GetScheduleFixedByDepartment(List<long> departmentFilter, DateTime date, int pCompanyIndex, int pPage, int pLimit);
        Task<List<ScheduleFixedByDepartmentImportExcel>> AddScheduleDepartmentFromExcel(List<ScheduleFixedByDepartmentImportExcel> param, UserInfo user);
        bool ExportInfoShift(string folderDetails); 
    }
}
