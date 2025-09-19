using EPAD_Common.Services;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EPAD_Services.Interface
{
    public interface ITA_EmployeeShiftService : IBaseServices<TA_EmployeeShift, EPAD_Context>
    {
        List<TA_EmployeeShift> GetListEmpoyeeShiftByDateAndEmps(DateTime from, DateTime to, List<string> employeeATIDs, int companyIndex);
        bool DeleteEmployeeShift(List<CM_EmployeeShiftModel> param);
        Task<List<EmployeeShiftTableRequest>> ImportShiftTable(List<EmployeeShiftTableRequest> shiftTableData, UserInfo user);
    }
}
