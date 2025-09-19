using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using EPAD_Logic.MainProcess;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EPAD_Logic
{
    public class IC_CachingLogic : IIC_CachingLogic
    {
        public EPAD_Context _dbContext;
        private ezHR_Context _integrateContext;
        private static IMemoryCache _cache;
        private ConfigObject _config;

        public IC_CachingLogic(IMemoryCache cache, EPAD_Context dbContext, ezHR_Context integrateContext)
        {
            _cache = cache;
            _dbContext = dbContext;
            _integrateContext = integrateContext;

        }

        public void ResetCommandCacheForService()
        {
            List<IC_Company> listCompany = _dbContext.IC_Company.ToList();
            foreach (IC_Company company in listCompany)
            {
                CompanyInfo companyInCache = CompanyInfo.GetFromCache(_cache, company.Index.ToString());
                foreach (UserKey serviceKeyInCache in companyInCache.ListCacheUserInfo.Where(s => s.UserName.Contains("Service_")))
                {
                    UserInfo serviceInCache = UserInfo.GetFromCache(_cache, serviceKeyInCache.GuidID);
                    serviceInCache.ListCommands = new List<CommandResult>();
                    CommandProcess.GetCommandsFromGeneralCacheForService(companyInCache, serviceInCache);

                }
            }
        }

        private CommandGroup CreateGroupAndListCommands(IC_CommandSystemGroup pGroup, List<IC_SystemCommand> pListCommand)
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

        private CommandResult CreateCommandResultFromCommandDB(IC_SystemCommand pCmdDB)
        {

            CommandParamDB param = JsonConvert.DeserializeObject<CommandParamDB>(pCmdDB.Params);
            CommandResult cmdCache = new CommandResult(pCmdDB.Command, CommandStatus.UnExecute);

            cmdCache.ID = pCmdDB.Index.ToString();
            cmdCache.SerialNumber = pCmdDB.SerialNumber;
            cmdCache.FromTime = param.FromTime;
            cmdCache.ToTime = param.ToTime;

            cmdCache.ListUsers = param.ListUsers;
            cmdCache.AccGroups = param.AccGroups;
            cmdCache.AccHolidays = param.AccHolidays;
            cmdCache.TimeZones= param.TimeZones;
            cmdCache.IPAddress = param.IPAddress;
            cmdCache.Port = param.Port;
            cmdCache.Port = param.Port;
            cmdCache.CreatedTime = pCmdDB.RequestedTime.Value;

            cmdCache.ExcutingServiceIndex = pCmdDB.ExcutingServiceIndex;
            cmdCache.GroupIndex = pCmdDB.GroupIndex.ToString();
            cmdCache.ExternalData = pCmdDB.EmployeeATIDs;
            cmdCache.IsOverwriteData = pCmdDB.IsOverwriteData;

            return cmdCache;
        }

        private void UpdateCommandsForGroup(CommandGroup groupCache, List<IC_SystemCommand> pListCommand)
        {
            List<IC_SystemCommand> listCommandInGroup = pListCommand.Where(t => t.GroupIndex == int.Parse(groupCache.ID)).ToList();
            groupCache.ListCommand = new List<CommandResult>();// remove all command
            //List<CommandResult> listCommandCache = groupCache.ListCommand;
            for (int i = 0; i < listCommandInGroup.Count; i++) // re add command into group
            {
                if (groupCache.ListCommand.Where(t => t.ID == listCommandInGroup[i].ToString()).FirstOrDefault() == null)
                {
                    groupCache.ListCommand.Add(CreateCommandResultFromCommandDB(listCommandInGroup[i]));
                }
            }
        }

        public void SyncSystemCommandCacheAndDatabase()
        {
            List<IC_CommandSystemGroup> listGroupUnexcuted = _dbContext.IC_CommandSystemGroup.Where(t => t.Excuted == false).ToList();
            List<IC_SystemCommand> listCommandUnexcuted = _dbContext.IC_SystemCommand.Where(t => t.Excuted == false && t.IsActive).ToList();
            List<IC_Company> listCompany = _dbContext.IC_Company.ToList();
            for (int i = 0; i < listCompany.Count; i++)
            {
                List<IC_CommandSystemGroup> listGroupInCompany = listGroupUnexcuted.Where(t => t.CompanyIndex == listCompany[i].Index).ToList();
                CompanyInfo companyInfo = CompanyInfo.GetFromCache(_cache, listCompany[i].Index.ToString());
                if (companyInfo == null)
                {
                    companyInfo = new CompanyInfo();
                    companyInfo.AddToCache(_cache, listCompany[i].Index.ToString());
                    // tạo group cache từ group data trong DB
                    for (int j = 0; j < listGroupInCompany.Count; j++)
                    {
                        IC_CommandSystemGroup groupOnDB = listGroupInCompany[j];
                        CommandGroup groupCache = new CommandGroup
                        {
                            CreatedTime = groupOnDB.CreatedDate ?? DateTime.Now,
                            ListCommand = listCommandUnexcuted
                                .Where(cmd => cmd.GroupIndex == groupOnDB.Index)
                                .Select(cmd => CreateCommandResultFromCommandDB(cmd))
                                .ToList(),
                            Excuted = false,
                            EventType = groupOnDB.EventType,
                            ID = groupOnDB.Index.ToString(),
                            Name = groupOnDB.GroupName,
                        };
                        companyInfo.ListCommandGroups.Add(groupCache);
                    }
                }
                else
                {
                    // remove group on cache if group not existed on database
                    var xxx = companyInfo.ListCommandGroups.RemoveAll(groupOnCache => listGroupInCompany.Find(groupOnDb => groupOnDb.Index.ToString() == groupOnCache.ID) == null);

                    for (int j = 0; j < listGroupInCompany.Count; j++)
                    {
                        IC_CommandSystemGroup groupOnDB = listGroupInCompany[j];
                        CommandGroup groupCache = companyInfo.ListCommandGroups.FirstOrDefault(g => g.ID == groupOnDB.Index.ToString());

                        // add group if group existed on database but not existed on cache
                        if (groupCache == null)
                        {
                            groupCache = new CommandGroup
                            {
                                CreatedTime = groupOnDB.CreatedDate ?? DateTime.Now,
                                ListCommand = listCommandUnexcuted
                                   .Where(cmd => cmd.GroupIndex == groupOnDB.Index)
                                   .Select(cmd => CreateCommandResultFromCommandDB(cmd))
                                   .ToList(),
                                Excuted = false,
                                EventType = groupOnDB.EventType,
                                ID = groupOnDB.Index.ToString(),
                                Name = groupOnDB.GroupName,
                            };
                            companyInfo.ListCommandGroups.Add(groupCache);
                        }
                        else
                        {
                            List<IC_SystemCommand> commandsOfGroupOnDB = listCommandUnexcuted
                                .Where(cmd => cmd.GroupIndex == groupOnDB.Index).ToList();

                            // remove all command existed on cache but not existed on database
                            groupCache.ListCommand.RemoveAll(cmdOnCache => commandsOfGroupOnDB
                                .Any(cmdOnDb => cmdOnDb.Index.ToString() == cmdOnCache.ID) == false);

                            // add command to cache if command not existed on cache but existed on database
                            commandsOfGroupOnDB.ForEach(cmdOnDB =>
                            {
                                if (groupCache.ListCommand.Any(cmdOnCache => cmdOnCache.ID == cmdOnDB.Index.ToString()) == false)
                                {
                                    groupCache.ListCommand.Add(CreateCommandResultFromCommandDB(cmdOnDB));
                                }
                            });
                        }
                    }
                }
            }
        }

    }

    public interface IIC_CachingLogic
    {
        /// <summary>
        ///     Put command from company in cache to service in cache.
        /// </summary>
        void ResetCommandCacheForService();
        /// <summary>
        ///     Sync system command / group command system between cache and database.
        /// </summary>
        void SyncSystemCommandCacheAndDatabase();
    }
}
