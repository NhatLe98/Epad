using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EPAD_Common.Utility;
using EPAD_Common.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace EPAD_Background.Schedule.Job
{
    public class CheckHardwareLicense : BaseJob
    {
        public CheckHardwareLicense(IServiceScopeFactory provider) : base(provider)
        {
        }

        public override async Task Execute(IJobExecutionContext context)
        {
#if !DEBUG
            await DoWorkAsync();
#endif
        }

        public async Task DoWorkAsync()
        {
            var appLicenseInfo = await _db.IC_AppLicense.ToListAsync();
            var hwLicense = await _db.IC_HardwareLicense.ToListAsync();

            var lcCache = _cache.GetAllHWLicense();

            foreach (var appKey in appLicenseInfo)
            {
                var hwLicenseByCompany = hwLicense.Where(x => x.CompanyIndex == appKey.CompanyIndex);
                foreach (var lc in hwLicenseByCompany)
                {
                    var hwLicenseInfo = StringHelper.GetHWLicenseInfo(appKey.PublicKey, lc.Data);
                    if (hwLicenseInfo == null) continue;

                    if (lcCache.ContainsKey(hwLicenseInfo.Serial))
                    {
                        if(lcCache[hwLicenseInfo.Serial].ExpiredDate < hwLicenseInfo.ExpiredDate)
                        {
                            lcCache[hwLicenseInfo.Serial] = hwLicenseInfo;
                        }
                    }
                    else
                    {
                        lcCache.Add(hwLicenseInfo.Serial, hwLicenseInfo);
                    }
                }
            }
            _cache.Set($"{StringHelper.UrnHWLicensePrefix}{1}", lcCache);
        }
    }
}
