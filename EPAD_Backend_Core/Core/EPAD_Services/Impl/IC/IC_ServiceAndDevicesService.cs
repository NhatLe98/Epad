using EPAD_Common.Services;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EPAD_Data.Models;

namespace EPAD_Services.Impl
{
    public class IC_ServiceAndDevicesService : BaseServices<IC_ServiceAndDevices, EPAD_Context>, IIC_ServiceAndDevicesService
    {
        public IC_ServiceAndDevicesService(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public async Task<List<IC_ServiceAndDevices>> GetAllBySerialNumber(string pSerialNumber, int pCompanyIndex)
        {
            var dummy = Where(x => x.CompanyIndex == pCompanyIndex && x.SerialNumber == pSerialNumber).ToList();
            return await Task.FromResult(dummy);
        }

        public async Task<List<IC_ServiceAndDevices>> GetAllBySerialNumbers(string[] pSerialNumber, int pCompanyIndex)
        {
            var serialLookup = pSerialNumber.ToHashSet();
            var dummy = Where(x => x.CompanyIndex == pCompanyIndex && serialLookup.Contains( x.SerialNumber)).ToList();
            return await Task.FromResult(dummy);
        }

        public async Task<bool> InsertOrUpdateServiceAndDevices(List<string> pSerialNumberList, int pServiceIndex, UserInfo user)
        {
            try
            {
                var listServiceAndDevices = Where(x => x.CompanyIndex == user.CompanyIndex && pSerialNumberList.Contains(x.SerialNumber)).ToList();
                foreach (var serialNumber in pSerialNumberList)
                {
                    var serviceAndDevice = listServiceAndDevices.FirstOrDefault(x => x.SerialNumber == serialNumber);
                    if (serviceAndDevice != null)
                    {
                        if (serviceAndDevice.ServiceIndex != pServiceIndex)
                        {
                            DbContext.IC_ServiceAndDevices.Remove(serviceAndDevice);
                            serviceAndDevice = new IC_ServiceAndDevices();
                            serviceAndDevice.ServiceIndex = pServiceIndex;
                            serviceAndDevice.SerialNumber = serialNumber;
                            serviceAndDevice.UpdatedUser = user.UserName;
                            serviceAndDevice.UpdatedDate = DateTime.Now;
                            serviceAndDevice.CompanyIndex = user.CompanyIndex;
                            await DbContext.IC_ServiceAndDevices.AddAsync(serviceAndDevice);
                        }
                    }
                    else
                    {
                        serviceAndDevice = new IC_ServiceAndDevices();
                        serviceAndDevice.ServiceIndex = pServiceIndex;
                        serviceAndDevice.SerialNumber = serialNumber;
                        serviceAndDevice.UpdatedUser = user.UserName;
                        serviceAndDevice.UpdatedDate = DateTime.Now;
                        serviceAndDevice.CompanyIndex = user.CompanyIndex;
                        await DbContext.IC_ServiceAndDevices.AddAsync(serviceAndDevice);
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
