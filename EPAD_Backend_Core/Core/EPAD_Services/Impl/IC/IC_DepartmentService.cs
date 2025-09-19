using EPAD_Common.Services;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using EPAD_Data.Models.IC;
using EPAD_Data.Models.Other;
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
    public class IC_DepartmentService : BaseServices<IC_Department, EPAD_Context>, IIC_DepartmentService
    {
        private ILogger _logger;
        ezHR_Context ezHR_Context;
        public IC_DepartmentService(IServiceProvider serviceProvider, ILoggerFactory loggerFactory) : base(serviceProvider)
        {
            ezHR_Context = serviceProvider.GetService<ezHR_Context>();
            _logger = loggerFactory.CreateLogger<IC_DepartmentService>();
        }

        public List<IC_Department> GetAllDepartment(int pCompanyIndex)
        {
            ConfigObject config = ConfigObject.GetConfig(_Cache);
            if (config.IntegrateDBOther == false)
            {
                var dummy = DbContext.IC_Department.Where(x => x.CompanyIndex == pCompanyIndex).ToList();
                dummy.Add(new IC_Department() { Index = 0, Name = "Kh�ng c� ph�ng ban" });
                return dummy;
            }
            else
            {
                ezHR_Context otherContext = ServiceProvider.GetService<ezHR_Context>();
                List<HR_Department> departmentDTOs = otherContext.HR_Department.Where(t => t.CompanyIndex == config.CompanyIndex).ToList();
                departmentDTOs.Add(new HR_Department { Index = 0, Name = "Kh�ng c� ph�ng ban" });

                var dummy = departmentDTOs.Select(x => _Mapper.Map<IC_Department>(x));
                return dummy.ToList();
            }
        }

        public List<IC_Department> GetAllActiveDepartment(int pCompanyIndex)
        {
            ConfigObject config = ConfigObject.GetConfig(_Cache);
            if (config.IntegrateDBOther == false)
            {
                var dummy = DbContext.IC_Department.Where(x => x.CompanyIndex == pCompanyIndex 
                    && (!x.IsInactive.HasValue || (x.IsInactive.HasValue && !x.IsInactive.Value))).ToList();
                dummy.Add(new IC_Department() { Index = 0, Name = "Kh�ng c� ph�ng ban" });
                return dummy;
            }
            else
            {
                ezHR_Context otherContext = ServiceProvider.GetService<ezHR_Context>();
                List<HR_Department> departmentDTOs = otherContext.HR_Department.Where(t => t.CompanyIndex == config.CompanyIndex).ToList();
                departmentDTOs.Add(new HR_Department { Index = 0, Name = "Kh�ng c� ph�ng ban" });

                var dummy = departmentDTOs.Select(x => _Mapper.Map<IC_Department>(x));
                return dummy.ToList();
            }
        }

        public List<IC_Department> GetDepartmentByListName(int pCompanyIndex, List<string> pListName)
        {
            ConfigObject config = ConfigObject.GetConfig(_Cache);
            if (config.IntegrateDBOther == false)
            {
                var result = DbContext.IC_Department.Where(x => x.CompanyIndex == pCompanyIndex && pListName.Contains(x.Name)).ToList();
                result.Add(new IC_Department() { Index = 0, Name = "Kh�ng c� ph�ng ban" });
                return result;
            }
            else
            {
                ezHR_Context otherContext = ServiceProvider.GetService<ezHR_Context>();
                var departmentDTOs = otherContext.HR_Department.Where(t => t.CompanyIndex == config.CompanyIndex
                    && pListName.Contains(t.Name)).ToList();
                departmentDTOs.Add(new HR_Department { Index = 0, Name = "Kh�ng c� ph�ng ban" });

                var result = departmentDTOs.Select(x => _Mapper.Map<IC_Department>(x));
                return result.ToList();
            }
        }

        public async Task<List<IC_Department>> GetByCompanyIndexAndDepIds(int companyIndex, List<long> departmentIds)
        {
            ConfigObject config = ConfigObject.GetConfig(_Cache);
            if (config.IntegrateDBOther == false)
            {
                return await DbContext.IC_Department.Where(x => x.CompanyIndex == companyIndex && departmentIds.Any(d => d == x.Index)
                            && (x.ParentIndex == null || x.ParentIndex == 0)).ToListAsync();
            }
            else
            {
                ezHR_Context otherContext = ServiceProvider.GetService<ezHR_Context>();
                List<HR_Department> departmentDTOs = await otherContext.HR_Department.Where(t => t.CompanyIndex == companyIndex && departmentIds.Any(d => d == t.Index)
                            && (t.ParentIndex == null || t.ParentIndex == 0)).ToListAsync();
                var dummy = departmentDTOs.Select(x => _Mapper.Map<IC_Department>(x));
                return dummy.ToList();

            }
        }

        public async Task<List<IC_Department>> GetByCompanyIndex(int companyIndex)
        {
            ConfigObject config = ConfigObject.GetConfig(_Cache);
            if (config.IntegrateDBOther == false)
            {
                return await DbContext.IC_Department.Where(x => x.CompanyIndex == companyIndex).ToListAsync();
            }
            else
            {
                ezHR_Context otherContext = ServiceProvider.GetService<ezHR_Context>();
                List<HR_Department> departmentDTOs = await otherContext.HR_Department.Where(t => t.CompanyIndex == companyIndex).ToListAsync();
                var dummy = departmentDTOs.Select(x => _Mapper.Map<IC_Department>(x));
                return dummy.ToList();

            }
        }

        public async Task<List<IC_Department>> GetDepartmentByIds(List<long> departmentIds)
        {
            return await DbContext.IC_Department.Where(x => departmentIds.Any(d => d == x.Index)
                && (x.ParentIndex == null || x.ParentIndex == 0))
                .ToListAsync();
        }

        public async Task<List<IC_Department>> GetActiveDepartmentByListIndex(List<long> departmentIds)
        {
            return await DbContext.IC_Department.AsNoTracking().Where(x => departmentIds.Contains(x.Index) 
                && (x.IsInactive == null || x.IsInactive == false))
                .ToListAsync();
        }

        public async Task<List<IC_Department>> GetActiveDepartment()
        {
            return await DbContext.IC_Department.AsNoTracking().Where(x => (x.IsInactive == null || x.IsInactive == false))
                .ToListAsync();
        }

        public async Task<object> GetActiveDepartmentByPermission(UserInfo user)
        {
            var data = from d in DbContext.IC_Department.AsNoTracking().Where(x => (x.IsInactive == null || x.IsInactive == false))
                       select new
                       {
                           DepartmentIndex = d.Index,
                           DepartmentName = d.Name,
                           RootDepartment = !d.ParentIndex.HasValue || (d.ParentIndex.HasValue && (d.ParentIndex.Value == 0 || d.ParentIndex.Value == d.Index)),
                       };

            var result = await data.ToListAsync();
            result.Add(new
            {
                DepartmentIndex = 0,
                DepartmentName = "Không có phòng ban",
                RootDepartment = true
            });
            result = result.Where(x => x.DepartmentIndex == 0
                || (user.ListDepartmentAssigned != null
                && user.ListDepartmentAssigned.Contains(x.DepartmentIndex))).ToList();

            return result;
        }

        public async Task<object> GetActiveDepartmentAndDeviceByPermission(UserInfo user)
        {
            var data = from d in DbContext.IC_Department.AsNoTracking().Where(x => (x.IsInactive == null || x.IsInactive == false))
                       join dad in DbContext.IC_DepartmentAndDevice
                       on d.Index equals dad.DepartmentIndex
                       into dd
                       from ddResult in dd.DefaultIfEmpty()
                       join de in DbContext.IC_Device
                       on ddResult.SerialNumber equals de.SerialNumber
                       into ddd
                       from dddResult in ddd.DefaultIfEmpty()
                       select new DepartmentDeviceModel
                       {
                           DepartmentIndex = d.Index,
                           DepartmentName = d.Name,
                           SerialNumber = ddResult.SerialNumber,
                           RootDepartment = !d.ParentIndex.HasValue || (d.ParentIndex.HasValue && (d.ParentIndex.Value == 0 || d.ParentIndex.Value == d.Index)),
                           RootDepartmentIndex = 0,
                       };

            var result = await data.ToListAsync();
            result.Add(new DepartmentDeviceModel
            { 
                DepartmentIndex = 0, 
                DepartmentName = "Không có phòng ban", 
                SerialNumber = "",
                RootDepartment = true ,
                RootDepartmentIndex = 0,
            });

            var allDepartment = await DbContext.IC_Department.AsNoTracking().ToListAsync();
            result.ForEach(x =>
            {
                var rootDepartmentIndex = FindRootDepartmentIndex(x.DepartmentIndex, allDepartment);
                if (rootDepartmentIndex != 0 && rootDepartmentIndex != x.DepartmentIndex)
                {
                    x.RootDepartmentIndex = rootDepartmentIndex;
                }
            });

            result = result.Where(x => x.DepartmentIndex == 0
                || (user.ListDepartmentAssigned != null 
                && user.ListDepartmentAssigned.Contains(x.DepartmentIndex))).ToList();

            return result;
        }

        private long FindRootDepartmentIndex(long departmentIndex, List<IC_Department> listDepartment)
        {
            var result = departmentIndex;
            var department = listDepartment.FirstOrDefault(x => x.Index == departmentIndex);
            if (department != null && department.ParentIndex != null && department.ParentIndex > 0
                && department.ParentIndex != department.Index)
            {
                result = FindRootDepartmentIndex(department.ParentIndex.Value, listDepartment);
            }

            return result;
        }

        public string GetDeparmentNameByIdFromList(int ParentIndex, List<IC_Department> DepartmentsList)
        {
            var department = DepartmentsList.FirstOrDefault(x => x.Index == ParentIndex);
            if (department != null && ParentIndex > 0)
                return department.Name;

            return "";
        }

        public async Task<List<int>> GetDepartmentNotIntegrate()
        {
            return await DbContext.IC_Department.Where(x => x.UpdatedUser != UpdatedUser.AutoIntegrateEmployee.ToString() && x.IsInactive != true && x.Description != DescriptionType.Security.ToString()).Select(x => x.Index).ToListAsync();
        }

        public async Task<List<int>> GetDepartmentSecurity()
        {
            return await DbContext.IC_Department.Where(x => x.UpdatedUser != UpdatedUser.AutoIntegrateEmployee.ToString() && x.IsInactive != true && x.Description == DescriptionType.Security.ToString()).Select(x => x.Index).ToListAsync();
        }

        public async Task<List<RegularDepartmentDataReponse>> GetDepartmentChildrentByName(string departmentName, int companyIndex)
        {
            try
            {
                List<HR_Department> departmentDTOs = await ezHR_Context.HR_Department.Where(x => x.CompanyIndex == companyIndex).ToListAsync();

                var departmentData = departmentDTOs.Select(x => _Mapper.Map<IC_Department>(x)).ToList();
                var lstDeptLevel1 = departmentData.Where(x => x.Name == departmentName).ToList();
                var regularDepartmentList = new List<RegularDepartmentReponse>();
                for (int i = 0; i < lstDeptLevel1.Count; i++)
                {
                    regularDepartmentList = RecursiveGetChildrentDepartment(departmentData, lstDeptLevel1[i].Index, 1, 2);
                }

                var dataDepartment = new List<RegularDepartmentDataReponse>();
                foreach (var regularDepartment in regularDepartmentList)
                {
                    if (regularDepartment.ListChildrent != null && regularDepartment.ListChildrent.Count > 0)
                    {
                        foreach (var departmentChildrent in regularDepartment.ListChildrent)
                        {
                            var department = new RegularDepartmentDataReponse()
                            {
                                DepartmentIndex = departmentChildrent.DepartmentIndex,
                                DepartmentName = departmentChildrent.DepartmentName
                            };
                            dataDepartment.Add(department);
                        }
                    }
                }
                return dataDepartment;
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetDepartmentChildrentByName: " + ex.Message);
                return new List<RegularDepartmentDataReponse>();
            }

        }
        private List<RegularDepartmentReponse> RecursiveGetChildrentDepartment(List<IC_Department> lstDept, long pCurrentIndex, decimal pId, int pLevel)
        {
            var lstChild = lstDept.Where(x => x.ParentIndex == pCurrentIndex).ToList();
            var output = new List<RegularDepartmentReponse>();
            for (int i = 0; i < lstChild.Count; i++)
            {
                output.Add( new RegularDepartmentReponse()
                {
                    DepartmentIndex = lstChild[i].Index,
                    DepartmentName = lstChild[i].Name,
                    ListChildrent = RecursiveGetChildrentDepartment(lstDept, lstChild[i].Index, lstChild[i].Index, pLevel + 1)
                });
            }
            return output;
        }
    }
}
