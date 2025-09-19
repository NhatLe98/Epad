using EPAD_Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EPAD_Data.Models
{
    public class UserInfo
    {
        public string EmployeeATID { get; set; }
        public string FullName { get; set; }
        public string Department { get; set; }
        public string Language { get; set; }

        public string UserName { get; set; }
        public int CompanyIndex { get; set; }

        public int PrivilegeIndex { get; set; }
        public string PrivilegeName { get; set; }
        public bool IsAdmin { get; set; }
        public List<UserPrivilege> ListPrivilege { get; set; }
        public List<long> ListDepartmentAssigned { get; set; }

        public List<long> ListDepartmentAssignedAndParent { get; set; }

        // info for service
        public int Index { get; set; }
        public string ServiceName { get; set; }
        public List<IC_Device> ListDevice { get; set; }
        public List<CommandResult> ListCommands { get; set; }
        public DateTime LastCheckCommandDB { get; set; }
        public bool NeedCheckCommandDB
        {
            get
            {
                if (this.LastCheckCommandDB == null || this.LastCheckCommandDB.AddMinutes(1) < DateTime.Now)
                {
                    this.LastCheckCommandDB = DateTime.Now;
                    return true;
                }
                return false;
            }
        }

        public UserInfo(string pUserName)
        {
            UserName = pUserName;
            ListDevice = new List<IC_Device>();
            ListCommands = new List<CommandResult>();
        }

        /// <summary>
        /// Khởi tạo danh sách phòng ban được phân quyền theo nhóm tài khoản
        /// </summary>
        /// <param name="context"></param>
        /// <param name="otherContext"></param>
        /// <param name="cache"></param>
        public void InitDepartmentAssignedAndParent(EPAD_Context context, ezHR_Context otherContext, IMemoryCache cache)
        {
            ConfigObject config = ConfigObject.GetConfig(cache);
            // gắn cả ds phòng ban của EPAD và ezHR (nếu có) cho list phòng ban được phân quyền
            // để khỏi kiểm tra phân quyền là có tích hợp hay không
            if (IsAdmin)
            {
                if (config.IntegrateDBOther)
                {
                    ListDepartmentAssigned = otherContext.HR_Department.Where(t => t.CompanyIndex == config.CompanyIndex).Select(x => x.Index).ToList();
                }
                else
                {
                    ListDepartmentAssigned = context.IC_Department.Where(t => t.CompanyIndex == CompanyIndex && t.IsInactive != true).Select(x => (long)x.Index).ToList();
                }
            }
            else
            {
                ListDepartmentAssigned = context.IC_PrivilegeDepartment.Where(t => t.CompanyIndex == CompanyIndex
                 && t.PrivilegeIndex == PrivilegeIndex).Select(x => x.DepartmentIndex).ToList();
            }

            List<IC_Department> listDepartment = context.IC_Department
                .Where(t => t.CompanyIndex == CompanyIndex && t.IsInactive != true).ToList();


            if (config.IntegrateDBOther)
            {
                List<IC_Department> departments = otherContext.HR_Department
                    .Where(t => t.CompanyIndex == config.CompanyIndex)
                    .Select(e => new IC_Department()
                    {
                        Index = (int)e.Index,
                        Code = e.Code,
                        Name = e.Name,
                        ParentIndex = (int)e.ParentIndex,
                    }).ToList();
                listDepartment.AddRange(departments);
            }

            List<long> listDepIndexTemp = new List<long>();
            List<long> listDepRemove = new List<long>();
            for (int i = 0; i < ListDepartmentAssigned.Count; i++)
            {
                IC_Department depInfo = listDepartment.Where(t => t.Index == ListDepartmentAssigned[i]).FirstOrDefault();
                if (depInfo == null)
                {
                    listDepRemove.Add(ListDepartmentAssigned[i]);
                    continue;
                }

                if (listDepIndexTemp.Contains(ListDepartmentAssigned[i]) == false)
                {
                    listDepIndexTemp.Add(ListDepartmentAssigned[i]);
                }
                RecursiveGetListParentDepartmentIndex(listDepartment, depInfo.ParentIndex == null ? 0 : depInfo.ParentIndex.Value, ref listDepIndexTemp);
            }
            //remove dep error
            for (int i = 0; i < listDepRemove.Count; i++)
            {
                ListDepartmentAssigned.Remove(listDepRemove[i]);
            }
            if (ListDepartmentAssigned.Where(e => e == 0).Count() == 0)
            {
                ListDepartmentAssigned.Add(0);
            }
            ListDepartmentAssignedAndParent = listDepIndexTemp;
        }
        private void RecursiveGetListParentDepartmentIndex(List<IC_Department> pListDep, long pParentDepIndex, ref List<long> listDepartmentIndex)
        {
            IC_Department parentDep = pListDep.Where(t => t.Index == pParentDepIndex).FirstOrDefault();
            if (parentDep != null)
            {
                if (listDepartmentIndex.Contains(parentDep.Index) == false)
                {
                    listDepartmentIndex.Add(parentDep.Index);
                }
                RecursiveGetListParentDepartmentIndex(pListDep, parentDep.ParentIndex == null ? long.Parse("0") : parentDep.ParentIndex.Value, ref listDepartmentIndex);
            }
        }

        #region service methods
        public List<CommandResult> GetSomeCommandsUnExecute(SerialNumberInfos lsSerialNumber)
        {
            ListCommands.RemoveAll(cmd => cmd == null);
            var listCommand = new List<CommandResult>();
            if (lsSerialNumber != null && lsSerialNumber.ServiceType != null && lsSerialNumber.ServiceType == GlobalParams.ServiceType.SDKInterfaceService)
            {
                listCommand = ListCommands.Where(t => t.Status == CommandStatus.UnExecute.ToString()
                        && lsSerialNumber.ListSerialNumber != null && lsSerialNumber.ListSerialNumber.Contains(t.SerialNumber)
                        && (t.AppliedTime == null || t.AppliedTime < DateTime.Now))
                    .Take(GlobalParams.COMMAND_NUMBER_RETURN_SDK).ToList();
            }
            else
            {
                // get commands by serial for push api
                listCommand = ListCommands.Where(t => t.Status == CommandStatus.UnExecute.ToString()
                    && lsSerialNumber?.ListSerialNumber != null
                    && lsSerialNumber.ListSerialNumber.Contains(t.SerialNumber)
                    && (t.AppliedTime == null || t.AppliedTime < DateTime.Now))
                    .Take(GlobalParams.COMMAND_NUMBER_RETURN_PUSH).ToList();
            }

            return listCommand;
        }

        public bool UpdateStatusCommand(int pId, string pStatus, string pError,
            string dataSuccess, string dataFailure,
            ref CommandResult pCommand, EPAD_Context context,
            ezHR_Context pOtherContext, ConfigObject pConfig)
        {
            bool success = false;
            IC_SystemCommand cmdModel = context.IC_SystemCommand
                .Include(x => x.IC_Audits)
                .FirstOrDefault(t => t.Index == pId && t.IsActive);
            if (cmdModel == null)
                return success;

            DateTime now = DateTime.Now;



            CommandResult command = ListCommands.FirstOrDefault(t => t.ID == pId.ToString());

            if (command != null)
            {
                pCommand = command;
                bool isDownloadAttendanceLogFail = command.Command == CommandAction.DownloadLogFromToTime.ToString() && pError == "Dữ liệu log đọc không thành công";
                if (pStatus == CommandStatus.Success.ToString())
                {
                    ListCommands.Remove(command);
                    cmdModel.Excuted = true;
                    cmdModel.Error = "";
                    cmdModel.UpdatedDate = now;

                    #region Save executed command history to IC_Audit
                    var latestAudit = cmdModel.IC_Audits.LastOrDefault();
                    if (latestAudit != null)
                    {
                        var audit = new IC_Audit()
                        {
                            UserName = latestAudit?.UserName,
                            Name = latestAudit?.Name,
                            Description = latestAudit.Description,
                            DescriptionEn = latestAudit.DescriptionEn,
                            DateTime = DateTime.Now,
                            TableName = latestAudit.TableName,
                            PageName = latestAudit.PageName,
                            Status = AuditStatus.Completed.ToString(),
                            CompanyIndex = latestAudit.CompanyIndex,
                            IC_SystemCommandIndex = latestAudit.IC_SystemCommandIndex,
                            State = latestAudit.State
                        };

                        if (cmdModel.Command == CommandAction.DownloadLogFromToTime.ToString())
                        {
                            List<dynamic> logInfos = JsonConvert.DeserializeObject<List<dynamic>>(dataSuccess);
                            if (logInfos != null)
                            {
                                audit.NumSuccess = logInfos.Count();
                                audit.NewValues = dataSuccess; ;
                                if (isDownloadAttendanceLogFail)
                                {
                                    audit.NumFailure = 0;
                                    audit.Description = $"Thiết bị {command.SerialNumber} không có log điểm danh";
                                    audit.DescriptionEn = $"Device {command.SerialNumber} has no attendance log";
                                }
                            }
                            else
                            {
                                audit.NewValues = dataSuccess;
                            }

                        }
                        else if (cmdModel.Command == CommandAction.SetTimeDevice.ToString()
                          || cmdModel.Command == CommandAction.DeleteLogFromToTime.ToString())
                        {
                            audit.NumFailure = null;
                            audit.NumSuccess = null;
                            audit.OldValues = null;
                            audit.NewValues = null;
                        }
                        else
                        {
                            // Trường hợp bình thường SDK_Interface trả về list userId
                            string[] userIdsFailure = JsonConvert.DeserializeObject<string[]>(dataFailure);
                            string[] userIdsSuccess = JsonConvert.DeserializeObject<string[]>(dataSuccess);
                            audit.NumFailure = userIdsFailure?.Length;
                            audit.NumSuccess = userIdsSuccess?.Length;
                        }
                        cmdModel.IC_Audits.Add(audit);
                    }
                    #endregion

                    context.SaveChanges();
                    //cập nhât working info

                    if ((cmdModel.Command == CommandAction.UploadUsers.ToString() || cmdModel.Command == CommandAction.DeleteUserById.ToString())
                        && cmdModel.EmployeeATIDs != "")
                    {
                        // kiểm tra xem còn lệnh thuộc workinginfo này chưa thực hiện ko
                        CommandResult cmdCheck = ListCommands.Where(t => (t.Command == CommandAction.UploadUsers.ToString() || t.Command == CommandAction.DeleteUserById.ToString())
                            && t.ExternalData == cmdModel.EmployeeATIDs).FirstOrDefault();
                        if (cmdCheck == null)
                        {
                            if (pConfig.IntegrateDBOther == true)
                            {
                                UpdateHRWorkingInfo(pOtherContext, cmdModel.EmployeeATIDs, true);
                            }
                            else
                            {
                                UpdateICWorkingInfo(context, cmdModel.EmployeeATIDs, true);
                            }
                        }
                    }
                }
                else if (pStatus == CommandStatus.DeleteManual.ToString())
                {
                    pCommand = command;
                    ListCommands.Remove(command);
                    //cập nhât working info
                    if ((cmdModel.Command == CommandAction.UploadUsers.ToString() || cmdModel.Command == CommandAction.DeleteUserById.ToString())
                        && cmdModel.EmployeeATIDs != "")
                    {
                        if (pConfig.IntegrateDBOther == true)
                        {
                            UpdateHRWorkingInfo(pOtherContext, cmdModel.EmployeeATIDs, false);
                        }
                        else
                        {
                            UpdateICWorkingInfo(context, cmdModel.EmployeeATIDs, false);
                        }
                    }
                    context.SaveChanges();
                }
                else
                {
                    command.ErrorCounter++;
                    command.AppliedTime = DateTime.Now.AddMinutes(5);
                    command.Error = pError;

                    cmdModel.Error = pError;
                    cmdModel.UpdatedDate = now;
                    cmdModel.Excuted = true;

                    #region Add Audit about error of systemCommand
                    var latestAudit = cmdModel.IC_Audits.LastOrDefault();

                    var audit = new IC_Audit()
                    {
                        UserName = latestAudit?.UserName,
                        Name = latestAudit?.Name,
                        Description = pError,
                        DescriptionEn = pError,
                        DateTime = DateTime.Now,
                        TableName = latestAudit?.TableName,
                        PageName = latestAudit?.PageName,
                        Status = AuditStatus.Error.ToString(),
                        CompanyIndex = cmdModel.CompanyIndex,
                        IC_SystemCommandIndex = cmdModel.Index,
                        State = latestAudit?.State
                    };
                    cmdModel.IC_Audits.Add(audit);
                    #endregion

                    if (command.ErrorCounter >= 3)
                    {
                        pCommand = command;
                        ListCommands.Remove(command);

                        //cập nhât working info
                        if ((cmdModel.Command == CommandAction.UploadUsers.ToString() || cmdModel.Command == CommandAction.DeleteUserById.ToString())
                            && cmdModel.EmployeeATIDs != "")
                        {
                            if (pConfig.IntegrateDBOther == true)
                            {
                                UpdateHRWorkingInfo(pOtherContext, cmdModel.EmployeeATIDs, false);
                            }
                            else
                            {
                                UpdateICWorkingInfo(context, cmdModel.EmployeeATIDs, false);
                            }
                        }
                    }
                    context.SaveChanges();
                }
            }
            success = true;
            return success;
        }


        private void UpdateHRWorkingInfo(ezHR_Context pOtherContext, string pData, bool pSuccess)
        {

            long index = 0;
            long.TryParse(pData, out index);
            HR_WorkingInfo working = pOtherContext.HR_WorkingInfo.Where(t => t.Index == index).FirstOrDefault();
            if (working != null)
            {
                if (pSuccess == true)
                {
                    working.Synched = 1;
                }
                else
                {
                    working.Synched = null;
                }
                pOtherContext.SaveChanges();
            }

        }
        private void UpdateICWorkingInfo(EPAD_Context pContext, string pData, bool pSuccess)
        {

            long index = 0;
            long.TryParse(pData, out index);
            IC_WorkingInfo working = pContext.IC_WorkingInfo.Where(t => t.Index == index).FirstOrDefault();
            if (working != null)
            {
                if (pSuccess == true)
                {
                    working.IsSync = true;
                }
                else
                {
                    working.IsSync = null;
                }
                pContext.SaveChanges();
            }

        }


        public bool CheckCommandExists(string pCommandAction, string pSerial, string pIp, DateTime pCreatedDate)
        {
            bool exists = false;
            CommandResult command = ListCommands.Find(t => t.Command == pCommandAction && t.SerialNumber == pSerial
                    && t.IPAddress == pIp && t.CreatedTime == pCreatedDate);
            if (command != null)
            {
                exists = true;
            }
            return exists;
        }
        #endregion


        public void AddToCache(IMemoryCache cache, string pGuid)
        {
            string id = $"urn:authinfo-{pGuid}";
            cache.Set(id, this);
        }

        public void AddToCacheWithMinutes(IMemoryCache cache, string pGuid, double pMinutes)
        {
            string id = $"urn:authinfo-{pGuid}";
            cache.Set(id, this, DateTime.Now.AddMinutes(pMinutes));
        }

        public void AddToCache(IMemoryCache cache, string pGuid, TimeSpan pAbsoluteExpiredTime)
        {
            // cache.Set(pGuid, this, TimeSpan.FromHours(24));
            // hết hạn sau 7 ngày (bất kể có truy cập hay không), hết hạn sau 24h nếu không truy cập
            cache.Set(pGuid, this, pAbsoluteExpiredTime);
        }

        /// <summary>
        /// Get user login object from cache with guid key
        /// </summary>
        /// <param name="pGuid"></param>
        /// <returns></returns>
        static public UserInfo GetFromCache(IMemoryCache cache, string pGuid)
        {
            if (cache.TryGetValue($"urn:authinfo-{pGuid}", out UserInfo user) == false)
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
            string id = $"urn:authinfo-{pGuid}";
            cache.Remove(id);
        }

        public void InitDepartmentAssignedAndParentForService(EPAD_Context context, ezHR_Context otherContext, IMemoryCache cache)
        {
            // gắn cả ds phòng ban của EPAD và ezHR (nếu có) cho list phòng ban được phân quyền
            // để khỏi kiểm tra phân quyền là có tích hợp hay không

            List<IC_Department> listDepartment = context.IC_Department
                .Where(t => t.CompanyIndex == CompanyIndex && t.IsInactive != true).ToList();

            ConfigObject config = ConfigObject.GetConfig(cache);
            if (config.IntegrateDBOther)
            {
                List<IC_Department> departments = otherContext.HR_Department
                    .Where(t => t.CompanyIndex == config.CompanyIndex)
                    .Select(e => new IC_Department()
                    {
                        Index = (int)e.Index,
                        Code = e.Code,
                        Name = e.Name,
                        ParentIndex = (int)e.ParentIndex,
                    }).ToList();
                listDepartment.AddRange(departments);
            }
            ListDepartmentAssigned = new List<long>();
            ListDepartmentAssigned.AddRange(listDepartment.Select(x => (long)x.Index));

            if (ListDepartmentAssigned.Where(e => e == 0).Count() == 0)
            {
                ListDepartmentAssigned.Add(0);
            }
        }
    }
}