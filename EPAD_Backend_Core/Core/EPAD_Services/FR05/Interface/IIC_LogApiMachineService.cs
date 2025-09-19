using EPAD_Data.Models;
using EPAD_Data.Models.FR05;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace EPAD_Services.FR05.Interface
{
    public interface IIC_LogApiMachineService
    {
        Task<LogCommandResult> DownloadAllLog(CommandResult pSystemCommand);
        Task<LogCommandResult> DownloadLogFromToTime(CommandResult pSystemCommand);
        Task<string> DeleteAllLog(CommandResult pSystemCommand);
        Task<string> DeleteLogFromToTime(CommandResult pSystemCommand);
    }
}
