using EPAD_Common.Services;
using EPAD_Common.Types;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using EPAD_Logic;
using EPAD_Services.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EPAD_Services.Impl
{
    public class HR_CardNumberInfoService : BaseServices<HR_CardNumberInfo, EPAD_Context>, IHR_CardNumberInfoService
    {
        ConfigObject _Config;
        ezHR_Context ezHR_Context;
        private readonly IHR_EmployeeLogic _IHR_EmployeeLogic;
        public HR_CardNumberInfoService(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _Cache = serviceProvider.GetService<IMemoryCache>();
            ezHR_Context = serviceProvider.GetService<ezHR_Context>();
            _Config = ConfigObject.GetConfig(_Cache);
            _IHR_EmployeeLogic = serviceProvider.GetService<IHR_EmployeeLogic>();
        }

        public async Task<List<HR_CardNumberInfoResult>> GetAllCardNumberInfo(string[] pUserATID, int companyIndex)
        {
            if (_Config.IntegrateDBOther)
            {
                var result = ezHR_Context.HR_Employee.Where(x => pUserATID.Length == 0 ? true : pUserATID.Contains(x.EmployeeATID));
                var rs = result.Select(x => _Mapper.Map<HR_CardNumberInfoResult>(x)).ToList();
                return await Task.FromResult(rs);
            }
            else
            {
                var dummy = Where(x => pUserATID.Length == 0 ? true : pUserATID.Contains(x.EmployeeATID));
                var rs = dummy.Select(x => _Mapper.Map<HR_CardNumberInfoResult>(x)).ToList();
                return await Task.FromResult(rs);
            }
        }

        public async Task<List<HR_CardNumberInfoResult>> GetAllCardNumberInfoByEmployee(string pEmployeeATID, int pCompanyIndex)
        {
            if (_Config.IntegrateDBOther)
            {
                var result = ezHR_Context.HR_Employee.Where(x => x.CompanyIndex == pCompanyIndex && x.EmployeeATID == pEmployeeATID);
                var rs = result.Select(x => _Mapper.Map<HR_CardNumberInfoResult>(x)).ToList();
                return await Task.FromResult(rs);
            }
            else
            {
                var dummy = Where(x => x.CompanyIndex == pCompanyIndex && x.EmployeeATID == pEmployeeATID);
                var rs = dummy.Select(x => _Mapper.Map<HR_CardNumberInfoResult>(x)).ToList();
                return await Task.FromResult(rs);
            }
        }

        public async Task<HR_CardNumberInfoResult> GetCardNumberInfo(string pEmployeeATID, string pCardNumber, int pCompanyIndex)
        {
            if (_Config.IntegrateDBOther)
            {
                var result = ezHR_Context.HR_Employee.FirstOrDefault(x => x.CompanyIndex == pCompanyIndex && x.EmployeeATID == pEmployeeATID && x.CardNumber == pCardNumber);
                return await Task.FromResult(_Mapper.Map<HR_CardNumberInfoResult>(result));
            }
            else
            {
                var result = FirstOrDefault(x => x.CompanyIndex == pCompanyIndex && x.EmployeeATID == pEmployeeATID && x.CardNumber == pCardNumber);
                return await Task.FromResult(_Mapper.Map<HR_CardNumberInfoResult>(result));
            }
            
        }

        public async Task<DataGridClass> GetPage(List<AddedParam> addedParams, int pCompanyIndex) {
            if (addedParams == null || addedParams.Count == 0)
                return null;

            List<HR_CardNumberInfoResult> dummy = new List<HR_CardNumberInfoResult>();
            var pageIndex = addedParams.FirstOrDefault(e => e.Key == "PageIndex");
            var pageSize = addedParams.FirstOrDefault(e => e.Key == "PageSize");
            var pPage = Convert.ToInt32(pageIndex.Value ?? 1);
            var pLimit = Convert.ToInt32(pageSize.Value ?? GlobalParams.ROWS_NUMBER_IN_PAGE);

            var query = Where(e => e.CompanyIndex == pCompanyIndex).AsQueryable();
            var total = 0;
            if (addedParams != null)
            {
                foreach (AddedParam param in addedParams)
                {
                    switch (param.Key)
                    {
                        case "Filter":
                            if (param.Value != null)
                            {
                                string filter = param.Value.ToString();
                                query = query.Where(u => u.EmployeeATID.Contains(filter)
                                || u.CardNumber.Contains(filter));
                            }
                            break;
                        case "EmployeeATID":
                            if (param.Value != null)
                            {
                                string EmployeeATID = param.Value.ToString();
                                query = query.Where(u => u.EmployeeATID == EmployeeATID);
                            }
                            break;
                        case "CardNumber":
                            if (param.Value != null)
                            {
                                string CardNumber = param.Value.ToString();
                                query = query.Where(u => u.CardNumber == CardNumber);
                            }
                            break;
                        case "IsActive":
                            if (param.Value != null)
                            {
                                bool IsActive = Convert.ToBoolean(param.Value.ToString());
                                query = query.Where(u => u.IsActive == IsActive);
                            }
                            break;
                    }
                }

                total = query.Count();
                if (pPage < 1) pPage = 1;
                dummy = query.OrderBy(x => x.Index).Skip((pPage - 1) * pLimit).Take(pLimit).AsEnumerable().Select(x =>
                {
                    var rs = _Mapper.Map<HR_CardNumberInfoResult>(x);
                    rs.Status = x.IsActive == true ? "Hoạt động" : "Không hoạt động";
                    return rs;
                }).ToList();
            }
            var rs = new DataGridClass(total, dummy);
            return await Task.FromResult(rs);
        }

        public async Task<DataGridClass> GetDataGrid(int pCompanyIndex,int pPage, int pLimit)
        {
            if (pPage < 1) pPage = 1;


            var total = Count(x => x.CompanyIndex == pCompanyIndex);

            var result = Where(x => x.CompanyIndex == pCompanyIndex)
                        .OrderBy(x => x.EmployeeATID)
                        .Skip((pPage - 1) * pLimit)
                        .Take(pLimit)
                        .ToList();

            var gridCard = new DataGridClass(total, result);

            return await Task.FromResult(gridCard);
        }

        public async Task<bool> CheckCardActiveCurrentEmployee(HR_CardNumberInfo hrCardInfo, int pCompanyIndex) {
            var existed = FirstOrDefault(e => e.CompanyIndex == pCompanyIndex && e.CardNumber == hrCardInfo.CardNumber && (hrCardInfo.EmployeeATID == e.EmployeeATID || hrCardInfo.EmployeeATID != e.EmployeeATID)&& e.IsActive == true);
            if (existed == null)
                return false;
            return true;
        }

        public async Task<bool> CheckCardActiveOtherEmployee(HR_CardNumberInfo hrCardInfo, int pCompanyIndex)
        {
            var existed = FirstOrDefault(e => e.CompanyIndex == pCompanyIndex && e.CardNumber == hrCardInfo.CardNumber && !string.IsNullOrWhiteSpace(e.CardNumber) && hrCardInfo.EmployeeATID != e.EmployeeATID && e.IsActive == true);
            if (existed == null)
                return false;
            return true;
        }

        public async Task CheckCardActivedOrCreateList(List<HR_CardNumberInfo> listHRCardInfo, int pCompanyIndex, bool? isOverwrite = null)
        {
            var listEmployeeATID = listHRCardInfo.Select(e => e.EmployeeATID).ToHashSet();
            var listCardInfo = await DbContext.HR_CardNumberInfo.Where(e => e.CompanyIndex == pCompanyIndex && listEmployeeATID.Contains(e.EmployeeATID) && e.IsActive == true).ToListAsync();
            foreach (var item in listHRCardInfo)
            {
                var existed = listCardInfo.FirstOrDefault(e => e.EmployeeATID == item.EmployeeATID);
                if (existed == null)
                {
                    existed = new HR_CardNumberInfo();
                    existed = _Mapper.Map<HR_CardNumberInfo>(item);
                    existed.CreatedDate = DateTime.Now;
                    await InsertAsync(existed);
                }
                else if (item.CardNumber != existed.CardNumber)
                {
                    // Update old data instead of setting IsActive = false and insert new data for saving Database Capacity.
                    if (isOverwrite == true && !string.IsNullOrEmpty(item.CardNumber))
                    {
                        existed.CardNumber = item.CardNumber;
                    }
                    else if(!string.IsNullOrEmpty(item.CardNumber))
                    {
                        existed.CardNumber = isOverwrite == true || string.IsNullOrEmpty(existed.CardNumber) 
                       ? item.CardNumber : existed.CardNumber;
                    }
                    existed.UpdatedDate = DateTime.Now;
                    DbContext.HR_CardNumberInfo.Update(existed);
                }
            }

            var s = await DbContext.SaveChangesAsync();
            
        }
        public async Task CheckCardActivedOrCreate(HR_CardNumberInfo hrCardInfo, int pCompanyIndex)
        {
            if (!string.IsNullOrWhiteSpace(hrCardInfo.CardNumber))
            {
                var existed = await DbContext.HR_CardNumberInfo.Where(e => e.CompanyIndex == pCompanyIndex && hrCardInfo.EmployeeATID == e.EmployeeATID && e.IsActive == true).ToListAsync();

                if (existed != null && !existed.Select(x => x.CardNumber).Contains(hrCardInfo.CardNumber))
                {
                    foreach(var item in existed)
                    {
                        item.IsActive = false;
                        item.UpdatedDate = DateTime.Now;
                        DbContext.HR_CardNumberInfo.Update(item);
                    }
                    await DbContext.SaveChangesAsync();
                    var newCard = new HR_CardNumberInfo();
                    hrCardInfo.Index = 0;
                    newCard = _Mapper.Map<HR_CardNumberInfo>(hrCardInfo);
                    newCard.IsActive = true;
                    newCard.CreatedDate = DateTime.Now;
                    await InsertAsync(newCard);

                }
                else if (existed == null)
                {
                    var item = new HR_CardNumberInfo();
                    item = _Mapper.Map<HR_CardNumberInfo>(hrCardInfo);
                    item.IsActive = true;
                    item.CreatedDate = DateTime.Now;
                    await InsertAsync(item);
                }
            }
            
        }

        public async Task<bool> ReturnOrDeleteCard(string cardNumber, UserInfo user)
        {
            var result = true;
            var lstEmployee = new List<string>();
            try
            {
                if (!string.IsNullOrWhiteSpace(cardNumber))
                {
                    var isUpdate = false;

                    var existedCustomer = await DbContext.HR_CardNumberInfo.Where(e
                        => e.CompanyIndex == user.CompanyIndex && e.CardNumber == cardNumber && e.IsActive == true).ToListAsync();
                    if (existedCustomer != null && existedCustomer.Count > 0)
                    {
                        foreach (var item in existedCustomer)
                        {
                            item.IsActive = false;
                            item.UpdatedDate = DateTime.Now;
                            item.UpdatedUser = user.FullName;
                            DbContext.HR_CardNumberInfo.Update(item);
                            lstEmployee.Add(item.EmployeeATID);
                        }
                        isUpdate = true;
                    }

                    var existedTruckDriver = await DbContext.GC_TruckDriverLog.Where(x
                        => x.CompanyIndex == user.CompanyIndex && x.CardNumber == cardNumber && !x.IsInactive).ToListAsync();
                    if (existedTruckDriver != null && existedTruckDriver.Count > 0)
                    {
                        foreach (var item in existedTruckDriver)
                        {
                            item.IsInactive = true;
                            item.UpdatedDate = DateTime.Now;
                            item.UpdatedUser = user.FullName;
                            DbContext.GC_TruckDriverLog.Update(item);
                        }
                        isUpdate = true;
                    }

                    var existedExtraTruckDriver = await DbContext.GC_TruckExtraDriverLog.Where(x
                        => x.CardNumber == cardNumber && !x.IsInactive).ToListAsync();
                    if (existedExtraTruckDriver != null && existedExtraTruckDriver.Count > 0)
                    {
                        foreach (var item in existedExtraTruckDriver)
                        {
                            item.IsInactive = true;
                            item.UpdatedDate = DateTime.Now;
                            item.UpdatedUser = user.FullName;
                            DbContext.GC_TruckExtraDriverLog.Update(item);
                        }
                        isUpdate = true;
                    }

                    if (isUpdate)
                    {
                        await DbContext.SaveChangesAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                return false;
            }
            if(lstEmployee.Count > 0)
            {
                await _IHR_EmployeeLogic.IntegrateCardToOffline(lstEmployee, null);
            }
           
            return result;
        }

        public async Task AddOrUpdateNewCard(HR_CardNumberInfo hrCardInfo, UserInfo user)
        {
            if (!string.IsNullOrWhiteSpace(hrCardInfo.CardNumber))
            {
                var existed = await DbContext.HR_CardNumberInfo.Where(e => e.CompanyIndex == user.CompanyIndex && hrCardInfo.EmployeeATID == e.EmployeeATID && e.IsActive == true).ToListAsync();

                if (existed != null && !existed.Select(x => x.CardNumber).Contains(hrCardInfo.CardNumber))
                {
                    foreach (var item in existed)
                    {
                        item.IsActive = false;
                        item.UpdatedDate = DateTime.Now;
                        DbContext.HR_CardNumberInfo.Update(item);
                    }
                    await DbContext.SaveChangesAsync();
                    var newCard = new HR_CardNumberInfo();
                    hrCardInfo.Index = 0;
                    newCard = _Mapper.Map<HR_CardNumberInfo>(hrCardInfo);
                    newCard.IsActive = true;
                    newCard.CompanyIndex = user.CompanyIndex;
                    newCard.CreatedDate = DateTime.Now;
                    newCard.UpdatedDate = DateTime.Now;
                    newCard.UpdatedUser = user.FullName;
                    //await InsertAsync(newCard);
                    await DbContext.HR_CardNumberInfo.AddAsync(newCard);

                }
                else if (existed == null)
                {
                    var item = new HR_CardNumberInfo();
                    item = _Mapper.Map<HR_CardNumberInfo>(hrCardInfo);
                    item.IsActive = true;
                    item.CompanyIndex = user.CompanyIndex;
                    item.CreatedDate = DateTime.Now;
                    item.UpdatedDate = DateTime.Now;
                    item.UpdatedUser = user.FullName;
                    //await InsertAsync(item);
                    await DbContext.HR_CardNumberInfo.AddAsync(item);
                }
                await DbContext.SaveChangesAsync();
            }
        }

        public async Task CheckCardNumberInforActivedOrCreate(HR_CardNumberInfo hrCardInfo, int pCompanyIndex)
        {
            if (!string.IsNullOrWhiteSpace(hrCardInfo.CardNumber))
            {
                var existed = DbContext.HR_CardNumberInfo.FirstOrDefault(e => e.CompanyIndex == pCompanyIndex && hrCardInfo.EmployeeATID == e.EmployeeATID && e.IsActive == true);

                if (existed != null && existed.CardNumber != hrCardInfo.CardNumber)
                {
                    existed.IsActive = false;
                    existed.UpdatedDate = DateTime.Now;
                    DbContext.HR_CardNumberInfo.Update(existed);
                    var newCard = new HR_CardNumberInfo();
                    hrCardInfo.Index = 0;
                    newCard = _Mapper.Map<HR_CardNumberInfo>(hrCardInfo);
                    newCard.IsActive = true;
                    newCard.UpdatedUser = hrCardInfo.UpdatedUser;
                    newCard.CreatedDate = DateTime.Now;
                    await InsertAsync(newCard);
                }
                else if (existed == null)
                {
                    existed = new HR_CardNumberInfo();
                    existed = _Mapper.Map<HR_CardNumberInfo>(hrCardInfo);
                    existed.IsActive = true;
                    existed.UpdatedUser = hrCardInfo.UpdatedUser;
                    existed.CreatedDate = DateTime.Now;
                    DbContext.HR_CardNumberInfo.Add(existed);
                }

            }
        }

        public async Task<HR_CardNumberInfo> CheckCardActived(HR_CardNumberInfo hrCardInfo, int pCompanyIndex)
        {
            if (!string.IsNullOrWhiteSpace(hrCardInfo.CardNumber))
            {
                var existed = DbContext.HR_CardNumberInfo
                    .FirstOrDefault(e => e.CompanyIndex == pCompanyIndex && e.Index == hrCardInfo.Index);
                if (existed != null)
                {
                    existed.IsActive = hrCardInfo.IsActive;
                    existed.UpdatedDate = DateTime.Now;
                    DbContext.HR_CardNumberInfo.Update(existed);
                }
                else if (existed == null)
                {
                    existed = new HR_CardNumberInfo();
                    existed = _Mapper.Map<HR_CardNumberInfo>(hrCardInfo);
                    existed.CreatedDate = DateTime.Now;
                    await InsertAsync(existed);
                }
                return existed;
            }
            return null;
        }
    }
}
