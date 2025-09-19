using EPAD_Common.Services;
using EPAD_Data.Entities;
using EPAD_Data;
using EPAD_Services.Interface;
using System;
using System.Collections.Generic;
using System.Text;
using EPAD_Common.Types;
using System.Linq;
using EPAD_Data.Models;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EPAD_Data.Models.IC;

namespace EPAD_Services.Impl
{
    public class TA_AnnualLeaveService : BaseServices<TA_AnnualLeave, EPAD_Context>, ITA_AnnualLeaveService
    {
        public EPAD_Context _dbContext;
        public TA_AnnualLeaveService(IServiceProvider serviceProvider, EPAD_Context context) : base(serviceProvider)
        {
            _dbContext = context;
        }

        public DataGridClass GetDataGrid(int pCompanyIndex, int pPage, int pLimit, string filter, List<long> departments, List<string> employeeatids)
        {
            DataGridClass dataGrid = null;
            int countPage = 0;
            var filterBy = new List<string>();
            if (filter != null)
            {
                filterBy = filter.Split(" ").Select(x => x.ToLower()).ToList();
            }
            var doors = (from annualLeave in DbContext.TA_AnnualLeave.Where(t => t.CompanyIndex == pCompanyIndex)
                         join user in DbContext.HR_User.Where(x => x.CompanyIndex == pCompanyIndex)
                         on annualLeave.EmployeeATID equals user.EmployeeATID into temp
                         from dummy in temp.DefaultIfEmpty()

                         join wk in DbContext.IC_WorkingInfo.Where(x => x.CompanyIndex == pCompanyIndex)
                             on dummy.EmployeeATID equals wk.EmployeeATID into wCheck
                         from wResult in wCheck.DefaultIfEmpty()

                         join de in DbContext.IC_Department.Where(x => x.CompanyIndex == pCompanyIndex && x.IsInactive != true)
                             on wResult.DepartmentIndex equals de.Index into deCheck
                         from deResult in deCheck.DefaultIfEmpty()
                         where
                            //  (string.IsNullOrEmpty(filter)
                            //? annualLeave.EmployeeATID.Contains("")
                            //: (
                            //       annualLeave.EmployeeATID.Contains(filter)
                            //    || filterBy.Contains(annualLeave.EmployeeATID)
                            //   || (!string.IsNullOrWhiteSpace(dummy.EmployeeCode) && (dummy.EmployeeCode == filter || dummy.EmployeeCode.Contains(filter) || filterBy.Contains(dummy.EmployeeCode)))
                            //   || (!string.IsNullOrWhiteSpace(dummy.FullName) && (dummy.FullName == filter || dummy.FullName.Contains(filter) || filterBy.Contains(dummy.FullName)))
                            //)) && 
                      ((departments != null && departments.Count > 0) ? departments.Contains(wResult.DepartmentIndex) : true)
                       && ((employeeatids != null && employeeatids.Count > 0) ? employeeatids.Contains(annualLeave.EmployeeATID) : true)
                        && wResult.Status == 1 && (!wResult.ToDate.HasValue
                        || (wResult.ToDate.HasValue && wResult.ToDate.Value.Date >= DateTime.Now.Date))
                         select new TA_AnnualLeaveDTO
                         {
                             EmployeeATID = annualLeave.EmployeeATID,
                             FullName = dummy.FullName,
                             DepartmentName = deResult.Name,
                             EmployeeCode = dummy.EmployeeCode,
                             Index = annualLeave.Index,
                             DepartmentIndex = wResult.DepartmentIndex,
                             AnnualLeave = annualLeave.AnnualLeave
                         }).ToList();

            if (!string.IsNullOrWhiteSpace(filter))
            {
                doors = doors.Where(x => filterBy.Contains(x.EmployeeATID) || filter.Contains(x.EmployeeATID)
                || (!string.IsNullOrWhiteSpace(x.EmployeeCode) && (filter.ToLower().Contains(x.EmployeeCode.ToLower())
                || filter.ToLower() == x.EmployeeCode.ToLower() || x.EmployeeCode.ToLower().Contains(filter.ToLower())))
                || (!string.IsNullOrWhiteSpace(x.FullName) && (filter.ToLower().Contains(x.FullName.ToLower())
                || filter.ToLower() == x.FullName.ToLower() || x.FullName.ToLower().Contains(filter.ToLower())))).ToList();
            }

            countPage = doors.Count();

            dataGrid = new DataGridClass(countPage, doors);
            if (pPage <= 1)
            {
                var lstAnnualLeave = doors.OrderBy(t => t.EmployeeATID).Take(pLimit).ToList();
                dataGrid = new DataGridClass(countPage, lstAnnualLeave);
            }
            else
            {
                int fromRow = pLimit * (pPage - 1);
                var lstAnnualLeave = doors.OrderBy(t => t.EmployeeATID).Skip(fromRow).Take(pLimit).ToList();
                dataGrid = new DataGridClass(countPage, lstAnnualLeave);
            }
            return dataGrid;
        }

        public async Task<bool> AddAnnualLeave(TA_AnnualLeaveInsertParam data, UserInfo user)
        {
            var result = true;
            try
            {
                foreach (var item in data.EmployeeATIDs)
                {
                    var entity = new TA_AnnualLeave
                    {
                        EmployeeATID = item,
                        AnnualLeave = data.AnnualLeave,
                        CompanyIndex = user.CompanyIndex,
                        CreatedDate = DateTime.Now,
                        UpdatedUser = user.FullName
                    };

                    await _dbContext.TA_AnnualLeave.AddAsync(entity);
                }
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                result = false;
            }
            return result;
        }

        public async Task<bool> UpdateAnnualLeave(TA_AnnualLeaveInsertParam data, UserInfo user)
        {
            var result = true;
            var annualLeave = _dbContext.TA_AnnualLeave.FirstOrDefault(x => x.Index == data.Index);
            try
            {
                if (annualLeave != null)
                {
                    annualLeave.AnnualLeave = data.AnnualLeave;
                    _dbContext.TA_AnnualLeave.Update(annualLeave);
                    await _dbContext.SaveChangesAsync();
                }
                else
                {
                    return false;
                }

            }
            catch (Exception ex)
            {
                result = false;
            }
            return result;
        }

        public async Task<bool> DeleteAnnualLeave(int index)
        {
            var result = true;
            try
            {
                var data = await _dbContext.TA_AnnualLeave.FirstOrDefaultAsync(x => x.Index == index);
                _dbContext.TA_AnnualLeave.Remove(data);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                result = false;
            }
            return result;
        }

        public async Task<List<TA_AnnualLeave>> GetAnnualLeaveEmployeeATID(string employeeATID, int companyIndex)
        {
            return await _dbContext.TA_AnnualLeave.Where(x => x.EmployeeATID == employeeATID && x.CompanyIndex == companyIndex).ToListAsync();
        }

        public async Task<List<TA_AnnualLeave>> GetAnnualLeaveEmployeeATIDs(List<string> employeeATIDs, int companyIndex)
        {
            return await _dbContext.TA_AnnualLeave.Where(x => employeeATIDs.Contains(x.EmployeeATID) && x.CompanyIndex == companyIndex).ToListAsync();
        }


        public async Task<TA_AnnualLeave> GetAnnualLeaveByIndex(int index, int companyIndex)
        {
            return await _dbContext.TA_AnnualLeave.FirstOrDefaultAsync(x => x.Index == index && x.CompanyIndex == companyIndex);
        }

        public async Task<bool> DeleteAnnualLeave(List<int> index)
        {
            var result = true;
            try
            {
                var data = await _dbContext.TA_AnnualLeave.Where(x => index.Contains(x.Index)).ToListAsync();
                if (data.Count > 0)
                {
                    _dbContext.TA_AnnualLeave.RemoveRange(data);
                    await _dbContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                result = false;
            }
            return result;
        }

        public List<TA_AnnualLeaveImportParam> ValidationImportAnnualLeave(List<TA_AnnualLeaveImportParam> param, UserInfo user)
        {
            var errorList = new List<TA_AnnualLeaveImportParam>();
            var lstEmployeeATIs = param.Select(x => x.EmployeeATID).ToList();
            var employeeATIs = _dbContext.HR_User.Where(x => lstEmployeeATIs.Contains(x.EmployeeATID)).Select(x => x.EmployeeATID).ToList();

            var employeeATIDNotExist = param.Where(x => !lstEmployeeATIs.Contains(x.EmployeeATID)).ToList();
            if (employeeATIDNotExist != null && employeeATIDNotExist.Count() > 0)
            {
                foreach (var item in employeeATIDNotExist)
                {
                    item.ErrorMessage += "Mã chấm công không tồn tại\r\n";
                }
            }

            var checkDuplicateEmployee = param.GroupBy(x => x.EmployeeATID).Where(x => x.Count() > 1).Select(x => x.Key).ToList();
            if (checkDuplicateEmployee != null && checkDuplicateEmployee.Count() > 0)
            {
                var duplicate = param.Where(e => checkDuplicateEmployee.Contains(e.EmployeeATID)).ToList();
                foreach (var item in duplicate)
                {
                    item.ErrorMessage = "Trùng mã chấm công\r\n";
                }
            }

            foreach (var item in param)
            {
                item.AnnualLeave = item.AnnualLeave.Replace(",", ".");
                double n;
                bool isNumeric = double.TryParse(item.AnnualLeave, out n);
                if (!isNumeric)
                {
                    item.ErrorMessage = "Phép năm phải là kiểu số\r\n";
                }
            }
            errorList = param.Where(x => !string.IsNullOrEmpty(x.ErrorMessage)).ToList();

            var dataSave = param.Where(x => string.IsNullOrEmpty(x.ErrorMessage)).ToList();
            var employeeATIDsSave = param.Select(x => x.EmployeeATID).ToList();
            if (dataSave != null && dataSave.Count() > 0)
            {
                var lstAnnualLeave = _dbContext.TA_AnnualLeave.Where(x => employeeATIDsSave.Contains(x.EmployeeATID)).ToList();
                foreach (var item in dataSave)
                {
                    var existAnnualLeave = lstAnnualLeave.FirstOrDefault(x => x.EmployeeATID == item.EmployeeATID);
                    double n;
                    bool isNumeric = double.TryParse(item.AnnualLeave, out n);

                    if (existAnnualLeave != null)
                    {
                        existAnnualLeave.AnnualLeave = n;
                        existAnnualLeave.UpdatedUser = user.FullName;

                        _dbContext.TA_AnnualLeave.Update(existAnnualLeave);
                    }
                    else
                    {
                        existAnnualLeave = new TA_AnnualLeave()
                        {
                            AnnualLeave = n,
                            EmployeeATID = item.EmployeeATID,
                            CreatedDate = DateTime.Now,
                            UpdatedUser = user.FullName
                        };
                        _dbContext.TA_AnnualLeave.Add(existAnnualLeave);
                    }
                }

                _dbContext.SaveChanges();
            }


            return errorList;
        }
    }
}
