using EPAD_Common.Services;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EPAD_Services.Interface
{
    public interface IIC_ServiceAndDevicesService : IBaseServices<IC_ServiceAndDevices, EPAD_Context>
    {
        public Task<List<IC_ServiceAndDevices>> GetAllBySerialNumber(string pSerialNumber, int pCompanyIndex);
        public Task<List<IC_ServiceAndDevices>> GetAllBySerialNumbers(string[] pSerialNumbers, int pCompanyIndex);
        public Task<bool> InsertOrUpdateServiceAndDevices(List<string> pSerialNumberList, int pServiceIndex, UserInfo user);
    }
}
