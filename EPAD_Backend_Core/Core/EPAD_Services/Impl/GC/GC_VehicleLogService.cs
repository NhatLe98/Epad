using EPAD_Common.Enums;
using EPAD_Common.Extensions;
using EPAD_Common.Services;
using EPAD_Common.Types;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using EPAD_Services.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
namespace EPAD_Services.Impl
{
    public class GC_VehicleLogService : BaseServices<IC_VehicleLog, EPAD_Context>, IGC_VehicleLogService
    {
        private IServiceScopeFactory _scopeFactory;
        public EPAD_Context _dbContext;
        ConfigObject _Config;
        private ILogger _logger;
        private IConfiguration _configuration;
        private string _configClientName;
        IGC_GatesService _GC_GatesService;
        IGC_Gates_LinesService _GC_Gates_LinesService;
        IGC_Rules_GeneralService _GC_Rules_GeneralService;
        IHR_EmployeeInfoService _HR_EmployeeInfoService;
        public GC_VehicleLogService(IServiceProvider serviceProvider, EPAD_Context context,
            ILoggerFactory loggerFactory, IConfiguration configuration, IServiceScopeFactory scopeFactory) : base(serviceProvider)
        {
            _scopeFactory = scopeFactory;
            _dbContext = context;
            _Cache = serviceProvider.GetService<IMemoryCache>();
            _Config = ConfigObject.GetConfig(_Cache);
            _logger = loggerFactory.CreateLogger<GC_VehicleLogService>();
            _configuration = configuration;
            _configClientName = _configuration.GetValue<string>("ClientName").ToUpper();
            _GC_GatesService = serviceProvider.GetService<IGC_GatesService>();
            _GC_Gates_LinesService = serviceProvider.GetService<IGC_Gates_LinesService>();
            _GC_Rules_GeneralService = serviceProvider.GetService<IGC_Rules_GeneralService>();
            _HR_EmployeeInfoService = serviceProvider.GetService<IHR_EmployeeInfoService>();
        }

        public DataGridClass GetPaginationList(IEnumerable<VehicleHistoryModel> histories, int page, int pageSize)
        {
            var skip = (page - 1) * pageSize;
            if (skip < 0)
            {
                skip = 0;
            }
            int countTotal = histories.Count();
            var dummy = histories.Skip(skip).Take(pageSize).ToList();
            DataGridClass grid = new DataGridClass(countTotal, dummy);
            return grid;
        }

        public List<VehicleHistoryModel> GetHistoryData(List<long> departmentIndexes, List<string> employeeIndexes, UserInfo user, DateTime FromTime, DateTime ToTime, string statusLog, string filter)
        {
            //var rulewarning = _DBContext.GC_Rules_Warning.ToList();
            var histories = DbContext.IC_VehicleLog
              .Where(e => ((employeeIndexes != null && employeeIndexes.Count > 0) ? employeeIndexes.Contains(e.EmployeeATID) : e.EmployeeATID != null)
              && (e.FromDate.Value.Date >= FromTime.Date && (e.ToDate == null || e.ToDate.Value.Date <= ToTime.Date))).ToList();

            var getAllEmployeeATIDInLog = histories.Select(x => x.EmployeeATID).ToList();
            var gateList = DbContext.GC_Gates.ToList();
            var lineList = DbContext.GC_Lines.ToList();
            var gateLine = DbContext.GC_Gates_Lines.ToList();
            var employeeInfoList = _HR_EmployeeInfoService.GetAllEmployeeInfo(getAllEmployeeATIDInLog.ToArray(), user.CompanyIndex).Result;
            if (user != null && user.ListDepartmentAssigned != null && user.ListDepartmentAssigned.Count > 0)
            {
                employeeInfoList = employeeInfoList.Where(x => user.ListDepartmentAssigned.Contains(x.DepartmentIndex)).ToList();
                histories = histories.Where(x => employeeInfoList.Any(y => y.EmployeeATID == x.EmployeeATID)).ToList();
            }
            if ((employeeIndexes == null || (employeeIndexes != null && employeeIndexes.Count == 0)) && departmentIndexes != null
                && departmentIndexes.Count > 0)
            {
                employeeInfoList = employeeInfoList.Where(x => departmentIndexes.Contains(x.DepartmentIndex)).ToList();
                histories = histories.Where(x => employeeInfoList.Any(y => y.EmployeeATID == x.EmployeeATID)).ToList();
            }

            var lstContent = new List<IC_CustomerTypeParam>()
            {
                new IC_CustomerTypeParam(){Code = "1", Name = "Xe máy"},
                new IC_CustomerTypeParam(){Code = "2", Name = "Xe đạp"},
                new IC_CustomerTypeParam(){Code = "3", Name = "Xe đạp điện"},
                new IC_CustomerTypeParam(){Code = "4", Name = "Xe oto"},
            };

            var historyData = new List<VehicleHistoryModel>();

            foreach (var item in histories)
            {
                var employeeInfo = employeeInfoList.FirstOrDefault(x => x.EmployeeATID == item.EmployeeATID);
                //var gateLineInfo = gateLine.FirstOrDefault(x => x.LineIndex == item?.LineIndex);
                //var gateInfo = gateList.FirstOrDefault(x => x.Index == gateLineInfo?.GateIndex);
                //var lineInfo = lineList.FirstOrDefault(x => x.Index == item.LineIndex);

                var data = new VehicleHistoryModel()
                {
                    EmployeeATID = employeeInfo?.EmployeeATID ?? item.EmployeeATID,
                    EmployeeCode = employeeInfo?.EmployeeCode ?? item.EmployeeATID,
                    CustomerName = employeeInfo?.FullName ?? "",
                    DepartmentName = employeeInfo?.DepartmentName ?? "",
                    FromTime = item.FromDate,
                    ToTime = item.ToDate,
                    ComputerIn = item.ComputerIn,
                    ComputerOut = item.ComputerOut,
                    Plate = item.Plate != null ? item.Plate : "",
                    VehicleType = lstContent.FirstOrDefault(x => x.Code == item.VehicleTypeId.ToString())!= null ? lstContent.FirstOrDefault(x => x.Code == item.VehicleTypeId.ToString()).Name : "", 
                    //LineIndex = (int)(lineInfo?.Index ?? item.LineIndex),
                    //LineName = lineInfo?.Name ?? "",
                    CardNumber = employeeInfo?.CardNumber,
                    PhoneNumber = employeeInfo?.Phone,
                    //CustomerImage = employeeInfo?.Avatar,
                    Note = item.Note,
                    Reason = item.Reason
                };

                historyData.Add(data);
            }

            if (!string.IsNullOrWhiteSpace(filter))
            {
                historyData = historyData.Where(x => x.Plate.Trim().ToLower().Contains(filter.Trim().ToLower()) || x.EmployeeATID.Contains(filter) || x.CustomerName.Trim().ToLower().Contains(filter.Trim().ToLower()) || x.VehicleType.Trim().ToLower().Contains(filter.Trim().ToLower())).ToList();
            }
            return historyData;
        }
    }

}
