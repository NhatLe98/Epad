using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;
using EPAD_Data.Models;
using EPAD_Data;
using EPAD_Common.Utility;
using EPAD_Data.Entities;
using CommandAction = EPAD_Data.CommandAction;
using CommandStatus = EPAD_Data.CommandStatus;
using EPAD_Common.Extensions;
using EPAD_Data.Entities;
using System.Reflection;

namespace EPAD_Logic.MainProcess
{
    public class CommandProcess
    {
        public static List<CommandResult> CreateListCommands(EPAD_Context context, List<string> listSerial, CommandAction pAction, string pExternalData,
            DateTime pFromTime, DateTime pToTime, List<UserInfoOnMachine> pListUsers, bool isOverwriteData, short privilege = 0)
        {
            List<CommandResult> listCommandService = new List<CommandResult>();
            DateTime now = DateTime.Now;

            for (int i = 0; i < listSerial.Count; i++)
            {
                CommandResult cmd = new CommandResult(pAction, CommandStatus.UnExecute);
                if (pAction == CommandAction.RESTART_SERVICE)
                {
                    cmd.SerialNumber = "Serial";
                    cmd.ExcutingServiceIndex = int.Parse(listSerial[i]);
                }
                else
                {
                    cmd.SerialNumber = listSerial[i];
                }
                // change privilege for push service only support 14 when using Push
                if (pListUsers != null && pListUsers.Count() > 0)
                {
                    var listServiceAndDevice = GetServiceType(context, listSerial);
                    if (listServiceAndDevice != null && listServiceAndDevice.Count() > 0)
                    {
                        var serviceType = listServiceAndDevice.FirstOrDefault(e => e.SerialNumber == listSerial[i]);
                        if (serviceType != null)
                        {
                            if (serviceType != null && serviceType.ServiceType == GlobalParams.ServiceType.PUSHInterfaceService && privilege == GlobalParams.DevicePrivilege.SDKAdminRole)
                            {
                                privilege = GlobalParams.DevicePrivilege.PUSHAdminRole; // 14 is admin role using for push service update to device
                                foreach (var em in pListUsers)
                                {
                                    em.Privilege = privilege;
                                }
                            }
                        }
                    }

                }

                cmd.FromTime = pFromTime;
                cmd.ToTime = pToTime;
                cmd.ListUsers = pListUsers;
                cmd.ExternalData = pExternalData;
                cmd.CreatedTime = now;
                cmd.Status = CommandStatus.UnExecute.ToString();
                cmd.IsOverwriteData = isOverwriteData;

                listCommandService.Add(cmd);
            }

            return listCommandService;
        }

        public static List<CommandResult> CreateListACCommands(EPAD_Context context, List<string> listSerial, CommandAction pAction, string pExternalData,
            DateTime pFromTime, DateTime pToTime, List<UserInfoOnMachine> pListUsers, List<AC_AccGroup> paccGroups, List<AC_AccHoliday> paccHolidays, List<AC_TimeZone> ptimeZones, bool isOverwriteData, IC_CommandRequestDTO commandRequest, short privilege = 0)
        {
            List<CommandResult> listCommandService = new List<CommandResult>();
            DateTime now = DateTime.Now;

            for (int i = 0; i < listSerial.Count; i++)
            {
                CommandResult cmd = new CommandResult(pAction, CommandStatus.UnExecute);
                if (pAction == CommandAction.RESTART_SERVICE)
                {
                    cmd.SerialNumber = "Serial";
                    cmd.ExcutingServiceIndex = int.Parse(listSerial[i]);
                }
                else
                {
                    cmd.SerialNumber = listSerial[i];
                }
                // change privilege for push service only support 14 when using Push
                if (pListUsers != null && pListUsers.Count() > 0)
                {
                    var listServiceAndDevice = GetServiceType(context, listSerial);
                    if (listServiceAndDevice != null && listServiceAndDevice.Count() > 0)
                    {
                        var serviceType = listServiceAndDevice.FirstOrDefault(e => e.SerialNumber == listSerial[i]);
                        if (serviceType != null)
                        {
                            if (serviceType != null && serviceType.ServiceType == GlobalParams.ServiceType.PUSHInterfaceService && privilege == GlobalParams.DevicePrivilege.SDKAdminRole)
                            {
                                privilege = GlobalParams.DevicePrivilege.PUSHAdminRole; // 14 is admin role using for push service update to device
                                foreach (var em in pListUsers)
                                {
                                    em.Privilege = privilege;
                                }
                            }
                        }
                    }

                }

                if (pAction == CommandAction.UploadTimeZone)
                {
                    var device = context.IC_Device.Where(x => x.SerialNumber == listSerial[i]).FirstOrDefault();
                    int deviceModel = 0;
                    if (string.IsNullOrEmpty(device.DeviceModel) && NumberExtensions.IsNumber(device.DeviceModel))
                    {
                        deviceModel = int.Parse(device.DeviceModel);
                    }
               
                    var listName = new List<string>() { "Name", "UID", "UpdatedDate", "CompanyIndex", "UIDIndex" };
                    if (ptimeZones != null && ptimeZones.Count() > 0)
                    {
                        foreach (var item in ptimeZones)
                        {
                            var uid = item.UIDIndex.Split(',').ToList();
                            for (var j = 1; j <= 3; j++)
                            {
                                foreach (var prop in item.GetType().GetProperties())
                                {
                                    if (!listName.Contains(prop.Name) && prop.Name.Contains(j.ToString()))
                                    {
                                        var value = prop.GetValue(item, null);

                                        if ((value == null || string.IsNullOrEmpty(value.ToString())) && deviceModel != (int)ProducerEnum.HIK)
                                        {
                                            if (prop.Name.Contains("Start"))
                                            {
                                                SetObjectProperty(prop.Name, "0000", item);
                                            }
                                            else if (prop.Name.Contains("End"))
                                            {
                                                SetObjectProperty(prop.Name, "0000", item);
                                            }


                                        }
                                    }

                                }
                            }
                        }
                    }
                }
                cmd.FromTime = pFromTime;
                cmd.ToTime = pToTime;
                cmd.ListUsers = pListUsers;
                cmd.AccGroups = paccGroups;
                cmd.AccHolidays = paccHolidays;
                cmd.TimeZones = ptimeZones;
                cmd.ExternalData = pExternalData;
                cmd.CreatedTime = now;
                cmd.AutoOffSecond = commandRequest.AutoOffSecond != null ?  commandRequest.AutoOffSecond.Value : 3;
                cmd.Status = CommandStatus.UnExecute.ToString();
                cmd.IsOverwriteData = isOverwriteData;
                

                listCommandService.Add(cmd);
            }

            return listCommandService;
        }
        private static void SetObjectProperty(string propertyName, string value, object obj)
        {
            PropertyInfo propertyInfo = obj.GetType().GetProperty(propertyName);
            // make sure object has the property we are after
            if (propertyInfo != null)
            {
                propertyInfo.SetValue(obj, value, null);
            }
        }

        private static List<IC_ServiceAndDeviceDTO> GetServiceType(EPAD_Context context, List<string> listSerialNumber)
        {
            var query = (from sad in context.IC_ServiceAndDevices
                         join s in context.IC_Service
                          on sad.ServiceIndex equals s.Index
                         join d in context.IC_Device
                         on sad.SerialNumber equals d.SerialNumber
                         where listSerialNumber.Contains(sad.SerialNumber)
                         select new IC_ServiceAndDeviceDTO
                         {
                             CompanyIndex = sad.CompanyIndex,
                             ServiceType = s.ServiceType,
                             SerialNumber = sad.SerialNumber,
                             ServiceIndex = sad.ServiceIndex,
                             AliasName = d.AliasName,
                             IPAddress = d.IPAddress

                         }).AsQueryable();
            return query.ToList();
        }

        public static void CreateGroupCommand(EPAD_Context context, IMemoryCache cache, int pCompanyIndex,
            string pUserName, string pGroupName, string pExternalData,
            List<CommandResult> pListCommands, string pEventType)
        {
            DateTime now = DateTime.Now;
            int groupIndex = 0;
            List<string> listSerial = new List<string>();
            for (int i = 0; i < pListCommands.Count; i++)
            {
                listSerial.Add(pListCommands[i].SerialNumber);
            }
            #region create group on database
            IC_CommandSystemGroup groupModel = new IC_CommandSystemGroup();
            groupModel.GroupName = pGroupName;
            groupModel.Excuted = false;

            groupModel.EventType = pEventType;
            groupModel.ExternalData = pExternalData;
            groupModel.CreatedDate = now;
            groupModel.UpdatedDate = now;
            groupModel.UpdatedUser = pUserName;
            groupModel.CompanyIndex = pCompanyIndex;

            context.IC_CommandSystemGroup.Add(groupModel);
            context.SaveChanges();
            groupIndex = groupModel.Index;
            #endregion
            #region create command on database

            List<IC_Device> listDevice = context.IC_Device.Where(t => t.CompanyIndex == pCompanyIndex && listSerial.Contains(t.SerialNumber)).ToList();
            List<IC_SystemCommand> listCommand = new List<IC_SystemCommand>();

            for (int i = 0; i < pListCommands.Count; i++)
            {
                IC_Device device = listDevice.Find(t => t.SerialNumber == pListCommands[i].SerialNumber);
              
                if (device != null && device.IPAddress != null && device.IPAddress != "")
                {
                    int deviceModel = 0;
                    if (string.IsNullOrEmpty(device.DeviceModel) && NumberExtensions.IsNumber(device.DeviceModel))
                    {
                        deviceModel = int.Parse(device.DeviceModel);
                    }
                    CommandParamDB param = new CommandParamDB();
                    param.IPAddress = device.IPAddress;
                    param.Port = device.Port == null ? 4370 : device.Port.Value;
                    param.FromTime = pListCommands[i].FromTime;
                    param.ToTime = pListCommands[i].ToTime;
                    param.ListUsers = pListCommands[i].ListUsers;
                    param.AccGroups = pListCommands[i].AccGroups;
                    param.AccHolidays = pListCommands[i].AccHolidays;
                    param.TimeZones = pListCommands[i].TimeZones;
                    param.AutoOffSecond = pListCommands[i].AutoOffSecond;
                    param.TimeZone = pListCommands[i].TimeZone;
                    param.ConnectionCode = device.ConnectionCode;

                    IC_SystemCommand command = CreateCommandModel(pListCommands[i].SerialNumber, pListCommands[i].Command,
                        param, groupIndex, 0, now, pUserName, pCompanyIndex, pListCommands[i].ExternalData, pListCommands[i].IsOverwriteData, deviceModel);
                    context.IC_SystemCommand.Add(command);
                    context.SaveChanges();
                    listCommand.Add(command);


                    pListCommands[i].IPAddress = device.IPAddress;
                    pListCommands[i].Port = device.Port == null ? 4370 : device.Port.Value;
                    pListCommands[i].GroupIndex = groupIndex.ToString();
                    pListCommands[i].ID = command.Index.ToString();

                }
                else
                {
                    // tạo command restart service
                    if (pListCommands[i].ExcutingServiceIndex != 0)
                    {
                        IC_SystemCommand command = CreateCommandModel(pListCommands[i].SerialNumber, pListCommands[i].Command,
                            new CommandParamDB(), groupIndex, pListCommands[i].ExcutingServiceIndex, now, pUserName, pCompanyIndex, pListCommands[i].ExternalData, pListCommands[i].IsOverwriteData, 0);
                        context.IC_SystemCommand.Add(command);
                        context.SaveChanges();
                        listCommand.Add(command);

                        pListCommands[i].IPAddress = "0";
                        pListCommands[i].Port = 100;
                        pListCommands[i].GroupIndex = groupIndex.ToString();
                        pListCommands[i].ID = command.Index.ToString();
                    }
                }
            }

            #endregion
            #region add command to cache
            CommandGroup groupCommand = new CommandGroup();
            groupCommand.ID = groupIndex.ToString();
            groupCommand.Name = pGroupName;
            groupCommand.Excuted = false;
            groupCommand.ListCommand = pListCommands;
            groupCommand.EventType = pEventType;

            CompanyInfo companyInfo = CompanyInfo.GetFromCache(cache, pCompanyIndex.ToString());
            // lấy ds service đang login
            List<UserInfo> listUser = companyInfo.GetListUserInfoIsService(cache);
            // cập nhật command cho các service ql
            pListCommands = RemoveCommandIsNullIPAddress(pListCommands);
            UpdateCommandForService(listUser, pListCommands);
            //cập nhật group command theo company
            AddGroupCommandsForCompany(companyInfo, cache, pCompanyIndex, groupCommand);
            #endregion
        }
        public static void UpdateCommandForService(List<UserInfo> pListUser, List<CommandResult> pListCommand)
        {
            for (int i = 0; i < pListCommand.Count; i++)
            {
                if (pListCommand[i].ExcutingServiceIndex == 0)
                {
                    // tìm service có quản lý device này
                    List<UserInfo> listUser = pListUser.Where(t => t.ListDevice.Find(s => s.SerialNumber == pListCommand[i].SerialNumber) != null).ToList();
                    foreach (UserInfo user in listUser)
                    {
                        if (user.CheckCommandExists(pListCommand[i].Command, pListCommand[i].SerialNumber, pListCommand[i].IPAddress, pListCommand[i].CreatedTime) == false)
                        {
                            user.ListCommands.Add(pListCommand[i]);
                            pListCommand[i].ExcutingServiceIndex = user.Index;
                        }
                    }
                }
                else
                {
                    // command restart service
                    UserInfo user = pListUser.Where(t => t.Index == pListCommand[i].ExcutingServiceIndex).FirstOrDefault();
                    if (user != null)
                    {
                        if (user.CheckCommandExists(pListCommand[i].Command, pListCommand[i].SerialNumber, pListCommand[i].IPAddress, pListCommand[i].CreatedTime) == false)
                        {
                            user.ListCommands.Add(pListCommand[i]);
                        }
                    }
                }
            }
        }

        public static void DeleteCommandForService(CompanyInfo companyInfo, IMemoryCache cache, List<CommandResult> pListCommand)
        {
            if (companyInfo != null)
            {
                List<UserInfo> serviceInCache = companyInfo.GetListUserInfoIsService(cache);
                for (int i = 0; i < pListCommand.Count; i++)
                {
                    // tìm service có quản lý device này
                    List<UserInfo> listUser = serviceInCache.Where(t => t.ListDevice.Find(s => s.SerialNumber == pListCommand[i].SerialNumber) != null).ToList();
                    foreach (UserInfo user in listUser)
                    {
                        if (user.CheckCommandExists(pListCommand[i].Command, pListCommand[i].SerialNumber, pListCommand[i].IPAddress, pListCommand[i].CreatedTime) == false)
                        {
                            user.ListCommands.Remove(pListCommand[i]);
                        }
                    }
                }
            }
        }

        public static void DeleteCommandForCompanyCache(CompanyInfo companyInfo, IMemoryCache cache, List<CommandResult> pListCommand)
        {
            if (companyInfo != null)
            {
                List<UserInfo> serviceInCache = companyInfo.GetListUserInfoIsService(cache);
                foreach (var deleteCommand in pListCommand)
                {

                    List<CommandGroup> groupsInCache = companyInfo.ListCommandGroups.Where(u => u.ID == deleteCommand.GroupIndex && u.ListCommand.Find(y => y.ID == deleteCommand.ID) != null).ToList();

                    var countCommandNotFinished = 0;
                    foreach (var group in groupsInCache)
                    {
                        group.ListCommand.Remove(deleteCommand);
                        // check all command not finished
                        countCommandNotFinished = group.ListCommand.Where(u => u.Status != CommandStatus.Success.ToString() || u.Status != CommandStatus.Failed.ToString()).Count();
                        if (countCommandNotFinished == 0)
                        {
                            companyInfo.ListCommandGroups.Remove(group);
                        }
                    }
                }
            }
        }
        private static List<CommandResult> RemoveCommandIsNullIPAddress(List<CommandResult> pListCommand)
        {
            List<CommandResult> listData = new List<CommandResult>();
            for (int i = 0; i < pListCommand.Count; i++)
            {
                if (pListCommand[i].IPAddress == "" || pListCommand[i].Port == 0)
                {
                    continue;
                }
                listData.Add(pListCommand[i]);
            }
            return listData;
        }
        public static void AddGroupCommandsForCompany(CompanyInfo companyInfo, IMemoryCache cache, int pCompany, CommandGroup pGroup)
        {
            if (companyInfo == null)
            {
                companyInfo = new CompanyInfo();
                companyInfo.AddToCache(cache, pCompany.ToString());
            }
            companyInfo.ListCommandGroups.Add(pGroup);
        }
        public static IC_SystemCommand CreateCommandModel(string pSerial, string pCommand, CommandParamDB pParam, int pGroupIndex, int pExcutingService,
            DateTime pNow, string pUserName, int pCompanyIndex, string pExternalData, bool isOverwriteData, int deviceModel)
        {
            IC_SystemCommand command = new IC_SystemCommand();
            command.SerialNumber = pSerial;
            command.Command = pCommand.ToString();
            command.CommandName = pCommand.ToString();

            var json = JsonConvert.SerializeObject(pParam);

            command.Params = json;
            command.EmployeeATIDs = pExternalData;
            command.RequestedTime = pNow;
            command.Excuted = false;

            command.CreatedDate = pNow;
            command.UpdatedDate = pNow;
            command.UpdatedUser = pUserName;
            command.CompanyIndex = pCompanyIndex;
            command.GroupIndex = pGroupIndex;
            command.ExcutingServiceIndex = pExcutingService;
            command.IsOverwriteData = isOverwriteData;
            if (deviceModel == (int)ProducerEnum.FR05)
            {
                command.Excuted = true;
                command.ExcutedTime = DateTime.Now;
            }

            return command;
        }
        public static void CreateGeneralCacheFromDB(IMemoryCache cache, EPAD_Context context)
        {
            List<IC_CommandSystemGroup> listGroupUnexcuted = context.IC_CommandSystemGroup.Where(t => t.Excuted == false).ToList();
            List<IC_SystemCommand> listCommandUnexcuted = context.IC_SystemCommand.Where(t => t.Excuted == false).ToList();
            List<IC_Company> listCompany = context.IC_Company.ToList();
            for (int i = 0; i < listCompany.Count; i++)
            {
                List<IC_CommandSystemGroup> listGroupInCompany = listGroupUnexcuted.Where(t => t.CompanyIndex == listCompany[i].Index).ToList();
                CompanyInfo companyInfo = CompanyInfo.GetFromCache(cache, listCompany[i].Index.ToString());
                if (companyInfo == null)
                {
                    companyInfo = new CompanyInfo();
                    companyInfo.AddToCache(cache, listCompany[i].Index.ToString());
                    // tạo group cache từ group data trong DB
                    for (int j = 0; j < listGroupInCompany.Count; j++)
                    {
                        CommandGroup groupCache = CreateGroupAndListCommands(listGroupInCompany[j], listCommandUnexcuted);
                        companyInfo.ListCommandGroups.Add(groupCache);
                    }
                }
                else
                {
                    for (int j = 0; j < listGroupInCompany.Count; j++)
                    {
                        CommandGroup groupCache = companyInfo.GetGroupById(listGroupInCompany[j].Index);
                        // nếu chưa có group này thì thêm
                        if (groupCache == null)
                        {
                            groupCache = CreateGroupAndListCommands(listGroupInCompany[j], listCommandUnexcuted);
                            companyInfo.ListCommandGroups.Add(groupCache);
                        }
                        else // có rồi thì update command
                        {
                            UpdateCommandsForGroup(groupCache, listCommandUnexcuted);
                        }
                    }
                }
            }
        }
        public static CommandGroup CreateGroupAndListCommands(IC_CommandSystemGroup pGroup, List<IC_SystemCommand> pListCommand)
        {
            CommandGroup groupCache = new CommandGroup();
            groupCache.ID = pGroup.Index.ToString();
            groupCache.Name = pGroup.GroupName;
            groupCache.Excuted = pGroup.Excuted;
            groupCache.EventType = pGroup.EventType;
            List<IC_SystemCommand> listCommandInGroup = pListCommand.Where(t => t.GroupIndex == pGroup.Index).ToList();
            List<CommandResult> listCommandCache = new List<CommandResult>();
            for (int i = 0; i < listCommandInGroup.Count; i++)
            {
                listCommandCache.Add(CreateCommandResultFromCommandDB(listCommandInGroup[i]));
            }

            groupCache.ListCommand = listCommandCache;

            return groupCache;
        }
        public static void UpdateCommandsForGroup(CommandGroup groupCache, List<IC_SystemCommand> pListCommand)
        {
            List<IC_SystemCommand> listCommandInGroup = pListCommand.Where(t => t.GroupIndex == int.Parse(groupCache.ID)).ToList();
            List<CommandResult> listCommandCache = groupCache.ListCommand;
            for (int i = 0; i < listCommandInGroup.Count; i++)
            {
                if (listCommandCache.Where(t => t.ID == listCommandInGroup[i].ToString()).FirstOrDefault() == null)
                {
                    listCommandCache.Add(CreateCommandResultFromCommandDB(listCommandInGroup[i]));
                }
            }
        }
        public static CommandResult CreateCommandResultFromCommandDB(IC_SystemCommand pCmdDB)
        {

            CommandParamDB param = JsonConvert.DeserializeObject<CommandParamDB>(pCmdDB.Params);
            CommandResult cmdCache = new CommandResult(pCmdDB.Command, CommandStatus.UnExecute);

            cmdCache.ID = pCmdDB.Index.ToString();
            cmdCache.SerialNumber = pCmdDB.SerialNumber;
            cmdCache.FromTime = param.FromTime;
            cmdCache.ToTime = param.ToTime;

            cmdCache.ListUsers = param.ListUsers;
            cmdCache.IPAddress = param.IPAddress;
            cmdCache.Port = param.Port;
            cmdCache.Port = param.Port;
            cmdCache.CreatedTime = pCmdDB.RequestedTime.Value;

            cmdCache.ExcutingServiceIndex = pCmdDB.ExcutingServiceIndex;
            cmdCache.GroupIndex = pCmdDB.GroupIndex.ToString();
            cmdCache.ExternalData = pCmdDB.EmployeeATIDs;
            cmdCache.IsOverwriteData = pCmdDB.IsOverwriteData;

            cmdCache.AccHolidays = param.AccHolidays;
            cmdCache.AccGroups = param.AccGroups;
            cmdCache.TimeZones = param.TimeZones;

            cmdCache.AutoOffSecond = param.AutoOffSecond;

            return cmdCache;
        }
        public static void GetCommandsFromGeneralCacheForService(CompanyInfo companyInfo, UserInfo service)
        {
            if (companyInfo != null)
            {
                var listGroup = companyInfo.ListCommandGroups.Where(g => g.ListCommand?.Count > 0);
                foreach (CommandGroup item in listGroup)
                {
                    List<CommandResult> listCommandCache = item.ListCommand;

                    for (int i = 0; i < listCommandCache.Count; i++)
                    {
                        IC_Device device = service.ListDevice.Find(t => t.SerialNumber == listCommandCache[i].SerialNumber);
                        // kiểm tra service login có ql thiết bị này ko và cmd này chưa dc add cho service khác
                        if (device != null && (listCommandCache[i].ExcutingServiceIndex == 0 || listCommandCache[i].ExcutingServiceIndex == service.Index))
                        {
                            listCommandCache[i].ExcutingServiceIndex = service.Index;
                            listCommandCache[i].Status = CommandStatus.UnExecute.ToString();
                            service.ListCommands.Add((CommandResult)listCommandCache[i].Clone());
                        }
                        else if (listCommandCache[i].Command == CommandAction.RESTART_SERVICE.ToString())
                        {
                            if (listCommandCache[i].ExcutingServiceIndex == service.Index)
                            {
                                listCommandCache[i].Status = CommandStatus.UnExecute.ToString();
                                service.ListCommands.Add((CommandResult)listCommandCache[i].Clone());
                            }
                        }
                    }
                }
            }

        }

    }
}