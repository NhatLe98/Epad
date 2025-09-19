using EPAD_Common.Services;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EPAD_Services.Interface
{
    public interface IIC_SystemCommandService : IBaseServices<IC_SystemCommand, EPAD_Context>
    {
        List<RemoteProcessLogObject> UpdateSystemCommandStatus(List<CommandParam> listParams, UserInfo user, ConfigObject config, CompanyInfo companyInfo);
        bool RenewSystemCommand(int systemCommandIndex, UserInfo user);

        Task AutoRunCommandOfFR05();
    }
}
