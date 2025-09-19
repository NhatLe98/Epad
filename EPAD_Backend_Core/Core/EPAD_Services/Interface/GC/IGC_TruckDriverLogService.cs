using EPAD_Common.Services;
using EPAD_Common.Types;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EPAD_Services.Interface
{
    public interface IGC_TruckDriverLogService : IBaseServices<GC_TruckDriverLog, EPAD_Context>
    {
        Task<IC_PlanDock> GetPlanDockByTripCode(string tripCode);
        Task<IC_PlanDock> GetPlanDockByVehiclePlate(string vehiclePlate);
        Task<List<IC_StatusDock>> GetAllStatusDock();
        Task<List<GC_TruckDriverLog>> GetTruckDriverLogByTripCode(string tripCode);
        List<GC_TruckDriverLog> GetActiveTruckDriverLogByTripCode(string tripCode);
        Task<List<GC_TruckDriverLog>> GetActiveTruckDriverLogByTripCodeAsync(string tripCode);
        Task<List<GC_TruckDriverLog>> GetActiveTruckDriverLogByCardNumber(string cardNumber);
        Task<List<GC_TruckExtraDriverLog>> GetExtraTruckDriverLogByTripCode(string tripCode);
        Task<List<GC_TruckExtraDriverLog>> GetExtraTruckDriverLogByListTripCode(List<string> tripCode);
        List<GC_TruckExtraDriverLog> GetActiveExtraTruckDriverLogByTripCode(string tripCode);
        Task<List<GC_TruckExtraDriverLog>> GetActiveExtraTruckDriverLogByTripCodeAsync(string tripCode);
        Task<GC_TruckExtraDriverLog> GetExtraTruckDriverLogByExtraDriverCode(string cccd);
        Task<List<GC_TruckExtraDriverLog>> GetListExtraTruckDriverLogByExtraDriverCode(string cccd);
        Task<GC_TruckExtraDriverLog> GetActiveExtraTruckDriverLogByExtraDriverCode(string cccd);
        Task<GC_TruckExtraDriverLog> GetActiveExtraTruckDriverLogByCardNumber(string cardNumber);
        Task<HR_UserResult> GetDriverByCCCD(string cccd);
        Task<HR_UserResult> GetDriverByCardNumber(string cardNumber);
        Task<IC_Company> GetCompanyByUser(UserInfo user);
        Task<bool> AddTruckDriverLog(GC_TruckDriverLog param);
        Task<bool> AddTruckDriverLog(GC_TruckDriverLog param, List<ExtraTruckDriverLogModel> extraDriver);
        Task<bool> SaveExtraTruckDriverLog(List<ExtraTruckDriverLogModel> extraDriver);
        Task<bool> DeleteExtraTruckDriverLog(int index);
        Task<List<TruckHistoryModel>> GetHistoryData(UserInfo user, DateTime fromTime, DateTime toTime, string filter);
        Task<string> ReturnDriverCard(ReturnDriverCardModel data, UserInfo user);
        DataGridClass GetPaginationList(IEnumerable<TruckHistoryModel> histories, int page, int pageSize);
    }
}
