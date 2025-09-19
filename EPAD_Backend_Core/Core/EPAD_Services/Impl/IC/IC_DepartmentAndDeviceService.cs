using EPAD_Common.Services;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EPAD_Services.Impl
{
    public class IC_DepartmentAndDeviceService : BaseServices<IC_DepartmentAndDevice, EPAD_Context>, IIC_DepartmentAndDeviceService
    {
        public IC_DepartmentAndDeviceService(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public async Task<List<IC_DepartmentAndDevice>> GetAllBySerialNumber(string pSerialNumber, int pCompanyIndex)
        {
            var dummy = Where(x => x.CompanyIndex == pCompanyIndex && x.SerialNumber == pSerialNumber).ToList();
            return await Task.FromResult(dummy);
        }

        public async Task<List<IC_DepartmentAndDevice>> GetAllBySerialNumbers(string[] pSerialNumber, int pCompanyIndex)
        {
            var serialLookup = pSerialNumber.ToHashSet();
            var dummy = Where(x => x.CompanyIndex == pCompanyIndex && serialLookup.Contains(x.SerialNumber)).ToList();
            return await Task.FromResult(dummy);
        }
    }
}
