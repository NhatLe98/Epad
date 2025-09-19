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
    public interface IGC_CustomerVehicleService : IBaseServices<GC_CustomerVehicle, EPAD_Context>
    {
        Task<GC_CustomerVehicle> GetByEmpAndPlateByUpdate(int index, string pEmployeeATID, string pPlate);
        Task<GC_CustomerVehicle> GetByEmpAndPlate(string pEmployeeATID, string pPlate);
        Task<DataGridClass> GetCustomerVehicleByFilter(string filter, int page, int pageSize);
        Task<DataGridClass> GetCustomerVehicleByFilterAdvance(CustomerVehicleRequest param);
        Task<List<CustomerVehicleImportData>> ImportCustomerVehicle(List<CustomerVehicleImportData> result, UserInfo user);
        Task<GC_CustomerVehicle> GetByIndex(int index);
        Task<bool> AddCustomerVehicle(CustomerVehicleRequest data, UserInfo user);
        Task<bool> UpdateCustomerVehicle(GC_CustomerVehicle data, UserInfo user);
        Task<bool> DeleteCustomerVehicle(List<int> indexes);
    }
}
