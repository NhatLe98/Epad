using EPAD_Common.Services;
using EPAD_Common.Types;
using EPAD_Data.Entities;
using EPAD_Data;
using EPAD_Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using EPAD_Data.Models;
using System.Threading.Tasks;

namespace EPAD_Services.Impl
{

    public class AC_DepartmentAccessedGroupService : BaseServices<AC_DepartmentAccessedGroup, EPAD_Context>, IAC_DepartmentAccessedGroupService
    {
        public AC_DepartmentAccessedGroupService(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public async Task<AC_DepartmentAccessedGroup> GetByDepartmentAndFromToDate(int departmentIndex, UserInfo user, int groupValue)
        {
            var doorIndex = DbContext.AC_AccGroup.FirstOrDefault(x => x.CompanyIndex == user.CompanyIndex && x.UID == groupValue).DoorIndex;
            var accGroup = DbContext.AC_AccGroup.Where(x => x.DoorIndex == doorIndex).Select(x => x.UID).ToList();
            var dummy = DbContext.AC_DepartmentAccessedGroup.Where(x => x.CompanyIndex == user.CompanyIndex
            && x.DepartmentIndex == departmentIndex
            && accGroup.Contains(x.GroupIndex)).FirstOrDefault();
            return await Task.FromResult(dummy);
        }

        public async Task<AC_DepartmentAccessedGroup> GetByDepartmentAndFromToDateEdit(int departmentIndex, UserInfo user, int groupValue)
        {
            var doorIndex = DbContext.AC_AccGroup.FirstOrDefault(x => x.CompanyIndex == user.CompanyIndex && x.UID == groupValue).DoorIndex;
            var accGroup = DbContext.AC_AccGroup.Where(x => x.DoorIndex == doorIndex).Select(x => x.UID).ToList();
            var dummy = DbContext.AC_DepartmentAccessedGroup.Where(x => x.CompanyIndex == user.CompanyIndex
            && x.DepartmentIndex == departmentIndex
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
            var doors = (from accessGroupLst in DbContext.AC_DepartmentAccessedGroup.Where(t => t.CompanyIndex == pCompanyIndex)

                         join de in DbContext.IC_Department.Where(x => x.CompanyIndex == pCompanyIndex && x.IsInactive != true)
                             on accessGroupLst.DepartmentIndex equals de.Index into deCheck
                         from deResult in deCheck.DefaultIfEmpty()

                         join ag in DbContext.AC_AccGroup.Where(x => x.CompanyIndex == pCompanyIndex)
                            on accessGroupLst.GroupIndex equals ag.UID into agCheck
                         from agResult in agCheck.DefaultIfEmpty()

                         where (!string.IsNullOrEmpty(filter) ? (deResult.Name.Contains(filter) || agResult.Name.Contains(filter)) : true)

                         && ((departments != null && departments.Count > 0) ? departments.Contains(accessGroupLst.DepartmentIndex) : true)
                         && ((groups != null && groups.Count > 0) ? groups.Any(y => y.Equals(agResult.UID)) : true)
                         select new AC_AccessedGroupDTO
                         {
                             DepartmentName = deResult != null ? deResult.Name : "NoDepartment",
                             DepartmentIndex = deResult != null ? deResult.Index : 0,
                             GroupIndex = accessGroupLst.GroupIndex,
                             GroupName = agResult.Name,
                             Index = accessGroupLst.Index

                         }).ToList();

            countPage = doors.Count();
            dataGrid = new DataGridClass(countPage, doors);
            if (pPage <= 1)
            {
                var lstHoliday = doors.OrderBy(t => t.DepartmentName).Take(pLimit).ToList();
                dataGrid = new DataGridClass(countPage, lstHoliday);
            }
            else
            {
                int fromRow = pLimit * (pPage - 1);
                var lstHoliday = doors.OrderBy(t => t.DepartmentName).Skip(fromRow).Take(pLimit).ToList();
                dataGrid = new DataGridClass(countPage, lstHoliday);
            }
            return dataGrid;
        }
    }
}
