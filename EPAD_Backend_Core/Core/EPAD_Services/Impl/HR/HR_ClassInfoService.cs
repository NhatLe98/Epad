using EPAD_Common.Clients;
using EPAD_Services.Interface;
using System;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using System.Collections.Generic;
using EPAD_Data.Models;
using Microsoft.Extensions.Caching.Memory;
using EPAD_Data.Entities;
using System.IO;
using System.Linq;
using EPAD_Data;
using EPAD_Common.Services;
using EPAD_Common.Types;

namespace EPAD_Services.Impl
{
    public class HR_ClassInfoService : BaseServices<HR_ClassInfo, EPAD_Context>, IHR_ClassInfoService
    {
        public List<HR_ClassInfo> ClassInfosMockup;

        public HR_ClassInfoService(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            ClassInfosMockup = new List<HR_ClassInfo>();
            if (File.Exists("ClassInfosMockup.json"))
            {
                try
                {
                    ClassInfosMockup = Newtonsoft.Json.JsonConvert.DeserializeObject<List<HR_ClassInfo>>(File.ReadAllText("ClassInfosMockup.json"));
                }
                catch (Exception)
                {
                }
            }
        }

        public async Task<HR_ClassInfo> GetAllClassInfoByClassCode(string code, int companyIndex)
        {
            var rs = ClassInfosMockup.FirstOrDefault(x => x.Code == code && x.CompanyIndex == companyIndex);
            return await Task.FromResult(rs);
        }

        public async Task<List<HR_ClassInfoResult>> GetAllClassInfo(string[] classIDs, int companyIndex)
        {
            var rs = Where(x => x.CompanyIndex == companyIndex).AsQueryable(); // ClassInfosMockup.Where(x => x.CompanyIndex == companyIndex);
            if (classIDs.Length > 0)
            {
                rs = rs.Where(x => classIDs.Contains(x.Index));
            }
            var result = rs.AsEnumerable().Select(x =>
            {
                var rs = _Mapper.Map<HR_ClassInfoResult>(x);
                return rs;
            }).OrderBy(x => x.Name).ToList();
            return await Task.FromResult(result);
        }

        public async Task<HR_ClassInfoResult> GetAllClassInfoByClassID(string classID, int companyIndex)
        {
            var rs = FirstOrDefaultAsync(x => x.Index == classID && x.CompanyIndex == companyIndex); //ClassInfosMockup.FirstOrDefault(x => x.Index == classID && x.CompanyIndex == companyIndex);
            var result = _Mapper.Map<HR_ClassInfoResult>(rs);
            return await Task.FromResult(result);
        }

        public async Task<List<HR_ClassInfoResult>> GetClassInfoByNanny(string employeeATID, int companyIndex)
        {
            ConfigObject config = ConfigObject.GetConfig(_Cache);
            if (config.IntegrateDBOther == true)
            {
                ezHR_Context otherContext = ServiceProvider.GetService<ezHR_Context>();
                List<EM_NannyClassroom> empDtos = otherContext.EM_NannyClassroom.Where(t => t.EmployeeATID == employeeATID && t.CompanyIndex == config.CompanyIndex).ToList();

                if (empDtos.Count == 0) return await Task.FromResult(new List<HR_ClassInfoResult>());

                var allClassID = empDtos.Select(x => x.Class).ToList();
                var rs = Where(x => allClassID.Contains(x.Index) && x.CompanyIndex == companyIndex).AsQueryable(); //ClassInfosMockup.Where(x => allClassID.Contains(x.Index) && x.CompanyIndex == companyIndex);
                var result = rs.AsEnumerable().Select(x =>
                {
                    var rs = _Mapper.Map<HR_ClassInfoResult>(x);
                    return rs;
                }).ToList();
                return await Task.FromResult(result);
            }
            else
            {
                return await Task.FromResult(new List<HR_ClassInfoResult>());
            }
        }

        public async Task<DataGridClass> GetPage(List<AddedParam> addedParams, int pCompanyIndex)
        {
            if (addedParams == null || addedParams.Count == 0)
                return null;

            List<HR_ClassInfoResult> dummy = new List<HR_ClassInfoResult>();
            var pageIndex = addedParams.FirstOrDefault(e => e.Key == "PageIndex");
            var pageSize = addedParams.FirstOrDefault(e => e.Key == "PageSize");
            var pPage = Convert.ToInt32(pageIndex.Value ?? 1);
            var pLimit = Convert.ToInt32(pageSize.Value ?? GlobalParams.ROWS_NUMBER_IN_PAGE);


            //var query = (from c in DbContext.HR_ClassInfo.Where(e => e.CompanyIndex == pCompanyIndex)
            //             select new {ClassInfo = c }
            //             ).AsQueryable();
                
            var query = Where(e=>e.CompanyIndex == pCompanyIndex).AsQueryable();
            var total = 0;
            if (addedParams != null)
            {
                foreach (AddedParam param in addedParams)
                {
                    switch (param.Key)
                    {
                        case "Filter":
                            if (param.Value != null)
                            {
                                string filter = param.Value.ToString();
                                query = query.Where(u => u.Name.Contains(filter)
                                || u.Code.Contains(filter)
                                || u.Description.Contains(filter));
                            }
                            break;
                        case "Name":
                            if (param.Value != null)
                            {
                                string Name = param.Value.ToString();
                                query = query.Where(u => u.Name == Name);
                            }
                            break;
                        case "Code":
                            if (param.Value != null)
                            {
                                string Code = param.Value.ToString();
                                query = query.Where(u => u.Code == Code);
                            }
                            break;

                    }
                }
                total = query.Count();
                if (pPage < 1) pPage = 1;
                dummy = query.OrderBy(x => x.Index).Skip((pPage - 1) * pLimit).Take(pLimit).AsEnumerable().Select(x =>
                {
                    var rs = _Mapper.Map<HR_ClassInfoResult>(x);
                    return rs;
                }).ToList();
            }
            var rs = new DataGridClass(total, dummy);
            return await Task.FromResult(rs);
        }

        public async Task<List<HR_ClassInfo>> CheckExistedOrCreate(List<HR_ClassInfo> listClassInfo, int pCompanyIndex) {
            var listCode = listClassInfo.Select(e => e.Code).ToHashSet();
            var listClassInfoDB = Where(e => listCode.Contains(e.Code) && e.CompanyIndex == pCompanyIndex).ToList();
            foreach(var item in listClassInfo) {
                var existed = listClassInfoDB.FirstOrDefault(e => e.Code == item.Code);
                if (existed == null) {
                    existed = new HR_ClassInfo();
                    existed = _Mapper.Map<HR_ClassInfo>(item);
                    await InsertAsync(existed);
                }
            }
            var result = Where(e => listCode.Contains(e.Code)).ToList();
            return await Task.FromResult(result);
        }

        public async Task<List<HR_ClassInfoResult>> GetManyToExport(int pCompanyIndex) {
            return null;
        }
    }
}
