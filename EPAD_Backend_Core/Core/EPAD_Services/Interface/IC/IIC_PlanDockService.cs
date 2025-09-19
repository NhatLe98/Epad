using EPAD_Common.Services;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EPAD_Services.Interface
{
    public interface IIC_PlanDockService : IBaseServices<IC_PlanDock, EPAD_Context>
    {
        Task AddDataToPlanDock(List<IC_PlanDockIntegrate> planDocks);
        Task UpdateDataPlanDock(List<IC_PlanDockIntegrate> planDock);
        Task<IC_PlanDock> GetPlanDockByDriverCode(string driverCode);
        Task<List<IC_PlanDock>> GetListPlanDockByDriverCode(string driverCode);
        Task<List<IC_PlanDock>> GetPlanDockByListTripCode(List<string> tripCode);
    }
}
