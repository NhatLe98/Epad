using EPAD_Data.Entities;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EPAD_Data.Models
{
    public class UserKey
    {
        public string UserName { get; set; }
        public string GuidID { get; set; }
    }
    public class CompanyInfo
    {
        public int CompanyIndex { get; set; }
        private List<IC_Config> ListConfig { get; set; }
        private List<IC_ConfigByGroupMachine> ListConfigByGroupMachine { get; set; }

        public List<CommandGroup> ListCommandGroups { get; set; }
        public Dictionary<string, string> DicCacheUserInfo { get; set; }

        public List<UserKey> ListCacheUserInfo { get; set; }

        public CompanyInfo()
        {
            ListCommandGroups = new List<CommandGroup>();
            DicCacheUserInfo = new Dictionary<string, string>();
            ListCacheUserInfo = new List<UserKey>();
        }
        public CommandGroup GetGroupById(int pID)
        {
            CommandGroup group = null;
            for (int i = 0; i < ListCommandGroups.Count; i++)
            {
                if (ListCommandGroups[i].ID == pID.ToString())
                {
                    group = ListCommandGroups[i];
                    break;
                }
            }
            return group;
        }
        public bool DeleteGroupById(string pID)
        {
            bool success = false;
            for (int i = 0; i < ListCommandGroups.Count; i++)
            {
                if (ListCommandGroups[i].ID == pID)
                {
                    ListCommandGroups.RemoveAt(i);
                    success = true;
                    break;
                }
            }
            return success;
        }

        public void UpdateFinishForCommands(List<string> listId, string groupIndex, string status, DateTime now)
        {
            for (int i = 0; i < ListCommandGroups.Count; i++)
            {
                if (ListCommandGroups[i].ID == groupIndex)
                {
                    List<CommandResult> listCommand = ListCommandGroups[i].ListCommand.Where(t => listId.Contains(t.ID)).ToList();
                    foreach (CommandResult item in listCommand)
                    {
                        item.Status = status;
                        item.ExecutedTime = now;
                    }
                    break;

                }
            }
        }
        public void UpdateCommandById(string commandId, string groupIndex, string status, string pError, DateTime now)
        {
            for (int i = 0; i < ListCommandGroups.Count; i++)
            {
                if (ListCommandGroups[i].ID == groupIndex)
                {
                    CommandResult command = ListCommandGroups[i].ListCommand.Where(t => t.ID == commandId).FirstOrDefault();
                    if (command != null)
                    {
                        if (status == CommandStatus.Success.ToString())
                        {
                            command.Status = CommandStatus.Success.ToString();
                        }
                        else if (status == CommandStatus.DeleteManual.ToString())
                        {
                            command.Status = CommandStatus.DeleteManual.ToString();
                        }
                        else
                        {
                            command.ErrorCounter++;
                            command.AppliedTime = now.AddMinutes(5);
                            if (command.ErrorCounter >= 3)
                            {
                                command.Error = pError;
                                command.Status = CommandStatus.Failed.ToString();
                            }
                        }
                    }
                    break;
                }
            }
        }
        public void DeleteCommandById(string commandId, string groupIndex)
        {
            for (int i = 0; i < ListCommandGroups.Count; i++)
            {
                if (ListCommandGroups[i].ID == groupIndex)
                {
                    CommandResult command = ListCommandGroups[i].ListCommand.Where(t => t.ID == commandId).FirstOrDefault();
                    if (command != null)
                    {
                        ListCommandGroups[i].ListCommand.Remove(command);
                    }
                    break;

                }
            }
        }

        public List<IC_Config> GetListConfig()
        {
            return ListConfig;
        }
        public void SetConfig(List<IC_Config> pListConfig)
        {
            this.ListConfig = pListConfig;
        }

        public void SetConfigGroupDevice(List<IC_ConfigByGroupMachine> pListConfig)
        {
            this.ListConfigByGroupMachine = pListConfig;
        }

        public List<UserInfo> GetListUserInfoIsService(IMemoryCache cache)
        {
            List<UserInfo> listUser = new List<UserInfo>();
            if (this == null)
            {
                return listUser;
            }

            for (int i = 0; i < this.ListCacheUserInfo.Count; i++)
            {
                var key = this.ListCacheUserInfo[i];
                if (key != null)
                {
                    UserInfo user = UserInfo.GetFromCache(cache, key.GuidID);
                    if (user.ServiceName != null && user.ServiceName != "")
                    {
                        listUser.Add(user);
                    }
                }
            }
            return listUser;
        }

        public List<UserInfo> GetUserInfos(IMemoryCache cache)
        {
            List<UserInfo> listUser = new List<UserInfo>();
            if (this == null)
            {
                return listUser;
            }

            for (int i = 0; i < this.ListCacheUserInfo.Count; i++)
            {
                var key = this.ListCacheUserInfo[i];
                if (key != null)
                {
                    UserInfo user = UserInfo.GetFromCache(cache, key.GuidID);

                    listUser.Add(user);

                }
            }
            return listUser;
        }

        public void AddToCache(IMemoryCache cache, string pGuid)
        {
            cache.Set(pGuid, this);
        }
        /// <summary>
        /// Get user login object from cache with guid key
        /// </summary>
        /// <param name="pGuid"></param>
        /// <returns></returns>
        static public CompanyInfo GetFromCache(IMemoryCache cache, string pGuid)
        {
            CompanyInfo user = new CompanyInfo();
            if (cache.TryGetValue(pGuid, out user) == false)
            {
                return null;
            }
            return user;
        }
        /// <summary>
        /// Remove user login object from cache with guid key
        /// </summary>
        /// <param name="pGuid"></param>
        static public void RemoveFromCache(IMemoryCache cache, string pGuid)
        {
            cache.Remove(pGuid);
        }
    }
}