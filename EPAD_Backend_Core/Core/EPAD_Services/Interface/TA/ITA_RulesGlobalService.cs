using EPAD_Common.Services;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EPAD_Services.Interface
{
    public interface ITA_RulesGlobalService : IBaseServices<TA_Rules_Global, EPAD_Context>
    {
        Task<TA_Rules_Global> GetRulesGlobal(UserInfo user);
        Task<bool> UpdateRulesGlobal(TA_Rules_Global param, UserInfo user);
    }
}
