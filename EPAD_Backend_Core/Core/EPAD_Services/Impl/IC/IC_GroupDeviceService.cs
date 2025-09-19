
using EPAD_Common.Services;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using EPAD_Services.Interface;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EPAD_Services.Impl
{
    public class IC_GroupDeviceService : BaseServices<IC_GroupDevice, EPAD_Context>, IIC_GroupDeviceService
    {
        private readonly IMemoryCache mCache;
        private readonly ILogger _logger;

        public IC_GroupDeviceService(IServiceProvider serviceProvider, ILoggerFactory loggerFactory) : base(serviceProvider)
        {
            mCache = serviceProvider.GetService<IMemoryCache>();
            _logger = loggerFactory.CreateLogger<IC_DeviceService>();
        }

        public List<GroupDeviceParam> GetGroupDevice(int companyIndex)
        {
            var groupDeviceDetails = DbContext.IC_GroupDeviceDetails.AsEnumerable()
               .Where(t => t.CompanyIndex == companyIndex).ToList();
            var groupDevices = DbContext.IC_GroupDevice.AsEnumerable()
                .Where(t => t.CompanyIndex == companyIndex).Select(e => new GroupDeviceParam
                {
                    Index = e.Index,
                    Name = e.Name,
                    Description = e.Description,
                    ListMachine = groupDeviceDetails.Where(d => d.GroupDeviceIndex == e.Index).Select(d => d.SerialNumber).ToList()
                }).ToList();
            return groupDevices;
        }

        public object GetGroupDeviceResult(UserInfo user)
        {
            if (user.IsAdmin)
            {
                var data = from d in DbContext.IC_Device
                           join gdd in DbContext.IC_GroupDeviceDetails
                           on d.SerialNumber equals gdd.SerialNumber
                           join gd in DbContext.IC_GroupDevice
                           on gdd.GroupDeviceIndex equals gd.Index
                           select new
                           {
                               GroupDeviceIndex = gdd.GroupDeviceIndex,
                               GroupDeviceName = gd.Name,
                               GroupDeviceDescription = gd.Description,
                               SerialNumber = gdd.SerialNumber,
                               DeviceName = d.AliasName
                           };

                var result = data.ToList();
                return result;
            }
            else
            {
                var data = from d in DbContext.IC_Device
                           join pridv in DbContext.IC_PrivilegeDeviceDetails
                           on d.SerialNumber equals pridv.SerialNumber
                           join pri in DbContext.IC_UserPrivilege
                           on pridv.PrivilegeIndex equals pri.Index
                           join ac in DbContext.IC_UserAccount
                on pri.Index equals ac.AccountPrivilege
                           where (ac.UserName.Equals(user.UserName) && (pridv.Role.Equals(Privilege.None.ToString()) == false))
                           join gdd in DbContext.IC_GroupDeviceDetails
                           on d.SerialNumber equals gdd.SerialNumber
                           join gd in DbContext.IC_GroupDevice
                           on gdd.GroupDeviceIndex equals gd.Index
                           select new
                           {
                               GroupDeviceIndex = gdd.GroupDeviceIndex,
                               GroupDeviceName = gd.Name,
                               GroupDeviceDescription = gd.Description,
                               SerialNumber = gdd.SerialNumber,
                               DeviceName = d.AliasName
                           };

                var result = data.ToList();
                return result;
            }
            
        }
    }
}
