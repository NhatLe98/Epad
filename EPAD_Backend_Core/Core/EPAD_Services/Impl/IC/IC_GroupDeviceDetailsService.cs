using EPAD_Common.Services;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using EPAD_Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EPAD_Services.Impl
{
    public class IC_GroupDeviceDetailsService : BaseServices<IC_GroupDeviceDetails, EPAD_Context>, IIC_GroupDeviceDetailsService
    {
        public IC_GroupDeviceDetailsService(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public async Task<bool> InsertOrUpdateGroupDevicesDetail(List<string> pSerialNumberList, int pGroupDeviceIndex, UserInfo user)
        {
            try
            {
                var listGroupDevicesDetail = Where(x => x.CompanyIndex == user.CompanyIndex && pSerialNumberList.Contains(x.SerialNumber)).ToList();
                foreach (var serialNumber in pSerialNumberList)
                {
                    var groupDeviceDetail = listGroupDevicesDetail.FirstOrDefault(x => x.SerialNumber == serialNumber);
                    if (groupDeviceDetail != null)
                    {
                        if (groupDeviceDetail.GroupDeviceIndex != pGroupDeviceIndex)
                        {
                            DbContext.IC_GroupDeviceDetails.Remove(groupDeviceDetail);
                            groupDeviceDetail = new IC_GroupDeviceDetails();
                            groupDeviceDetail.GroupDeviceIndex = pGroupDeviceIndex;
                            groupDeviceDetail.SerialNumber = serialNumber;
                            groupDeviceDetail.UpdatedUser = user.UserName;
                            groupDeviceDetail.UpdatedDate = DateTime.Now;
                            groupDeviceDetail.CompanyIndex = user.CompanyIndex;
                            await DbContext.IC_GroupDeviceDetails.AddAsync(groupDeviceDetail);
                        }
                    }
                    else
                    {
                        groupDeviceDetail = new IC_GroupDeviceDetails();
                        groupDeviceDetail.GroupDeviceIndex = pGroupDeviceIndex;
                        groupDeviceDetail.SerialNumber = serialNumber;
                        groupDeviceDetail.UpdatedUser = user.UserName;
                        groupDeviceDetail.UpdatedDate = DateTime.Now;
                        groupDeviceDetail.CompanyIndex = user.CompanyIndex;
                        await DbContext.IC_GroupDeviceDetails.AddAsync(groupDeviceDetail);
                    }
                }

                await DbContext.SaveChangesAsync();
                return await Task.FromResult(true);
            }
            catch
            {
                return await Task.FromResult(false);
            }
        }
    }
}
