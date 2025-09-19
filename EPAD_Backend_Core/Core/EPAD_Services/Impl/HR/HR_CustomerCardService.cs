using DocumentFormat.OpenXml.Bibliography;
using EPAD_Backend_Core.Models.DTOs;
using EPAD_Common.Extensions;
using EPAD_Common.Services;
using EPAD_Common.Types;
using EPAD_Common.Utility;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using EPAD_Logic;
using EPAD_Services.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EPAD_Services.Impl
{
    public class HR_CustomerCardService : BaseServices<HR_CustomerCard, EPAD_Context>, IHR_CustomerCardService
    {
        private ConfigObject _config;
        private IIC_CommandLogic _iC_CommandLogic;
        private IIC_AuditLogic _iIC_AuditLogic;
        private IHR_EmployeeLogic _hR_EmployeeLogic;

        public HR_CustomerCardService(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _Cache = serviceProvider.GetService<IMemoryCache>();
            _config = ConfigObject.GetConfig(_Cache);
            _iC_CommandLogic = serviceProvider.GetService<IIC_CommandLogic>();
            _iIC_AuditLogic = serviceProvider.GetService<IIC_AuditLogic>();
            _hR_EmployeeLogic = serviceProvider.GetService<IHR_EmployeeLogic>();
        }

        public async Task<List<CustomerCardModel>> GetAllCustomerCard(UserInfo user)
        {
            var listCustomerCard = await DbContext.HR_CustomerCard.AsNoTracking().Where(x => x.CompanyIndex == user.CompanyIndex).ToListAsync();
            if (listCustomerCard.Count > 0)
            {
                var listCardNumber = listCustomerCard.Select(x => x.CardNumber).ToList();

                var listCustomerUsingCard = await DbContext.HR_CardNumberInfo.AsNoTracking().Where(x => x.CompanyIndex == user.CompanyIndex
                    && listCardNumber.Contains(x.CardNumber) && x.IsActive == true)
                    .Join(DbContext.HR_User.AsNoTracking().Where(x => x.CompanyIndex == user.CompanyIndex
                        && x.EmployeeType == (short)EmployeeType.Guest),
                    a => a.EmployeeATID, b => b.EmployeeATID, (a, b) => new { a, b })
                    .Join(DbContext.HR_CustomerInfo.AsNoTracking().Where(x => x.CompanyIndex == user.CompanyIndex
                    && x.FromTime.Date <= DateTime.Now.Date
                        && x.ToTime.Date >= DateTime.Now.Date), ab => ab.b.EmployeeATID, c => c.EmployeeATID, (ab, c)
                        => new CustomerCardModel
                        {
                            CardNumber = ab.a.CardNumber,
                            UserName = ab.b.FullName,
                            UserCode = c.EmployeeATID,
                            UpdatedDate = ab.a.UpdatedDate
                        }).ToListAsync();

                //var listDriverUsingCard = await DbContext.GC_TruckDriverLog.AsNoTracking().Where(x => x.CompanyIndex == user.CompanyIndex
                //    && listCardNumber.Contains(x.CardNumber)).ToListAsync();
                //if (listDriverUsingCard.Count > 0)
                //{
                //    listDriverUsingCard = listDriverUsingCard.Where(x => x.InOutMode == (short)InOutMode.Input
                //        && !x.IsInactive
                //        && !listDriverUsingCard.Any(y => y.TripCode == x.TripCode
                //        && y.InOutMode == (short)InOutMode.Output)).ToList();
                //}

                var listDriverUsingCard = await DbContext.GC_TruckDriverLog.AsNoTracking().Where(x => x.CompanyIndex == user.CompanyIndex
                    && listCardNumber.Contains(x.CardNumber) && !x.IsInactive)
                    .Join(DbContext.IC_PlanDock.AsNoTracking().Where(x => x.CompanyIndex == user.CompanyIndex),
                    a => a.TripCode, b => b.TripId, (a, b)
                    => new CustomerCardModel
                    {
                        CardNumber = a.CardNumber,
                        UserName = b.DriverName,
                        UserCode = b.DriverCode,
                        TripCode = a.TripCode,
                        UpdatedDate = a.UpdatedDate,
                    }).ToListAsync();

                var listExtraDriverUsingCard = await DbContext.GC_TruckExtraDriverLog.AsNoTracking().Where(x 
                    => listCardNumber.Contains(x.CardNumber) && !x.IsInactive)
                    .Join(DbContext.GC_TruckDriverLog.AsNoTracking().Where(x => x.CompanyIndex == user.CompanyIndex),
                    a => a.TripCode, b => b.TripCode, (a, b)
                    => new CustomerCardModel
                    {
                        CardNumber = a.CardNumber,
                        UserName = a.ExtraDriverName,
                        UserCode = a.ExtraDriverCode,
                        TripCode = a.TripCode,
                        UpdatedDate = b.UpdatedDate,
                    }).ToListAsync();

                var result = listCustomerCard.Select(x => new CustomerCardModel
                {
                    Index = x.Index,
                    CardNumber = x.CardNumber,
                    CardID = x.CardID,
                    UserName = listCustomerUsingCard.Any(y => y.CardNumber == x.CardNumber)
                        ? listCustomerUsingCard.FirstOrDefault(y => y.CardNumber == x.CardNumber).UserName
                        : (listDriverUsingCard.Any(y => y.CardNumber == x.CardNumber)
                        ? listDriverUsingCard.FirstOrDefault(y => y.CardNumber == x.CardNumber).UserName
                        : (listExtraDriverUsingCard.Any(y => y.CardNumber == x.CardNumber)
                        ? listExtraDriverUsingCard.FirstOrDefault(y => y.CardNumber == x.CardNumber).UserName
                        : string.Empty)),
                    UserCode = listCustomerUsingCard.Any(y => y.CardNumber == x.CardNumber)
                        ? listCustomerUsingCard.FirstOrDefault(y => y.CardNumber == x.CardNumber).UserCode
                        : (listDriverUsingCard.Any(y => y.CardNumber == x.CardNumber)
                        ? listDriverUsingCard.FirstOrDefault(y => y.CardNumber == x.CardNumber).UserCode
                        : (listExtraDriverUsingCard.Any(y => y.CardNumber == x.CardNumber)
                        ? listExtraDriverUsingCard.FirstOrDefault(y => y.CardNumber == x.CardNumber).UserCode
                        : string.Empty)),
                    Object = listCustomerUsingCard.Any(y => y.CardNumber == x.CardNumber)
                        ? EmployeeType.Guest.ToString()
                        : (listDriverUsingCard.Any(y => y.CardNumber == x.CardNumber)
                        ? EmployeeType.Driver.ToString()
                        : (listExtraDriverUsingCard.Any(y => y.CardNumber == x.CardNumber)
                        ? "Extra" + EmployeeType.Driver.ToString()
                        : string.Empty)),
                    CreatedDate = x.CreatedDate,
                    CreatedDateString = x.CreatedDate.ToddMMyyyyHHmmssMinus(),
                    UpdatedDate = x.UpdatedDate,
                    UpdatedDateString = x.UpdatedDate.HasValue ? x.UpdatedDate.Value.ToddMMyyyyHHmmssMinus() : string.Empty,
                    CardUpdatedDate = listCustomerUsingCard.Any(y => y.CardNumber == x.CardNumber)
                        ? listCustomerUsingCard.FirstOrDefault(y => y.CardNumber == x.CardNumber).UpdatedDate
                        : (listDriverUsingCard.Any(y => y.CardNumber == x.CardNumber)
                        ? listDriverUsingCard.FirstOrDefault(y => y.CardNumber == x.CardNumber).UpdatedDate
                        : (listExtraDriverUsingCard.Any(y => y.CardNumber == x.CardNumber)
                        ? listExtraDriverUsingCard.FirstOrDefault(y => y.CardNumber == x.CardNumber).UpdatedDate
                        : null)),
                }).ToList();
                result.ForEach(x =>
                {
                    x.Status = !string.IsNullOrEmpty(x.UserCode);
                    x.TripCode = (x.Object == EmployeeType.Driver.ToString() || x.Object == "Extra" + EmployeeType.Driver.ToString()) 
                    ? (listDriverUsingCard.Any(y => y.CardNumber == x.CardNumber)
                        ? listDriverUsingCard.FirstOrDefault(y => y.CardNumber == x.CardNumber).TripCode
                        : (listExtraDriverUsingCard.Any(y => y.CardNumber == x.CardNumber)
                        ? listExtraDriverUsingCard.FirstOrDefault(y => y.CardNumber == x.CardNumber).TripCode
                        : string.Empty)) : string.Empty;
                });
                return result;
            }
            return new List<CustomerCardModel>();
        }

        public async Task<CustomerCardModel> GetCustomerCardById(string id, UserInfo user)
        {
            var customerCard = await DbContext.HR_CustomerCard.AsNoTracking().FirstOrDefaultAsync(x
                => x.CompanyIndex == user.CompanyIndex && x.CardID == id);
            if (customerCard != null)
            {
                var cardNumber = customerCard.CardNumber;

                var listCustomerUsingCard = await DbContext.HR_CardNumberInfo.AsNoTracking().Where(x => x.CompanyIndex == user.CompanyIndex
                    && cardNumber == x.CardNumber && x.IsActive == true)
                    .Join(DbContext.HR_User.AsNoTracking().Where(x => x.CompanyIndex == user.CompanyIndex
                        && x.EmployeeType == (short)EmployeeType.Guest),
                    a => a.EmployeeATID, b => b.EmployeeATID, (a, b) => new { a, b })
                    .Join(DbContext.HR_CustomerInfo.AsNoTracking().Where(x => x.CompanyIndex == user.CompanyIndex
                    //&& x.FromTime.Date <= DateTime.Now.Date
                    //    && x.ToTime.Date >= DateTime.Now.Date
                        ), ab => ab.b.EmployeeATID, c => c.EmployeeATID, (ab, c)
                        => new CustomerCardModel
                        {
                            CardNumber = ab.a.CardNumber,
                            UserName = ab.b.FullName,
                            UserCode = c.EmployeeATID,
                            UpdatedDate = ab.a.UpdatedDate,
                            IsActive = ab.a.IsActive ?? false,
                            CardUserIndex = ab.a.Index
                        }).ToListAsync();

                //var listDriverUsingCard = await DbContext.GC_TruckDriverLog.AsNoTracking().Where(x => x.CompanyIndex == user.CompanyIndex
                //    && cardNumber == x.CardNumber).ToListAsync();
                //if (listDriverUsingCard.Count > 0)
                //{
                //    listDriverUsingCard = listDriverUsingCard.Where(x => x.InOutMode == (short)InOutMode.Input
                //        && !x.IsInactive
                //        && !listDriverUsingCard.Any(y => y.TripCode == x.TripCode
                //        && y.InOutMode == (short)InOutMode.Output)).ToList();
                //}

                var listDriverUsingCard = await DbContext.GC_TruckDriverLog.AsNoTracking().Where(x => x.CompanyIndex == user.CompanyIndex
                    && cardNumber == x.CardNumber && !x.IsInactive)
                    .Join(DbContext.IC_PlanDock.AsNoTracking().Where(x => x.CompanyIndex == user.CompanyIndex),
                    a => a.TripCode, b => b.TripId, (a, b)
                    => new CustomerCardModel
                    {
                        CardNumber = a.CardNumber,
                        UserName = b.DriverName,
                        UserCode = b.DriverCode,
                        TripCode = a.TripCode,
                        UpdatedDate = a.UpdatedDate,
                        IsActive = !a.IsInactive,
                        CardUserIndex = b.Index
                    }).ToListAsync();

                //var listExtraDriverUsingCard = await DbContext.GC_TruckExtraDriverLog.AsNoTracking().Where(x
                //   => cardNumber == x.CardNumber && !x.IsInactive)
                //   .Join(DbContext.GC_TruckDriverLog.AsNoTracking().Where(x => x.CompanyIndex == user.CompanyIndex),
                //   a => a.TripCode, b => b.TripCode, (a, b)
                //   => new CustomerCardModel
                //   {
                //       CardNumber = a.CardNumber,
                //       UserName = a.ExtraDriverName,
                //       UserCode = a.ExtraDriverCode,
                //       TripCode = a.TripCode,
                //       UpdatedDate = b.UpdatedDate,
                //       CardUserIndex = a.Index
                //   }).ToListAsync();

                var listExtraDriverUsingCard = await DbContext.GC_TruckExtraDriverLog.AsNoTracking().Where(x
                   => cardNumber == x.CardNumber && !x.IsInactive)
                   .Join(DbContext.GC_TruckDriverLog.AsNoTracking().Where(x => x.CompanyIndex == user.CompanyIndex),
                   a => a.TripCode, b => b.TripCode, (a, b) => new { a, b })
                   .Join(DbContext.IC_PlanDock.AsNoTracking().Where(x => x.CompanyIndex == user.CompanyIndex
                        ), ab => ab.b.TripCode, c => c.TripId, (ab, c)
                        => new CustomerCardModel
                        {
                            CardNumber = ab.a.CardNumber,
                            UserName = ab.a.ExtraDriverName,
                            UserCode = ab.a.ExtraDriverCode,
                            TripCode = ab.a.TripCode,
                            UpdatedDate = ab.b.UpdatedDate,
                            CardUserIndex = c.Index
                        }).ToListAsync();

                var result = new CustomerCardModel();
                result.Index = customerCard.Index;
                result.CardNumber = customerCard.CardNumber;
                result.CardID = customerCard.CardID;
                result.UserCode = listCustomerUsingCard.Any(y => y.CardNumber == customerCard.CardNumber)
                        ? listCustomerUsingCard.FirstOrDefault(y => y.CardNumber == customerCard.CardNumber).UserCode
                        : (listDriverUsingCard.Any(y => y.CardNumber == customerCard.CardNumber)
                        ? listDriverUsingCard.FirstOrDefault(y => y.CardNumber == customerCard.CardNumber).UserCode
                        : (listExtraDriverUsingCard.Any(y => y.CardNumber == customerCard.CardNumber)
                        ? listExtraDriverUsingCard.FirstOrDefault(y => y.CardNumber == customerCard.CardNumber).UserCode
                        : string.Empty));
                result.UserName = listCustomerUsingCard.Any(y => y.CardNumber == customerCard.CardNumber)
                        ? listCustomerUsingCard.FirstOrDefault(y => y.CardNumber == customerCard.CardNumber).UserName
                        : (listDriverUsingCard.Any(y => y.CardNumber == customerCard.CardNumber)
                        ? listDriverUsingCard.FirstOrDefault(y => y.CardNumber == customerCard.CardNumber).UserName
                        : (listExtraDriverUsingCard.Any(y => y.CardNumber == customerCard.CardNumber)
                        ? listExtraDriverUsingCard.FirstOrDefault(y => y.CardNumber == customerCard.CardNumber).UserName
                        : string.Empty));
                result.Object = listCustomerUsingCard.Any(y => y.CardNumber == customerCard.CardNumber)
                        ? EmployeeType.Guest.ToString()
                        : (listDriverUsingCard.Any(y => y.CardNumber == customerCard.CardNumber)
                        ? EmployeeType.Driver.ToString()
                        : (listExtraDriverUsingCard.Any(y => y.CardNumber == customerCard.CardNumber)
                        ? "Extra" + EmployeeType.Driver.ToString()
                        : string.Empty));
                result.CardUserIndex = listCustomerUsingCard.Any(y => y.CardNumber == customerCard.CardNumber)
                        ? listCustomerUsingCard.FirstOrDefault(y => y.CardNumber == customerCard.CardNumber).CardUserIndex
                        : (listDriverUsingCard.Any(y => y.CardNumber == customerCard.CardNumber)
                        ? listDriverUsingCard.FirstOrDefault(y => y.CardNumber == customerCard.CardNumber).CardUserIndex
                        : (listExtraDriverUsingCard.Any(y => y.CardNumber == customerCard.CardNumber)
                        ? listExtraDriverUsingCard.FirstOrDefault(y => y.CardNumber == customerCard.CardNumber).CardUserIndex
                        : 0));
                result.UpdatedDate = customerCard.UpdatedDate;
                result.CardUpdatedDate = listCustomerUsingCard.Any(y => y.CardNumber == customerCard.CardNumber)
                        ? listCustomerUsingCard.FirstOrDefault(y => y.CardNumber == customerCard.CardNumber).UpdatedDate
                        : (listDriverUsingCard.Any(y => y.CardNumber == customerCard.CardNumber)
                        ? listDriverUsingCard.FirstOrDefault(y => y.CardNumber == customerCard.CardNumber).UpdatedDate
                        : (listExtraDriverUsingCard.Any(y => y.CardNumber == customerCard.CardNumber)
                        ? listExtraDriverUsingCard.FirstOrDefault(y => y.CardNumber == customerCard.CardNumber).UpdatedDate
                        : null));
                //result.Status = !string.IsNullOrEmpty(result.UserCode);
                result.Status = result.CardUserIndex > 0;
                result.TripCode = (result.Object == EmployeeType.Driver.ToString() || result.Object == "Extra" + EmployeeType.Driver.ToString()) 
                    ? (listDriverUsingCard.Any(y => y.CardNumber == result.CardNumber)
                        ? listDriverUsingCard.FirstOrDefault(y => y.CardNumber == result.CardNumber).TripCode
                        : (listExtraDriverUsingCard.Any(y => y.CardNumber == customerCard.CardNumber)
                        ? listExtraDriverUsingCard.FirstOrDefault(y => y.CardNumber == customerCard.CardNumber).TripCode
                        : string.Empty)) : string.Empty;

                //if (!string.IsNullOrWhiteSpace(result.UserCode))
                //{
                //    if (result.Object == EmployeeType.Guest.ToString())
                //    {
                //        var userData = await DbContext.HR_User.AsNoTracking().FirstOrDefaultAsync(x
                //            => x.EmployeeATID == result.UserCode);
                //        if (userData != null)
                //        {
                //            result.UserName = userData.FullName;
                //        }
                //    }
                //    else if (result.Object == EmployeeType.Driver.ToString())
                //    {
                //        var userData = await DbContext.IC_PlanDock.AsNoTracking().FirstOrDefaultAsync(x
                //            => x.TripId == result.TripCode);
                //        if (userData != null)
                //        {
                //            result.UserName = userData.DriverName;
                //        }
                //    }
                //}

                return result;
            }
            return null;
        }

        public async Task<CustomerCardModel> GetCardNumberByNumber(string number, UserInfo user)
        {
            number = number.TrimStart('0');
            var customerCard = await DbContext.HR_CustomerCard.AsNoTracking().FirstOrDefaultAsync(x
                => x.CompanyIndex == user.CompanyIndex && x.CardNumber == number);
            if (customerCard != null)
            {
                var cardNumber = customerCard.CardNumber;

                var listCustomerUsingCard = await DbContext.HR_CardNumberInfo.AsNoTracking().Where(x => x.CompanyIndex == user.CompanyIndex
                    && cardNumber == x.CardNumber && x.IsActive == true)
                    .Join(DbContext.HR_User.AsNoTracking().Where(x => x.CompanyIndex == user.CompanyIndex
                        && x.EmployeeType == (short)EmployeeType.Guest),
                    a => a.EmployeeATID, b => b.EmployeeATID, (a, b) => new { a, b })
                    .Join(DbContext.HR_CustomerInfo.AsNoTracking().Where(x => x.CompanyIndex == user.CompanyIndex
                    //&& x.FromTime.Date <= DateTime.Now.Date
                    //    && x.ToTime.Date >= DateTime.Now.Date
                        ), ab => ab.b.EmployeeATID, c => c.EmployeeATID, (ab, c)
                        => new CustomerCardModel
                        {
                            CardNumber = ab.a.CardNumber,
                            UserName = ab.b.FullName,
                            UserCode = c.EmployeeATID,
                            UpdatedDate = ab.a.UpdatedDate,
                            IsActive = ab.a.IsActive ?? false,
                            CardUserIndex = ab.a.Index
                        }).ToListAsync();

                //var listDriverUsingCard = await DbContext.GC_TruckDriverLog.AsNoTracking().Where(x => x.CompanyIndex == user.CompanyIndex
                //    && cardNumber == x.CardNumber).ToListAsync();
                //if (listDriverUsingCard.Count > 0)
                //{
                //    listDriverUsingCard = listDriverUsingCard.Where(x => x.InOutMode == (short)InOutMode.Input
                //        && !x.IsInactive
                //        && !listDriverUsingCard.Any(y => y.TripCode == x.TripCode
                //        && y.InOutMode == (short)InOutMode.Output)).ToList();
                //}

                var listDriverUsingCard = await DbContext.GC_TruckDriverLog.AsNoTracking().Where(x => x.CompanyIndex == user.CompanyIndex
                    && cardNumber == x.CardNumber && !x.IsInactive)
                    .Join(DbContext.IC_PlanDock.AsNoTracking().Where(x => x.CompanyIndex == user.CompanyIndex),
                    a => a.TripCode, b => b.TripId, (a, b)
                    => new CustomerCardModel
                    {
                        CardNumber = a.CardNumber,
                        UserName = b.DriverName,
                        UserCode = b.DriverCode,
                        TripCode = a.TripCode,
                        UpdatedDate = a.UpdatedDate,
                        IsActive = !a.IsInactive,
                        CardUserIndex = b.Index
                    }).ToListAsync();

                //var listExtraDriverUsingCard = await DbContext.GC_TruckExtraDriverLog.AsNoTracking().Where(x
                //   => cardNumber == x.CardNumber && !x.IsInactive)
                //   .Join(DbContext.GC_TruckDriverLog.AsNoTracking().Where(x => x.CompanyIndex == user.CompanyIndex),
                //   a => a.TripCode, b => b.TripCode, (a, b)
                //   => new CustomerCardModel
                //   {
                //       CardNumber = a.CardNumber,
                //       UserName = a.ExtraDriverName,
                //       UserCode = a.ExtraDriverCode,
                //       TripCode = a.TripCode,
                //       UpdatedDate = b.UpdatedDate,
                //       CardUserIndex = a.Index
                //   }).ToListAsync();

                var listExtraDriverUsingCard = await DbContext.GC_TruckExtraDriverLog.AsNoTracking().Where(x
                   => cardNumber == x.CardNumber && !x.IsInactive)
                   .Join(DbContext.GC_TruckDriverLog.AsNoTracking().Where(x => x.CompanyIndex == user.CompanyIndex),
                   a => a.TripCode, b => b.TripCode, (a, b) => new { a, b })
                   .Join(DbContext.IC_PlanDock.AsNoTracking().Where(x => x.CompanyIndex == user.CompanyIndex
                        ), ab => ab.b.TripCode, c => c.TripId, (ab, c)
                        => new CustomerCardModel
                        {
                            CardNumber = ab.a.CardNumber,
                            UserName = ab.a.ExtraDriverName,
                            UserCode = ab.a.ExtraDriverCode,
                            TripCode = ab.a.TripCode,
                            UpdatedDate = ab.b.UpdatedDate,
                            CardUserIndex = c.Index
                        }).ToListAsync();

                var result = new CustomerCardModel();
                result.Index = customerCard.Index;
                result.CardNumber = customerCard.CardNumber;
                result.CardID = customerCard.CardID;
                result.UserCode = listCustomerUsingCard.Any(y => y.CardNumber == customerCard.CardNumber)
                        ? listCustomerUsingCard.FirstOrDefault(y => y.CardNumber == customerCard.CardNumber).UserCode
                        : (listDriverUsingCard.Any(y => y.CardNumber == customerCard.CardNumber)
                        ? listDriverUsingCard.FirstOrDefault(y => y.CardNumber == customerCard.CardNumber).UserCode
                        : (listExtraDriverUsingCard.Any(y => y.CardNumber == customerCard.CardNumber)
                        ? listExtraDriverUsingCard.FirstOrDefault(y => y.CardNumber == customerCard.CardNumber).UserCode
                        : string.Empty));
                result.UserName = listCustomerUsingCard.Any(y => y.CardNumber == customerCard.CardNumber)
                        ? listCustomerUsingCard.FirstOrDefault(y => y.CardNumber == customerCard.CardNumber).UserName
                        : (listDriverUsingCard.Any(y => y.CardNumber == customerCard.CardNumber)
                        ? listDriverUsingCard.FirstOrDefault(y => y.CardNumber == customerCard.CardNumber).UserName
                        : (listExtraDriverUsingCard.Any(y => y.CardNumber == customerCard.CardNumber)
                        ? listExtraDriverUsingCard.FirstOrDefault(y => y.CardNumber == customerCard.CardNumber).UserName
                        : string.Empty));
                result.CardUserIndex = listCustomerUsingCard.Any(y => y.CardNumber == customerCard.CardNumber)
                        ? listCustomerUsingCard.FirstOrDefault(y => y.CardNumber == customerCard.CardNumber).CardUserIndex
                        : (listDriverUsingCard.Any(y => y.CardNumber == customerCard.CardNumber)
                        ? listDriverUsingCard.FirstOrDefault(y => y.CardNumber == customerCard.CardNumber).CardUserIndex
                        : (listExtraDriverUsingCard.Any(y => y.CardNumber == customerCard.CardNumber)
                        ? listExtraDriverUsingCard.FirstOrDefault(y => y.CardNumber == customerCard.CardNumber).CardUserIndex
                        : 0));
                result.Object = listCustomerUsingCard.Any(y => y.CardNumber == customerCard.CardNumber)
                        ? EmployeeType.Guest.ToString()
                        : (listDriverUsingCard.Any(y => y.CardNumber == customerCard.CardNumber)
                        ? EmployeeType.Driver.ToString()
                        : (listExtraDriverUsingCard.Any(y => y.CardNumber == customerCard.CardNumber)
                        ? "Extra" + EmployeeType.Driver.ToString()
                        : string.Empty));
                result.UpdatedDate = customerCard.UpdatedDate;
                result.CardUpdatedDate = listCustomerUsingCard.Any(y => y.CardNumber == customerCard.CardNumber)
                        ? listCustomerUsingCard.FirstOrDefault(y => y.CardNumber == customerCard.CardNumber).UpdatedDate
                        : (listDriverUsingCard.Any(y => y.CardNumber == customerCard.CardNumber)
                        ? listDriverUsingCard.FirstOrDefault(y => y.CardNumber == customerCard.CardNumber).UpdatedDate
                        : (listExtraDriverUsingCard.Any(y => y.CardNumber == customerCard.CardNumber)
                        ? listExtraDriverUsingCard.FirstOrDefault(y => y.CardNumber == customerCard.CardNumber).UpdatedDate
                        : null));
                //result.Status = !string.IsNullOrEmpty(result.UserCode);
                result.Status = result.CardUserIndex > 0;
                result.TripCode = (result.Object == EmployeeType.Driver.ToString() || result.Object == "Extra" + EmployeeType.Driver.ToString())
                    ? (listDriverUsingCard.Any(y => y.CardNumber == result.CardNumber)
                        ? listDriverUsingCard.FirstOrDefault(y => y.CardNumber == result.CardNumber).TripCode
                        : (listExtraDriverUsingCard.Any(y => y.CardNumber == customerCard.CardNumber)
                        ? listExtraDriverUsingCard.FirstOrDefault(y => y.CardNumber == customerCard.CardNumber).TripCode
                        : string.Empty)) : string.Empty;

                //if (!string.IsNullOrWhiteSpace(result.UserCode))
                //{
                //    if (result.Object == EmployeeType.Guest.ToString())
                //    {
                //        var userData = await DbContext.HR_User.AsNoTracking().FirstOrDefaultAsync(x
                //            => x.EmployeeATID == result.UserCode);
                //        if (userData != null)
                //        {
                //            result.UserName = userData.FullName;
                //        }
                //    }
                //    else if (result.Object == EmployeeType.Driver.ToString())
                //    {
                //        var userData = await DbContext.IC_PlanDock.AsNoTracking().FirstOrDefaultAsync(x
                //            => x.TripId == result.TripCode);
                //        if (userData != null)
                //        {
                //            result.UserName = userData.DriverName;
                //        }
                //    }
                //}

                return result;
            }
            return null;
        }

        public async Task<DataGridClass> GetPage(List<AddedParam> addedParams, UserInfo user)
        {
            if (addedParams == null || addedParams.Count == 0)
                return null;

            var pageIndex = addedParams.FirstOrDefault(e => e.Key == "PageIndex");
            var pageSize = addedParams.FirstOrDefault(e => e.Key == "PageSize");
            var pPage = Convert.ToInt32(pageIndex?.Value ?? 1);
            var pLimit = Convert.ToInt32(pageSize?.Value ?? GlobalParams.ROWS_NUMBER_IN_PAGE);

            var result = new List<CustomerCardModel>();

            var listCustomerCard = await DbContext.HR_CustomerCard.AsNoTracking().Where(x => x.CompanyIndex == user.CompanyIndex).ToListAsync();
            if (listCustomerCard.Count > 0)
            {
                var listCardNumber = listCustomerCard.Select(x => x.CardNumber).ToList();

                var listCustomerUsingCard = await DbContext.HR_CardNumberInfo.AsNoTracking().Where(x => x.CompanyIndex == user.CompanyIndex
                    && listCardNumber.Contains(x.CardNumber) && x.IsActive == true)
                    .Join(DbContext.HR_User.AsNoTracking().Where(x => x.CompanyIndex == user.CompanyIndex
                        && x.EmployeeType == (short)EmployeeType.Guest),
                    a => a.EmployeeATID, b => b.EmployeeATID, (a, b) => new { a, b })
                    .Join(DbContext.HR_CustomerInfo.AsNoTracking().Where(x => x.CompanyIndex == user.CompanyIndex), ab => ab.b.EmployeeATID, c => c.EmployeeATID, (ab, c)
                        => new CustomerCardModel
                        {
                            CardNumber = ab.a.CardNumber,
                            UserName = ab.b.FullName,
                            UserCode = c.EmployeeATID,
                            UpdatedDate = ab.a.UpdatedDate,
                            CardUserIndex = ab.a.Index, 
                        }).ToListAsync();

                //var listDriverUsingCard = await DbContext.GC_TruckDriverLog.AsNoTracking().Where(x => x.CompanyIndex == user.CompanyIndex
                //    && listCardNumber.Contains(x.CardNumber)).ToListAsync();
                //if (listDriverUsingCard.Count > 0)
                //{
                //    listDriverUsingCard = listDriverUsingCard.Where(x => x.InOutMode == (short)InOutMode.Input
                //        && !x.IsInactive
                //        && !listDriverUsingCard.Any(y => y.TripCode == x.TripCode
                //        && y.InOutMode == (short)InOutMode.Output)).ToList();
                //}

                var listDriverUsingCard = await DbContext.GC_TruckDriverLog.AsNoTracking().Where(x => x.CompanyIndex == user.CompanyIndex
                    && listCardNumber.Contains(x.CardNumber) && !x.IsInactive)
                    .Join(DbContext.IC_PlanDock.AsNoTracking().Where(x => x.CompanyIndex == user.CompanyIndex),
                    a => a.TripCode, b => b.TripId, (a, b)
                    => new CustomerCardModel
                    {
                        CardNumber = a.CardNumber,
                        UserName = b.DriverName,
                        UserCode = b.DriverCode,
                        TripCode = a.TripCode,
                        UpdatedDate = a.UpdatedDate,
                        CardUserIndex = b.Index
                    }).ToListAsync();

                //var listExtraDriverUsingCard = await DbContext.GC_TruckExtraDriverLog.AsNoTracking().Where(x
                //   => listCardNumber.Contains(x.CardNumber) && !x.IsInactive)
                //   .Join(DbContext.GC_TruckDriverLog.AsNoTracking().Where(x => x.CompanyIndex == user.CompanyIndex),
                //   a => a.TripCode, b => b.TripCode, (a, b)
                //   => new CustomerCardModel
                //   {
                //       CardNumber = a.CardNumber,
                //       UserName = a.ExtraDriverName,
                //       UserCode = a.ExtraDriverCode,
                //       TripCode = a.TripCode,
                //       UpdatedDate = b.UpdatedDate,
                //       CardUserIndex = a.Index
                //   }).ToListAsync();

                var listExtraDriverUsingCard = await DbContext.GC_TruckExtraDriverLog.AsNoTracking().Where(x
                   => listCardNumber.Contains(x.CardNumber) && !x.IsInactive)
                   .Join(DbContext.GC_TruckDriverLog.AsNoTracking().Where(x => x.CompanyIndex == user.CompanyIndex),
                   a => a.TripCode, b => b.TripCode, (a, b) => new { a, b })
                   .Join(DbContext.IC_PlanDock.AsNoTracking().Where(x => x.CompanyIndex == user.CompanyIndex
                        ), ab => ab.b.TripCode, c => c.TripId, (ab, c)
                        => new CustomerCardModel
                        {
                            CardNumber = ab.a.CardNumber,
                            UserName = ab.a.ExtraDriverName,
                            UserCode = ab.a.ExtraDriverCode,
                            TripCode = ab.a.TripCode,
                            UpdatedDate = ab.b.UpdatedDate,
                            CardUserIndex = c.Index
                        }).ToListAsync();

                result = listCustomerCard.Select(x => new CustomerCardModel
                {
                    Index = x.Index,
                    CardNumber = x.CardNumber,
                    CardID = x.CardID,
                    UserName = listCustomerUsingCard.Any(y => y.CardNumber == x.CardNumber)
                        ? listCustomerUsingCard.FirstOrDefault(y => y.CardNumber == x.CardNumber).UserName
                        : (listDriverUsingCard.Any(y => y.CardNumber == x.CardNumber)
                        ? listDriverUsingCard.FirstOrDefault(y => y.CardNumber == x.CardNumber).UserName
                        : (listExtraDriverUsingCard.Any(y => y.CardNumber == x.CardNumber)
                        ? listExtraDriverUsingCard.FirstOrDefault(y => y.CardNumber == x.CardNumber).UserName
                        : string.Empty)),
                    UserCode = listCustomerUsingCard.Any(y => y.CardNumber == x.CardNumber)
                        ? listCustomerUsingCard.FirstOrDefault(y => y.CardNumber == x.CardNumber).UserCode
                        : (listDriverUsingCard.Any(y => y.CardNumber == x.CardNumber)
                        ? listDriverUsingCard.FirstOrDefault(y => y.CardNumber == x.CardNumber).UserCode
                        : (listExtraDriverUsingCard.Any(y => y.CardNumber == x.CardNumber)
                        ? listExtraDriverUsingCard.FirstOrDefault(y => y.CardNumber == x.CardNumber).UserCode
                        : string.Empty)),
                    CardUserIndex = listCustomerUsingCard.Any(y => y.CardNumber == x.CardNumber)
                        ? listCustomerUsingCard.FirstOrDefault(y => y.CardNumber == x.CardNumber).CardUserIndex
                        : (listDriverUsingCard.Any(y => y.CardNumber == x.CardNumber)
                        ? listDriverUsingCard.FirstOrDefault(y => y.CardNumber == x.CardNumber).CardUserIndex
                        : (listExtraDriverUsingCard.Any(y => y.CardNumber == x.CardNumber)
                        ? listExtraDriverUsingCard.FirstOrDefault(y => y.CardNumber == x.CardNumber).CardUserIndex
                        : 0)),
                    Object = listCustomerUsingCard.Any(y => y.CardNumber == x.CardNumber)
                        ? EmployeeType.Guest.ToString()
                        : (listDriverUsingCard.Any(y => y.CardNumber == x.CardNumber)
                        ? EmployeeType.Driver.ToString()
                        : (listExtraDriverUsingCard.Any(y => y.CardNumber == x.CardNumber)
                        ? "Extra" + EmployeeType.Driver.ToString()
                        : string.Empty)),
                    CreatedDate = x.CreatedDate,
                    CreatedDateString = x.CreatedDate.ToddMMyyyyHHmmssMinus(),
                    UpdatedDate = x.UpdatedDate,
                    UpdatedDateString = x.UpdatedDate.HasValue ? x.UpdatedDate.Value.ToddMMyyyyHHmmssMinus() : string.Empty,
                    CardUpdatedDate = listCustomerUsingCard.Any(y => y.CardNumber == x.CardNumber)
                        ? listCustomerUsingCard.FirstOrDefault(y => y.CardNumber == x.CardNumber).UpdatedDate
                        : (listDriverUsingCard.Any(y => y.CardNumber == x.CardNumber)
                        ? listDriverUsingCard.FirstOrDefault(y => y.CardNumber == x.CardNumber).UpdatedDate
                        : (listExtraDriverUsingCard.Any(y => y.CardNumber == x.CardNumber)
                        ? listExtraDriverUsingCard.FirstOrDefault(y => y.CardNumber == x.CardNumber).UpdatedDate
                        : null)),
                }).ToList();
                result.ForEach(x =>
                {
                    //x.Status = !string.IsNullOrEmpty(x.UserCode);
                    x.Status = x.CardUserIndex > 0;
                    x.TripCode = (x.Object == EmployeeType.Driver.ToString() || x.Object == "Extra" + EmployeeType.Driver.ToString()) 
                    ? (listDriverUsingCard.Any(y => y.CardNumber == x.CardNumber)
                        ? listDriverUsingCard.FirstOrDefault(y => y.CardNumber == x.CardNumber).TripCode
                        : (listExtraDriverUsingCard.Any(y => y.CardNumber == x.CardNumber)
                        ? listExtraDriverUsingCard.FirstOrDefault(y => y.CardNumber == x.CardNumber).TripCode
                        : string.Empty)) : string.Empty;
                });
            }

            foreach (AddedParam param in addedParams)
            {
                switch (param.Key)
                {
                    case "Filter":
                        if (param.Value != null && !string.IsNullOrWhiteSpace(param.Value.ToString()))
                        {
                            string filter = param.Value.ToString();
                            var filterList = new List<string>();
                            if (filter.Contains(" "))
                            {
                                filterList = filter.Split(" ").ToList();
                            }
                            result = result.Where(u => u.CardNumber.Contains(filter) || u.CardID.Contains(filter)
                            || filterList.Contains(u.CardNumber) || filterList.Contains(u.CardID)
                            || (!string.IsNullOrEmpty(u.UserCode)
                            && (u.UserCode.Contains(filter) || filterList.Contains(u.UserCode)))
                            || (!string.IsNullOrEmpty(u.UserName)
                            && (u.UserName.Contains(filter) || filterList.Contains(u.UserName)))).ToList();
                        }
                        break;
                }
            }

            var count = result.Count;

            if (pPage < 1) pPage = 1;
            result = result.Skip((pPage - 1) * pLimit).Take(pLimit).ToList();

            var rs = new DataGridClass(count, result);
            return await Task.FromResult(rs);
        }

        public async Task<bool> IsCardNumberExisted(string cardNumber, UserInfo user)
        {
            return await DbContext.HR_CustomerCard.AsNoTracking().AnyAsync(x
                => x.CardNumber == cardNumber && x.CompanyIndex == user.CompanyIndex);
        }

        public async Task<bool> IsCardNumberUsing(string cardNumber, UserInfo user)
        {
            var listCustomerUsingCard = await DbContext.HR_CardNumberInfo.AsNoTracking().Where(x => x.CompanyIndex == user.CompanyIndex
                    && cardNumber == x.CardNumber && x.IsActive == true)
                    .Join(DbContext.HR_User.AsNoTracking().Where(x => x.CompanyIndex == user.CompanyIndex
                        && x.EmployeeType == (short)EmployeeType.Guest),
                    a => a.EmployeeATID, b => b.EmployeeATID, (a, b) => new { a, b })
                    .Join(DbContext.HR_CustomerInfo.AsNoTracking().Where(x => x.CompanyIndex == user.CompanyIndex), ab => ab.b.EmployeeATID, c => c.EmployeeATID, (ab, c)
                        => new CustomerCardModel
                        {
                            CardNumber = ab.a.CardNumber,
                            UserCode = c.EmployeeATID,
                            UpdatedDate = ab.a.UpdatedDate
                        }).ToListAsync();

            var listDriverUsingCard = await DbContext.GC_TruckDriverLog.AsNoTracking().Where(x => x.CompanyIndex == user.CompanyIndex
                && !x.IsInactive
                && cardNumber == x.CardNumber).ToListAsync();

            if (listCustomerUsingCard.Count > 0 || listDriverUsingCard.Any(x => x.InOutMode == (short)InOutMode.Input
                    && !listDriverUsingCard.Any(y => y.TripCode == x.TripCode
                    && y.InOutMode == (short)InOutMode.Output)))
            {
                return true;
            }
            return false;
        }

        public async Task<bool> IsCardNumberUsing(List<string> cardNumber, UserInfo user)
        {
            var listCustomerUsingCard = await DbContext.HR_CardNumberInfo.AsNoTracking().Where(x => x.CompanyIndex == user.CompanyIndex
                    && cardNumber.Contains(x.CardNumber) && x.IsActive == true)
                    .Join(DbContext.HR_User.AsNoTracking().Where(x => x.CompanyIndex == user.CompanyIndex
                        && x.EmployeeType == (short)EmployeeType.Guest),
                    a => a.EmployeeATID, b => b.EmployeeATID, (a, b) => new { a, b })
                    .Join(DbContext.HR_CustomerInfo.AsNoTracking().Where(x => x.CompanyIndex == user.CompanyIndex), ab => ab.b.EmployeeATID, c => c.EmployeeATID, (ab, c)
                        => new CustomerCardModel
                        {
                            CardNumber = ab.a.CardNumber,
                            UserCode = c.EmployeeATID,
                            UpdatedDate = ab.a.UpdatedDate
                        }).ToListAsync();

            var listDriverUsingCard = await DbContext.GC_TruckDriverLog.AsNoTracking().Where(x => x.CompanyIndex == user.CompanyIndex
                && !x.IsInactive
                && cardNumber.Contains(x.CardNumber)).ToListAsync();

            var listExtraDriverUsingCard = await DbContext.GC_TruckExtraDriverLog.AsNoTracking().Where(x 
                => !x.IsInactive
                && cardNumber.Contains(x.CardNumber)).ToListAsync();

            if (listCustomerUsingCard.Count > 0 || listDriverUsingCard.Any(x => x.InOutMode == (short)InOutMode.Input
                    && !listDriverUsingCard.Any(y => y.TripCode == x.TripCode
                    && y.InOutMode == (short)InOutMode.Output))
                || listExtraDriverUsingCard.Count > 0)
            {
                return true;
            }
            return false;
        }

        public async Task<bool> IsCardNumberUsing(int index, UserInfo user)
        {
            var customerCard = await DbContext.HR_CustomerCard.FirstOrDefaultAsync(x => x.Index == index);
            var cardNumber = customerCard?.CardNumber ?? string.Empty;
            var listCustomerUsingCard = await DbContext.HR_CardNumberInfo.AsNoTracking().Where(x => x.CompanyIndex == user.CompanyIndex
                    && cardNumber == x.CardNumber && x.IsActive == true)
                    .Join(DbContext.HR_User.AsNoTracking().Where(x => x.CompanyIndex == user.CompanyIndex
                        && x.EmployeeType == (short)EmployeeType.Guest),
                    a => a.EmployeeATID, b => b.EmployeeATID, (a, b) => new { a, b })
                    .Join(DbContext.HR_CustomerInfo.AsNoTracking().Where(x => x.CompanyIndex == user.CompanyIndex), ab => ab.b.EmployeeATID, c => c.EmployeeATID, (ab, c)
                        => new CustomerCardModel
                        {
                            CardNumber = ab.a.CardNumber,
                            UserCode = c.EmployeeATID,
                            UpdatedDate = ab.a.UpdatedDate
                        }).ToListAsync();

            var listDriverUsingCard = await DbContext.GC_TruckDriverLog.AsNoTracking().Where(x => x.CompanyIndex == user.CompanyIndex
                && !x.IsInactive
                && cardNumber == x.CardNumber).ToListAsync();

            var listExtraDriverUsingCard = await DbContext.GC_TruckExtraDriverLog.AsNoTracking().Where(x
                => !x.IsInactive
                && cardNumber == x.CardNumber).ToListAsync();

            if (listCustomerUsingCard.Count > 0 || listDriverUsingCard.Any(x => x.InOutMode == (short)InOutMode.Input
                    && !listDriverUsingCard.Any(y => y.TripCode == x.TripCode
                    && y.InOutMode == (short)InOutMode.Output))
                || listExtraDriverUsingCard.Count > 0)
            {
                return true;
            }
            return false;
        }

        public async Task<EmployeeUsingCard> GetEmployeeATIDUsingCard(string employeeATID, int companyIndex, DateTime time)
        {
            var employeeReturn = new EmployeeUsingCard();
            var customerCard = await DbContext.HR_CustomerCard.FirstOrDefaultAsync(x => x.CardID == employeeATID);
            var dateNow = time.Date;
            if (customerCard != null)
            {
                var cardNumber = customerCard?.CardNumber ?? string.Empty;
                var listCustomerUsingCard = await DbContext.HR_CardNumberInfo.AsNoTracking().Where(x => x.CompanyIndex == companyIndex
                   && cardNumber == x.CardNumber && x.IsActive == true)
                   .Join(DbContext.HR_User.AsNoTracking().Where(x => x.CompanyIndex == companyIndex
                       && x.EmployeeType == (short)EmployeeType.Guest),
                   a => a.EmployeeATID, b => b.EmployeeATID, (a, b) => new { a, b })
                   .Join(DbContext.HR_CustomerInfo.AsNoTracking().Where(x => x.CompanyIndex == companyIndex && x.FromTime <= time && x.ToTime >= time), ab => ab.b.EmployeeATID, c => c.EmployeeATID, (ab, c)
                       => new CustomerCardModel
                       {
                           CardNumber = ab.a.CardNumber,
                           UserName = ab.b.FullName,
                           UserCode = c.EmployeeATID,
                           UpdatedDate = ab.a.UpdatedDate,
                           FromTime = c.FromTime,
                           ToTime = c.ToTime,
                       }).ToListAsync();

                var listDriverUsingCard = await DbContext.GC_TruckDriverLog.AsNoTracking().Where(x => x.CompanyIndex == companyIndex
                    && cardNumber == x.CardNumber && !x.IsInactive).ToListAsync();
                var listExtraDriverUsingCard = await DbContext.GC_TruckExtraDriverLog.AsNoTracking().Where(x 
                    => cardNumber == x.CardNumber && !x.IsInactive).ToListAsync();
                if (listCustomerUsingCard.Count > 0)
                {
                    employeeReturn.EmployeeATID = listCustomerUsingCard.FirstOrDefault().UserCode;
                    employeeReturn.CardNumber = listCustomerUsingCard.FirstOrDefault().CardNumber;
                    employeeReturn.UserType = (int)EmployeeType.Guest;
                    return employeeReturn;
                }
                else if (listDriverUsingCard.Any(x => x.InOutMode == (short)InOutMode.Input
                 && !listDriverUsingCard.Any(y => y.TripCode == x.TripCode
                 && y.InOutMode == (short)InOutMode.Output)))
                {
                    var emp = listDriverUsingCard.FirstOrDefault(x => x.InOutMode == (short)InOutMode.Input
                 && !listDriverUsingCard.Any(y => y.TripCode == x.TripCode
                 && y.InOutMode == (short)InOutMode.Output));
                    employeeReturn.EmployeeATID = emp.TripCode;
                    employeeReturn.CardNumber = emp.CardNumber;
                    employeeReturn.UserType = (int)EmployeeType.Driver;
                    return employeeReturn;
                }
                else if (listExtraDriverUsingCard.Count > 0)
                {
                    var emp = listExtraDriverUsingCard.FirstOrDefault();
                    employeeReturn.EmployeeATID = emp.TripCode;
                    employeeReturn.CardNumber = emp.CardNumber;
                    employeeReturn.UserType = (int)EmployeeType.Driver;
                    return employeeReturn;
                }
                else
                {
                    employeeReturn.EmployeeATID = employeeATID;
                    return employeeReturn;
                }
            }
            else
            {
                employeeReturn.EmployeeATID = employeeATID;
                return employeeReturn;
            }

        }

       

        public async Task<bool> IsCardNumberUsing(List<int> indexes, UserInfo user)
        {
            var listCustomerCard = await DbContext.HR_CustomerCard.Where(x => indexes.Contains(x.Index)).ToListAsync();
            var listCardNumber = listCustomerCard.Select(x => x.CardNumber).ToList();
            var listCustomerUsingCard = await DbContext.HR_CardNumberInfo.AsNoTracking().Where(x => x.CompanyIndex == user.CompanyIndex
                    && listCardNumber.Contains(x.CardNumber) && x.IsActive == true)
                    .Join(DbContext.HR_User.AsNoTracking().Where(x => x.CompanyIndex == user.CompanyIndex
                        && x.EmployeeType == (short)EmployeeType.Guest),
                    a => a.EmployeeATID, b => b.EmployeeATID, (a, b) => new { a, b })
                    .Join(DbContext.HR_CustomerInfo.AsNoTracking().Where(x => x.CompanyIndex == user.CompanyIndex), ab => ab.b.EmployeeATID, c => c.EmployeeATID, (ab, c)
                        => new CustomerCardModel
                        {
                            CardNumber = ab.a.CardNumber,
                            UserCode = c.EmployeeATID,
                            UpdatedDate = ab.a.UpdatedDate
                        }).ToListAsync();

            var listDriverUsingCard = await DbContext.GC_TruckDriverLog.AsNoTracking().Where(x => x.CompanyIndex == user.CompanyIndex
                && !x.IsInactive
                && listCardNumber.Contains(x.CardNumber)).ToListAsync();

            var listExtraDriverUsingCard = await DbContext.GC_TruckExtraDriverLog.AsNoTracking().Where(x
                => !x.IsInactive
                && listCardNumber.Contains(x.CardNumber)).ToListAsync();

            if (listCustomerUsingCard.Count > 0 || listDriverUsingCard.Any(x => x.InOutMode == (short)InOutMode.Input
                    && !listDriverUsingCard.Any(y => y.TripCode == x.TripCode
                    && y.InOutMode == (short)InOutMode.Output))
                || listExtraDriverUsingCard.Count > 0)
            {
                return true;
            }
            return false;
        }

        public async Task<bool> AddCustomerCard(string cardNumber, bool isSyncToDevice, UserInfo user)
        {
            var result = true;
            try
            {
                var maxLength = _config.MaxLenghtEmployeeATID;
                var prefixID = _config.AutoGenerateIDPrefix;
                var listEmployeeATID = await DbContext.HR_User.AsNoTracking().Select(x => x.EmployeeATID).ToListAsync();
                var listCustomerCardIDExisted = await DbContext.HR_CustomerCard.AsNoTracking().Select(x => x.CardID).ToListAsync();
                listEmployeeATID.AddRange(listCustomerCardIDExisted);
                listEmployeeATID = listEmployeeATID.Distinct().ToList();

                var cardID = GenerateUniqueNumberStrings(1, maxLength, prefixID, listEmployeeATID);

                await DbContext.HR_CustomerCard.AddAsync(new HR_CustomerCard
                {
                    CardNumber = cardNumber,
                    CardID = cardID[0],
                    CompanyIndex = user.CompanyIndex,
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now,
                    UpdatedUser = user.FullName
                });
                await DbContext.SaveChangesAsync();

                await _hR_EmployeeLogic.IntegrateCustomerCardToOffline(new List<string> { cardID[0]}, null);

                if (isSyncToDevice)
                {
                    await SyncCustomerCardToDevice(new List<CustomerCardModel> { new CustomerCardModel
                    {
                        CardID = cardID[0],
                        CardNumber = cardNumber
                    }}, user);
                }

                result = true;
            }
            catch (Exception ex)
            {
                result = false;
            }
            return result;
        }

        public async Task<bool> DeleteCustomerCard(int index)
        {
            var result = true;
            try
            {
                var customerCard = await DbContext.HR_CustomerCard.FirstOrDefaultAsync(x => x.Index == index);
                if (customerCard != null)
                {
                    DbContext.HR_CustomerCard.Remove(customerCard);
                    await DbContext.SaveChangesAsync();
                    await _hR_EmployeeLogic.DeleteCustomerCardToOffline(new List<string> { customerCard.CardID }, null);
                }

                result = true;
            }
            catch (Exception ex)
            {
                result = false;
            }
            return result;
        }

        public async Task<bool> DeleteCustomerCard(List<int> index, UserInfo user)
        {
            var result = true;
            try
            {
                var customerCard = await DbContext.HR_CustomerCard.Where(x => index.Contains(x.Index)).ToListAsync();
                if (customerCard != null && customerCard.Count > 0)
                {
                    var listCustomerCard = customerCard.Select(x => new CustomerCardModel
                    {
                        Index = x.Index,
                        CardID = x.CardID,
                        CardNumber = x.CardNumber,
                    }).ToList();
                    await UnsyncCustomerCardToDevice(listCustomerCard, user);

                    DbContext.HR_CustomerCard.RemoveRange(customerCard);
                    await DbContext.SaveChangesAsync();

                    await _hR_EmployeeLogic.DeleteCustomerCardToOffline(customerCard.Select(x => x.CardID).ToList(), null);
                }

                result = true;
            }
            catch (Exception ex)
            {
                result = false;
            }
            return result;
        }

        public async Task<List<string>> SyncCustomerCardToDevice(List<CustomerCardModel> listCustomerCard, UserInfo user)
        {
            var listSerial = new List<string>();

            var emergencyPrefixMachineName = _config?.EmergencyPrefixMachineName != null ? _config.EmergencyPrefixMachineName : string.Empty;
            if (!string.IsNullOrWhiteSpace(emergencyPrefixMachineName))
            {
                var listEmergencyMachine = await DbContext.IC_Device.AsNoTracking().Where(x
                        => x.CompanyIndex == user.CompanyIndex && x.AliasName.StartsWith(emergencyPrefixMachineName)).ToListAsync();
                if (listEmergencyMachine != null && listEmergencyMachine.Count > 0)
                {
                    listSerial.AddRange(listEmergencyMachine.Select(x => x.SerialNumber));
                }
            }

            var customerAndDriverLine = await DbContext.GC_Lines.Where(x => x.LineForCustomer || x.LineForDriver).ToListAsync();
            if (customerAndDriverLine.Count > 0)
            {
                var listLineIndex = customerAndDriverLine.Select(x => x.Index).ToList();
                var listDeviceIn = await DbContext.GC_Lines_CheckInDevice.Where(x => listLineIndex.Contains(x.LineIndex)).ToListAsync();
                var listDeviceOut = await DbContext.GC_Lines_CheckOutDevice.Where(x => listLineIndex.Contains(x.LineIndex)).ToListAsync();
                if (listDeviceIn.Count > 0)
                {
                    listSerial.AddRange(listDeviceIn.Select(x => x.CheckInDeviceSerial).ToList());
                }
                if (listDeviceOut.Count > 0)
                {
                    listSerial.AddRange(listDeviceOut.Select(x => x.CheckOutDeviceSerial).ToList());
                }
            }

            var parkings = await DbContext.IC_Device.Where(x => x.DeviceModule == "PA").Select(x => x.SerialNumber).ToListAsync();
            if(parkings != null && parkings.Count > 0)
            {
                listSerial.AddRange(parkings);
            }


            if (listSerial.Count <= 0)
            {
                return null;
            }

            listSerial = listSerial.Distinct().ToList();

            var lstUser = listCustomerCard.Select(x => new UserInfoOnMachine
            {
                EmployeeATID = x.CardID,
                CardNumber = x.CardNumber, 
                UserID = x.CardID
            }).ToList();

            IC_CommandParamDTO commandParam = new IC_CommandParamDTO();
            commandParam.IsOverwriteData = false;
            commandParam.Action = CommandAction.UploadUsers;
            commandParam.CommandName = StringHelper.GetCommandType((short)EmployeeType.Employee);
            commandParam.AuthenMode = string.Join(",", new List<string> { UserSyncAuthMode.CardNumber.ToString() });
            commandParam.FromTime = new DateTime(2000, 1, 1);
            commandParam.ToTime = DateTime.Now;
            commandParam.ListEmployee = lstUser;
            commandParam.ListSerialNumber = listSerial;
            commandParam.Privilege = GlobalParams.DevicePrivilege.SDKStandardRole;
            commandParam.ExternalData = JsonConvert.SerializeObject(new
            {
                AuthenModes = new List<string> { UserSyncAuthMode.CardNumber.ToString() }
            });

            List<CommandResult> lstCmd = _iC_CommandLogic.CreateListCommands(commandParam);

            if (lstCmd != null && lstCmd.Count() > 0)
            {
                IC_GroupCommandParamDTO groupCommand = new IC_GroupCommandParamDTO();
                groupCommand.CompanyIndex = user.CompanyIndex;
                groupCommand.UserName = user.UserName;
                groupCommand.ListCommand = lstCmd;
                groupCommand.GroupName = GroupName.UploadUsers.ToString();
                groupCommand.EventType = "";
                _iC_CommandLogic.CreateGroupCommands(groupCommand);
            }
            return listSerial;
        }

        public async Task<List<string>> UnsyncCustomerCardToDevice(List<CustomerCardModel> listCustomerCard, UserInfo user)
        {
            var listSerial = new List<string>();

            var emergencyPrefixMachineName = _config?.EmergencyPrefixMachineName != null ? _config.EmergencyPrefixMachineName : string.Empty;
            if (!string.IsNullOrWhiteSpace(emergencyPrefixMachineName))
            {
                var listEmergencyMachine = await DbContext.IC_Device.AsNoTracking().Where(x
                        => x.CompanyIndex == user.CompanyIndex && x.AliasName.StartsWith(emergencyPrefixMachineName)).ToListAsync();
                if (listEmergencyMachine != null && listEmergencyMachine.Count > 0)
                {
                    listSerial.AddRange(listEmergencyMachine.Select(x => x.SerialNumber));
                }
            }

            var customerAndDriverLine = await DbContext.GC_Lines.Where(x => x.LineForCustomer || x.LineForDriver).ToListAsync();
            if (customerAndDriverLine.Count > 0)
            {
                var listLineIndex = customerAndDriverLine.Select(x => x.Index).ToList();
                var listDeviceIn = await DbContext.GC_Lines_CheckInDevice.Where(x => listLineIndex.Contains(x.LineIndex)).ToListAsync();
                var listDeviceOut = await DbContext.GC_Lines_CheckOutDevice.Where(x => listLineIndex.Contains(x.LineIndex)).ToListAsync();
                if (listDeviceIn.Count > 0)
                {
                    listSerial.AddRange(listDeviceIn.Select(x => x.CheckInDeviceSerial).ToList());
                }
                if (listDeviceOut.Count > 0)
                {
                    listSerial.AddRange(listDeviceOut.Select(x => x.CheckOutDeviceSerial).ToList());
                }
            }
            if (listSerial.Count <= 0)
            {
                return null;
            }
            var lstUser = listCustomerCard.Select(x => new UserInfoOnMachine
            {
                EmployeeATID = x.CardID,
                CardNumber = x.CardNumber,
                UserID = x.CardID
            }).ToList();
            IC_CommandParamDTO commandParam = new IC_CommandParamDTO();
            commandParam.IsOverwriteData = false;
            commandParam.Action = CommandAction.DeleteUserById;
            commandParam.FromTime = new DateTime(2000, 1, 1);
            commandParam.ToTime = DateTime.Now;
            commandParam.ListEmployee = lstUser;
            commandParam.ListSerialNumber = listSerial;
            commandParam.ExternalData = "";

            List<CommandResult> lstCmd = _iC_CommandLogic.CreateListCommands(commandParam);

            if (lstCmd != null && lstCmd.Count() > 0)
            {
                IC_GroupCommandParamDTO groupCommand = new IC_GroupCommandParamDTO();
                groupCommand.CompanyIndex = user.CompanyIndex;
                groupCommand.UserName = user.UserName;
                groupCommand.ListCommand = lstCmd;
                groupCommand.GroupName = GroupName.DeleteUserById.ToString();
                groupCommand.EventType = "";
                _iC_CommandLogic.CreateGroupCommands(groupCommand);
            }
            return listSerial;
        }

        public List<string> GenerateUniqueNumberStrings(int listLength, int lengthString, string prefix, List<string> existingList)
        {
            List<string> result = new List<string>();
            HashSet<string> uniqueStrings = new HashSet<string>(existingList);

            long maxValue = listLength;
            long minValue = 0;

            for (long i = minValue; i < maxValue; i++)
            {
                string str = prefix + i.ToString().PadLeft(lengthString - prefix.Length, '0');
                if (!uniqueStrings.Contains(str))
                {
                    result.Add(str);
                    uniqueStrings.Add(str);
                }
                else
                {
                    var j = 0;
                    while (uniqueStrings.Contains(str))
                    {
                        str = prefix + (i + j).ToString().PadLeft(lengthString - prefix.Length, '0');
                        if (!uniqueStrings.Contains(str))
                        {
                            result.Add(str);
                            uniqueStrings.Add(str);
                            break;
                        }
                        j++;
                    }
                }
            }

            return result;
        }



        public async Task<List<CustomerCardModel>> ValidationImportCustomerCard(List<CustomerCardModel> param, UserInfo user)
        {
            var errorList = new List<CustomerCardModel>();

            var maxLength = _config.MaxLenghtEmployeeATID;
            var prefixID = _config.AutoGenerateIDPrefix;
            var listEmployeeATID = await DbContext.HR_User.AsNoTracking().Select(x => x.EmployeeATID).ToListAsync();
            var listCustomerCardExisted = await DbContext.HR_CustomerCard.AsNoTracking().ToListAsync();
            var listCustomerCardIDExisted = listCustomerCardExisted.Select(x => x.CardID).ToList();
            listEmployeeATID.AddRange(listCustomerCardIDExisted);
            listEmployeeATID = listEmployeeATID.Distinct().ToList();

            //var checkDuplicate = param.GroupBy(x => x.EmployeeATID).Where(g => g.Count() > 1).Select(y => y.Key).ToList();
            var checkDuplicateCard = param.Where(x => x.CardNumber != "0"
                && !string.IsNullOrEmpty(x.CardNumber)).GroupBy(x => x.CardNumber).Where(g => g.Count() > 1).Select(y => y.Key).ToList();
            //var checkMaxLength = param.Where(e => e.CardNumber.Length > maxLength).ToList();
            var checkIsNull = param.Where(e => string.IsNullOrWhiteSpace(e.CardNumber)).ToList();
            var checkIsZeroCardNumber = param.Where(e => !string.IsNullOrWhiteSpace(e.CardNumber)
                && e.CardNumber.All(x => x == '0')).ToList();

            var listExistCard = new List<CustomerCardModel>();
            foreach (var item in param)
            {
                if (listExistCard.Any(x => x.CardNumber == item.CardNumber))
                {
                    item.ErrorMessage += "Trùng mã thẻ trong tệp tin\r\n";
                }
                listExistCard.Add(item);
                if (string.IsNullOrWhiteSpace(item.CardNumber))
                {
                    item.ErrorMessage += "Mã thẻ không được để trống\r\n";
                }
                //if (item.CardNumber.Length > maxLength)
                //{
                //    item.ErrorMessage += "Mã thẻ không được dài hơn " + maxLength.ToString() + " ký tự" + "\r\n";
                //}
                if (!string.IsNullOrWhiteSpace(item.CardNumber))
                {
                    if (item.CardNumber.All(x => x == '0'))
                    {
                        item.ErrorMessage += "Mã thẻ phải khác 0\r\n";
                    }
                    if (item.CardNumber.All(x => !Char.IsDigit(x)))
                    {
                        item.ErrorMessage += "Mã thẻ không được chứa ký tự chữ\r\n";
                    }
                }
                //if (!string.IsNullOrWhiteSpace(item.CardNumber) && !item.CardNumber.StartsWith(prefixID))
                //{
                //    item.ErrorMessage += "Mã thẻ phải bắt đầu với ký tự '" + prefixID.ToString() +"'\r\n";
                //}
                if (listCustomerCardExisted.Any(x => x.CardNumber == item.CardNumber))
                {
                    item.ErrorMessage += "Mã thẻ đã tồn tại\r\n";
                }
                if (!string.IsNullOrWhiteSpace(item.ErrorMessage))
                {
                    errorList.Add(item);
                }
            }

            var notErrorList = param.Where(x => string.IsNullOrWhiteSpace(x.ErrorMessage)).ToList();
            if (notErrorList.Count > 0)
            {
                var cardID = GenerateUniqueNumberStrings(notErrorList.Count, maxLength, prefixID, listEmployeeATID);

                for (int i = 0; i < notErrorList.Count; i++)
                {
                    notErrorList[i].CardID = cardID[i];

                    await DbContext.HR_CustomerCard.AddAsync(new HR_CustomerCard
                    {
                        CardNumber = notErrorList[i].CardNumber,
                        CardID = notErrorList[i].CardID,
                        CompanyIndex = user.CompanyIndex,
                        CreatedDate = DateTime.Now,
                        UpdatedDate = DateTime.Now,
                        UpdatedUser = user.FullName
                    });
                }

                await DbContext.SaveChangesAsync();

                await _hR_EmployeeLogic.IntegrateCustomerCardToOffline(notErrorList.Select(x => x.CardID).ToList(), null);

                if (notErrorList.Any(x => x.IsSyncToDevice))
                {
                    await SyncCustomerCardToDevice(notErrorList.Where(x => x.IsSyncToDevice).ToList(), user);
                }
            }

            return await Task.FromResult(errorList);
        }
    }
}
