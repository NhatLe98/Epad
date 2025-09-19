using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using EPAD_Services.Interface;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPAD_Services.Impl
{
    public class IC_OfflineService : IIC_OfflineService
    {
        private readonly EPAD_Context _dbContext;
        public IC_OfflineService(EPAD_Context dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task IntegrateBlackList(List<GC_BlackList> blacklists)
        {
            var blackLists = await _dbContext.GC_BlackList.ToListAsync();
            foreach (var item in blacklists)
            {
                var blackList = blackLists.FirstOrDefault(x => (x.EmployeeATID == item.EmployeeATID && !string.IsNullOrEmpty(x.EmployeeATID)) || (x.Nric == item.Nric && !string.IsNullOrEmpty(x.Nric)));

                if (blackList != null)
                {
                    blackList.FromDate = item.FromDate;
                    blackList.ToDate = item.ToDate;
                    blackList.Reason = item.Reason;
                    blackList.ReasonRemove = item.ReasonRemove;
                    blackList.UpdatedDate = DateTime.Now;
                    _dbContext.GC_BlackList.Update(blackList);
                }
                else
                {
                    blackList = new GC_BlackList()
                    {
                        EmployeeATID = item.EmployeeATID,
                        FullName = item.FullName,
                        IsEmployeeSystem = item.IsEmployeeSystem,
                        FromDate = item.FromDate,
                        ToDate = item.ToDate,
                        Reason = item.Reason,
                        ReasonRemove = item.ReasonRemove,
                        UpdatedDate = DateTime.Now,
                        Nric = item.Nric,
                        CompanyIndex = item.CompanyIndex,
                        UpdatedUser = UpdatedUser.AutoIntegrateEmployee.ToString()
                    };
                    await _dbContext.GC_BlackList.AddAsync(blackList);
                }
            }

            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteBlackList(List<string> employeeATIDs, List<string> nricLst)
        {
            var listBlack = await _dbContext.GC_BlackList.Where(x => employeeATIDs.Contains(x.EmployeeATID) || nricLst.Contains(x.Nric)).ToListAsync();

            if (listBlack.Count > 0)
            {
                _dbContext.RemoveRange(listBlack);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task IntegrateCustomerCardToOffline(List<HR_CustomerCard> employeeATIDs)
        {
            var customerCards = await _dbContext.HR_CustomerCard.ToListAsync();
            foreach (var item in employeeATIDs)
            {
                var customerCard = customerCards.FirstOrDefault(x => (x.CardID == item.CardID));

                if (customerCard != null)
                {
                    customerCard.CardNumber = item.CardNumber;
                    customerCard.UpdatedUser = UpdatedUser.AutoIntegrateEmployee.ToString();
                    customerCard.UpdatedDate = DateTime.Now;
                }
                else
                {
                    customerCard = new HR_CustomerCard()
                    {
                        CardID = item.CardID,
                        CardNumber = item.CardNumber,
                        CompanyIndex = item.CompanyIndex,
                        Description = item.Description,
                        CreatedDate = DateTime.Now,
                        UpdatedUser = UpdatedUser.AutoIntegrateEmployee.ToString(),

                    };
                    await _dbContext.HR_CustomerCard.AddAsync(customerCard);
                }
            }

            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteCustomerCardToOffline(List<string> employeeATIDs)
        {
            var listBlack = await _dbContext.HR_CustomerCard.Where(x => employeeATIDs.Contains(x.CardID)).ToListAsync();

            if (listBlack.Count > 0)
            {
                _dbContext.RemoveRange(listBlack);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task IntegrateCardToOffline(List<HR_CardNumberInfo> employeeATIDs)
        {
            var customerCards = await _dbContext.HR_CardNumberInfo.ToListAsync();
            foreach (var item in employeeATIDs)
            {
                var customerCard = customerCards.Where(x => x.EmployeeATID == item.EmployeeATID && x.IsActive == true).ToList();

                if (customerCard != null && customerCard.Count > 0)
                {
                    var haveCard = false;
                    foreach (var item1 in customerCard)
                    {
                        if (item1.CardNumber == item.CardNumber && item1.EmployeeATID == item.EmployeeATID && item1.IsActive == true)
                        {
                            item1.UpdatedDate = DateTime.Now;
                            haveCard = true;
                        }
                        else
                        {
                            item1.IsActive = false;
                            item1.UpdatedDate = DateTime.Now;
                        }
                    }

                    if (!haveCard)
                    {
                        var card = new HR_CardNumberInfo()
                        {
                            EmployeeATID = item.EmployeeATID,
                            CardNumber = item.CardNumber,
                            IsActive = true,
                            CompanyIndex = item.CompanyIndex,
                            UpdatedDate = DateTime.Now,
                            UpdatedUser = UpdatedUser.AutoIntegrateEmployee.ToString()
                        };
                        await _dbContext.HR_CardNumberInfo.AddAsync(card);
                    }
                }
                else
                {
                    var card = new HR_CardNumberInfo()
                    {
                        EmployeeATID = item.EmployeeATID,
                        CardNumber = item.CardNumber,
                        IsActive = true,
                        CompanyIndex = item.CompanyIndex,
                        UpdatedDate = DateTime.Now,
                        UpdatedUser = UpdatedUser.AutoIntegrateEmployee.ToString()
                    };
                    await _dbContext.HR_CardNumberInfo.AddAsync(card);
                }
            }

            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteCardToOffline(List<HR_CardNumberInfo> employeeATIDs)
        {
            var customerCards = await _dbContext.HR_CardNumberInfo.ToListAsync();
            foreach (var item in employeeATIDs)
            {
                var customerCard = customerCards.Where(x => x.EmployeeATID == item.EmployeeATID && x.IsActive == true).ToList();

                if (customerCard != null && customerCard.Count > 0)
                {
                    foreach (var item1 in customerCard)
                    {
                        if (item1.CardNumber == item.CardNumber && item1.EmployeeATID == item.EmployeeATID && item1.IsActive == true)
                        {
                            item1.UpdatedDate = DateTime.Now;
                            item1.IsActive = false;
                        }
                    }
                }
            }

            await _dbContext.SaveChangesAsync();
        }

        public async Task IntegrateDepartmentToOffline(List<IC_Department> departmentList)
        {
            var đepartments = await _dbContext.IC_Department.ToListAsync();
            foreach (var item in departmentList)
            {
                var department = đepartments.FirstOrDefault(x => x.Code == item.Code);
                var parentDepartment = đepartments.FirstOrDefault(x => x.Code == item.ParentCode);

                if (department != null)
                {
                    department.Name = item.Name;
                    department.ParentIndex = parentDepartment != null ? parentDepartment.Index : 0;
                    department.UpdatedUser = UpdatedUser.AutoIntegrateEmployee.ToString();
                    department.UpdatedDate = DateTime.Now;
                    department.IsInactive = false;
                    department.IsContractorDepartment = item.IsContractorDepartment;
                    department.IsDriverDepartment = item.IsDriverDepartment;
                }
                else
                {
                    department = new IC_Department()
                    {
                        Code = item.Code,
                        Name = item.Name,
                        CompanyIndex = item.CompanyIndex,
                        ParentIndex = parentDepartment != null ? parentDepartment.Index : 0,
                        CreatedDate = DateTime.Now,
                        UpdatedUser = UpdatedUser.AutoIntegrateEmployee.ToString(),
                        IsContractorDepartment = item.IsContractorDepartment,
                        IsDriverDepartment = item.IsDriverDepartment
                    };
                    await _dbContext.IC_Department.AddAsync(department);
                }
            }

            await _dbContext.SaveChangesAsync();
        }



        public async Task DeleteDepartmentToOffline(List<string> codes)
        {
            var lstDepartment = await _dbContext.IC_Department.Where(x => codes.Contains(x.Code) && !string.IsNullOrEmpty(x.Code)).ToListAsync();
            _dbContext.RemoveRange(lstDepartment);
            await _dbContext.SaveChangesAsync();
        }

        public async Task IntegrateUserToOffline(List<HR_User> userList)
        {
            var emps = userList.Select(x => x.EmployeeATID).Distinct().ToList();
            var users = await _dbContext.HR_User.Where(x => emps.Contains(x.EmployeeATID)).ToListAsync();
            foreach (var item in userList)
            {
                var user = users.FirstOrDefault(x => x.EmployeeATID == item.EmployeeATID);

                if (user != null)
                {
                    user.EmployeeCode = item.EmployeeCode;
                    user.FullName = item.FullName;
                    user.Gender = item.Gender;
                    user.EmployeeType = item.EmployeeType;
                    user.Nric = item.Nric;
                    user.IsAllowPhone = item.IsAllowPhone;
                    user.UpdatedDate = DateTime.Now;
                    user.UpdatedUser = UpdatedUser.AutoIntegrateEmployee.ToString();
                }
                else
                {
                    user = new HR_User()
                    {
                        EmployeeATID = item.EmployeeATID,
                        EmployeeCode = item.EmployeeCode,
                        FullName = item.FullName,
                        Gender = item.Gender,
                        EmployeeType = item.EmployeeType,
                        Nric = item.Nric,
                        IsAllowPhone = item.IsAllowPhone,
                        UpdatedDate = DateTime.Now,
                        UpdatedUser = UpdatedUser.AutoIntegrateEmployee.ToString(),
                        CompanyIndex = item.CompanyIndex,
                    };
                    await _dbContext.HR_User.AddAsync(user);

                    var userMaster = new IC_UserMaster()
                    {
                        EmployeeATID = item.EmployeeATID,
                        CompanyIndex = item.CompanyIndex,
                        CardNumber = "0",
                        Privilege = 0,
                        NameOnMachine = "",
                        CreatedDate = DateTime.Now,
                        UpdatedUser = UpdatedUser.AutoIntegrateEmployee.ToString()
                    };
                    await _dbContext.IC_UserMaster.AddAsync(userMaster);
                }
            }

            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteUserToOffline(List<string> employees)
        {
            var users = await _dbContext.HR_User.Where(x => employees.Contains(x.EmployeeATID)).ToListAsync();
            var workingInfos = await _dbContext.IC_WorkingInfo.Where(x => employees.Contains(x.EmployeeATID)).ToListAsync();
            var cardNumberInfs = await _dbContext.HR_CardNumberInfo.Where(x => employees.Contains(x.EmployeeATID)).ToListAsync();
            var blackLists = await _dbContext.GC_BlackList.Where(x => employees.Contains(x.EmployeeATID)).ToListAsync();
            var usermasters = await _dbContext.IC_UserMaster.Where(x => employees.Contains(x.EmployeeATID)).ToListAsync();
            var employeeInfos = await _dbContext.HR_EmployeeInfo.Where(x => employees.Contains(x.EmployeeATID)).ToListAsync();
            var customerInfos = await _dbContext.HR_CustomerInfo.Where(x => employees.Contains(x.EmployeeATID)).ToListAsync();
            foreach (var item in employees)
            {
                var user = users.Where(x => x.EmployeeATID == item).ToList();
                var workingInfo = workingInfos.Where(x => x.EmployeeATID == item).ToList();
                var cardNumberInf = cardNumberInfs.Where(x => x.EmployeeATID == item).ToList();
                var blackList = blackLists.Where(x => x.EmployeeATID == item).ToList();
                var usermaster = usermasters.Where(x => x.EmployeeATID == item).ToList();
                var employeeInfo = employeeInfos.Where(x => x.EmployeeATID == item).ToList();
                var customerInfo = customerInfos.Where(x => x.EmployeeATID == item).ToList();

                _dbContext.HR_User.RemoveRange(user);
                _dbContext.IC_WorkingInfo.RemoveRange(workingInfo);
                _dbContext.HR_CardNumberInfo.RemoveRange(cardNumberInf);
                _dbContext.GC_BlackList.RemoveRange(blackList);
                _dbContext.IC_UserMaster.RemoveRange(usermaster);
                _dbContext.HR_EmployeeInfo.RemoveRange(employeeInfo);
                _dbContext.HR_CustomerInfo.RemoveRange(customerInfo);
            }
            await _dbContext.SaveChangesAsync();

        }
        public async Task IntegrateWorkingInfo(List<IC_WorkingInfoIntegrate> workingInfoList)
        {
            var emps = workingInfoList.Select(x => x.EmployeeATID).Distinct().ToList();
            var workingInfos = await _dbContext.IC_WorkingInfo.Where(x => emps.Contains(x.EmployeeATID)).ToListAsync();
            var departments = await _dbContext.IC_Department.ToListAsync();
            foreach (var item in workingInfoList)
            {
                var userlst = workingInfos.Where(x => x.EmployeeATID == item.EmployeeATID).ToList();
                if (userlst != null && userlst.Count > 1)
                {
                    var isFirst = true;
                    foreach (var item1 in userlst)
                    {
                        if (isFirst)
                        {
                            isFirst = false;
                            var department = departments.FirstOrDefault(x => x.Code == item.DepartmentCode);
                            item1.DepartmentIndex = department != null ? department.Index : 0;
                            item1.UpdatedDate = DateTime.Now;
                            item1.UpdatedUser = UpdatedUser.AutoIntegrateEmployee.ToString();
                            item1.FromDate = item.FromDate;
                            item1.ToDate = item.ToDate;
                            item1.PositionIndex = item.PositionIndex;
                            item1.Status = item.Status;
                            item1.IsManager = item.IsManager;
                            _dbContext.IC_WorkingInfo.Update(item1);
                        }
                        else
                        {
                            _dbContext.IC_WorkingInfo.Remove(item1);
                        }

                    }
                }
                else if (userlst != null && userlst.Count == 1)
                {
                    var item1 = userlst.FirstOrDefault();
                    var department = departments.FirstOrDefault(x => x.Code == item.DepartmentCode);
                    item1.DepartmentIndex = department != null ? department.Index : 0;
                    item1.UpdatedDate = DateTime.Now;
                    item1.UpdatedUser = UpdatedUser.AutoIntegrateEmployee.ToString();
                    item1.FromDate = item.FromDate;
                    item1.ToDate = item.ToDate;
                    item1.PositionIndex = item.PositionIndex;
                    item1.Status = item.Status;
                    item1.IsManager = item.IsManager;
                    _dbContext.IC_WorkingInfo.Update(item1);

                }
                else
                {
                    var item1 = new IC_WorkingInfo();
                    var department = departments.FirstOrDefault(x => x.Code == item.DepartmentCode);
                    item1.EmployeeATID = item.EmployeeATID;
                    item1.DepartmentIndex = department != null ? department.Index : 0;
                    item1.UpdatedDate = DateTime.Now;
                    item1.UpdatedUser = UpdatedUser.AutoIntegrateEmployee.ToString();
                    item1.FromDate = item.FromDate;
                    item1.ToDate = item.ToDate;
                    item1.PositionIndex = item.PositionIndex;
                    item1.Status = item.Status;
                    item1.IsManager = item.IsManager;
                    item1.CompanyIndex = item.CompanyIndex;
                    _dbContext.IC_WorkingInfo.Add(item1);
                }

            }

            await _dbContext.SaveChangesAsync();
        }

        public async Task IntegrateEmployeeToOffline(List<HR_EmployeeInfo> userList)
        {
            var emps = userList.Select(x => x.EmployeeATID).Distinct().ToList();
            var users = await _dbContext.HR_EmployeeInfo.Where(x => emps.Contains(x.EmployeeATID)).ToListAsync();
            foreach (var item in userList)
            {
                var user = users.FirstOrDefault(x => x.EmployeeATID == item.EmployeeATID);

                if (user == null)
                {
                    user = new HR_EmployeeInfo()
                    {
                        EmployeeATID = item.EmployeeATID,
                        CompanyIndex = item.CompanyIndex,
                        JoinedDate = item.JoinedDate,
                        UpdatedDate = DateTime.Now,
                        UpdatedUser = UpdatedUser.AutoIntegrateEmployee.ToString()
                    };
                    await _dbContext.HR_EmployeeInfo.AddAsync(user);
                }

                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task IntegrateCustmerToOffline(List<HR_CustomerInfo> userList)
        {
            var emps = userList.Select(x => x.EmployeeATID).Distinct().ToList();
            var users = await _dbContext.HR_CustomerInfo.Where(x => emps.Contains(x.EmployeeATID)).ToListAsync();
            foreach (var item in userList)
            {
                var user = users.FirstOrDefault(x => x.EmployeeATID == item.EmployeeATID);

                if (user != null)
                {
                    user.NRIC = item.NRIC;
                    user.RegisterTime = item.RegisterTime;
                    user.FromTime = item.FromTime;
                    user.ToTime = item.ToTime;
                    user.IsAllowPhone = item.IsAllowPhone;
                    user.UpdatedDate = DateTime.Now;
                    user.UpdatedUser = UpdatedUser.AutoIntegrateEmployee.ToString();
                }
                else
                {
                    user = new HR_CustomerInfo()
                    {
                        EmployeeATID = item.EmployeeATID,
                        NRIC = item.NRIC,
                        RegisterTime = item.RegisterTime,
                        FromTime = item.FromTime,
                        ToTime = item.ToTime,
                        IsAllowPhone = item.IsAllowPhone,
                        UpdatedDate = DateTime.Now,
                        UpdatedUser = UpdatedUser.AutoIntegrateEmployee.ToString(),
                        CompanyIndex = item.CompanyIndex,
                    };
                    await _dbContext.HR_CustomerInfo.AddAsync(user);
                }

                await _dbContext.SaveChangesAsync();
            }
        }
    }

}
