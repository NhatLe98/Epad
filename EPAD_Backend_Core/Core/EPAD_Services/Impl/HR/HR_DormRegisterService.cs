using EPAD_Common.Extensions;
using EPAD_Common.Services;
using EPAD_Common.Types;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using EPAD_Data.Models.HR;
using EPAD_Services.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EPAD_Services.Impl
{
    public class HR_DormRegisterService : BaseServices<HR_DormRegister, EPAD_Context>, IHR_DormRegisterService
    {
        private ILogger _logger;
        public HR_DormRegisterService(IServiceProvider serviceProvider, ILoggerFactory loggerFactory) : base(serviceProvider)
        {
            _logger = loggerFactory.CreateLogger<HR_DormRegisterService>();
        }

        public async Task<List<HR_DormActivity>> GetDormActivity(UserInfo user) 
        {
            return await DbContext.HR_DormActivity.AsNoTracking().Where(x => x.CompanyIndex == user.CompanyIndex).ToListAsync();
        }

        public async Task<List<HR_DormLeaveType>> GetDormLeaveType(UserInfo user)
        {
            return await DbContext.HR_DormLeaveType.AsNoTracking().Where(x => x.CompanyIndex == user.CompanyIndex).ToListAsync();
        }

        public async Task<List<HR_DormRation>> GetDormRation(UserInfo user)
        {
            return await DbContext.HR_DormRation.AsNoTracking().Where(x => x.CompanyIndex == user.CompanyIndex).ToListAsync();
        }

        public object GetDormAccessMode()
        { 
            var listAccessMode = Enum.GetValues(typeof(DormAccessMode)).Cast<DormAccessMode>().Select(x 
                => new { Index = (int)x, Name = x.ToString() }).ToList();
            return listAccessMode;
        }

        public async Task<DataGridClass> GetDormRegister(UserInfo user, DormRegisterRequest requestParam)
        {
            var result = new DataGridClass(0, null);
            var queryData = from dr in DbContext.HR_DormRegister.Where(x => x.CompanyIndex == user.CompanyIndex)
                            join u in DbContext.HR_User.Where(x => x.CompanyIndex == user.CompanyIndex)
                            on dr.EmployeeATID equals u.EmployeeATID
                            join w in DbContext.IC_WorkingInfo.Where(w => w.CompanyIndex == user.CompanyIndex
                                && w.Status == (short)TransferStatus.Approve
                                && w.FromDate.Date <= DateTime.Now.Date && (!w.ToDate.HasValue
                                    || (w.ToDate.HasValue && w.ToDate.Value.Date > DateTime.Now.Date)))
                            on u.EmployeeATID equals w.EmployeeATID into workingInfo
                            from wkResult in workingInfo.DefaultIfEmpty()
                            join d in DbContext.IC_Department.Where(x => x.CompanyIndex == user.CompanyIndex && x.IsInactive != true)
                            on wkResult.DepartmentIndex equals d.Index into depInfo
                            from depResult in depInfo.DefaultIfEmpty()
                            join dorm in DbContext.HR_DormRoom.Where(x => x.CompanyIndex == user.CompanyIndex)
                            on dr.DormRoomIndex equals dorm.Index
                            join dl in DbContext.HR_DormLeaveType
                            on dr.DormLeaveIndex equals dl.Index into dlInfo
                            from dlResult in dlInfo.DefaultIfEmpty()
                            select new DormRegisterViewModel
                            {
                                Index = dr.Index,
                                EmployeeATID = dr.EmployeeATID,
                                FullName = u.FullName,
                                FromDate = dr.RegisterDate,
                                FromDateString = dr.RegisterDate.ToString("dd/MM/yyyy"),
                                ToDate = dr.RegisterDate,
                                ToDateString = dr.RegisterDate.ToString("dd/MM/yyyy"),
                                RegisterDate = dr.RegisterDate,
                                RegisterDateString = dr.RegisterDate.ToString("dd/MM/yyyy"),
                                StayInDorm = dr.StayInDorm,
                                DormRoomIndex = dr.DormRoomIndex,
                                DormRoomName = dorm.Name,
                                DepartmentIndex = depResult != null ? depResult.Index : 0,
                                DepartmentName = depResult != null ? depResult.Name : string.Empty,
                                DormEmployeeCode = dr.DormEmployeeCode,
                                DormLeaveIndex = dr.DormLeaveIndex,
                                DormLeaveName = dlResult != null ? dlResult.Name : string.Empty,
                                CreatedDate = dr.CreatedDate,
                                UpdatedDate = dr.UpdatedDate,
                                UpdatedUser = dr.UpdatedUser
                            };

            var format = "yyyy-MM-dd";
            if (requestParam != null)
            {
                if (!string.IsNullOrWhiteSpace(requestParam.filter))
                {
                    queryData = queryData.Where(x => x.EmployeeATID.Contains(requestParam.filter));
                }

                if (!string.IsNullOrWhiteSpace(requestParam.from))
                {
                    if (DateTime.TryParseExact(requestParam.from, format, null,
                        System.Globalization.DateTimeStyles.None, out DateTime fromDate))
                    {
                        queryData = queryData.Where(x => x.RegisterDate.Date >= fromDate.Date);
                    }
                }

                if (!string.IsNullOrWhiteSpace(requestParam.to))
                {
                    if (DateTime.TryParseExact(requestParam.to, format, null,
                        System.Globalization.DateTimeStyles.None, out DateTime toDate))
                    {
                        queryData = queryData.Where(x => x.RegisterDate.Date <= toDate.Date);
                    }
                }

                if (requestParam.departments != null && requestParam.departments.Count > 0)
                {
                    queryData = queryData.Where(x => requestParam.departments.Contains(x.DepartmentIndex));
                }

                if (requestParam.dormRooms != null && requestParam.dormRooms.Count > 0)
                {
                    queryData = queryData.Where(x => requestParam.dormRooms.Contains(x.DormRoomIndex));
                }
            }

            var listData = await queryData.ToListAsync();
            if (listData != null && listData.Count > 0 && listData.Any(x => x.StayInDorm))
            {
                var listRegisterStayInDormIndex = listData.Where(x => x.StayInDorm).Select(x => x.Index).ToList();
                var listRegisterDormRation = await DbContext.HR_DormRegister_Ration.Where(x
                    => listRegisterStayInDormIndex.Contains(x.DormRegisterIndex)).ToListAsync();
                var listRegisterDormActivity = await DbContext.HR_DormRegister_Activity.Where(x
                    => listRegisterStayInDormIndex.Contains(x.DormRegisterIndex)).ToListAsync();

                var listDormRation = new List<HR_DormRation>();
                var listDormActivity = new List<HR_DormActivity>();

                if (listRegisterDormRation != null && listRegisterDormRation.Count > 0)
                {
                    var listDormRationIndex = listRegisterDormRation.Select(x => x.DormRationIndex).ToList();
                    listDormRation = await DbContext.HR_DormRation.Where(x => listDormRationIndex.Contains(x.Index)).ToListAsync();
                }

                if (listRegisterDormActivity != null && listRegisterDormActivity.Count > 0)
                {
                    var listDormActivityIndex = listRegisterDormActivity.Select(x => x.DormActivityIndex).ToList();
                    listDormActivity = await DbContext.HR_DormActivity.Where(x => listDormActivityIndex.Contains(x.Index)).ToListAsync();
                }

                listData.ForEach(x =>
                {
                    x.DormRegisterRation = listRegisterDormRation.Where(y => y.DormRegisterIndex == x.Index)
                        .Select(x => new DormRegisterRationViewModel
                        {
                            DormRegisterIndex = x.DormRegisterIndex,
                            DormRationIndex = x.DormRationIndex,
                            DormRationName = listDormRation.FirstOrDefault(y => y.Index == x.DormRationIndex)?.Name ?? string.Empty,
                            CompanyIndex = x.CompanyIndex,
                            CreatedDate = x.CreatedDate,
                            UpdatedDate = x.UpdatedDate,
                            UpdatedUser = x.UpdatedUser
                        }).ToList();
                    x.DormRegisterRationName = String.Join(", ", x.DormRegisterRation.Select(x => x.DormRationName).Distinct());

                    x.DormRegisterActivity = listRegisterDormActivity.Where(y => y.DormRegisterIndex == x.Index)
                        .Select(x => new DormRegisterActivityViewModel
                        {
                            DormRegisterIndex = x.DormRegisterIndex,
                            DormActivityIndex = x.DormActivityIndex,
                            DormActivityName = listDormActivity.FirstOrDefault(y => y.Index == x.DormActivityIndex)?.Name ?? string.Empty,
                            DormAccessMode = x.DormAccessMode,
                            CompanyIndex = x.CompanyIndex,
                            CreatedDate = x.CreatedDate,
                            UpdatedDate = x.UpdatedDate,
                            UpdatedUser = x.UpdatedUser
                        }).ToList();
                    x.DormRegisterActivityName = String.Join(", ", x.DormRegisterActivity.Select(x => x.DormActivityName).Distinct());
                });
            }

            result.total = listData.Count();
            result.data = listData.Skip((requestParam.page - 1) * requestParam.limit).Take(requestParam.limit).ToList();

            return result;
        }

        public async Task<List<HR_DormRegister>> GetByDormEmployeeCode(UserInfo user, string dormEmployeeCode)
        {
            return await DbContext.HR_DormRegister.AsNoTracking().Where(x => x.CompanyIndex == user.CompanyIndex
                && x.DormEmployeeCode == dormEmployeeCode).ToListAsync();
        }

        public async Task<HR_DormRegister> GetByIndex(int index)
        {
            return await DbContext.HR_DormRegister.AsNoTracking().FirstOrDefaultAsync(x => x.Index == index);
        }

        public async Task<List<HR_DormRegister>> GetByEmployeATID(UserInfo user, string employeeATID)
        {
            return await DbContext.HR_DormRegister.AsNoTracking().Where(x => x.CompanyIndex == user.CompanyIndex
                && x.EmployeeATID == employeeATID).ToListAsync();
        }

        public async Task<bool> AddDormRegister(UserInfo user, DormRegisterViewModel param) 
        {
            var result = true;
            try
            {
                var listDate = ListDate(param.FromDate, param.ToDate);
                var listDormRegister = new List<HR_DormRegister>();
                foreach (var date in listDate)
                {
                    var dormRegister = new HR_DormRegister();
                    dormRegister.EmployeeATID = param.EmployeeATID;
                    dormRegister.RegisterDate = date;
                    dormRegister.StayInDorm = param.StayInDorm;
                    dormRegister.CreatedDate = DateTime.Now;
                    dormRegister.UpdatedDate = DateTime.Now;
                    dormRegister.UpdatedUser = user.FullName;
                    dormRegister.CompanyIndex = user.CompanyIndex;
                    if (dormRegister.StayInDorm)
                    {
                        dormRegister.DormRoomIndex = param.DormRoomIndex;
                        dormRegister.DormEmployeeCode = param.DormEmployeeCode;
                    }
                    else
                    {
                        dormRegister.DormLeaveIndex = param.DormLeaveIndex;
                    }
                    listDormRegister.Add(dormRegister);
                }
                await DbContext.HR_DormRegister.AddRangeAsync(listDormRegister);
                await DbContext.SaveChangesAsync();

                if (param.StayInDorm)
                {
                    if (param.DormRegisterRation != null && param.DormRegisterRation.Count > 0)
                    {
                        var listDormRegisterRation = new List<HR_DormRegister_Ration>();
                        foreach (var ration in param.DormRegisterRation)
                        {
                            foreach (var dorm in listDormRegister)
                            {
                                var rationRegister = new HR_DormRegister_Ration();
                                rationRegister.DormRegisterIndex = dorm.Index;
                                rationRegister.DormRationIndex = ration.DormRationIndex;
                                rationRegister.CreatedDate = DateTime.Now;
                                rationRegister.UpdatedDate = DateTime.Now;
                                rationRegister.UpdatedUser = user.FullName;
                                rationRegister.CompanyIndex = user.CompanyIndex;
                                listDormRegisterRation.Add(rationRegister);
                            }
                        }
                        await DbContext.HR_DormRegister_Ration.AddRangeAsync(listDormRegisterRation);
                    }
                    if (param.DormRegisterActivity != null && param.DormRegisterActivity.Count > 0)
                    {
                        var listDormRegisterActivity = new List<HR_DormRegister_Activity>();
                        foreach (var activity in param.DormRegisterActivity)
                        {
                            foreach (var dorm in listDormRegister)
                            {
                                var activityRegister = new HR_DormRegister_Activity();
                                activityRegister.DormRegisterIndex = dorm.Index;
                                activityRegister.DormActivityIndex = activity.DormActivityIndex;
                                activityRegister.DormAccessMode = activity.DormAccessMode;
                                activityRegister.CreatedDate = DateTime.Now;
                                activityRegister.UpdatedDate = DateTime.Now;
                                activityRegister.UpdatedUser = user.FullName;
                                activityRegister.CompanyIndex = user.CompanyIndex;
                                listDormRegisterActivity.Add(activityRegister);
                            }
                        }
                        await DbContext.HR_DormRegister_Activity.AddRangeAsync(listDormRegisterActivity);
                    }
                    await DbContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                result = false;
            }
            return result;
        }

        public async Task<bool> UpdateDormRegister(UserInfo user, DormRegisterViewModel param)
        {
            var result = true;
            try
            {
                var existedDormRegister = await DbContext.HR_DormRegister.FirstOrDefaultAsync(x => x.Index == param.Index);
                existedDormRegister.StayInDorm = param.StayInDorm;
                existedDormRegister.UpdatedDate = DateTime.Now;
                existedDormRegister.UpdatedUser = user.FullName;
                if (existedDormRegister.StayInDorm)
                {
                    existedDormRegister.DormRoomIndex = param.DormRoomIndex;
                    existedDormRegister.DormEmployeeCode = param.DormEmployeeCode;
                }
                else
                {
                    existedDormRegister.DormLeaveIndex = param.DormLeaveIndex;
                }

                var registeredRation = await DbContext.HR_DormRegister_Ration.Where(x => x.DormRegisterIndex == param.Index).ToListAsync();
                if (registeredRation != null && registeredRation.Count > 0)
                {
                    DbContext.HR_DormRegister_Ration.RemoveRange(registeredRation);
                }

                var registeredActivity = await DbContext.HR_DormRegister_Activity.Where(x => x.DormRegisterIndex == param.Index).ToListAsync();
                if (registeredActivity != null && registeredActivity.Count > 0)
                {
                    DbContext.HR_DormRegister_Activity.RemoveRange(registeredActivity);
                }

                if (param.StayInDorm)
                {
                    if (param.DormRegisterRation != null && param.DormRegisterRation.Count > 0)
                    {
                        var listDormRegisterRation = new List<HR_DormRegister_Ration>();
                        foreach (var ration in param.DormRegisterRation)
                        {
                            var rationRegister = new HR_DormRegister_Ration();
                            rationRegister.DormRegisterIndex = param.Index;
                            rationRegister.DormRationIndex = ration.DormRationIndex;
                            rationRegister.CreatedDate = DateTime.Now;
                            rationRegister.UpdatedDate = DateTime.Now;
                            rationRegister.UpdatedUser = user.FullName;
                            rationRegister.CompanyIndex = user.CompanyIndex;
                            listDormRegisterRation.Add(rationRegister);
                        }
                        await DbContext.HR_DormRegister_Ration.AddRangeAsync(listDormRegisterRation);
                        
                    }
                    if (param.DormRegisterActivity != null && param.DormRegisterActivity.Count > 0)
                    {
                        var listDormRegisterActivity = new List<HR_DormRegister_Activity>();
                        foreach (var activity in param.DormRegisterActivity)
                        {
                            var activityRegister = new HR_DormRegister_Activity();
                            activityRegister.DormRegisterIndex = param.Index;
                            activityRegister.DormActivityIndex = activity.DormActivityIndex;
                            activityRegister.DormAccessMode = activity.DormAccessMode;
                            activityRegister.CreatedDate = DateTime.Now;
                            activityRegister.UpdatedDate = DateTime.Now;
                            activityRegister.UpdatedUser = user.FullName;
                            activityRegister.CompanyIndex = user.CompanyIndex;
                            listDormRegisterActivity.Add(activityRegister);
                        }
                        await DbContext.HR_DormRegister_Activity.AddRangeAsync(listDormRegisterActivity);
                        
                    }
                }
                await DbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                result = false;
            }
            return result;
        }

        public async Task<bool> DeleteDormRegister(List<int> param)
        {
            var result = true;
            try
            {
                var existedDormRegister = await DbContext.HR_DormRegister.Where(x => param.Contains(x.Index)).ToListAsync();
                if (existedDormRegister != null && existedDormRegister.Count > 0)
                {
                    DbContext.HR_DormRegister.RemoveRange(existedDormRegister);
                }
                var existedDormRegisterRation = await DbContext.HR_DormRegister_Ration.Where(x => param.Contains(x.DormRegisterIndex)).ToListAsync();
                if (existedDormRegisterRation != null && existedDormRegisterRation.Count > 0)
                {
                    DbContext.HR_DormRegister_Ration.RemoveRange(existedDormRegisterRation);
                }
                var existedDormRegisterActivity = await DbContext.HR_DormRegister_Activity.Where(x => param.Contains(x.DormRegisterIndex)).ToListAsync();
                if (existedDormRegisterActivity != null && existedDormRegisterActivity.Count > 0)
                {
                    DbContext.HR_DormRegister_Activity.RemoveRange(existedDormRegisterActivity);
                }
                await DbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                result = false;
            }
            return result;
        }

        public async Task<List<DormRegisterViewModel>> ValidationImportDormRegsiter(List<DormRegisterViewModel> param, UserInfo user)
        {
            var listParamEmployeeATIDs = param.Where(x => !string.IsNullOrWhiteSpace(x.EmployeeATID)).Select(x => x.EmployeeATID);
            var listExistedUser = await DbContext.HR_User.AsNoTracking().Where(x => listParamEmployeeATIDs.Contains(x.EmployeeATID)).ToListAsync();
            var listExcusedAbsentImport = await DbContext.HR_ExcusedAbsent.Where(x
                => listParamEmployeeATIDs.Contains(x.EmployeeATID)).ToListAsync();
            var errorList = new List<DormRegisterViewModel>();
            var checkMaxLength = param.Where(e => e.EmployeeATID.Length > 50).ToList();
            var checkIsNull = param.Where(e => string.IsNullOrWhiteSpace(e.EmployeeATID)
                || string.IsNullOrWhiteSpace(e.FromDateString) 
                || string.IsNullOrWhiteSpace(e.ToDateString)).ToList();

            if (checkMaxLength != null && checkMaxLength.Count() > 0)
            {
                foreach (var item in checkMaxLength)
                {
                    if (item.EmployeeATID.Length > 50) item.ErrorMessage += "Mã ID lớn hơn 50 ký tự" + "\r\n";
                }
            }
            if (checkIsNull != null && checkIsNull.Count() > 0)
            {
                foreach (var item in checkIsNull)
                {
                    if (string.IsNullOrWhiteSpace(item.EmployeeATID)) item.ErrorMessage += "Mã ID không được để trống\r\n";
                    if (string.IsNullOrWhiteSpace(item.FromDateString)) item.ErrorMessage += "Từ ngày không điểm danh không được để trống\r\n";
                    if (string.IsNullOrWhiteSpace(item.ToDateString)) item.ErrorMessage += "Đến ngày không điểm danh không được để trống\r\n";
                }
            }

            var listEmployeeATID = param.Where(x => !string.IsNullOrWhiteSpace(x.EmployeeATID)).Select(x => x.EmployeeATID).ToList();
            var listEmployee = await DbContext.HR_User.Where(x => listEmployeeATID.Contains(x.EmployeeATID)).ToListAsync();

            var listDormEmployeeCode = param.Where(x => !string.IsNullOrWhiteSpace(x.DormEmployeeCode)).Select(x => x.DormEmployeeCode).ToList();
            var listExistedDormByEmployeeCode = await DbContext.HR_DormRegister.AsNoTracking().Where(x 
                => listDormEmployeeCode.Contains(x.DormEmployeeCode)).ToListAsync();

            var listDataExist = new List<DormRegisterViewModel>();
            var listImportData = new List<DormRegisterViewModel>();
            string format = "dd/MM/yyyy";

            param.ForEach(x =>
            {
                if (!listEmployee.Any(y => y.EmployeeATID == x.EmployeeATID))
                {
                    x.ErrorMessage += "Nhân viên không tồn tại\r\n";
                }
                var isWrongFromToDate = false;
                if (DateTime.TryParseExact(x.FromDateString, format, null, System.Globalization.DateTimeStyles.None, out DateTime fromDate))
                {
                    x.FromDate = fromDate;
                }
                else
                { 
                    x.ErrorMessage += "Từ ngày không đúng format\r\n";
                    isWrongFromToDate = true;
                }
                if (DateTime.TryParseExact(x.ToDateString, format, null, System.Globalization.DateTimeStyles.None, out DateTime toDate))
                {
                    x.ToDate = toDate;
                }
                else
                {
                    x.ErrorMessage += "Đến ngày không đúng format\r\n";
                    isWrongFromToDate = true;
                }
                if (!isWrongFromToDate && !AreAllWeekendDates(x.FromDate, x.ToDate))
                {
                    x.ErrorMessage += "Chỉ đăng ký được cho thứ bảy, chủ nhật. Vui lòng kiểm tra lại.\r\n";
                }
                if (x.StayInDorm)
                {
                    if (x.DormRoomIndex <= 0)
                    {
                        x.ErrorMessage += "Phòng không được để trống khi đăng ký ở lại\r\n";
                    }
                    if (string.IsNullOrWhiteSpace(x.DormEmployeeCode))
                    {
                        x.ErrorMessage += "Mã số không được để trống khi đăng ký ở lại\r\n";
                    }
                    else if(!string.IsNullOrWhiteSpace(x.DormEmployeeCode) 
                        && listExistedDormByEmployeeCode.Any(y => y.EmployeeATID == x.EmployeeATID 
                            && y.DormEmployeeCode != x.DormEmployeeCode)) 
                    {
                        x.ErrorMessage += "Mã số đã tồn tại\r\n";
                    }
                }   
                if (!x.StayInDorm && x.DormLeaveIndex <= 0)
                {
                    x.ErrorMessage += "Đăng ký di chuyển không được để trống khi không đăng ký ở lại\r\n";
                }
                if (string.IsNullOrWhiteSpace(x.ErrorMessage))
                {
                    if (listDataExist.Any(y => y.EmployeeATID == x.EmployeeATID
                            && x.FromDate.Date <= y.ToDate.Date && x.ToDate.Date >= y.FromDate.Date))
                    {
                        x.ErrorMessage += "Thông tin đăng ký ký túc xá đã tồn tại trong tệp tin\r\n";
                    }
                    else 
                    {
                        var listDate = ListDate(x.FromDate, x.ToDate);
                        foreach (var date in listDate)
                        {
                            var importData = new DormRegisterViewModel();
                            importData = ObjectExtensions.CopyToNewObject(x);
                            importData.RegisterDate = date;
                            listImportData.Add(importData);
                        }
                        listDataExist.Add(x);
                    }
                }
            });

            if (listImportData != null && listImportData.Count > 0)
            {
                var listDormRegister = new List<HR_DormRegister>();
                var existedImportData = new List<HR_DormRegister>();

                var listImporEmployeeATID = listImportData.Select(x => x.EmployeeATID).ToList();
                existedImportData = await DbContext.HR_DormRegister.Where(x => listImporEmployeeATID.Contains(x.EmployeeATID)).ToListAsync();
                foreach (var item in listImportData)
                {
                    if (existedImportData.Any(x => x.EmployeeATID == item.EmployeeATID && x.RegisterDate.Date == item.RegisterDate.Date))
                    {
                        var existedData = existedImportData.FirstOrDefault(x => x.EmployeeATID == item.EmployeeATID
                            && x.RegisterDate.Date == item.RegisterDate.Date);
                        existedData.StayInDorm = item.StayInDorm;
                        existedData.DormRoomIndex = item.DormRoomIndex;
                        existedData.DormEmployeeCode = item.DormEmployeeCode;
                        existedData.DormLeaveIndex = item.DormLeaveIndex;
                        existedData.UpdatedDate = DateTime.Now;
                        existedData.UpdatedUser = user.FullName;
                    }
                    else
                    {
                        var importNew = new HR_DormRegister
                        {
                            EmployeeATID = item.EmployeeATID,
                            RegisterDate = item.RegisterDate,
                            StayInDorm = item.StayInDorm,
                            DormRoomIndex = item.DormRoomIndex,
                            DormEmployeeCode = item.DormEmployeeCode,
                            DormLeaveIndex = item.DormLeaveIndex,
                            CreatedDate = DateTime.Now,
                            UpdatedDate = DateTime.Now,
                            UpdatedUser = user.FullName,
                            CompanyIndex = user.CompanyIndex
                        };
                        listDormRegister.Add(importNew);
                    }
                }
                if (listDormRegister != null && listDormRegister.Count > 0)
                {
                    await DbContext.HR_DormRegister.AddRangeAsync(listDormRegister);
                }
                await DbContext.SaveChangesAsync();

                var importedData = await DbContext.HR_DormRegister.AsNoTracking().Where(x 
                    => listImporEmployeeATID.Contains(x.EmployeeATID)).ToListAsync();
                var importedDataRation = new List<HR_DormRegister_Ration>();
                var importedDataActivity = new List<HR_DormRegister_Activity>();
                if (importedData != null && importedData.Count > 0)
                {
                    var importedDataIndexes = importedData.Select(x => x.Index).ToList();
                    importedDataRation = await DbContext.HR_DormRegister_Ration.AsNoTracking().Where(x
                        => importedDataIndexes.Contains(x.DormRegisterIndex)).ToListAsync();
                    importedDataActivity = await DbContext.HR_DormRegister_Activity.AsNoTracking().Where(x
                        => importedDataIndexes.Contains(x.DormRegisterIndex)).ToListAsync();
                    foreach (var item in listImportData)
                    {
                        var itemData = importedData.FirstOrDefault(x => x.EmployeeATID == item.EmployeeATID
                            && x.RegisterDate.Date == item.RegisterDate.Date);
                        if (itemData != null)
                        {
                            var itemDataRation = importedDataRation.Where(x => x.DormRegisterIndex == itemData.Index).ToList();
                            DbContext.HR_DormRegister_Ration.RemoveRange(itemDataRation);
                            var itemDataActivity = importedDataActivity.Where(x => x.DormRegisterIndex == itemData.Index).ToList();
                            DbContext.HR_DormRegister_Activity.RemoveRange(itemDataActivity);

                            if (item.StayInDorm)
                            {
                                if (item.DormRegisterRation != null && item.DormRegisterRation.Count > 0)
                                {
                                    foreach (var itemRation in item.DormRegisterRation)
                                    {
                                        await DbContext.HR_DormRegister_Ration.AddAsync(new HR_DormRegister_Ration
                                        {
                                            DormRegisterIndex = itemData.Index,
                                            DormRationIndex = itemRation.DormRationIndex,
                                            CreatedDate = DateTime.Now,
                                            UpdatedDate = DateTime.Now,
                                            UpdatedUser = user.FullName,
                                            CompanyIndex = user.CompanyIndex
                                        });
                                    }
                                }
                                if (item.DormRegisterActivity != null && item.DormRegisterActivity.Count > 0)
                                {
                                    foreach (var itemActivity in item.DormRegisterActivity)
                                    {
                                        await DbContext.HR_DormRegister_Activity.AddAsync(new HR_DormRegister_Activity
                                        {
                                            DormRegisterIndex = itemData.Index,
                                            DormActivityIndex = itemActivity.DormActivityIndex,
                                            DormAccessMode = itemActivity.DormAccessMode,
                                            CreatedDate = DateTime.Now,
                                            UpdatedDate = DateTime.Now,
                                            UpdatedUser = user.FullName,
                                            CompanyIndex = user.CompanyIndex
                                        });
                                    }
                                }
                            }
                        }
                    }
                }
                await DbContext.SaveChangesAsync();
            }            

            errorList = param.Where(x => !string.IsNullOrEmpty(x.ErrorMessage)).ToList();
            return errorList;
        }

        private List<DateTime> ListDate(DateTime fromDate, DateTime toDate)
        {
            var currentDate = fromDate;
            List<DateTime> weekendDates = new List<DateTime>();

            while (currentDate <= toDate)
            {
                weekendDates.Add(currentDate.Date);
                currentDate = currentDate.AddDays(1);
            }

            return weekendDates;
        }

        private bool AreAllWeekendDates(DateTime fromDate, DateTime toDate)
        {
            var currentDate = fromDate;
            List<DateTime> weekendDates = new List<DateTime>();

            while (currentDate <= toDate)
            {
                weekendDates.Add(currentDate.Date);
                currentDate = currentDate.AddDays(1);
            }

            bool allWeekend = weekendDates.TrueForAll(IsWeekend);

            return allWeekend;
        }

        private bool IsWeekend(DateTime date)
        {
            return date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;
        }
    }
}
