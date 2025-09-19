using EPAD_Common.Services;
using EPAD_Common.Types;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using EPAD_Services.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EPAD_Services.Impl
{
    public class IC_WorkingInfoService : BaseServices<IC_WorkingInfo, EPAD_Context>, IIC_WorkingInfoService
    {
        public IC_WorkingInfoService(IServiceProvider serviceProvider) : base(serviceProvider)
        {
           
        }

        public async Task<List<IC_WorkingInfo>> GetDataByCompanyIndex(int companyIndex, List<string> employeeIds, List<long> departmentIDs)
        {
            ConfigObject config = ConfigObject.GetConfig(_Cache);
            if (config.IntegrateDBOther == false)
            {
                return await DbContext.IC_WorkingInfo
                .Where(x => x.CompanyIndex == companyIndex && employeeIds.Contains(x.EmployeeATID)
                    && (departmentIDs == null || departmentIDs.Contains(x.DepartmentIndex))/*&& x.Status == (short)WorkingStatus.Approved*/)
                .ToListAsync();
            }
            else
            {
                var otherContext = ServiceProvider.GetService<ezHR_Context>();
                var workingInfos = await otherContext.HR_WorkingInfo
                    .Where(t => t.CompanyIndex == companyIndex && employeeIds.Contains(t.EmployeeATID))
                    .ToListAsync();
                var dummy = workingInfos.Select(x => _Mapper.Map<IC_WorkingInfo>(x));
                return dummy.ToList();

            }
        }

        public async Task<IC_WorkingInfo> GetNewestDataByEmployeeATID(int companyIndex, string employeeATID)
        {
            return await DbContext.IC_WorkingInfo.OrderByDescending(x => x.Index).FirstOrDefaultAsync(x => x.CompanyIndex == companyIndex
                && x.EmployeeATID == employeeATID && x.Status == (short)TransferStatus.Approve);
        }
    }
}
