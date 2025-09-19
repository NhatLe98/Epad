using EPAD_Common.Services;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Services.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Collections.Generic;
using EPAD_Data.Models.IC;
using EPAD_Common.Enums;
using EPAD_Data.Models;
using System.Threading.Tasks;
using EPAD_Common.Types;

namespace EPAD_Services.Impl
{
    public class IC_PrivilegeMachineRealtimeService : BaseServices<IC_PrivilegeMachineRealtime, EPAD_Context>, IIC_PrivilegeMachineRealtimeService
    {
        public EPAD_Context _dbContext;
        private readonly ILogger _logger;
        private static IMemoryCache _cache;
        public IC_PrivilegeMachineRealtimeService(IServiceProvider serviceProvider, EPAD_Context context, IMemoryCache cache, 
            ILogger<IC_PrivilegeMachineRealtimeService> logger) : base(serviceProvider)
        {
            _dbContext = context;
            _cache = cache;
            _logger = logger;
        }

        public async Task<DataGridClass> GetPrivilegeMachineRealtime(string filter,
            int page, int pageSize, int companyIndex)
        {
            var result = new List<IC_PrivilegeMachineRealtimeDTO>();
            var listPrivilegeMachineRealtime = new List<IC_PrivilegeMachineRealtime>();
            listPrivilegeMachineRealtime = _dbContext.IC_PrivilegeMachineRealtime.Where(x => x.CompanyIndex == companyIndex)
                .ToList();
            if (listPrivilegeMachineRealtime != null && listPrivilegeMachineRealtime.Count > 0)
            {
                var listGroupDeviceIndex = new List<int>();
                var listDeviceSerialIndex = new List<string>();

                foreach (var privilegeMachineRealtime in listPrivilegeMachineRealtime)
                {
                    if (!string.IsNullOrWhiteSpace(privilegeMachineRealtime.GroupDeviceIndex))
                    {
                        if (privilegeMachineRealtime.GroupDeviceIndex.Contains(":/:"))
                        {
                            var splitGroupDeviceIndex = privilegeMachineRealtime.GroupDeviceIndex.Split(":/:").Select(int.Parse).ToList();
                            listGroupDeviceIndex.AddRange(splitGroupDeviceIndex);
                        }
                        else
                        {
                            listGroupDeviceIndex.Add(int.Parse(privilegeMachineRealtime.GroupDeviceIndex));
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(privilegeMachineRealtime.DeviceSerial))
                    {
                        if (privilegeMachineRealtime.DeviceSerial.Contains(":/:"))
                        {
                            var splitDeviceSerialIndex = privilegeMachineRealtime.DeviceSerial.Split(":/:").ToList();
                            listDeviceSerialIndex.AddRange(splitDeviceSerialIndex);
                        }
                        else
                        {
                            listDeviceSerialIndex.Add(privilegeMachineRealtime.DeviceSerial);
                        }
                    }                        
                }

                listGroupDeviceIndex = listGroupDeviceIndex.Distinct().ToList();
                var listGroupDevice = _dbContext.IC_GroupDevice.Where(x => listGroupDeviceIndex.Contains(x.Index)).ToList();

                listDeviceSerialIndex = listDeviceSerialIndex.Distinct().ToList();
                var listDevice = _dbContext.IC_Device.Where(x => listDeviceSerialIndex.Contains(x.SerialNumber)).ToList();


                List<int> listPrivilegeGroupDeviceIndex;
                List<string> listPrivilegeDeviceSerialIndex;
                List<string> listPrivilegeDeviceModule;
                IC_PrivilegeMachineRealtimeDTO privilegeMachineRealtimeDTO;
                foreach (var privilegeMachineRealtime in listPrivilegeMachineRealtime)
                {
                    listPrivilegeGroupDeviceIndex = new List<int>();
                    listPrivilegeDeviceSerialIndex = new List<string>();
                    listPrivilegeDeviceModule = new List<string>();
                    privilegeMachineRealtimeDTO = new IC_PrivilegeMachineRealtimeDTO();

                    if(!string.IsNullOrWhiteSpace(privilegeMachineRealtime.GroupDeviceIndex))
                    {
                        if (privilegeMachineRealtime.GroupDeviceIndex.Contains(":/:"))
                        {
                            listPrivilegeGroupDeviceIndex = privilegeMachineRealtime.GroupDeviceIndex.Split(":/:").Select(int.Parse).ToList();
                        }
                        else
                        {
                            listPrivilegeGroupDeviceIndex.Add(int.Parse(privilegeMachineRealtime.GroupDeviceIndex));
                        }
                    }
                    var listPrivilegeGroupDevice = listGroupDevice.Where(x => listPrivilegeGroupDeviceIndex.Contains(x.Index)).ToList();
                    if (!string.IsNullOrWhiteSpace(privilegeMachineRealtime.DeviceSerial))
                    {
                        if (privilegeMachineRealtime.DeviceSerial.Contains(":/:"))
                        {
                            listPrivilegeDeviceSerialIndex = privilegeMachineRealtime.DeviceSerial.Split(":/:").ToList();
                        }
                        else
                        {
                            listPrivilegeDeviceSerialIndex.Add(privilegeMachineRealtime.DeviceSerial);
                        }
                    }
                    var listPrivilegeDevice = listDevice.Where(x => listPrivilegeDeviceSerialIndex.Contains(x.SerialNumber)).ToList();
                    if (!string.IsNullOrWhiteSpace(privilegeMachineRealtime.DeviceModule))
                    {
                        if (privilegeMachineRealtime.DeviceModule.Contains(":/:"))
                        {
                            listPrivilegeDeviceModule = privilegeMachineRealtime.DeviceModule.Split(":/:").ToList();
                        }
                        else
                        {
                            listPrivilegeDeviceModule.Add(privilegeMachineRealtime.DeviceModule);
                        }
                    }                        

                    privilegeMachineRealtimeDTO.UserName = privilegeMachineRealtime.UserName;
                    privilegeMachineRealtimeDTO.ListUserName = new List<string> { privilegeMachineRealtime.UserName };
                    privilegeMachineRealtimeDTO.PrivilegeGroup = privilegeMachineRealtime.PrivilegeGroup;
                    privilegeMachineRealtimeDTO.PrivilegeGroupName = ((PrivilegeGroup)privilegeMachineRealtime.PrivilegeGroup).ToString();
                    privilegeMachineRealtimeDTO.ListGroupDeviceIndex = listPrivilegeGroupDeviceIndex;
                    privilegeMachineRealtimeDTO.ListGroupDeviceName = listPrivilegeGroupDevice.Where(x
                        => listPrivilegeGroupDeviceIndex.Contains(x.Index)).Select(x => x.Name).ToList();
                    privilegeMachineRealtimeDTO.GroupDeviceIndex = string.Join(",", privilegeMachineRealtimeDTO.ListGroupDeviceIndex);
                    privilegeMachineRealtimeDTO.GroupDeviceName = string.Join(",", privilegeMachineRealtimeDTO.ListGroupDeviceName);
                    privilegeMachineRealtimeDTO.ListDeviceModule = listPrivilegeDeviceModule;
                    privilegeMachineRealtimeDTO.ListDeviceModuleName = listPrivilegeDeviceModule;
                    privilegeMachineRealtimeDTO.DeviceModule = string.Join(",", privilegeMachineRealtimeDTO.ListDeviceModule);
                    privilegeMachineRealtimeDTO.DeviceModuleName = string.Join(",", privilegeMachineRealtimeDTO.ListDeviceModuleName);
                    privilegeMachineRealtimeDTO.ListDeviceSerial = listPrivilegeDeviceSerialIndex;
                    privilegeMachineRealtimeDTO.ListDeviceName = listDevice.Where(x
                        => listPrivilegeDeviceSerialIndex.Contains(x.SerialNumber)).Select(x => x.AliasName).ToList();
                    privilegeMachineRealtimeDTO.DeviceSerial = string.Join(",", privilegeMachineRealtimeDTO.ListDeviceSerial);
                    privilegeMachineRealtimeDTO.DeviceName = string.Join(", ", privilegeMachineRealtimeDTO.ListDeviceName);

                    result.Add(privilegeMachineRealtimeDTO);
                }
            }

            if (!string.IsNullOrWhiteSpace(filter))
            {
                result = result.Where(x => x.UserName.Contains(filter) || x.DeviceSerial.Contains(filter) 
                    || x.DeviceName.Contains(filter)).ToList();
            }

            if (page <= 1) page = 1;
            int fromRow = pageSize * (page - 1);
            int record = result.Count();
            var lsDevice = result.OrderBy(t => t.UserName).Skip(fromRow).Take(pageSize).ToList();
            DataGridClass dataGrid = new DataGridClass(record, lsDevice);
            return await Task.FromResult(dataGrid);
        }

        public List<string> GetPrivilegeMachineRealtimeByUserName(string userName, int companyIndex)
        {
            var result = _dbContext.IC_PrivilegeMachineRealtime.FirstOrDefault(x => x.UserName == userName && x.CompanyIndex == companyIndex)
                ?.DeviceSerial.Split(":/:").ToList();
            return result;
        }

        public async Task<List<string>> AddPrivilegeMachineRealtime(IC_PrivilegeMachineRealtimeDTO param, UserInfo user)
        {
            var now = DateTime.Now;
            var result = new List<string>();

            var listUserName = param.ListUserName;
            var listExistedUserName = _dbContext.IC_PrivilegeMachineRealtime.Where(x 
                => listUserName.Contains(x.UserName)).Select(x => x.UserName).ToList();
            if (listExistedUserName != null && listExistedUserName.Count > 0)
            {
                result.Add("MSG_UserNameHasBeenDeclarePrivilege:/:" + string.Join(",", listExistedUserName));
                return result;
            }
            foreach (var userName in listUserName)
            {
                var newPrivilege = new IC_PrivilegeMachineRealtime();
                newPrivilege.UserName = userName;
                newPrivilege.PrivilegeGroup = param.PrivilegeGroup;
                newPrivilege.GroupDeviceIndex = string.Join(":/:", param.ListGroupDeviceIndex);
                newPrivilege.DeviceModule = string.Join(":/:", param.ListDeviceModule);
                newPrivilege.DeviceSerial = string.Join(":/:", param.ListDeviceSerial);
                newPrivilege.CompanyIndex = user.CompanyIndex;
                newPrivilege.CreatedDate = now;
                newPrivilege.UpdatedDate = now;
                newPrivilege.UpdatedUser = user.UserName;
                await _dbContext.IC_PrivilegeMachineRealtime.AddAsync(newPrivilege);
            }
            await _dbContext.SaveChangesAsync();

            return result;
        }

        public async Task<List<string>> UpdatePrivilegeMachineRealtime(IC_PrivilegeMachineRealtimeDTO param, UserInfo user)
        {
            var now = DateTime.Now;
            var result = new List<string>();

            var listUserName = param.ListUserName;
            var existedUserNamePrivilege = _dbContext.IC_PrivilegeMachineRealtime.FirstOrDefault(x
                => listUserName[0] == x.UserName);
            if (existedUserNamePrivilege == null)
            {
                result.Add("MSG_UserNameNotDeclarePrivilege:/:" + string.Join(",", listUserName));
                return result;
            }

            existedUserNamePrivilege.PrivilegeGroup = param.PrivilegeGroup;
            existedUserNamePrivilege.GroupDeviceIndex = string.Join(":/:", param.ListGroupDeviceIndex);
            existedUserNamePrivilege.DeviceModule = string.Join(":/:", param.ListDeviceModule);
            existedUserNamePrivilege.DeviceSerial = string.Join(":/:", param.ListDeviceSerial);
            existedUserNamePrivilege.CompanyIndex = user.CompanyIndex;
            existedUserNamePrivilege.CreatedDate = now;
            existedUserNamePrivilege.UpdatedDate = now;
            existedUserNamePrivilege.UpdatedUser = user.UserName;

            _dbContext.IC_PrivilegeMachineRealtime.Update(existedUserNamePrivilege);
            await _dbContext.SaveChangesAsync();

            return result;
        }

        public async Task<List<string>> DeletePrivilegeMachineRealtime(List<IC_PrivilegeMachineRealtimeDTO> param, UserInfo user)
        {
            var now = DateTime.Now;
            var result = new List<string>();

            var listUserName = param.Select(x => x.ListUserName[0]).ToList();
            var existedUserNamePrivilege = _dbContext.IC_PrivilegeMachineRealtime.Where(x
                => listUserName.Contains(x.UserName)).ToList();
            if (existedUserNamePrivilege == null || (existedUserNamePrivilege != null && existedUserNamePrivilege.Count == 0))
            {
                result.Add("MSG_UserNameNotDeclarePrivilege:/:" + string.Join(",", listUserName));
                return result;
            }

            _dbContext.IC_PrivilegeMachineRealtime.RemoveRange(existedUserNamePrivilege);
            await _dbContext.SaveChangesAsync();

            return result;
        }
    }
}
