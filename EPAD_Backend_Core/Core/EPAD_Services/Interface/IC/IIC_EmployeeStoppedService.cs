using EPAD_Common.Services;
using EPAD_Common.Types;
using EPAD_Data;
using EPAD_Data.Entities;

using EPAD_Data.Models;
using EPAD_Data.Models.IC;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EPAD_Services.Interface
{
    public interface IIC_EmployeeStoppedService : IBaseServices<IC_EmployeeStopped, EPAD_Context>
    {
        DataGridClass GetDataGrid(int pCompanyIndex, int pPage, int pLimit, string filter);
        Task<string> AddEmployeeStopped(IC_EmployeeStoppedDTO ic_EmployeeStopped, UserInfo user);
        Task<bool> UpdateEmployeeStopped(IC_EmployeeStoppedDTO param, UserInfo user, IC_EmployeeStopped existingEmployee);
        Task<IC_EmployeeStopped> GetEmployeeStoppedByIndex(int index, int companyIndex);
        Task<bool> DeleteEmployeeStopped(List<int> listIndex);
        List<IC_EmployeeStoppedImportDTO> ValidationImportEmployeeStopped(List<IC_EmployeeStoppedImportDTO> param, UserInfo user);
    }
}
