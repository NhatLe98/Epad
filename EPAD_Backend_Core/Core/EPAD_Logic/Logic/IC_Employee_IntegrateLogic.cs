using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EPAD_Logic
{
    public class IC_Employee_IntegrateLogic : IIC_Employee_IntegrateLogic
    {
        private readonly Sync_Context _integrateCustomerContext;
        private readonly ILogger _logger;

        public IC_Employee_IntegrateLogic(Sync_Context integrateCustomerContext, ILoggerFactory loggerFactory) {
            _integrateCustomerContext = integrateCustomerContext;
            _logger = loggerFactory.CreateLogger<IC_Employee_IntegrateLogic>();
        }

        public async Task<List<IC_Department_Integrate_OVN>> GetAllDepartment()
        {
            return await _integrateCustomerContext.IC_Department_Integrate_OVN.ToListAsync();
        }

        public async Task<List<IC_Employee_Integrate>> GetAllAsync()
        {
            return await _integrateCustomerContext.IC_Employee_Integrate.ToListAsync();
        }

        public async Task<List<IC_EmployeeIntegrate>> GetAllEmployeeIntegrateAsync()
        {
            return await _integrateCustomerContext.IC_EmployeeIntegrate.ToListAsync();
        }

        public async Task<List<IC_DepartmentIntegrate>> GetAllDepartmentIntegrateAsync()
        {
            return await _integrateCustomerContext.IC_DepartmentIntegrate.ToListAsync();
        }

        public async Task<List<IC_AttendanceLogIntegrate>> GetAllAttendancelogNotSendAsync()
        {
            return await _integrateCustomerContext.IC_AttendanceLogIntegrate.Where(x => x.IsSend == false || x.IsSend == null).ToListAsync();
        }

        public async Task<List<IC_PositionIntegrate>> GetAllPositionIntegrateAsync()
        {
            return await _integrateCustomerContext.IC_PositionIntegrate.ToListAsync();
        }

        public async Task<List<IC_CardNumberInfo>> GetAllCardNumberInfoIntegrateAsync()
        {
            return await _integrateCustomerContext.IC_CardNumberInfo.ToListAsync();
        }

        public async Task<List<IC_TransferUserIntegrate>> GetAllTransferUserIntegrateAsync()
        {
            return await _integrateCustomerContext.IC_TransferUserIntegrate.Where(x => x.FromDate >= DateTime.Now.Date).ToListAsync() ;
        }


        public void InsertList(List<IC_Employee_Integrate> request)
        {
            try
            {
                _integrateCustomerContext.IC_Employee_Integrate.AddRange(request);
                _integrateCustomerContext.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex}");
            }
        }

        public void RemoveAll()
        {
            try
            {
                if (_integrateCustomerContext.IC_Employee_Integrate.Any())
                {
                    _integrateCustomerContext.IC_Employee_Integrate.RemoveRange(_integrateCustomerContext.IC_Employee_Integrate);
                    _integrateCustomerContext.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex}");
            }
        }

        public List<IC_Employee_Integrate> Update(EmployeeIntegrateResult result)
        {
            try
            {
                if (result != null && result.ListIndexSuccess.Count > 0)
                {
                    var listEmployeeDelete = _integrateCustomerContext.IC_Employee_Integrate
                        .Where(u => result.ListIndexSuccess.Contains(u.Index)).ToList();
                    _integrateCustomerContext.IC_Employee_Integrate.RemoveRange(listEmployeeDelete);
                    _integrateCustomerContext.SaveChanges();
                    return listEmployeeDelete;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Update: {ex}");
            }
            return null;
        }
    }

    public interface IIC_Employee_IntegrateLogic {
        Task<List<IC_Employee_Integrate>> GetAllAsync();
        List<IC_Employee_Integrate> Update(EmployeeIntegrateResult result);
        void RemoveAll();
        void InsertList(List<IC_Employee_Integrate> request);
        Task<List<IC_Department_Integrate_OVN>> GetAllDepartment();
        Task<List<IC_EmployeeIntegrate>> GetAllEmployeeIntegrateAsync();
        Task<List<IC_TransferUserIntegrate>> GetAllTransferUserIntegrateAsync();
        Task<List<IC_CardNumberInfo>> GetAllCardNumberInfoIntegrateAsync();
        Task<List<IC_PositionIntegrate>> GetAllPositionIntegrateAsync();
        Task<List<IC_DepartmentIntegrate>> GetAllDepartmentIntegrateAsync();
        Task<List<IC_AttendanceLogIntegrate>> GetAllAttendancelogNotSendAsync();
    }
}
