using EPAD_Common.Services;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using EPAD_Services.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EPAD_Services.Impl
{
    public class IC_EmployeeShiftService : BaseServices<IC_Employee_Shift, EPAD_Context>, IIC_EmployeeShiftService
    {
        private readonly ILogger _logger;

        public IC_EmployeeShiftService(IServiceProvider serviceProvider, ILogger<IC_EmployeeShiftService> logger) : base(serviceProvider)
        {
            _logger = logger;
        }

        public Task<List<IC_Employee_Shift>> GetAllEmployeeShifts(GetListEmployeeShiftRequest request, int companyIndex)
        {
            try
            {
                request.FromDate = request.FromDate.Date + new TimeSpan(0, 0, 0);
                request.EndDate = request.EndDate.Date + new TimeSpan(24, 0, 0);
                //_logger.LogError($"request.FromDate: {request.FromDate} - {request.EndDat e}");
                //Get all data for this case
                var result = DbSet.Where(x => x.CompanyIndex == companyIndex 
                        //&& x.ShiftApplyDate >= DateTime.Now.AddDays(-1))
                        // && request.ShiftIds.Contains(x.IC_ShiftId))
                        && x.ShiftApplyDate >= request.FromDate && x.ShiftApplyDate <= request.EndDate)
                    //.Include(x => x.IC_Shift)
                    .ToListAsync();

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetAllEmployeeShifts: {ex}");
            }
            return null;
        }
    }
}
