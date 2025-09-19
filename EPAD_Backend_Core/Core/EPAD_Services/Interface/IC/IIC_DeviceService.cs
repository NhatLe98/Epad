using EPAD_Common.Services;
using EPAD_Common.Types;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EPAD_Services.Interface
{
    public interface IIC_DeviceService : IBaseServices<IC_Device, EPAD_Context>
    {
        public Task<DataGridClass> GetDataGrid(string pFilter, UserInfo pUser, int pPage, int pPageSize);
        Task<DataGridClass> GetDeviceInfo(UserInfo pUser);
        public Task<IEnumerable<ComboboxItem>> GetDeviceAll(UserInfo pUser);

        public Task<IEnumerable<IC_Device>> GetAllWithPrivilegeFull(UserInfo pUser);

        public Task<IC_Device> GetBySerialNumber(string pSerialNumber, int pCompanyIndex);
        public Task<List<IC_Device>> GetBySerialNumbers(string[] pSerialNumber, int pCompanyIndex);
        public Task<IC_Device> GetCanEditableBySerialNumber(string pSerialNumber, UserInfo pUser);
        public Task<IC_Device> GetCanFullPrivilegeBySerialNumber(string pSerialNumber, UserInfo pUser);
        public Task<List<IC_Device>> GetListCanFullPrivilegeBySerialNumber(string[] pSerialNumbers, UserInfo pUser);

        public Task<string> TryDeleteDevices(string[] pSerialNumbers, UserInfo pUser);
        Task<IC_Device> CreateNewDeviceAsync(IC_Device device, int userPrivilegeIndex, int? serviceIndex, int? groupDeviceIndex);
        Task<List<IC_Device>> GetListDeviceByDeviceModule(List<string> pListDeviceModule, int pCompanyIndex);
        Task UpdateDataDevice(DeviceParamInfo param, int pCompanyIndex);
        Task UpdateTransactionCount(DeviceParamInfo param, int pCompanyIndex);
    }
}
