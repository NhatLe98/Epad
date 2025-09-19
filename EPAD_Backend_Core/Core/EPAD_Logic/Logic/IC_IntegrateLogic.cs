using AutoMapper;
using EPAD_Data;
using EPAD_Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPAD_Logic
{

    public class IC_IntegrateLogic : IIC_IntegrateLogic
    {
        private readonly Sync_Context _dbSyncContext;
        private readonly IMapper _mapper;
        private ILogger _logger;
        private string _IntegrateLimitDay;
        private IConfiguration _Configuration;

        public IC_IntegrateLogic(Sync_Context dbSyncContext, IMapper mapper, ILoggerFactory loggerFactory, IConfiguration configuration)
        {
            _dbSyncContext = dbSyncContext;
            _mapper = mapper;
            _logger = loggerFactory.CreateLogger<IC_IntegrateLogic>();
            _Configuration = configuration;
            _IntegrateLimitDay = _Configuration.GetValue<string>("IntegrateLimitDay");
            if (string.IsNullOrEmpty(_IntegrateLimitDay))
            {
                _IntegrateLimitDay = "7";
            }
        }

        public async Task SaveOrUpdateBusinessTravelIntegrate(List<Att_BusinessTravel> businessTravels)
        {
            try
            {
                var limitDay = 0;
                var results = new List<IC_BussinessTravel_Integrate_AVN>();
                if (!string.IsNullOrEmpty(_IntegrateLimitDay))
                {
                    limitDay = int.Parse(_IntegrateLimitDay);
                }
                if (limitDay > 0)
                {
                    var limitDate = DateTime.Now.Date.AddDays(-limitDay);
                    results = await _dbSyncContext.IC_BussinessTravel_Integrate_AVN.Where(x => x.IntegrateDate.Date == DateTime.Now.Date || x.IntegrateDate < limitDate).ToListAsync();
                }
                else
                {
                    results = await _dbSyncContext.IC_BussinessTravel_Integrate_AVN.Where(x => x.IntegrateDate.Date == DateTime.Now.Date).ToListAsync();
                }

                if (results.Count > 0)
                {
                    _dbSyncContext.IC_BussinessTravel_Integrate_AVN.RemoveRange(results);
                }

                var businessTravelIntegrate = _mapper.Map<List<IC_BussinessTravel_Integrate_AVN>>(businessTravels);
                await _dbSyncContext.AddRangeAsync(businessTravelIntegrate);
                await _dbSyncContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"SaveOrUpdateBusinessTravelIntegrate: {ex}");
            }
        }

        public async Task SaveOrUpdateDepartmentIntegrate(List<Cat_OrgStructure> departments)
        {
            try
            {
                var limitDay = 0;
                var results = new List<IC_Department_Integrate_AVN>();
                if (!string.IsNullOrEmpty(_IntegrateLimitDay))
                {
                    limitDay = int.Parse(_IntegrateLimitDay);
                }
                if (limitDay > 0)
                {
                    var limitDate = DateTime.Now.Date.AddDays(-limitDay);
                    results = await _dbSyncContext.IC_Department_Integrate_AVN.Where(x => x.IntegrateDate.Date == DateTime.Now.Date || x.IntegrateDate < limitDate).ToListAsync();
                }
                else
                {
                    results = await _dbSyncContext.IC_Department_Integrate_AVN.Where(x => x.IntegrateDate.Date == DateTime.Now.Date).ToListAsync();
                }


                if (results.Count > 0)
                {
                    _dbSyncContext.IC_Department_Integrate_AVN.RemoveRange(results);
                }

                var departmentIntegrate = _mapper.Map<List<IC_Department_Integrate_AVN>>(departments);
                await _dbSyncContext.AddRangeAsync(departmentIntegrate);
                await _dbSyncContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"SaveOrUpdateDepartmentIntegrate: {ex}");
            }
        }

        public async Task SaveOrUpdateEmployeeIntegrate(List<Hre_Profile> employees)
        {
            try
            {
                var limitDay = 0;
                var results = new List<IC_Employee_Integrate_AVN>();
                if (!string.IsNullOrEmpty(_IntegrateLimitDay))
                {
                    limitDay = int.Parse(_IntegrateLimitDay);
                }
                if (limitDay > 0)
                {
                    var limitDate = DateTime.Now.Date.AddDays(-limitDay);
                    results = await _dbSyncContext.IC_Employee_Integrate_AVN.Where(x => x.IntegrateDate.Date == DateTime.Now.Date || x.IntegrateDate < limitDate).ToListAsync();
                }
                else
                {
                    results = await _dbSyncContext.IC_Employee_Integrate_AVN.Where(x => x.IntegrateDate.Date == DateTime.Now.Date).ToListAsync();
                }

                if (results.Count > 0)
                {
                    _dbSyncContext.IC_Employee_Integrate_AVN.RemoveRange(results);
                }

                var employeeIntegrate = _mapper.Map<List<IC_Employee_Integrate_AVN>>(employees);
                await _dbSyncContext.AddRangeAsync(employeeIntegrate);
                await _dbSyncContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"SaveOrUpdateEmployeeIntegrate: {ex}");
            }
        }

        public async Task SaveOrUpdateEmployeeShiftIntegrate(List<Att_Roster> employeeShifts)
        {
            try
            {
                var limitDay = 0;
                var results = new List<IC_EmployeeShift_Integrate_AVN>();
                if (!string.IsNullOrEmpty(_IntegrateLimitDay))
                {
                    limitDay = int.Parse(_IntegrateLimitDay);
                }
                if (limitDay > 0)
                {
                    var limitDate = DateTime.Now.Date.AddDays(-limitDay);
                    results = await _dbSyncContext.IC_EmployeeShift_Integrate_AVN.Where(x => x.IntegrateDate.Date == DateTime.Now.Date || x.IntegrateDate < limitDate).ToListAsync();
                }
                else
                {
                    results = await _dbSyncContext.IC_EmployeeShift_Integrate_AVN.Where(x => x.IntegrateDate.Date == DateTime.Now.Date).ToListAsync();
                }
                if (results.Count > 0)
                {
                    _dbSyncContext.IC_EmployeeShift_Integrate_AVN.RemoveRange(results);
                }

                var employeeShiftIntegrate = _mapper.Map<List<IC_EmployeeShift_Integrate_AVN>>(employeeShifts);
                await _dbSyncContext.AddRangeAsync(employeeShiftIntegrate);
                await _dbSyncContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"SaveOrUpdateEmployeeShiftIntegrate: {ex}");
            }
        }

        public async Task SaveOrUpdateOverTimePlanIntegrate(List<Att_OverTimePlan> overTimePlans)
        {
            try
            {
                var limitDay = 0;
                var results = new List<IC_OverTimePlan_Integrate_AVN>();
                if (!string.IsNullOrEmpty(_IntegrateLimitDay))
                {
                    limitDay = int.Parse(_IntegrateLimitDay);
                }
                if (limitDay > 0)
                {
                    var limitDate = DateTime.Now.Date.AddDays(-limitDay);
                    results = await _dbSyncContext.IC_OverTimePlan_Integrate_AVN.Where(x => x.IntegrateDate.Date == DateTime.Now.Date || x.IntegrateDate < limitDate).ToListAsync();
                }
                else
                {
                    results = await _dbSyncContext.IC_OverTimePlan_Integrate_AVN.Where(x => x.IntegrateDate.Date == DateTime.Now.Date).ToListAsync();
                }
                if (results.Count > 0)
                {
                    _dbSyncContext.IC_OverTimePlan_Integrate_AVN.RemoveRange(results);
                }

                var overTimePlanIntegrate = _mapper.Map<List<IC_OverTimePlan_Integrate_AVN>>(overTimePlans);
                await _dbSyncContext.AddRangeAsync(overTimePlanIntegrate);
                await _dbSyncContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"SaveOrUpdateOverTimePlanIntegrate: {ex}");
            }
        }

        public async Task SaveOrUpdatePositionIntegrate(List<Cat_Position> positions)
        {
            try
            {
                var limitDay = 0;
                var results = new List<IC_Position_Integrate_AVN>();
                if (!string.IsNullOrEmpty(_IntegrateLimitDay))
                {
                    limitDay = int.Parse(_IntegrateLimitDay);
                }
                if (limitDay > 0)
                {
                    var limitDate = DateTime.Now.Date.AddDays(-limitDay);
                    results = await _dbSyncContext.IC_Position_Integrate_AVN.Where(x => x.IntegrateDate.Date == DateTime.Now.Date || x.IntegrateDate < limitDate).ToListAsync();
                }
                else
                {
                    results = await _dbSyncContext.IC_Position_Integrate_AVN.Where(x => x.IntegrateDate.Date == DateTime.Now.Date).ToListAsync();
                }
                if (results.Count > 0)
                {
                    _dbSyncContext.IC_Position_Integrate_AVN.RemoveRange(results);
                }

                var positionIntegrate = _mapper.Map<List<IC_Position_Integrate_AVN>>(positions);
                await _dbSyncContext.AddRangeAsync(positionIntegrate);
                await _dbSyncContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"SaveOrUpdatePositionIntegrate: {ex}");
            }
        }

        public async Task SaveOrUpdateShiftIntegrate(List<Cat_Shift> shifts)
        {
            try
            {
                var limitDay = 0;
                var results = new List<IC_Shift_Integrate_AVN>();
                if (!string.IsNullOrEmpty(_IntegrateLimitDay))
                {
                    limitDay = int.Parse(_IntegrateLimitDay);
                }
                if (limitDay > 0)
                {
                    var limitDate = DateTime.Now.Date.AddDays(-limitDay);
                    results = await _dbSyncContext.IC_Shift_Integrate_AVN.Where(x => x.IntegrateDate.Date == DateTime.Now.Date || x.IntegrateDate < limitDate).ToListAsync();
                }
                else
                {
                    results = await _dbSyncContext.IC_Shift_Integrate_AVN.Where(x => x.IntegrateDate.Date == DateTime.Now.Date).ToListAsync();
                }
                if (results.Count > 0)
                {
                    _dbSyncContext.IC_Shift_Integrate_AVN.RemoveRange(results);
                }

                var shiftIntegrate = _mapper.Map<List<IC_Shift_Integrate_AVN>>(shifts);
                await _dbSyncContext.AddRangeAsync(shiftIntegrate);
                await _dbSyncContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"SaveOrUpdateShiftIntegrate: {ex}");
            }
        }
    }

    public interface IIC_IntegrateLogic
    {
        Task SaveOrUpdateDepartmentIntegrate(List<Cat_OrgStructure> departments);
        Task SaveOrUpdateBusinessTravelIntegrate(List<Att_BusinessTravel> businessTravels);
        Task SaveOrUpdateEmployeeIntegrate(List<Hre_Profile> employees);
        Task SaveOrUpdateEmployeeShiftIntegrate(List<Att_Roster> employeeShifts);
        Task SaveOrUpdateOverTimePlanIntegrate(List<Att_OverTimePlan> overTimePlans);
        Task SaveOrUpdatePositionIntegrate(List<Cat_Position> positions);
        Task SaveOrUpdateShiftIntegrate(List<Cat_Shift> shifts);
    }
}
