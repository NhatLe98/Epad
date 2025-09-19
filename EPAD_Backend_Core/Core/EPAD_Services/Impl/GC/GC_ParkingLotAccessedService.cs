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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPAD_Services.Impl
{
    public class GC_ParkingLotAccessedService : BaseServices<GC_ParkingLotAccessed, EPAD_Context>, IGC_ParkingLotAccessedService
    {
        public EPAD_Context _dbContext;
        ConfigObject _Config;
        IMemoryCache _Cache;
        private ILogger _logger;
        private IConfiguration _configuration;
        private string _configClientName;

        private readonly IHR_UserService _HR_UserService;
        private readonly IHR_CustomerInfoService _HR_CustomerInfoService;
        public GC_ParkingLotAccessedService(IServiceProvider serviceProvider, EPAD_Context context,
            ILoggerFactory loggerFactory, IConfiguration configuration) : base(serviceProvider)
        {
            _dbContext = context;
            _Cache = serviceProvider.GetService<IMemoryCache>();
            _Config = ConfigObject.GetConfig(_Cache);
            _logger = loggerFactory.CreateLogger<GC_LinesService>();
            _configuration = configuration;
            _configClientName = _configuration.GetValue<string>("ClientName").ToUpper();

            _HR_UserService = serviceProvider.GetService<IHR_UserService>();
            _HR_CustomerInfoService = serviceProvider.GetService<IHR_CustomerInfoService>();
        }

        public async Task<DataGridClass> GetByFilter(List<short> accessType, List<int> parkingLotIndex, DateTime fromDate, DateTime? toDate, 
            string filter, int page, int pageSize)
        {
            var dataQuery = from pkla in _dbContext.GC_ParkingLotAccessed

                            join pkl in _dbContext.GC_ParkingLot
                            on pkla.ParkingLotIndex equals pkl.Index
                            //into pa
                            //from paResult in pa.DefaultIfEmpty()

                            join emp in _dbContext.HR_User
                            on pkla.EmployeeATID equals emp.EmployeeATID
                            into pke
                            from pkeResult in pke.DefaultIfEmpty()

                            join wk in _dbContext.IC_WorkingInfo.Where(x => x.FromDate.Date <= DateTime.Now.Date
                                && (!x.ToDate.HasValue || (x.ToDate.HasValue && x.ToDate.Value.Date > DateTime.Now.Date)))
                            on pkla.EmployeeATID equals wk.EmployeeATID
                            into pkw
                            from pkwResult in pkw.DefaultIfEmpty()

                            join d in _dbContext.IC_Department
                            on pkwResult.DepartmentIndex equals d.Index
                            into wkd
                            from wkdResult in wkd.DefaultIfEmpty()

                            select new
                            {
                                Index = pkla.Index,
                                EmployeeATID = pkla.EmployeeATID,
                                EmployeeCode = pkeResult != null ? pkeResult.EmployeeCode : string.Empty,
                                FullName = pkeResult != null ? pkeResult.FullName : string.Empty,
                                DepartmentIndex = pkwResult != null ? pkwResult.DepartmentIndex : 0,
                                DepartmentName = wkdResult != null ? wkdResult.Name : "NoDepartment",
                                AccessType = pkla.AccessType,
                                AccessTypeString = pkla.AccessType == 0 ? "Employee" : "Guest",
                                ParkingLotIndex = pkla.ParkingLotIndex,
                                ParkingLotName = pkl != null ? pkl.Name : string.Empty,
                                FromDate = pkla.FromDate,
                                ToDate = pkla.ToDate,
                                FromDateString = pkla.FromDate.ToddMMyyyy(),
                                ToDateString = pkla.ToDate.HasValue ? pkla.ToDate.Value.ToddMMyyyy() : string.Empty,
                                Description = pkla.Description
                            };

            dataQuery = dataQuery.Where(x => x.FromDate.Date >= fromDate.Date);
            if (!string.IsNullOrWhiteSpace(filter))
            {
                var filterList = filter.Split(' ').ToList();
                if (filterList != null && filterList.Count > 0)
                {
                    dataQuery = dataQuery.Where(x => filterList.Contains(x.EmployeeATID) || filterList.Contains(x.EmployeeCode));
                }
            }
            if (toDate.HasValue)
            {
                dataQuery = dataQuery.Where(x => !x.ToDate.HasValue || (x.ToDate.HasValue && x.ToDate.Value.Date <= toDate.Value.Date));
            }
            if (accessType != null && accessType.Count > 0)
            {
                dataQuery = dataQuery.Where(x => accessType.Contains(x.AccessType));
            }
            if (parkingLotIndex != null && parkingLotIndex.Count > 0)
            {
                dataQuery = dataQuery.Where(x => parkingLotIndex.Contains(x.ParkingLotIndex));
            }

            if (page < 1) page = 1;
            var totalCount = dataQuery.Count();
            var result = await dataQuery.OrderBy(x => x.Index).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            var gridClass = new DataGridClass(totalCount, result);

            return gridClass;
        }

        public async Task<GC_ParkingLotAccessed> GetByFilterCustomerAndCompanyIndex(short pAccessType, int pParkingLotIndex, string pCustomerIndex, DateTime pFromDate, DateTime? pToDate, int pCompanyIndex)
        {
            return await DbContext.GC_ParkingLotAccessed
                .FirstOrDefaultAsync(x => x.CompanyIndex == pCompanyIndex
                && x.AccessType == pAccessType
                && x.ParkingLotIndex == pParkingLotIndex
                && x.CustomerIndex == pCustomerIndex
                && (x.ToDate == null || pFromDate <= x.ToDate.Value)
                && (pToDate == null || pToDate >= x.FromDate));
        }
        public async Task<GC_ParkingLotAccessed> GetByFilterAndCompanyIndex(short pAccessType, int pParkingLotIndex, string pEmployeeATID, DateTime pFromDate, DateTime? pToDate, int pCompanyIndex)
        {
            return await DbContext.GC_ParkingLotAccessed
                .FirstOrDefaultAsync(x => x.CompanyIndex == pCompanyIndex
                && x.AccessType == pAccessType
                && x.ParkingLotIndex == pParkingLotIndex
                && x.EmployeeATID == pEmployeeATID
                && (x.ToDate == null || pFromDate <= x.ToDate.Value)
                && (pToDate == null || pToDate >= x.FromDate));
        }
        public async Task<bool> GetByFilterAndCompanyIndexExcludeThis(short pAccessType, int pParkingLotIndex, string pEmployeeATID, DateTime pFromDate, DateTime? pToDate, long pIndex, int pCompanyIndex)
        {
            return await DbContext.GC_ParkingLotAccessed
                .AnyAsync(x => x.CompanyIndex == pCompanyIndex
                && x.Index != pIndex
                && x.AccessType == pAccessType
                && x.ParkingLotIndex == pParkingLotIndex
                && x.EmployeeATID == pEmployeeATID
                && ((x.ToDate.HasValue && ((pToDate.HasValue && ((pToDate.Value.Date >= x.FromDate.Date && pFromDate.Date <= x.FromDate.Date)
                    || (pFromDate.Date >= x.FromDate.Date && pToDate.Value.Date <= x.ToDate.Value.Date)
                    || (pFromDate.Date <= x.ToDate.Value.Date && pToDate.Value.Date >= x.ToDate.Value.Date)))
                            || (!pToDate.HasValue && (pFromDate.Date <= x.FromDate.Date 
                            || (pFromDate.Date >= x.FromDate.Date && pFromDate.Date <= x.ToDate.Value.Date)))))
                        || (!x.ToDate.HasValue && (!pToDate.HasValue
                            || (pToDate.HasValue && pToDate.Value.Date >= x.FromDate.Date)))));
        }
        public async Task<List<GC_ParkingLotAccessed>> GetByFilterEmployeeAndCompanyIndex(short pAccessType, int pParkingLotIndex,
            List<string> pEmployeeATIDs, DateTime pFromDate, DateTime? pToDate, int pCompanyIndex)
        {

            return await _dbContext.GC_ParkingLotAccessed
                .Where(x => x.CompanyIndex == pCompanyIndex
                && x.AccessType == pAccessType
                && x.ParkingLotIndex == pParkingLotIndex
                && pEmployeeATIDs.Contains(x.EmployeeATID)
                && ((x.ToDate.HasValue && ((pToDate.HasValue && ((pToDate.Value.Date >= x.FromDate.Date && pFromDate.Date <= x.FromDate.Date)
                    || (pFromDate.Date >= x.FromDate.Date && pToDate.Value.Date <= x.ToDate.Value.Date)
                    || (pFromDate.Date <= x.ToDate.Value.Date && pToDate.Value.Date >= x.ToDate.Value.Date)))
                            || (!pToDate.HasValue && (pFromDate.Date <= x.FromDate.Date
                            || (pFromDate.Date >= x.FromDate.Date && pFromDate.Date <= x.ToDate.Value.Date)))))
                        || (!x.ToDate.HasValue && (!pToDate.HasValue
                            || (pToDate.HasValue && pToDate.Value.Date >= x.FromDate.Date))))).ToListAsync();
        }
        public async Task<GC_ParkingLotAccessed> GetDataByIndexInt64(long pIndex)
        {
            return await DbContext.GC_ParkingLotAccessed.FindAsync(pIndex);
        }
        public async Task<GC_ParkingLotAccessed> GetDataByIndex(long pIndex)
        {
            return await _dbContext.GC_ParkingLotAccessed.FirstOrDefaultAsync(x => x.Index == pIndex);
        }
        public GC_ParkingLotAccessed GetParkingLotAccessed(List<GC_ParkingLotAccessed> gC_ParkingLotAccessed, short pAccessType, int pParkingLotIndex, string pEmployeeATID, DateTime pFromDate, DateTime? pToDate, int pCompanyIndex)
        {
            return gC_ParkingLotAccessed
                .FirstOrDefault(x => x.CompanyIndex == pCompanyIndex
                && x.AccessType == pAccessType
                && x.ParkingLotIndex == pParkingLotIndex
                && x.EmployeeATID == pEmployeeATID
                && (x.ToDate == null || pFromDate <= x.ToDate.Value)
                && (pToDate == null || pToDate >= x.FromDate));
        }
        public async Task<bool> AddParkingLotAccessed(ParkingLotAccessedParams param, UserInfo user)
        {
            var result = true;
            try
            {
                if (!string.IsNullOrEmpty(param.CustomerIndex) && param.CustomerIndex != "empty")
                {
                    var parkingLotAccessed = new GC_ParkingLotAccessed
                    {
                        AccessType = param.AccessType,
                        EmployeeATID = "",
                        CustomerIndex = param.CustomerIndex,
                        ParkingLotIndex = param.ParkingLotIndex,
                        FromDate = param.FromDate,
                        ToDate = param.ToDate,
                        Description = param.Description,
                        CompanyIndex = user.CompanyIndex,
                        UpdatedDate = DateTime.Now,
                        UpdatedUser = user.UserName
                    };
                    await _dbContext.GC_ParkingLotAccessed.AddAsync(parkingLotAccessed);
                }
                else
                {
                    List<string> EmployeeATIDList = param.EmployeeATIDs;
                    foreach (string EmployeeATID in EmployeeATIDList)
                    {
                        var parkingLotAccessed = new GC_ParkingLotAccessed
                        {
                            AccessType = param.AccessType,
                            EmployeeATID = EmployeeATID,
                            CustomerIndex = "",
                            ParkingLotIndex = param.ParkingLotIndex,
                            FromDate = param.FromDate,
                            ToDate = param.ToDate,
                            Description = param.Description,
                            CompanyIndex = user.CompanyIndex,
                            UpdatedDate = DateTime.Now,
                            UpdatedUser = user.UserName
                        };
                        await _dbContext.GC_ParkingLotAccessed.AddAsync(parkingLotAccessed);
                    }
                }

                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError("AddParkingLotAccessed: " + ex);
                result = false;
            }
            return result;
        }

        public async Task<bool> UpdateParkingLotAccessed(ParkingLotAccessedParams param, UserInfo user)
        {
            var result = true;
            try
            {
                var parkingLotAccessed = await _dbContext.GC_ParkingLotAccessed.FirstOrDefaultAsync(x => x.Index == param.Index);
                parkingLotAccessed.FromDate = param.FromDate;
                parkingLotAccessed.ToDate = param.ToDate;
                parkingLotAccessed.Description = param.Description;
                parkingLotAccessed.CompanyIndex = user.CompanyIndex;
                parkingLotAccessed.UpdatedDate = DateTime.Now;
                parkingLotAccessed.UpdatedUser = user.UserName;
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError("AddParkingLotAccessed: " + ex);
                result = false;
            }
            return result;
        }

        public async Task<bool> DeleteParkingLotAccessed(List<long> indexes)
        {
            var result = true;
            try
            {
                var parkingLotAccessed = await _dbContext.GC_ParkingLotAccessed.Where(x => indexes.Contains(x.Index)).ToListAsync();
                if (parkingLotAccessed != null && parkingLotAccessed.Count > 0)
                {
                    _dbContext.GC_ParkingLotAccessed.RemoveRange(parkingLotAccessed);
                    await _dbContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("AddParkingLotAccessed: " + ex);
                result = false;
            }
            return result;
        }

        public async Task<List<ParkingLotAccessedParams>> ImportParkingLotAccessed(List<ParkingLotAccessedParams> param, UserInfo user)
        {
            var result = param;
            try
            {
                var employeeATIDs = result.Select(x => x.EmployeeATID).ToHashSet();
                var employees = await _HR_UserService.GetEmployeeCompactInfoByEmployeeATID(employeeATIDs.ToList(), DateTime.Now, user.CompanyIndex);

                var parkingLotNames = result.Select(x => x.ParkingLot).ToHashSet();
                var parkingLots = await _dbContext.GC_ParkingLot.Where(x => parkingLotNames.Contains(x.Name)).ToListAsync();

                string[] formats = { "dd/MM/yyyy" };

                long i = 0;
                result.ForEach(x =>
                {
                    ++i;
                    x.RowIndex = i;
                    if (string.IsNullOrWhiteSpace(x.FromDateString))
                    {
                        x.ErrorMessage += "Từ ngày không được để trống\r\n";
                    }
                    else if (!string.IsNullOrWhiteSpace(x.FromDateString))
                    {
                        var fromDate = new DateTime();
                        var convertFromDate = DateTime.TryParseExact(x.FromDateString, formats,
                            System.Globalization.CultureInfo.InvariantCulture,
                            System.Globalization.DateTimeStyles.None, out fromDate);
                        if (!convertFromDate)
                        {
                            x.ErrorMessage += "Từ ngày không hợp lệ\r\n";
                        }
                        else
                        {
                            x.FromDate = fromDate;
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(x.ToDateString))
                    {
                        var toDate = new DateTime();
                        var convertToDate = DateTime.TryParseExact(x.ToDateString, formats,
                            System.Globalization.CultureInfo.InvariantCulture,
                            System.Globalization.DateTimeStyles.None, out toDate);
                        if (!convertToDate)
                        {
                            x.ErrorMessage += "Đến ngày không hợp lệ\r\n";
                        }
                        else
                        {
                            x.ToDate = toDate;
                        }
                    }

                    if (x.ToDate.HasValue && x.ToDate.Value.Date < x.FromDate.Date) 
                    {
                        x.ErrorMessage += "Từ ngày không được lớn hơn đến ngày\r\n";
                    }

                    if (string.IsNullOrWhiteSpace(x.ParkingLot))
                    {
                        x.ErrorMessage += "Nhà xe không được để trống\r\n";
                    }
                    else if (!string.IsNullOrWhiteSpace(x.ParkingLot))
                    {
                        var parkingLot = parkingLots.FirstOrDefault(y => y.Name == x.ParkingLot);
                        if (parkingLot != null)
                        {
                            x.ParkingLotIndex = parkingLot.Index;
                        }
                        else
                        {
                            x.ErrorMessage += "Nhà xe không hợp lệ\r\n";
                        }
                    }

                    if (string.IsNullOrWhiteSpace(x.EmployeeATID))
                    {
                        x.ErrorMessage += "Nhân viên không được để trống\r\n";
                    }
                    else if (!string.IsNullOrWhiteSpace(x.EmployeeATID))
                    {
                        var employee = employees.FirstOrDefault(y => y.EmployeeATID == x.EmployeeATID);
                        if (employee != null)
                        {
                            x.FullName = employee.FullName;
                        }
                        else
                        {
                            x.ErrorMessage += "Nhân viên không tồn tại\r\n";
                        }
                    }
                });

                //if (result.Any(x => !string.IsNullOrWhiteSpace(x.ErrorMessage)))
                //{
                //    return result;
                //}
                //else
                //{
                var listEmpIDs = result.Select(x => x.EmployeeATID).ToHashSet();
                var existedParkingLots = await _dbContext.GC_ParkingLotAccessed.Where(x => listEmpIDs.Contains(x.EmployeeATID)).ToListAsync();
                    
                var allEmployee = await _HR_CustomerInfoService.GetAllCustomerInfo(new string[0], user.CompanyIndex);

                result.ForEach(x =>
                {
                    var employeeExistedParkingLots = existedParkingLots.Where(y => y.EmployeeATID == x.EmployeeATID
                            && y.AccessType == x.AccessType).ToList();
                    var existedEmployee = allEmployee.FirstOrDefault(y => y.EmployeeATID == x.EmployeeATID);

                    if (result.Any(y => y.RowIndex != x.RowIndex 
                        && y.EmployeeATID == x.EmployeeATID 
                        && y.ParkingLotIndex == x.ParkingLotIndex
                        && ((y.ToDate.HasValue && ((x.ToDate.HasValue && ((x.ToDate.Value.Date >= y.FromDate.Date 
                        && x.FromDate.Date <= y.FromDate.Date)
                    || (x.FromDate.Date >= y.FromDate.Date && x.ToDate.Value.Date <= y.ToDate.Value.Date)
                    || (x.FromDate.Date <= y.ToDate.Value.Date && x.ToDate.Value.Date >= y.ToDate.Value.Date)))
                            || (!x.ToDate.HasValue && (x.FromDate.Date <= x.FromDate.Date
                            || (x.FromDate.Date >= y.FromDate.Date && x.FromDate.Date <= y.ToDate.Value.Date)))))
                        || (!y.ToDate.HasValue && (!x.ToDate.HasValue
                            || (x.ToDate.HasValue && x.ToDate.Value.Date >= y.FromDate.Date))))))
                    {
                        x.ErrorMessage += "Dữ liệu đăng ký bị trùng trong tập tin\r\n";
                    }

                    if (employeeExistedParkingLots != null && employeeExistedParkingLots.Count > 0)
                    {
                        if (employeeExistedParkingLots.Any(y => y.ParkingLotIndex == x.ParkingLotIndex
                            && ((y.ToDate.HasValue && ((x.ToDate.HasValue && ((x.ToDate.Value.Date >= y.FromDate.Date 
                        && x.FromDate.Date <= y.FromDate.Date)
                    || (x.FromDate.Date >= y.FromDate.Date && x.ToDate.Value.Date <= y.ToDate.Value.Date)
                    || (x.FromDate.Date <= y.ToDate.Value.Date && x.ToDate.Value.Date >= y.ToDate.Value.Date)))
                            || (!x.ToDate.HasValue && (x.FromDate.Date <= x.FromDate.Date
                            || (x.FromDate.Date >= y.FromDate.Date && x.FromDate.Date <= y.ToDate.Value.Date)))))
                        || (!y.ToDate.HasValue && (!x.ToDate.HasValue
                            || (x.ToDate.HasValue && x.ToDate.Value.Date >= y.FromDate.Date))))))
                        {
                            x.ErrorMessage += "Không được đăng ký cùng nhà xe trong cùng khoảng thời gian\r\n";
                        }
                        else
                        {
                            var parkingLot = new GC_ParkingLotAccessed();
                            parkingLot.EmployeeATID = x.EmployeeATID;
                            parkingLot.AccessType = x.AccessType;
                            parkingLot.FromDate = x.FromDate;
                            if (x.CustomerIndex == "empty")
                            {
                                parkingLot.CustomerIndex = string.Empty;
                            }
                            if (x.ToDate.HasValue)
                            {
                                parkingLot.ToDate = x.ToDate;
                            }
                            //parkingLot.AccessType = x.AccessType;
                            parkingLot.AccessType = (short)((existedEmployee != null
                                && existedEmployee.EmployeeType == (int)EmployeeType.Guest) ? 1 : 0);
                            parkingLot.ParkingLotIndex = x.ParkingLotIndex;
                            parkingLot.Description = x.Description;
                            parkingLot.UpdatedDate = DateTime.Now;
                            parkingLot.UpdatedUser = user.UserName;
                            parkingLot.CompanyIndex = user.CompanyIndex;
                            _dbContext.GC_ParkingLotAccessed.Add(parkingLot);
                        }
                    }
                    else
                    {
                        var parkingLot = new GC_ParkingLotAccessed();
                        parkingLot.EmployeeATID = x.EmployeeATID;
                        parkingLot.AccessType = x.AccessType;
                        parkingLot.FromDate = x.FromDate;
                        if (x.CustomerIndex == "empty")
                        {
                            parkingLot.CustomerIndex = string.Empty;
                        }
                        if (x.ToDate.HasValue)
                        {
                            parkingLot.ToDate = x.ToDate;
                        }
                        //parkingLot.AccessType = x.AccessType;
                        parkingLot.AccessType = (short)((existedEmployee != null
                            && existedEmployee.EmployeeType == (int)EmployeeType.Guest) ? 1 : 0);
                        parkingLot.ParkingLotIndex = x.ParkingLotIndex;
                        parkingLot.Description = x.Description;
                        parkingLot.UpdatedDate = DateTime.Now;
                        parkingLot.UpdatedUser = user.UserName;
                        parkingLot.CompanyIndex = user.CompanyIndex;
                        _dbContext.GC_ParkingLotAccessed.Add(parkingLot);
                    }
                });
                await _dbContext.SaveChangesAsync();
                //}
            }
            catch (Exception ex)
            {
                _logger.LogError("AddParkingLotAccessed: " + ex);
            }
            return result;
        }
    }
}
