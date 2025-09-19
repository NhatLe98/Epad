using ClosedXML.Excel;
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
    public class HR_ExcusedLateEntryService : BaseServices<HR_ExcusedLateEntry, EPAD_Context>, IHR_ExcusedLateEntryService
    {
        private ILogger _logger;
        public HR_ExcusedLateEntryService(IServiceProvider serviceProvider, ILoggerFactory loggerFactory) : base(serviceProvider)
        {
            _logger = loggerFactory.CreateLogger<HR_ExcusedLateEntryService>();
        }

        public async Task<DataGridClass> GetExcusedLateEntryAtPage(ExcusedLateEntryRequest requestParam, UserInfo user)
        {
            var result = new DataGridClass(0, null);
            var query = from ea in DbContext.HR_ExcusedLateEntry.Where(x => x.CompanyIndex == user.CompanyIndex)
                        join u in DbContext.HR_User.Where(x => x.CompanyIndex == user.CompanyIndex)
                        on ea.EmployeeATID equals u.EmployeeATID
                        join w in DbContext.IC_WorkingInfo.Where(w => w.CompanyIndex == user.CompanyIndex
                            && w.Status == (short)TransferStatus.Approve
                            && w.FromDate.Date <= DateTime.Now.Date && (!w.ToDate.HasValue 
                                || (w.ToDate.HasValue && w.ToDate.Value.Date > DateTime.Now.Date)))
                        on u.EmployeeATID equals w.EmployeeATID into workingInfo
                        from wkResult in workingInfo.DefaultIfEmpty()
                        join d in DbContext.IC_Department.Where(x => x.CompanyIndex == user.CompanyIndex && x.IsInactive != true)
                        on wkResult.DepartmentIndex equals d.Index into depInfo
                        from depResult in depInfo.DefaultIfEmpty()
                        select new
                        {
                            Index = ea.Index,
                            EmployeeATID = ea.EmployeeATID,
                            FullName = u.FullName,
                            LateDate = ea.LateDate,
                            Description = ea.Description,
                            DepartmentIndex = depResult != null ? depResult.Index : 0,
                            DepartmentName = depResult != null ? depResult.Name : string.Empty,
                            CreatedDate = ea.CreatedDate,
                            UpdatedDate = ea.UpdatedDate,
                            UpdatedUser = ea.UpdatedUser
                        };

            var format = "yyyy-MM-dd";
            if (requestParam != null)
            {
                if (!string.IsNullOrWhiteSpace(requestParam.filter))
                {
                    query = query.Where(x => x.EmployeeATID.Contains(requestParam.filter)
                    || x.FullName.Contains(requestParam.filter) || x.DepartmentName.Contains(requestParam.filter)
                    || x.Description.Contains(requestParam.filter));
                }

                if (!string.IsNullOrWhiteSpace(requestParam.from))
                {
                    if (DateTime.TryParseExact(requestParam.from, format, null,
                        System.Globalization.DateTimeStyles.None, out DateTime fromDate))
                    {
                        query = query.Where(x => x.LateDate.Date >= fromDate.Date);
                    }
                }

                if (!string.IsNullOrWhiteSpace(requestParam.to))
                {
                    if (DateTime.TryParseExact(requestParam.to, format, null,
                        System.Globalization.DateTimeStyles.None, out DateTime toDate))
                    {
                        query = query.Where(x => x.LateDate.Date <= toDate.Date);
                    }
                }

                if (requestParam.departments != null && requestParam.departments.Count > 0)
                {
                    query = query.Where(x => requestParam.departments.Contains(x.DepartmentIndex));
                }
            }

            result.total = query.Count();
            result.data = await query.Skip((requestParam.page - 1) * requestParam.limit).Take(requestParam.limit).ToListAsync();

            return result;
        }

        public async Task<List<HR_ExcusedLateEntry>> GetByEmployeeATIDs(List<string> listEmployeeATIDs)
        {
            return await DbContext.HR_ExcusedLateEntry.AsNoTracking().Where(x => listEmployeeATIDs.Contains(x.EmployeeATID)).ToListAsync(); 
        }

        public async Task<List<HR_ExcusedLateEntry>> GetByListIndex(List<int> index)
        {
            return await DbContext.HR_ExcusedLateEntry.AsNoTracking().Where(x => index.Contains(x.Index)).ToListAsync();
        }

        public async Task<HR_ExcusedLateEntry> GetByIndex(int index)
        {
            return await DbContext.HR_ExcusedLateEntry.AsNoTracking().FirstOrDefaultAsync(x => index == x.Index);
        }

        public async Task<bool> AddExcusedLateEntry(ExcusedLateEntryParam param, UserInfo user)
        {
            var result = true;
            try
            {
                if (param.EmployeeATIDs != null && param.EmployeeATIDs.Count > 0)
                {
                    param.EmployeeATIDs.ForEach(x =>
                    {
                        DbContext.HR_ExcusedLateEntry.Add(new HR_ExcusedLateEntry
                        {
                            EmployeeATID = x,
                            LateDate = param.LateDate,
                            Description = param.Description,
                            CompanyIndex = user.CompanyIndex,
                            CreatedDate = DateTime.Now,
                            UpdatedDate = DateTime.Now,
                            UpdatedUser = user.FullName
                        });
                    });
                }
                await DbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                result = false;
            }
            return result;
        }

        public async Task<bool> UpdateExcusedLateEntry(ExcusedLateEntryParam param, UserInfo user)
        {
            var result = true;
            try
            {
                var existedExcusedLateEntry = await DbContext.HR_ExcusedLateEntry.FirstOrDefaultAsync(x => x.Index == param.Index);
                if (existedExcusedLateEntry != null)
                {
                    existedExcusedLateEntry.Description = param.Description;
                    existedExcusedLateEntry.UpdatedDate = DateTime.Now;
                    existedExcusedLateEntry.UpdatedUser = user.FullName;
                    await DbContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                result = false;
            }
            return result;
        }

        public async Task<bool> DeleteExcusedLateEntry(List<int> param)
        {
            var result = true;
            try
            {
                var existedExcusedLateEntry = await GetByListIndex(param);
                if (existedExcusedLateEntry != null)
                {
                    DbContext.HR_ExcusedLateEntry.RemoveRange(existedExcusedLateEntry);
                }
                await DbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                result = false;
            }
            return result;
        }

        public async Task<List<ExcusedLateEntryImportParam>> ValidationImportExcusedLateEntry(List<ExcusedLateEntryImportParam> param, UserInfo user)
        {
            var listParamEmployeeATIDs = param.Where(x => !string.IsNullOrWhiteSpace(x.EmployeeATID)).Select(x => x.EmployeeATID);
            var listExistedUser = await DbContext.HR_User.AsNoTracking().Where(x => listParamEmployeeATIDs.Contains(x.EmployeeATID)).ToListAsync();
            var listExcusedLateEntryImport = await DbContext.HR_ExcusedLateEntry.Where(x 
                => listParamEmployeeATIDs.Contains(x.EmployeeATID)).ToListAsync();
            var errorList = new List<ExcusedLateEntryImportParam>();
            var checkMaxLength = param.Where(e => e.EmployeeATID.Length > 50
                || e.Description.Length > 2000
            ).ToList();
            var checkIsNull = param.Where(e => string.IsNullOrWhiteSpace(e.EmployeeATID) 
                || string.IsNullOrWhiteSpace(e.LateDate)).ToList();

            //var checkExistedCode = param.Where(e => !string.IsNullOrWhiteSpace(e.EmployeeATID) 
            //    && !string.IsNullOrWhiteSpace(e.LateDate) 
            //    && listDepartmentNameCodeImport.Any(x => x.EmployeeATID == e.EmployeeATID 
            //    && x.LateDate.Date.ToString("dd/MM/yyyy") == e.LateDate)).ToList();

            if (checkMaxLength != null && checkMaxLength.Count() > 0)
            {
                foreach (var item in checkMaxLength)
                {
                    if (item.EmployeeATID.Length > 50) item.ErrorMessage += "Mã ID lớn hơn 50 ký tự" + "\r\n";
                    if (item.Description.Length > 2000) item.ErrorMessage += "Ghi chú lớn hơn 2000 ký tự" + "\r\n";
                }
            }
            if (checkIsNull != null && checkIsNull.Count() > 0)
            {
                foreach (var item in checkIsNull)
                {
                    if (string.IsNullOrWhiteSpace(item.EmployeeATID)) item.ErrorMessage += "Mã ID không được để trống\r\n";
                    if (string.IsNullOrWhiteSpace(item.LateDate)) item.ErrorMessage += "Ngày đi trễ không được để trống\r\n";
                }
            }

            //if (checkExistedCode != null && checkExistedCode.Count > 0)
            //{
            //    foreach (var item in checkExistedCode)
            //    {
            //        item.ErrorMessage += "Thông tin vắng đã tồn tại\r\n";
            //    }
            //}

            var listDataExist = new List<ExcusedLateEntryImportParam>();
            string format = "dd/MM/yyyy";
            var isAddOrUpdate = false;
            param.ForEach(x =>
            {
                var employeeExistExcel = listDataExist.FirstOrDefault(y => y.EmployeeATID == x.EmployeeATID
                            && y.LateDate == x.LateDate);
                if (employeeExistExcel != null)
                {
                    x.ErrorMessage += "Dữ liệu đăng ký bị trùng trong tập tin\r\n";
                }
                if (!string.IsNullOrWhiteSpace(x.EmployeeATID))
                {
                    var existUser = listExistedUser.FirstOrDefault(y => y.EmployeeATID == x.EmployeeATID);
                    if (existUser == null)
                    {
                        x.ErrorMessage += "Nhân viên không tồn tại\r\n";
                    }
                }
                if (!string.IsNullOrWhiteSpace(x.LateDate))
                {
                    if (DateTime.TryParseExact(x.LateDate, format, null, System.Globalization.DateTimeStyles.None, out DateTime LateDate))
                    {
                        if (string.IsNullOrWhiteSpace(x.ErrorMessage))
                        {
                            if (listExcusedLateEntryImport.Any(y => y.EmployeeATID == x.EmployeeATID
                                && y.LateDate.Date == LateDate.Date))
                            {
                                var existedExcusedLateEntry = listExcusedLateEntryImport.FirstOrDefault(y => y.EmployeeATID == x.EmployeeATID
                                && y.LateDate.Date == LateDate.Date);
                                existedExcusedLateEntry.Description = x.Description;
                                existedExcusedLateEntry.UpdatedDate = DateTime.Now;
                                existedExcusedLateEntry.UpdatedUser = user.FullName;
                                listDataExist.Add(x);
                            }
                            else
                            {
                                DbContext.HR_ExcusedLateEntry.Add(new HR_ExcusedLateEntry
                                {
                                    EmployeeATID = x.EmployeeATID,
                                    LateDate = LateDate,
                                    Description = x.Description,
                                    CompanyIndex = user.CompanyIndex,
                                    CreatedDate = DateTime.Now,
                                    UpdatedDate = DateTime.Now,
                                    UpdatedUser = user.FullName
                                });
                                listDataExist.Add(x);
                            }
                            isAddOrUpdate = true;
                        }
                    }
                    else
                    {
                        x.ErrorMessage += "Ngày đi trễ không đúng format\r\n";
                    }
                }
            });

            if (isAddOrUpdate)
            {
               await DbContext.SaveChangesAsync();
            }

            errorList = param.Where(x => !string.IsNullOrEmpty(x.ErrorMessage)).ToList();
            return errorList;
        }
    }
}
