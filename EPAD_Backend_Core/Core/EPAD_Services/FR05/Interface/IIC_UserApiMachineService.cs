using EPAD_Common.Observables;
using EPAD_Data.Models;
using EPAD_Data.Models.FR05;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace EPAD_Services.FR05.Interface
{
    public interface IIC_UserApiMachineService
    {
        Task<UserInfoCommandResult> DownloadAllUser( CommandResult param, DeviceProcessTracker tracker);
        Task<UserInfoCommandResult> DownloadUserById(CommandResult param, DeviceProcessTracker tracker);
        Task DeleteAllFingerPrint(CommandResult param);
        Task<string> DeleteAllUser(CommandResult param);
        Task<UserInfoCommandResult> DeleteUserById(CommandResult param, DeviceProcessTracker tracker);
        Task<UserInfoCommandResult> UploadUsers(CommandResult param, DeviceProcessTracker tracker);
    }
}
