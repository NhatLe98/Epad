using EPAD_Common.Services;
using EPAD_Data.Entities;
using EPAD_Data;
using EPAD_Services.Interface;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EPAD_Common.Types;
using EPAD_Common.Enums;
using EPAD_Data.Models;
using System.Linq;
using DocumentFormat.OpenXml.Math;
using EPAD_Common.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using System.Net.NetworkInformation;
using System.Globalization;
using ClosedXML.Excel;
using Microsoft.AspNetCore.JsonPatch.Operations;
using DocumentFormat.OpenXml.Bibliography;

namespace EPAD_Services.Impl
{
    public class HR_DriverInfoService : BaseServices<IC_PlanDock, EPAD_Context>, IHR_DriverInfoService
    {
        private ILogger _logger;
        private readonly EPAD_Context _dbContext;
        public HR_DriverInfoService(IServiceProvider serviceProvider, ILoggerFactory loggerFactory) : base(serviceProvider)
        {
            _logger = loggerFactory.CreateLogger<HR_DriverInfoService>();
            _Mapper = serviceProvider.GetService<IMapper>();
            _dbContext = serviceProvider.GetService<EPAD_Context>();
        }

        public async Task AddDriverInfo(HR_DriverInfoDTO param, UserInfo user)
        {
            var planDock = _Mapper.Map<HR_DriverInfoDTO, IC_PlanDock>(param);
            planDock.CreatedDate = DateTime.Now;
            planDock.UpdatedUser = user.UserName;
            planDock.CompanyIndex = user.CompanyIndex;

            await _dbContext.AddAsync(planDock);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<DataGridClass> GetPage(HR_DriverInfoParam addedParams, int pCompanyIndex)
        {
            try
            {
                var fromDate = DateTime.Now;
                var toDate = DateTime.Now;
                if (!string.IsNullOrEmpty(addedParams.FromDate))
                {
                    fromDate = Convert.ToDateTime(addedParams.FromDate);
                }
                if (!string.IsNullOrEmpty(addedParams.ToDate))
                {
                    toDate = Convert.ToDateTime(addedParams.ToDate);
                }
                DataGridClass dataGrid = null;
                int countPage = 0;
                var query = from planDock in DbContext.IC_PlanDock.Where(t => t.CompanyIndex == pCompanyIndex)
                            join statusDock in DbContext.IC_StatusDock
                            on planDock.StatusDock equals statusDock.Key into eStatus
                            from eStatusResult in eStatus.DefaultIfEmpty()
                            join fromDateLog in DbContext.GC_TruckDriverLog.Where(x => x.CompanyIndex == pCompanyIndex && x.InOutMode == 0)
                          on planDock.TripId equals fromDateLog.TripCode into efromDateLog
                            from efromDateLogResult in efromDateLog.DefaultIfEmpty()
                            join toDateLog in DbContext.GC_TruckDriverLog.Where(x => x.CompanyIndex == pCompanyIndex && x.InOutMode == 1)
                          on planDock.TripId equals toDateLog.TripCode into etoDateLog
                            from etoDateLogResult in etoDateLog.DefaultIfEmpty()
                            join operators in DbContext.IC_LocationOperator
                         on planDock.Operation equals operators.Name into eOperator
                            from eOperatorResult in eOperator.DefaultIfEmpty()
                            join supplier in DbContext.IC_Department
                      on planDock.SupplierId equals supplier.Index into eSupplier
                            from eSupplierResult in eSupplier.DefaultIfEmpty()
                            where (string.IsNullOrWhiteSpace(addedParams.Filter) || (!string.IsNullOrWhiteSpace(addedParams.Filter) && (planDock.DriverCode.Contains(addedParams.Filter) || planDock.DriverName.Contains(addedParams.Filter) || planDock.TripId.Contains(addedParams.Filter)) || string.IsNullOrWhiteSpace(addedParams.Filter)))
                          && ((!string.IsNullOrWhiteSpace(addedParams.VehiclePlate) && planDock.TrailerNumber.Contains(addedParams.VehiclePlate)) || string.IsNullOrWhiteSpace(addedParams.VehiclePlate))
                         && ((!string.IsNullOrEmpty(addedParams.FromDate) && planDock.TimesDock >= fromDate.Date) || string.IsNullOrEmpty(addedParams.FromDate))
                            && ((!string.IsNullOrEmpty(addedParams.ToDate) && planDock.TimesDock <= toDate.Date) || string.IsNullOrEmpty(addedParams.ToDate))
                            && ((addedParams.Status == null || addedParams.Status.Count == 0 || addedParams.Status.Contains(planDock.StatusDock)))
                            && ((addedParams.DepartmentIDs == null || addedParams.DepartmentIDs.Count == 0 || (planDock.SupplierId != null && addedParams.DepartmentIDs.Contains(planDock.SupplierId.Value))))
                            select new HR_DriverInfoResult
                            {
                                Index = planDock.Index,
                                Avatar = planDock.Avatar,
                                CompanyIndex = planDock.CompanyIndex,
                                DriverCode = planDock.DriverCode,
                                DriverName = planDock.DriverName,
                                DriverPhone = planDock.DriverPhone,
                                Eta = planDock.Eta,
                                LocationFrom = planDock.LocationFrom,
                                OrderCode = planDock.OrderCode,
                                Status = planDock.Status,
                                StatusString = planDock.Status == "C" ? "Thêm mới" : planDock.Status == "D" ? "Xóa" : planDock.Status == "U" ? "Cập nhật" : string.Empty,
                                StatusDock = planDock.StatusDock,
                                StatusDockString = eStatusResult != null ? eStatusResult.Name : string.Empty,
                                Supplier = eSupplierResult != null ? eSupplierResult.Name : string.Empty,
                                TimesDock = planDock.TimesDock,
                                TimesDockString = planDock.TimesDock != null ? planDock.TimesDock.Value.ToString("dd/MM/yyyy HH:mm:ss") : string.Empty,
                                TrailerNumber = planDock.TrailerNumber,
                                TripId = planDock.TripId,
                                Type = planDock.Type,
                                Vc = planDock.Vc,
                                FromDate = efromDateLogResult != null ? efromDateLogResult.Time.ToString("dd/MM/yyyy HH:mm:ss") : string.Empty,
                                ToDate = etoDateLogResult != null ? etoDateLogResult.Time.ToString("dd/MM/yyyy HH:mm:ss") : string.Empty,
                                EtaString = planDock.Eta != null ? planDock.Eta.Value.ToString("dd/MM/yyyy HH:mm:ss") : string.Empty,
                                BirthDay = planDock.BirthDay != null ? planDock.BirthDay : null,
                                Operation = planDock.Operation,
                                LocationOperator = (planDock.Type == "XB" || planDock.Type == "CK") ? "Kho Thành phẩm" : eOperatorResult != null ? eOperatorResult.Department : string.Empty,
                                SupplierId = planDock.SupplierId,
                            };
                countPage = query.Count();
                dataGrid = new DataGridClass(countPage, query);
                if (addedParams.Page <= 1)
                {
                    var lsDevice = query.Take(addedParams.PageSize).ToList();
                    dataGrid = new DataGridClass(countPage, lsDevice);
                }
                else
                {
                    int fromRow = addedParams.PageSize * (addedParams.Page - 1);
                    var lsDevice = query.Skip(fromRow).Take(addedParams.PageSize).ToList();
                    dataGrid = new DataGridClass(countPage, lsDevice);
                }
                return dataGrid;

            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex}");
                return await Task.FromResult(new DataGridClass(0, null));
            }
        }



        public async Task UpdateDriverInfo(HR_DriverInfoDTO param, UserInfo user)
        {
            var planDock = await _dbContext.IC_PlanDock.FirstOrDefaultAsync(x => x.Index == param.Index);
            _Mapper.Map<HR_DriverInfoDTO, IC_PlanDock>(param, planDock);
            planDock.UpdatedUser = user.UserName;
            planDock.CompanyIndex = user.CompanyIndex;
            planDock.UpdatedDate = DateTime.Now;

            _dbContext.Update(planDock);
            await _dbContext.SaveChangesAsync();
        }

        public List<HR_DriveInfoImportParam> ValidationImportDriverInfo(List<HR_DriveInfoImportParam> param, UserInfo user)
        {
            var lstStatus = _dbContext.IC_StatusDock.ToList();
            var errorList = new List<HR_DriveInfoImportParam>();
            var lstTrips = param.Select(x => x.TripId).ToList();

            var checkTripIdInExcel = param.GroupBy(x => x.TripId).Where(x => x.Count() > 1).Select(x => x.Key).ToList();
            if (checkTripIdInExcel != null && checkTripIdInExcel.Count() > 0)
            {
                var duplicate = param.Where(e => checkTripIdInExcel.Contains(e.TripId)).ToList();
                foreach (var item in duplicate)
                {
                    item.ErrorMessage = "Trùng mã đơn hàng trên file Excel\r\n";
                }
            }

            var checkTripIdIsNull = param.Where(x => string.IsNullOrEmpty(x.TripId)).ToList();
            if (checkTripIdIsNull != null && checkTripIdIsNull.Count() > 0)
            {
                foreach (var item in checkTripIdIsNull)
                {
                    item.ErrorMessage = "Mã đơn hàng không được bỏ trống\r\n";
                }
            }

            var checkDriverCodeIsNull = param.Where(x => string.IsNullOrEmpty(x.DriverCode)).ToList();
            if (checkDriverCodeIsNull != null && checkDriverCodeIsNull.Count() > 0)
            {
                foreach (var item in checkDriverCodeIsNull)
                {
                    item.ErrorMessage = "Số CCCD không được bỏ trống\r\n";
                }
            }

            var checkCCCD = param.Where(x => !string.IsNullOrEmpty(x.DriverCode)).ToList();
            if (checkCCCD != null && checkCCCD.Count() > 0)
            {
                foreach (var item in checkCCCD)
                {
                    if (!string.IsNullOrEmpty(item.DriverCode) && char.IsDigit(item.DriverCode[0]) && item.DriverCode.Length != 12)
                    {
                        item.ErrorMessage += "CCCD phải đúng 12 ký tự \r\n";
                    }
                }
            }

            var dataCheckBlackList = param.Where(x => !string.IsNullOrWhiteSpace(x.DriverCode)).ToList();
            var employeeInBlackList = DbContext.GC_BlackList.Where(x => dataCheckBlackList.Select(z => z.DriverCode).Contains(x.Nric)).ToList();
            foreach (var item in param)
            {
                var now = DateTime.Now;
                if (!string.IsNullOrEmpty(item.BirthDayStr))
                {
                    string[] formats = { "dd/MM/yyyy" };
                    var birthDay = new DateTime();

                    var convertFromDate = DateTime.TryParseExact(item.BirthDayStr, formats,
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None, out birthDay);
                    if (!convertFromDate)
                    {
                        item.ErrorMessage += "Ngày sinh không hợp lệ\r\n";
                    }
                    else
                    {

                        item.BirthDay = birthDay;
                    }
                }

                if (string.IsNullOrEmpty(item.TripId)) item.ErrorMessage += "Mã chuyến không được bỏ trống\r\n";
                if (string.IsNullOrEmpty(item.LocationFrom)) item.ErrorMessage += "Điểm nhận hàng không được bỏ trống\r\n";
                if (string.IsNullOrEmpty(item.DriverName)) item.ErrorMessage += "Họ tên không được bỏ trống\r\n";
                if (string.IsNullOrEmpty(item.TrailerNumber)) item.ErrorMessage += "Biển số xe không được bỏ trống\r\n";
                if (string.IsNullOrEmpty(item.DriverPhone)) item.ErrorMessage += "Số điện thoại không được bỏ trống\r\n";
                if (!string.IsNullOrEmpty(item.EtaStr))
                {
                    string[] formats = { "dd/MM/yyyy HH:mm:ss" };
                    var eTADay = new DateTime();

                    var convertFromDate = DateTime.TryParseExact(item.EtaStr, formats,
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None, out eTADay);
                    if (!convertFromDate)
                    {
                        item.ErrorMessage += "Thời gian dự kiến đến điểm lấy không hợp lệ\r\n";
                    }
                    else
                    {
                        item.Eta = eTADay;
                    }
                }

                if (!string.IsNullOrWhiteSpace(item.TimesDockStr))
                {
                    string[] formats = { "dd/MM/yyyy HH:mm:ss" };
                    var timeDock = new DateTime();

                    var convertFromDate = DateTime.TryParseExact(item.TimesDockStr, formats,
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None, out timeDock);
                    if (!convertFromDate)
                    {
                        item.ErrorMessage += "Thời gian vào Dock không hợp lệ\r\n";
                    }
                    else
                    {
                        item.TimesDock = timeDock;
                    }
                }

                var checkEmployeeInBlackList = employeeInBlackList.Any(x => ((!string.IsNullOrEmpty(item.DriverCode) && x.Nric == item.DriverCode))
                                                                                                        && x.FromDate.Date <= now.Date
                                                                                                        && (x.ToDate == null || (x.ToDate != null && now.Date <= x.ToDate.Value.Date)));
                if (checkEmployeeInBlackList)
                {
                    item.ErrorMessage += "Người dùng thuộc danh sách đen\r\n";
                }
            }

            errorList = param.Where(x => !string.IsNullOrEmpty(x.ErrorMessage)).ToList();

            var dataSave = param.Where(x => string.IsNullOrEmpty(x.ErrorMessage)).ToList();
            var tripIdSave = param.Select(x => x.TripId).ToList();
            var lstDepartment = _dbContext.IC_Department.Where(x => x.IsInactive != true).ToList();
            var lstDeparmentImport = dataSave.Select(x => x.Supplier).Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();
            if (dataSave != null && dataSave.Count() > 0)
            {
                var lstAnnualLeave = _dbContext.IC_PlanDock.Where(x => tripIdSave.Contains(x.TripId)).ToList();

                var save = false;
                foreach (var supplier in lstDeparmentImport)
                {
                    if (!lstDepartment.Any(x => x.Name == supplier))
                    {
                        var suppli = new IC_Department()
                        {
                            Name = supplier,
                            Code = DateTime.Now.Ticks.ToString(),
                            CompanyIndex = user.CompanyIndex,
                            ParentIndex = 0,
                            ParentCode = string.Empty,
                            IsDriverDepartment = true,
                            UpdatedDate = DateTime.Now,
                            UpdatedUser = UpdatedUser.AutoIntegrateEmployee.ToString(),
                            CreatedDate = DateTime.Now
                        };
                        _dbContext.IC_Department.Add(suppli);
                        save = true;
                    }
                }
                if (save)
                {
                    _dbContext.SaveChanges();
                    lstDepartment = _dbContext.IC_Department.Where(x => x.IsInactive != true).ToList();
                }

                foreach (var item in dataSave)
                {
                    var existAnnualLeave = lstAnnualLeave.FirstOrDefault(x => x.TripId == item.TripId);
                    if (existAnnualLeave == null)
                    {
                        existAnnualLeave = new IC_PlanDock();
                        _Mapper.Map<HR_DriveInfoImportParam, IC_PlanDock>(item, existAnnualLeave);

                        int department = 0;
                        if (!string.IsNullOrEmpty(existAnnualLeave.Supplier))
                        {
                            department = lstDepartment.FirstOrDefault(x => x.Name == existAnnualLeave.Supplier)?.Index ?? 0;
                        }
                        if (!string.IsNullOrEmpty(item.StatusDockString))
                        {
                            var status = lstStatus.FirstOrDefault(x => x.Name == item.StatusDockString);
                            if (status != null)
                            {
                                existAnnualLeave.StatusDock = status.Key;
                            }
                        }
                        existAnnualLeave.SupplierId = department;
                        existAnnualLeave.UpdatedUser = user.UserName;
                        existAnnualLeave.CompanyIndex = user.CompanyIndex;
                        existAnnualLeave.CreatedDate = DateTime.Now;

                        _dbContext.IC_PlanDock.Add(existAnnualLeave);
                    }
                    else
                    {
                        _Mapper.Map<HR_DriveInfoImportParam, IC_PlanDock>(item, existAnnualLeave);
                        if (!string.IsNullOrEmpty(item.StatusDockString))
                        {
                            var status = lstStatus.FirstOrDefault(x => x.Name == item.StatusDockString);
                            if (status != null)
                            {
                                existAnnualLeave.StatusDock = status.Key;
                            }
                        }

                        int department = 0;
                        if (!string.IsNullOrEmpty(existAnnualLeave.Supplier))
                        {
                            department = lstDepartment.FirstOrDefault(x => x.Name == existAnnualLeave.Supplier)?.Index ?? 0;
                        }
                        existAnnualLeave.SupplierId = department;
                        existAnnualLeave.UpdatedUser = user.UserName;
                        existAnnualLeave.CompanyIndex = user.CompanyIndex;
                        existAnnualLeave.UpdatedDate = DateTime.Now;

                        _dbContext.IC_PlanDock.Update(existAnnualLeave);
                    }
                }

                _dbContext.SaveChanges();
            }
            return errorList;
        }

        public bool CheckExistDriverByTripId(string tripId, int pCompanyIndex)
        {
            return _dbContext.IC_PlanDock.Any(x => x.TripId == tripId && x.CompanyIndex == pCompanyIndex);
        }

        public List<HR_DriverInfoResult> GetManyExportDriver(HR_DriverInfoParam addedParams, int pCompanyIndex)
        {
            try
            {
                var fromDate = DateTime.Now;
                var toDate = DateTime.Now;
                if (!string.IsNullOrEmpty(addedParams.FromDate))
                {
                    fromDate = Convert.ToDateTime(addedParams.FromDate);
                }
                if (!string.IsNullOrEmpty(addedParams.ToDate))
                {
                    toDate = Convert.ToDateTime(addedParams.ToDate);
                }

                var query = (from planDock in DbContext.IC_PlanDock.Where(t => t.CompanyIndex == pCompanyIndex)
                             join statusDock in DbContext.IC_StatusDock
                             on planDock.StatusDock equals statusDock.Key into eStatus
                             from eStatusResult in eStatus.DefaultIfEmpty()
                             join fromDateLog in DbContext.GC_TruckDriverLog.Where(x => x.CompanyIndex == pCompanyIndex && x.InOutMode == 0)
                           on planDock.TripId equals fromDateLog.TripCode into efromDateLog
                             from efromDateLogResult in efromDateLog.DefaultIfEmpty()
                             join toDateLog in DbContext.GC_TruckDriverLog.Where(x => x.CompanyIndex == pCompanyIndex && x.InOutMode == 1)
                           on planDock.TripId equals toDateLog.TripCode into etoDateLog
                             from etoDateLogResult in etoDateLog.DefaultIfEmpty()
                             join operators in DbContext.IC_LocationOperator
                                on planDock.Operation equals operators.Name into eOperator
                             from eOperatorResult in eOperator.DefaultIfEmpty()
                             join supplier in DbContext.IC_Department
                         on planDock.SupplierId equals supplier.Index into eSupplier
                             from eSupplierResult in eSupplier.DefaultIfEmpty()
                             where (string.IsNullOrWhiteSpace(addedParams.Filter) || (!string.IsNullOrWhiteSpace(addedParams.Filter) && (planDock.DriverCode.Contains(addedParams.Filter) || planDock.DriverName.Contains(addedParams.Filter)) || string.IsNullOrWhiteSpace(addedParams.Filter)))
                           && ((!string.IsNullOrWhiteSpace(addedParams.VehiclePlate) && planDock.TrailerNumber.Contains(addedParams.VehiclePlate)) || string.IsNullOrWhiteSpace(addedParams.VehiclePlate))
                          && ((!string.IsNullOrEmpty(addedParams.FromDate) && planDock.TimesDock >= fromDate.Date) || string.IsNullOrEmpty(addedParams.FromDate))
                             && ((!string.IsNullOrEmpty(addedParams.ToDate) && planDock.TimesDock <= toDate.Date) || string.IsNullOrEmpty(addedParams.ToDate))
                             && ((addedParams.Status == null || addedParams.Status.Count == 0 || addedParams.Status.Contains(planDock.StatusDock)))
                             && ((addedParams.DepartmentIDs == null || addedParams.DepartmentIDs.Count == 0 || (planDock.SupplierId != null && addedParams.DepartmentIDs.Contains(planDock.SupplierId.Value))))
                             select new HR_DriverInfoResult
                             {
                                 Index = planDock.Index,
                                 Avatar = planDock.Avatar,
                                 CompanyIndex = planDock.CompanyIndex,
                                 DriverCode = planDock.DriverCode,
                                 DriverName = planDock.DriverName,
                                 DriverPhone = planDock.DriverPhone,
                                 Eta = planDock.Eta,
                                 LocationFrom = planDock.LocationFrom,
                                 OrderCode = planDock.OrderCode,
                                 Status = planDock.Status,
                                 StatusDock = planDock.StatusDock,
                                 StatusDockString = eStatusResult != null ? eStatusResult.Name : string.Empty,
                                 Supplier = eSupplierResult != null ? eSupplierResult.Name : string.Empty,
                                 TimesDock = planDock.TimesDock,
                                 TimesDockString = planDock.TimesDock != null ? planDock.TimesDock.Value.ToString("dd/MM/yyyy HH:mm:ss") : string.Empty,
                                 TrailerNumber = "'" + planDock.TrailerNumber,
                                 TripId = planDock.TripId,
                                 Type = planDock.Type,
                                 Vc = planDock.Vc,
                                 FromDate = efromDateLogResult != null ? efromDateLogResult.Time.ToString("dd/MM/yyyy HH:mm:ss") : string.Empty,
                                 ToDate = etoDateLogResult != null ? etoDateLogResult.Time.ToString("dd/MM/yyyy HH:mm:ss") : string.Empty,
                                 Operation = planDock.Operation,
                                 LocationOperator = (planDock.Type == "XB" || planDock.Type == "CK") ? "Kho Thành phẩm" : eOperatorResult != null ? eOperatorResult.Department : string.Empty,
                                 SupplierId = planDock.SupplierId,
                             }).ToList();

                return query;

            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex}");
                return new List<HR_DriverInfoResult>();
            }
        }

        public async Task DeleteDriverInfoMulti(List<string> tripIds, int pCompanyIndex)
        {
            var drivers = await _dbContext.IC_PlanDock.Where(x => tripIds.Contains(x.TripId)).ToListAsync();
            foreach (var driver in drivers)
            {
                _dbContext.IC_PlanDock.Remove(driver);
            }
            await _dbContext.SaveChangesAsync();
        }
    }
}
