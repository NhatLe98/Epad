using EPAD_Common.Types;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using static EPAD_Data.Models.CommonUtils;

namespace EPAD_Logic
{
    public class IC_UserNotificationLogic : IIC_UserNotificationLogic
    {
        private EPAD_Context _dbContext;
        public IC_UserNotificationLogic(EPAD_Context dbContext)
        {
            _dbContext = dbContext;
        }
        public int Delete(int index)
        {
            var query = _dbContext.IC_UserNotification.AsQueryable();
            var notify = query.Single(u => u.Index == index);
            if (notify != null)
            {
                _dbContext.IC_UserNotification.Remove(notify);
                _dbContext.SaveChanges();
                return index;
            }
            return 0;
        }
        public bool DeleteList(List<int> listNotify)
        {
            var query = _dbContext.IC_UserNotification.AsQueryable();
            var data = query.Where(e => listNotify.Contains(e.Index)).ToList();
            if (data != null)
            {
                _dbContext.IC_UserNotification.RemoveRange(data);
                _dbContext.SaveChanges();
                return true;
            }
            return false;
        }

        public IEnumerable<IC_UserNotificationDTO> GetMany(List<AddedParam> addedParams)
        {
            var query = _dbContext.IC_UserNotification.AsQueryable();
            if (addedParams != null)
            {
                foreach (AddedParam p in addedParams)
                {
                    switch (p.Key)
                    {
                        case "Filter":
                            if (p.Value != null)
                            {
                            }
                            break;
                        case "Index":
                            if (p.Value != null)
                            {
                                int index = Convert.ToInt32(p.Value);
                                query = query.Where(u => u.Index == index);
                            }
                            break;
                        case "UserName":
                            if (p.Value != null)
                            {
                                string userName = p.Value.ToString();
                                query = query.Where(u => u.UserName.Equals(userName));
                            }
                            break;


                    }
                }
            }
            query = query.OrderBy(u => u.UserName);
            var count = query.Count();
            var data = query.Select(u => new IC_UserNotificationDTO
            {
                Index = u.Index,
                UserName = u.UserName,
                Status = u.Status,
                Type = u.Type,
                Message = u.Message,
                Data = !string.IsNullOrWhiteSpace(u.Message) ? JsonConvert.DeserializeObject<MessageBodyDTO>(u.Message) : null
            }).ToList();
            return data;

        }

        public IC_UserNotificationDTO Create(IC_UserNotificationDTO notify)
        {
            IC_UserNotification noitify = (IC_UserNotification)ConvertObject(notify, new IC_UserNotification());
            _dbContext.IC_UserNotification.Add(noitify);
            _dbContext.SaveChanges();

            return notify;
        }

        public List<IC_UserNotificationDTO> CreateList(List<IC_UserNotificationDTO> listNotifies)
        {
            foreach (var notify in listNotifies)
            {
                IC_UserNotification noitify = (IC_UserNotification)ConvertObject(notify, new IC_UserNotification());
                _dbContext.IC_UserNotification.Add(noitify);
            }
            _dbContext.SaveChanges();

            return listNotifies;
        }

        public List<IC_UserNotificationDTO> GetListUserNotify(List<AddedParam> addedParams)
        {
            var query = _dbContext.IC_UserAccount.Join(_dbContext.IC_PrivilegeDepartment,
                    user => user.AccountPrivilege, // Key Join
                    pd => pd.PrivilegeIndex, // Key Join
                    (user, pd) => new { UserAccount = user, PrivilegeDepartment = pd });

            if (addedParams != null)
            {
                foreach (AddedParam p in addedParams)
                {
                    switch (p.Key)
                    {
                        case "CompanyIndex":
                            if (p.Value != null)
                            {
                                int companyIndex = Convert.ToInt32(p.Value);
                                query = query.Where(u => u.UserAccount.CompanyIndex == companyIndex);
                            }
                            break;
                        case "DepartmentIndex":
                            if (p.Value != null)
                            {
                                int departmentIndex = Convert.ToInt32(p.Value);
                                query = query.Where(u => u.PrivilegeDepartment.DepartmentIndex == departmentIndex);
                            }
                            break;
                    }
                }
            }

            var data = query.AsEnumerable().Select(us => new IC_UserNotificationDTO
                            {
                                UserName = us.UserAccount.UserName
                            }).GroupBy(g=>g.UserName).Select(s=>s.First()).ToList();

            return data;

        }

        public void UpdateExpiratedTransferEmployee()
        {
            
            List<IC_EmployeeTransferDTO> listEmployeeTransfer = new List<IC_EmployeeTransferDTO>();


            //get employee transfer from 2 table anh update status
            List<IC_EmployeeTransfer> listEmpTransfer = _dbContext.IC_EmployeeTransfer.Where(t => t.Status == 0 && t.FromTime.Date < DateTime.Now.Date).ToList();
            List<IC_WorkingInfo> listWorking = _dbContext.IC_WorkingInfo.Where(t => t.Status == 0 && t.FromDate.Date < DateTime.Now.Date).ToList();
            for (int i = 0; i < listEmpTransfer.Count; i++)
            {
                listEmpTransfer[i].Status = 3;
                listEmployeeTransfer.Add(new IC_EmployeeTransferDTO { CompanyIndex = listEmpTransfer[i].CompanyIndex, NewDepartment = listEmpTransfer[i].NewDepartment });
            }
            for (int i = 0; i < listWorking.Count; i++)
            {
                listWorking[i].Status = 3;
                listEmployeeTransfer.Add(new IC_EmployeeTransferDTO { CompanyIndex = listWorking[i].CompanyIndex, NewDepartment = listWorking[i].DepartmentIndex });
            }
            _dbContext.IC_EmployeeTransfer.UpdateRange(listEmpTransfer);
            _dbContext.IC_WorkingInfo.UpdateRange(listWorking);


            // Add notification
            if (listEmployeeTransfer.Count > 0)
            {
                List<AddedParam> addedParams = new List<AddedParam>();
                List<IC_UserNotificationDTO> listUserNotify = new List<IC_UserNotificationDTO>();

                var listCountEmployee = listEmployeeTransfer.GroupBy(u => new { u.CompanyIndex, u.NewDepartment }).Select(c => new { c.Key, Count = c.Count() }).ToList();
                foreach (var employee in listCountEmployee)
                {
                    addedParams = new List<AddedParam>();
                    addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = employee.Key.CompanyIndex });
                    addedParams.Add(new AddedParam { Key = "DepartmentIndex", Value = employee.Key.NewDepartment });
                    listUserNotify.AddRange(GetListUserNotify(addedParams));

                    foreach (var item in listUserNotify)
                    {
                        item.Status = 0;
                        item.Type = 3; // 0 is submit , 1 is approve, 2 is reject
                        item.Message = JsonConvert.SerializeObject(new MessageBodyDTO
                        {
                            Message = employee.Count.ToString(),
                        });
                    }
                }
                CreateList(listUserNotify);
            }
            _dbContext.SaveChanges();
        }
    }
    public interface IIC_UserNotificationLogic
    {
        IEnumerable<IC_UserNotificationDTO> GetMany(List<AddedParam> addedParams);
        int Delete(int index);
        bool DeleteList(List<int> listNotify);
        List<IC_UserNotificationDTO> GetListUserNotify(List<AddedParam> addedParams);
        IC_UserNotificationDTO Create(IC_UserNotificationDTO notify);
        List<IC_UserNotificationDTO> CreateList(List<IC_UserNotificationDTO> listNotifies);
        void  UpdateExpiratedTransferEmployee();
    }

}
