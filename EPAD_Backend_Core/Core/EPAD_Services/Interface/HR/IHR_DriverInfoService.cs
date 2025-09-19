using EPAD_Common.Services;
using EPAD_Data.Entities;
using EPAD_Data;
using System;
using System.Collections.Generic;
using System.Text;
using EPAD_Common.Types;
using System.Threading.Tasks;
using EPAD_Data.Models;

namespace EPAD_Services.Interface
{
    public interface IHR_DriverInfoService : IBaseServices<IC_PlanDock, EPAD_Context>
    {
        Task<DataGridClass> GetPage(HR_DriverInfoParam addedParams, int pCompanyIndex);
        Task AddDriverInfo(HR_DriverInfoDTO param, UserInfo user);
        Task UpdateDriverInfo(HR_DriverInfoDTO param, UserInfo user);
        List<HR_DriveInfoImportParam> ValidationImportDriverInfo(List<HR_DriveInfoImportParam> param, UserInfo user);
        List<HR_DriverInfoResult> GetManyExportDriver(HR_DriverInfoParam addedParams, int pCompanyIndex);
        bool CheckExistDriverByTripId(string tripId, int pCompanyIndex);
        Task DeleteDriverInfoMulti(List<string> tripIds, int pCompanyIndex);
    }
}
