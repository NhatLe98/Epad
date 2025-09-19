using EPAD_Common.Services;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models.Other;
using EPAD_Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EPAD_Services.Impl
{
    public class IC_PrivilegeDeviceDetailsService : BaseServices<IC_PrivilegeDeviceDetails, EPAD_Context>, IIC_PrivilegeDeviceDetailsService
    {
        public IC_PrivilegeDeviceDetailsService(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public async Task<List<PrivilegeDeviceDetails>> GetPrivilegeDeviceDetail(int pCompanyIndex)
        {
            var dummy = from dev in DbContext.IC_Device.Where(x => x.CompanyIndex == pCompanyIndex)
                        join pri in DbContext.IC_PrivilegeDeviceDetails.Where(x => x.CompanyIndex == pCompanyIndex)
                        on dev.SerialNumber equals pri.SerialNumber
                        into priCheck
                        from p in priCheck.DefaultIfEmpty()
                        select new PrivilegeDeviceDetails
                        {
                            PrivilegeIndex = p == null ? 0 : p.PrivilegeIndex,
                            SerialNumber = p == null ? dev.SerialNumber : p.SerialNumber,
                            Role = p.Role
                        };
            var rs = dummy.ToList();
            return await Task.FromResult(rs);
        }
    }
}
