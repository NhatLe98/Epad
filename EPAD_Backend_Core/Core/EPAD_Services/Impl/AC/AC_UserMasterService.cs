using EPAD_Common.Services;
using EPAD_Common.Types;
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

    public class AC_UserMasterService : BaseServices<AC_UserMaster, EPAD_Context>, IAC_UserMasterService
    {
        public AC_UserMasterService(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public void AddUserMasterToHistory(List<string> employeeATIDLst, List<int> doorLst, UserInfo user, int timezone)
        {
            if (employeeATIDLst != null && employeeATIDLst.Count > 0)
            {
                var employeesData = DbContext.AC_UserMaster.Where(x => employeeATIDLst.Contains(x.EmployeeATID) && doorLst.Contains(x.DoorIndex)).ToList();
                foreach (var item in employeeATIDLst)
                {
                    foreach (var itemx in doorLst)
                    {
                        var updateData = new AC_UserMaster()
                        {
                            CompanyIndex = user.CompanyIndex,
                            DoorIndex = itemx,
                            EmployeeATID = item,
                            Timezone = timezone,
                            UpdatedDate = DateTime.Now,
                            UpdatedUser = user.UserName,
                            Operation = (int)ACOperation.Sync
                        };
                        DbContext.Add(updateData);
                    }
                }

                DbContext.SaveChanges();
            }
        }

        public void AddUserMasterToHistoryFromExcel(List<AC_UserImportDTO> listImport, UserInfo user)
        {
            var employeeATIDLst = listImport.Select(x => x.EmployeeATID).ToList();
            var employeesData = DbContext.AC_UserMaster.Where(x => employeeATIDLst.Contains(x.EmployeeATID)).ToList();
            foreach (var item in listImport)
            {
                foreach (var itemx in item.DoorIndexLst)
                {
                    var updateData = new AC_UserMaster()
                    {
                        CompanyIndex = user.CompanyIndex,
                        DoorIndex = itemx,
                        EmployeeATID = item.EmployeeATID,
                        Timezone = item.TimezoneIndex,
                        UpdatedDate = DateTime.Now,
                        UpdatedUser = user.UserName,
                        Operation = (int)ACOperation.Sync
                    };
                    DbContext.Add(updateData);                 
                }

            }

            DbContext.SaveChanges();
        }

        public void AddUserMasterToHistoryFromExcel(List<AC_AccessedGroupImportDTO> listImport, UserInfo user)
        {
            var employeeATIDLst = listImport.Select(x => x.EmployeeATID).ToList();
            var employeesData = DbContext.AC_UserMaster.Where(x => employeeATIDLst.Contains(x.EmployeeATID)).ToList();
            foreach (var item in listImport)
            {
                foreach (var itemx in item.DoorIndexLst)
                {
                    var updateData = new AC_UserMaster()
                    {
                        CompanyIndex = user.CompanyIndex,
                        DoorIndex = itemx,
                        EmployeeATID = item.EmployeeATID,
                        Timezone = int.Parse(item.Timezone),
                        UpdatedDate = DateTime.Now,
                        UpdatedUser = user.UserName,
                        Operation = (int)ACOperation.Sync
                    };
                    DbContext.Add(updateData);
                }
            }

            DbContext.SaveChanges();
        }

        public void DeleteUserMaster(List<string> employeeATIDLst, List<int> doorLst, UserInfo user)
        {
            if (employeeATIDLst != null && employeeATIDLst.Count > 0)
            {
                var employeesData = DbContext.AC_UserMaster.Where(x => employeeATIDLst.Contains(x.EmployeeATID) && doorLst.Contains(x.DoorIndex)).ToList();
                foreach (var item in employeeATIDLst)
                {
                    foreach (var itemx in doorLst)
                    {
                        var updateData = new AC_UserMaster()
                        {
                            CompanyIndex = user.CompanyIndex,
                            DoorIndex = itemx,
                            EmployeeATID = item,
                            UpdatedDate = DateTime.Now,
                            UpdatedUser = user.UserName,
                            Operation = (int)ACOperation.Delete
                        };
                        DbContext.Add(updateData);
                    }

                }

                DbContext.SaveChanges();
            }
        }

        public object GetACOperation()
        {
            var listAccessMode = Enum.GetValues(typeof(ACOperation)).Cast<ACOperation>().Select(x
                => new { Index = (int)x, Name = x.ToString() }).ToList();
            return listAccessMode;
        }
        public async Task<DataGridClass> GetACSync(string pFilter, DateTime fromDate, DateTime toDate, 
            List<long> pDepartmentIds, int pCompanyIndex, int pPageIndex, int pPageSize, List<int> listDoor, List<int> listArea,
            int viewMode, List<int> viewOperation
            )
        {
            var filterBy = new List<string>();
            if (pFilter != null)
            {
                filterBy = pFilter.Split(" ").ToList();
            }
            var attendanceLog = DbContext.AC_UserMaster.Where(x => x.CompanyIndex == pCompanyIndex);
            var obj = from att in attendanceLog

                      join wi in DbContext.IC_WorkingInfo.Where(x => x.CompanyIndex == pCompanyIndex)
                             on att.EmployeeATID equals wi.EmployeeATID into eWork

                      from eWorkResult in eWork.DefaultIfEmpty()
                      join emp in DbContext.HR_User.Where(x => x.CompanyIndex == pCompanyIndex)
                        on att.EmployeeATID equals emp.EmployeeATID

                      join dp in DbContext.IC_Department.Where(x => x.CompanyIndex == pCompanyIndex)
                        on eWorkResult.DepartmentIndex equals dp.Index into temp
                      from dummy in temp.DefaultIfEmpty()

                      join d in DbContext.AC_Door.Where(x => x.CompanyIndex == pCompanyIndex)
                         on att.DoorIndex equals d.Index into dDoor
                      from dResult in dDoor.DefaultIfEmpty()

                      join p in DbContext.AC_AreaAndDoor.Where(x => x.CompanyIndex == pCompanyIndex)
                           on dResult.Index equals p.DoorIndex into pAreaDoor
                      from pResult in pAreaDoor.DefaultIfEmpty()

                      join a in DbContext.AC_Area.Where(x => x.CompanyIndex == pCompanyIndex)
                         on pResult.AreaIndex equals a.Index into aArea
                      from aResult in aArea.DefaultIfEmpty()

                      join t in DbContext.AC_TimeZone.Where(x => x.CompanyIndex == pCompanyIndex)
                        on att.Timezone equals t.UID into tTimezone
                      from tResult in tTimezone.DefaultIfEmpty()

                      where
                       (string.IsNullOrEmpty(pFilter)
                            ? emp.EmployeeATID.Contains("")
                            : (
                                   att.EmployeeATID.Contains(pFilter)
                                || (!string.IsNullOrEmpty(emp.EmployeeCode) && emp.EmployeeCode.Contains(pFilter))
                                || emp.FullName.Contains(pFilter)
                                || filterBy.Contains(att.EmployeeATID)
                            //|| dummy.Name.Contains(pFilter)

                            ))
                      && ((pDepartmentIds != null && pDepartmentIds.Count > 0) ? pDepartmentIds.Contains(eWorkResult.DepartmentIndex) : true)
                      //&& ((viewOperation != null && viewOperation.Count > 0) ? viewOperation.Contains(att.Operation) : true)
                       && ((listDoor != null && listDoor.Count > 0) ? listDoor.Any(y => y.Equals(dResult.Index)) : true)
                        && ((listArea != null && listArea.Count > 0) ? listArea.Any(y => y.Equals(aResult.Index)) : true)
                        && dResult != null && aResult != null
                         && eWorkResult.Status == 1 && (!eWorkResult.ToDate.HasValue
                        || (eWorkResult.ToDate.HasValue && eWorkResult.ToDate.Value.Date >= DateTime.Now.Date))
                      select new AC_UserMasterDTO()
                      {
                          EmployeeATID = att.EmployeeATID,
                          EmployeeCode = emp.EmployeeCode,
                          FullName = emp.FullName,
                          UpdatedDate = att.UpdatedDate,
                          DepartmentName = dummy == null ? "" : dummy.Name,
                          AreaIndex = aResult == null ? 0 : aResult.Index,
                          AreaName = aResult == null ? "" : aResult.Name,
                          DoorIndex = dResult == null ? 0 : dResult.Index,
                          DoorName = dResult == null ? "" : dResult.Name,
                          Operation = att.Operation,
                          OperationString = ((ACOperation)att.Operation).ToString(),
                          TimezoneName = tResult == null ? "" : tResult.Name,
                          UpdatedDateString = att.UpdatedDate.ToString("dd/MM/yyyy HH:mm:ss")
                      };

            var listData = await obj.ToListAsync();

            if (viewMode == (int)ViewMode.LatestData)
            {
                var groupData = listData
                .GroupBy(x => new { x.EmployeeATID, x.DoorIndex });
                if (viewOperation != null && viewOperation.Count > 0)
                {
                    if (viewOperation.Count == 1)
                    {
                        if (viewOperation[0] == (int)ACOperation.Sync)
                        {
                            listData = groupData
                            .Select(g =>
                            {
                                var adds = g.Where(x => x.Operation == (int)ACOperation.Sync)
                                                .OrderByDescending(x => x.UpdatedDate)
                                                .Take(1);

                                return adds;
                            })
                            .SelectMany(x => x)
                            .ToList();
                        }
                        else
                        {
                            listData = groupData
                            .Select(g =>
                            {
                                var deletes = g.Where(x => x.Operation == (int)ACOperation.Delete)
                                                .OrderByDescending(x => x.UpdatedDate)
                                                .Take(1);

                                return deletes;
                            })
                            .SelectMany(x => x)
                            .ToList();
                        }
                    }
                    else
                    {
                        listData = groupData
                        .Select(g =>
                        {
                            var adds = g.Where(x => x.Operation == (int)ACOperation.Sync)
                                            .OrderByDescending(x => x.UpdatedDate)
                                            .Take(1);

                            var deletes = g.Where(x => x.Operation == (int)ACOperation.Delete)
                                            .OrderByDescending(x => x.UpdatedDate)
                                            .Take(1);

                            return adds.Concat(deletes);
                        })
                        .SelectMany(x => x)
                        .ToList();
                    }
                }
                else
                {
                    listData = groupData
                        .Select(g => g.OrderByDescending(x => x.UpdatedDate).First())
                        .ToList();
                }
            }
            else
            {
                if (viewOperation != null && viewOperation.Count > 0)
                {
                    listData = listData.Where(x => viewOperation.Contains(x.Operation)).ToList();
                }
            }

            if (pPageIndex <= 1)
            {
                pPageIndex = 1;
            }

            int fromRow = pPageSize * (pPageIndex - 1);
            var lsAttendanceLog = listData.OrderByDescending(t => t.UpdatedDate).Skip(fromRow).Take(pPageSize).ToList();
            var dataGrid = new DataGridClass(listData.Count(), lsAttendanceLog);
            return await Task.FromResult(dataGrid);
        }
    }
}
