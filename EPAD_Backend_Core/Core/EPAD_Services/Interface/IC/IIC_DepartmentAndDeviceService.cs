using EPAD_Common.Services;
using EPAD_Data;
using EPAD_Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EPAD_Services.Interface
{
    public interface IIC_DepartmentAndDeviceService : IBaseServices<IC_DepartmentAndDevice, EPAD_Context>
    {
        public Task<List<IC_DepartmentAndDevice>> GetAllBySerialNumber(string pSerialNumber, int pCompanyIndex);
        public Task<List<IC_DepartmentAndDevice>> GetAllBySerialNumbers(string[] pSerialNumbers, int pCompanyIndex);
    }
}
