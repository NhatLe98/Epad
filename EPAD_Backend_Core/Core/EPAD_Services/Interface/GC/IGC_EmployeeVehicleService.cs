using EPAD_Common.Services;
using EPAD_Common.Types;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;


namespace EPAD_Services.Interface
{
    public interface IGC_EmployeeVehicleService : IBaseServices<GC_EmployeeVehicle, EPAD_Context>
    {
        Task<GC_EmployeeVehicle> GetByEmpAndPlateByUpdate(int index, string pEmployeeATID, string pPlate);
        Task<GC_EmployeeVehicle> GetByEmpAndPlate(string pEmployeeATID, string pPlate);
        Task<DataGridClass> GetEmployeeVehicleByFilter(string filter, int page, int pageSize);
        Task<DataGridClass> GetEmployeeVehicleByFilterAdvance(EmployeeVehicleRequest param);
        Task<List<EmployeeVehicleImportData>> ImportEmployeeVehicle(List<EmployeeVehicleImportData> result, UserInfo user);
    }
}
