using EPAD_Common.Services;
using EPAD_Common.Types;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Entities.HR;
using EPAD_Data.Models.HR;
using EPAD_Services.Interface;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EPAD_Services.Impl
{
    public class HR_PositionInfoService : BaseServices<HR_PositionInfo, EPAD_Context>, IHR_PositionInfoService
    {
        private readonly ILogger _logger;

        public HR_PositionInfoService(IServiceProvider serviceProvider, ILogger<HR_PositionInfoService> logger) : base(serviceProvider)
        {
            _logger = logger;
        }

        public async Task<List<HR_PositionInfoResult>> GetListPositionInfoByIndex(long[] postionIndexs, int companyIndex)
        {
            var rs = Where(x => x.CompanyIndex == companyIndex).AsQueryable(); // ClassInfosMockup.Where(x => x.CompanyIndex == companyIndex);
            if (postionIndexs.Length > 0)
            {
                rs = rs.Where(x => postionIndexs.Contains(x.Index));
            }
            var result = rs.AsEnumerable().Select(x =>
            {
                var rs = _Mapper.Map<HR_PositionInfoResult>(x);
                return rs;
            }).ToList();
            return await Task.FromResult(result);
        }

        public async Task<HR_PositionInfoResult> GetPositionInfoByIndex(long index, int companyIndex)
        {
            try
            {
                var rs =await FirstOrDefaultAsync(x => x.Index == index && x.CompanyIndex == companyIndex); //ClassInfosMockup.FirstOrDefault(x => x.Index == classID && x.CompanyIndex == companyIndex);

                var result = _Mapper.Map<HR_PositionInfoResult>(rs);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex}");
                return null;
            }
        }

        public async Task<List<HR_PositionInfoResult>> GetAllPositionInfo(int companyIndex)
        {
            var rs = Where(x => x.CompanyIndex == companyIndex).AsQueryable(); 
            
            var result = rs.AsEnumerable().Select(x =>
            {
                var rs = _Mapper.Map<HR_PositionInfoResult>(x);
                return rs;
            }).ToList();
            
            return await Task.FromResult(result);
        }

        public async Task<DataGridClass> GetPage(List<AddedParam> addedParams, int pCompanyIndex)
        {
            if (addedParams == null || addedParams.Count == 0)
                return null;

            var dummy = new List<HR_PositionInfoResult>();
            var pageIndex = addedParams.FirstOrDefault(e => e.Key == "PageIndex");
            var pageSize = addedParams.FirstOrDefault(e => e.Key == "PageSize");
            var pPage = Convert.ToInt32(pageIndex.Value ?? 1);
            var pLimit = Convert.ToInt32(pageSize.Value ?? GlobalParams.ROWS_NUMBER_IN_PAGE);

            var query = DbContext.HR_PositionInfo.Where(e => e.CompanyIndex == pCompanyIndex);
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
                    var rs = _Mapper.Map<HR_PositionInfoResult>(x);
                    return rs;
                }).ToList();
            }
            var rs = new DataGridClass(total, dummy);
            return await Task.FromResult(rs);
        }
    }
}
