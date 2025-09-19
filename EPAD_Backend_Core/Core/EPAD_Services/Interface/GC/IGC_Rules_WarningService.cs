using EPAD_Common.Services;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EPAD_Services.Interface
{
    public interface IGC_Rules_WarningService : IBaseServices<GC_Rules_Warning, EPAD_Context>
    {
        Task<GC_Rules_Warning> GetDataByIndex(int index);
        Task<List<GC_Rules_WarningGroup>> GetRulesWarningGroupsByCompanyIndex(int companyIndex = 0);
        GC_Rules_WarningGroup GetRulesWarningGroupsByGroupIndex(int groupIndex);
        Task<List<GC_Rules_Warning>> GetRulesWarningByCompanyIndex(int companyIndex = 0);
        GC_Rules_Warning GetRulesWarningByCompanyIndexAndCode(string code, int companyIndex = 0);
        GC_Rules_Warning GetRulesWarningByGroupIndex(int groupIndex, int companyIndex = 0);
        Task<List<GC_Rules_Warning_EmailSchedule>> GetRulesWarningEmailSchedule(int rulesWarningIndex, int companyIndex = 0);
        Task<List<GC_Rules_Warning_ControllerChannel>> GetRulesWarningControllerChannel(int rulesWarningIndex, int companyIndex = 0);
        Task<int> AddRulesWarning(GC_Rules_Warning data, UserInfo user);
        Task<int> UpdateRulesWarning(GC_Rules_Warning data, UserInfo user);
        Task<bool> DeleteRulesWarning(int index, UserInfo user);
        Task<bool> AddRulesWarningEmailSchedule(List<EmailScheduleRequestModel> data, UserInfo user);
        Task<bool> AddRulesWarningControllerChannels(List<GC_Rules_Warning_ControllerChannel> data, UserInfo user);
        Task<bool> AddEzFileRulesWarning(EzFileRequestSimple data, UserInfo user);
        Task SendReloadWarningRuleToClientAsync(int companyIndex, string logContent);

    }
}
