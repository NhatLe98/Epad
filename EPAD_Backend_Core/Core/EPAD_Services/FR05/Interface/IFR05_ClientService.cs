using EPAD_Data.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EPAD_Services.FR05.Interface
{
    public interface IFR05_ClientService
    {
        Task ProcessCommand(CommandResult command, UserInfo user, ConfigObject config, CompanyInfo companyInfo, bool isAuto);
    }
}
