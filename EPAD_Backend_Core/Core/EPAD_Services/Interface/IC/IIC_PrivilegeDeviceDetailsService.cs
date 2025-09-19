using EPAD_Common.Services;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models.Other;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EPAD_Services.Interface
{
    public interface IIC_PrivilegeDeviceDetailsService : IBaseServices<IC_PrivilegeDeviceDetails, EPAD_Context>
    {
        public Task<List<PrivilegeDeviceDetails>> GetPrivilegeDeviceDetail(int pCompanyIndex);
    }
}
