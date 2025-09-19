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
    public class HR_ExcusedAbsentService : BaseServices<HR_ExcusedAbsent, EPAD_Context>, IHR_ExcusedAbsentService
    {
        private ILogger _logger;
        public HR_ExcusedAbsentService(IServiceProvider serviceProvider, ILoggerFactory loggerFactory) : base(serviceProvider)
        {
            _logger = loggerFactory.CreateLogger<HR_ExcusedAbsentService>();
        }

        public async Task<DataGridClass> GetExcusedAbsentAtPage(ExcusedAbsentRequest requestParam, UserInfo user)
        {
            var result = new DataGridClass(0, null);
            var query = from ea in DbContext.HR_ExcusedAbsent.Where(x => x.CompanyIndex == user.CompanyIndex)
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
                            AbsentDate = ea.AbsentDate,
                            ExcusedAbsentReasonIndex = ea.ExcusedAbsentReasonIndex,
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
                        query = query.Where(x => x.AbsentDate.Date >= fromDate.Date);
                    }
                }

                if (!string.IsNullOrWhiteSpace(requestParam.to))
                {
                    if (DateTime.TryParseExact(requestParam.to, format, null,
                        System.Globalization.DateTimeStyles.None, out DateTime toDate))
                    {
                        query = query.Where(x => x.AbsentDate.Date <= toDate.Date);
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

        public async Task<List<HR_ExcusedAbsentReason>> GetExcusedAbsentReason(UserInfo user)
        {
            return await DbContext.HR_ExcusedAbsentReason.Where(x => x.CompanyIndex == user.CompanyIndex).ToListAsync();
        }

        public async Task<List<HR_ExcusedAbsent>> GetByEmployeeATIDs(List<string> listEmployeeATIDs)
        {
            return await DbContext.HR_ExcusedAbsent.AsNoTracking().Where(x => listEmployeeATIDs.Contains(x.EmployeeATID)).ToListAsync(); 
        }

        public async Task<List<HR_ExcusedAbsent>> GetByListIndex(List<int> index)
        {
            return await DbContext.HR_ExcusedAbsent.AsNoTracking().Where(x => index.Contains(x.Index)).ToListAsync();
        }

        public async Task<HR_ExcusedAbsent> GetByIndex(int index)
        {
            return await DbContext.HR_ExcusedAbsent.AsNoTracking().FirstOrDefaultAsync(x => index == x.Index);
        }

        public async Task<bool> AddExcusedAbsent(ExcusedAbsentParam param, UserInfo user)
        {
            var result = true;
            try
            {
                if (param.EmployeeATIDs != null && param.EmployeeATIDs.Count > 0)
                {
                    param.EmployeeATIDs.ForEach(x =>
                    {
                        DbContext.HR_ExcusedAbsent.Add(new HR_ExcusedAbsent
                        {
                            EmployeeATID = x,
                            AbsentDate = param.AbsentDate,
                            ExcusedAbsentReasonIndex = param.ExcusedAbsentReasonIndex,
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

        public async Task<bool> UpdateExcusedAbsent(ExcusedAbsentParam param, UserInfo user)
        {
            var result = true;
            try
            {
                var existedExcusedAbsent = await DbContext.HR_ExcusedAbsent.FirstOrDefaultAsync(x => x.Index == param.Index);
                if (existedExcusedAbsent != null)
                {
                    existedExcusedAbsent.ExcusedAbsentReasonIndex = param.ExcusedAbsentReasonIndex;
                    existedExcusedAbsent.Description = param.Description;
                    existedExcusedAbsent.UpdatedDate = DateTime.Now;
                    existedExcusedAbsent.UpdatedUser = user.FullName;
                    await DbContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                result = false;
            }
            return result;
        }

        public async Task<bool> DeleteExcusedAbsent(List<int> param)
        {
            var result = true;
            try
            {
                var existedExcusedAbsent = await GetByListIndex(param);
                if (existedExcusedAbsent != null)
                {
                    DbContext.HR_ExcusedAbsent.RemoveRange(existedExcusedAbsent);
                }
                await DbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                result = false;
            }
            return result;
        }

        public bool ExportTemplateExcusedAbsent(string folderDetails)
        {
            try
            {
                var employeeTypeList = DbContext.HR_ExcusedAbsentReason.AsNoTracking().Select(x => x.Name).ToList();

                using (var workbook = new XLWorkbook(folderDetails))
                {
                    var worksheet = workbook.Worksheets;
                    IXLWorksheet worksheet1;
                    IXLWorksheet worksheet5;

                    var w1 = worksheet.TryGetWorksheet("ExcusedAbsentReasonData", out worksheet1);
                    worksheet1.Cells().Clear();
                    string startCanteenCell = "A1";
                    string endCanteenCell = string.Empty;
                    for (int i = 0; i < employeeTypeList.Count; i++)
                    {
                        if (i == (employeeTypeList.Count - 1))
                        {
                            endCanteenCell = "A" + (i + 1);
                        }
                        worksheet1.Cell("A" + (i + 1)).Value = employeeTypeList[i];
                    }

                    var w = worksheet.TryGetWorksheet("Sheet1", out worksheet5);
                    if (!string.IsNullOrWhiteSpace(startCanteenCell) && !string.IsNullOrWhiteSpace(endCanteenCell))
                        worksheet5.Range("E2:E10002").SetDataValidation().List(worksheet1.Range(startCanteenCell
                            + ":" + endCanteenCell), true);

                    workbook.Save();
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError("ExportDataExcusedAbsentReason: " + ex.ToString());
                return false;
            }
        }

        public async Task<List<ExcusedAbsentImportParam>> ValidationImportExcusedAbsent(List<ExcusedAbsentImportParam> param, UserInfo user)
        {
            var listParamEmployeeATIDs = param.Where(x => !string.IsNullOrWhiteSpace(x.EmployeeATID)).Select(x => x.EmployeeATID);
            var listExistedUser = await DbContext.HR_User.AsNoTracking().Where(x => listParamEmployeeATIDs.Contains(x.EmployeeATID)).ToListAsync();
            var listExcusedAbsentImport = await DbContext.HR_ExcusedAbsent.Where(x 
                => listParamEmployeeATIDs.Contains(x.EmployeeATID)).ToListAsync();
            var errorList = new List<ExcusedAbsentImportParam>();
            var checkMaxLength = param.Where(e => e.EmployeeATID.Length > 50
                || e.Description.Length > 2000
            ).ToList();
            var checkIsNull = param.Where(e => string.IsNullOrWhiteSpace(e.EmployeeATID) 
                || string.IsNullOrWhiteSpace(e.AbsentDate) || string.IsNullOrWhiteSpace(e.ExcusedAbsentReason)).ToList();

            //var checkExistedCode = param.Where(e => !string.IsNullOrWhiteSpace(e.EmployeeATID) 
            //    && !string.IsNullOrWhiteSpace(e.AbsentDate) 
            //    && listDepartmentNameCodeImport.Any(x => x.EmployeeATID == e.EmployeeATID 
            //    && x.AbsentDate.Date.ToString("dd/MM/yyyy") == e.AbsentDate)).ToList();

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
                    if (string.IsNullOrWhiteSpace(item.AbsentDate)) item.ErrorMessage += "Ngày không điểm danh không được để trống\r\n";
                    if (string.IsNullOrWhiteSpace(item.ExcusedAbsentReason)) item.ErrorMessage += "Lý do không điểm danh không được để trống\r\n";
                }
            }

            //if (checkExistedCode != null && checkExistedCode.Count > 0)
            //{
            //    foreach (var item in checkExistedCode)
            //    {
            //        item.ErrorMessage += "Thông tin vắng đã tồn tại\r\n";
            //    }
            //}

            var listImportExusedAbsentReason = param.Where(x => !string.IsNullOrWhiteSpace(x.ExcusedAbsentReason)).Select(x 
                => x.ExcusedAbsentReason).ToList();
            var listExcusedAbsentReason = DbContext.HR_ExcusedAbsentReason.AsNoTracking().Where(x
                => listImportExusedAbsentReason.Contains(x.Name)).ToList();

            var listDataExist = new List<ExcusedAbsentImportParam>();
            string format = "dd/MM/yyyy";
            var isAddOrUpdate = false;
            param.ForEach(x =>
            {
                var employeeExistExcel = listDataExist.FirstOrDefault(y => y.EmployeeATID == x.EmployeeATID
                            && y.AbsentDate == x.AbsentDate);
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
                var excusedAbsentReason = listExcusedAbsentReason.FirstOrDefault(y => y.Name == x.ExcusedAbsentReason);
                if (!string.IsNullOrWhiteSpace(x.ExcusedAbsentReason) && excusedAbsentReason == null)
                {
                    x.ErrorMessage += "Lý do không điểm danh không tồn tại\r\n";
                }
                if (!string.IsNullOrWhiteSpace(x.AbsentDate))
                {
                    if (DateTime.TryParseExact(x.AbsentDate, format, null, System.Globalization.DateTimeStyles.None, out DateTime absentDate))
                    {
                        if (string.IsNullOrWhiteSpace(x.ErrorMessage))
                        {
                            if (listExcusedAbsentImport.Any(y => y.EmployeeATID == x.EmployeeATID
                                && y.AbsentDate.Date == absentDate.Date))
                            {
                                var existedExcusedAbsent = listExcusedAbsentImport.FirstOrDefault(y => y.EmployeeATID == x.EmployeeATID
                                && y.AbsentDate.Date == absentDate.Date);
                                existedExcusedAbsent.ExcusedAbsentReasonIndex = excusedAbsentReason.Index;
                                existedExcusedAbsent.Description = x.Description;
                                existedExcusedAbsent.UpdatedDate = DateTime.Now;
                                existedExcusedAbsent.UpdatedUser = user.FullName;
                                listDataExist.Add(x);
                            }
                            else
                            {
                                DbContext.HR_ExcusedAbsent.Add(new HR_ExcusedAbsent
                                {
                                    EmployeeATID = x.EmployeeATID,
                                    AbsentDate = absentDate,
                                    ExcusedAbsentReasonIndex = excusedAbsentReason.Index,
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
                        x.ErrorMessage += "Ngày không điểm danh không đúng format\r\n";
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
