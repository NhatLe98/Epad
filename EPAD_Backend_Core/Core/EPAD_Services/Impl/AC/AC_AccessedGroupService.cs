using EPAD_Common.Services;
using EPAD_Common.Types;
using EPAD_Data.Entities;
using EPAD_Data;
using EPAD_Services.Interface;
using System;
using System.Collections.Generic;
using System.Text;
using EPAD_Backend_Core.Migrations;
using System.Linq;
using EPAD_Data.Models;
using System.Threading.Tasks;

namespace EPAD_Services.Impl
{

    public class AC_AccessedGroupService : BaseServices<AC_AccessedGroup, EPAD_Context>, IAC_AccessedGroupService
    {
        public AC_AccessedGroupService(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public async Task<AC_AccessedGroup> GetByEmployeeAndFromToDate(string pEmployeeATID,UserInfo user,int groupValue)
        {
            var doorIndex = DbContext.AC_AccGroup.FirstOrDefault(x => x.CompanyIndex == user.CompanyIndex && x.UID == groupValue).DoorIndex;
            var accGroup  = DbContext.AC_AccGroup.Where(x => x.DoorIndex == doorIndex).Select(x => x.UID).ToList();
            var dummy = Where(x => x.CompanyIndex == user.CompanyIndex
            && x.EmployeeATID == pEmployeeATID
            && accGroup.Contains(x.GroupIndex)
            )
          .FirstOrDefault();
            return await Task.FromResult(dummy);
        }

        public async Task<AC_AccessedGroup> GetByEmployeeAndFromToDateEdit(string pEmployeeATID, UserInfo user, int groupValue)
        {
            var doorIndex = DbContext.AC_AccGroup.FirstOrDefault(x => x.CompanyIndex == user.CompanyIndex && x.UID == groupValue).DoorIndex;
            var accGroup = DbContext.AC_AccGroup.Where(x => x.DoorIndex == doorIndex).Select(x => x.UID).ToList();
            var dummy = Where(x => x.CompanyIndex == user.CompanyIndex
            && x.EmployeeATID == pEmployeeATID
            && accGroup.Contains(x.GroupIndex)
            && x.GroupIndex != groupValue
            )
          .FirstOrDefault();
            return await Task.FromResult(dummy);
        }

        public DataGridClass GetDataGrid(int pCompanyIndex, int pPage, int pLimit, string filter, List<int> groups, List<long> departments)
        {
            DataGridClass dataGrid = null;
            int countPage = 0;
            var filterBy = new List<string>();
            if (filter != null)
            {
                filterBy = filter.Split(" ").ToList();
            }
            var doors = (from accessGroupLst in DbContext.AC_AccessedGroup.Where(t => t.CompanyIndex == pCompanyIndex)
                         join user in DbContext.HR_User.Where(x => x.CompanyIndex == pCompanyIndex)
                         on accessGroupLst.EmployeeATID equals user.EmployeeATID into temp
                         from dummy in temp.DefaultIfEmpty()

                         join wk in DbContext.IC_WorkingInfo.Where(x => x.CompanyIndex == pCompanyIndex)
                             on dummy.EmployeeATID equals wk.EmployeeATID into wCheck
                         from wResult in wCheck.DefaultIfEmpty()

                         join de in DbContext.IC_Department.Where(x => x.CompanyIndex == pCompanyIndex && x.IsInactive != true)
                             on wResult.DepartmentIndex equals de.Index into deCheck
                         from deResult in deCheck.DefaultIfEmpty()

                         join ag in DbContext.AC_AccGroup.Where(x => x.CompanyIndex == pCompanyIndex)
                            on accessGroupLst.GroupIndex equals ag.UID into agCheck
                         from agResult in agCheck.DefaultIfEmpty()
                         where
                              (string.IsNullOrEmpty(filter)
                            ? accessGroupLst.EmployeeATID.Contains("")
                            : (
                                   accessGroupLst.EmployeeATID.Contains(filter)
                                || (!string.IsNullOrEmpty(dummy.EmployeeCode) && dummy.EmployeeCode.Contains(filter))
                                || dummy.FullName.Contains(filter)
                                || filterBy.Contains(accessGroupLst.EmployeeATID)

                            ))
                      && ((departments != null && departments.Count > 0) ? departments.Contains(wResult.DepartmentIndex) : true)
                       && ((groups != null && groups.Count > 0) ? groups.Any(y => y.Equals(agResult.UID)) : true)
                        &&      wResult.Status == 1 && (!wResult.ToDate.HasValue
                        || (wResult.ToDate.HasValue && wResult.ToDate.Value.Date >= DateTime.Now.Date))
                         select new AC_AccessedGroupDTO
                         {
                             EmployeeATID = accessGroupLst.EmployeeATID,
                             FullName = dummy.FullName,
                             DepartmentName = deResult.Name,
                             EmployeeCode = dummy.EmployeeCode,
                             GroupIndex = accessGroupLst.GroupIndex,
                             GroupName = agResult.Name,
                           Index = accessGroupLst.Index

                         }).ToList();

            countPage = doors.Count();
            dataGrid = new DataGridClass(countPage, doors);
            if (pPage <= 1)
            {
                var lstHoliday = doors.OrderBy(t => t.EmployeeATID).Take(pLimit).ToList();
                dataGrid = new DataGridClass(countPage, lstHoliday);
            }
            else
            {
                int fromRow = pLimit * (pPage - 1);
                var lstHoliday = doors.OrderBy(t => t.EmployeeATID).Skip(fromRow).Take(pLimit).ToList();
                dataGrid = new DataGridClass(countPage, lstHoliday);
            }
            return dataGrid;
        }
    }
}
