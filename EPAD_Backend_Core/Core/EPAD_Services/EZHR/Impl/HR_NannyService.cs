using EPAD_Common.Clients;
using EPAD_Services.Interface;
using System;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using System.Collections.Generic;
using EPAD_Data.Models;
using Microsoft.Extensions.Caching.Memory;

namespace EPAD_Services.Impl
{
    public class HR_NannyService : IHR_NannyService
    {
        private readonly EZHRClient _EzClient;
        ConfigObject _Config;
        public HR_NannyService(IServiceProvider serviceProvider, IMemoryCache pCache)
        {
            _EzClient = serviceProvider.GetService<EZHRClient>();
            _Config = ConfigObject.GetConfig(pCache);
        }

        public async Task<List<HR_NannyInfoResult>> GetAllNannyInfo(string[] pEmployeeATIDs, int pCompanyIndex)
        {
            if(_Config.IntegrateDBOther)
                return await _EzClient.GetAllNannyInfo(pEmployeeATIDs, pCompanyIndex);

            return await Task.FromResult(new List<HR_NannyInfoResult>());
        }

        public async Task<HR_NannyInfoResult> GetNannyInfo(string pEmployeeATID, int pCompanyIndex)
        {
            if(_Config.IntegrateDBOther)
                return await _EzClient.GetNannyInfo(pEmployeeATID, pCompanyIndex);

            return null;
        }
    }
}
