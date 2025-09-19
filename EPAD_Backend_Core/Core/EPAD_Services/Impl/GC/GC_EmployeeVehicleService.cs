using EPAD_Common.Enums;
using EPAD_Common.Extensions;
using EPAD_Common.Services;
using EPAD_Common.Types;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using EPAD_Services.Interface;
using EPAD_Services.Plugins;
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
    public class GC_EmployeeVehicleService : BaseServices<GC_EmployeeVehicle, EPAD_Context>, IGC_EmployeeVehicleService
    {
        public EPAD_Context _dbContext;
        private readonly IHR_UserService _HR_UserService;
        public GC_EmployeeVehicleService(IServiceProvider serviceProvider, EPAD_Context context) : base(serviceProvider)
        {
            _dbContext = context;
            _HR_UserService = serviceProvider.GetService<IHR_UserService>();
        }

        public Task<GC_EmployeeVehicle> GetByEmpAndPlate(string pEmployeeATID, string pPlate)
        {
            return FirstOrDefaultAsync(x => x.EmployeeATID == pEmployeeATID && x.Plate == pPlate);
        }
        public Task<GC_EmployeeVehicle> GetByEmpAndPlateByUpdate(int index, string pEmployeeATID, string pPlate)
        {
            return FirstOrDefaultAsync(x => x.Index != index && x.EmployeeATID == pEmployeeATID && x.Plate == pPlate);
        }
        public async Task<DataGridClass> GetEmployeeVehicleByFilter(string filter, int page, int pageSize)
        {
            var dataQuery = from pkla in _dbContext.GC_EmployeeVehicle
                            join emp in _dbContext.HR_User
                            on pkla.EmployeeATID equals emp.EmployeeATID
                            into pke
                            from pkeResult in pke.DefaultIfEmpty()

                            join wk in _dbContext.IC_WorkingInfo.Where(x => x.FromDate.Date <= DateTime.Now.Date
                                && (!x.ToDate.HasValue || (x.ToDate.HasValue && x.ToDate.Value.Date > DateTime.Now.Date)))
                            on pkla.EmployeeATID equals wk.EmployeeATID
                            into pkw
                            from pkwResult in pkw.DefaultIfEmpty()

                            select new
                            {
                                Index = pkla.Index,
                                EmployeeATID = pkla.EmployeeATID,
                                EmployeeCode = pkeResult != null ? pkeResult.EmployeeCode : string.Empty,
                                FullName = pkeResult != null ? pkeResult.FullName : string.Empty,
                                Type = pkla.Type,
                                StatusType = pkla.StatusType,
                                Plate = pkla.Plate,
                                Branch = pkla.Branch,
                                Color = pkla.Color,
                                FromDate = pkla.FromDate,
                                ToDate = pkla.ToDate,
                                FromDateString = pkla.FromDate.ToddMMyyyy(),
                                ToDateString = pkla.ToDate.HasValue ? pkla.ToDate.Value.ToddMMyyyy() : string.Empty
                            };


            if (!string.IsNullOrWhiteSpace(filter))
            {
                dataQuery = dataQuery.Where(x => x.EmployeeATID == filter || x.EmployeeCode == filter);
            }

            if (page < 1) page = 1;
            var totalCount = dataQuery.Count();
            var result = await dataQuery.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            var gridClass = new DataGridClass(totalCount, result);

            return gridClass;
        }

        public async Task<DataGridClass> GetEmployeeVehicleByFilterAdvance(EmployeeVehicleRequest param)
        {
            var dataQuery = from pkla in _dbContext.GC_EmployeeVehicle
                            join emp in _dbContext.HR_User
                            on pkla.EmployeeATID equals emp.EmployeeATID
                            into pke
                            from pkeResult in pke.DefaultIfEmpty()

                            join wk in _dbContext.IC_WorkingInfo.Where(x => x.FromDate.Date <= DateTime.Now.Date
                                && (!x.ToDate.HasValue || (x.ToDate.HasValue && x.ToDate.Value.Date > DateTime.Now.Date)))
                            on pkla.EmployeeATID equals wk.EmployeeATID
                            into pkw
                            from pkwResult in pkw.DefaultIfEmpty()

                            join dp in _dbContext.IC_Department
                            on pkwResult.DepartmentIndex equals dp.Index
                            into pkd
                            from pkdResult in pkd.DefaultIfEmpty()

                            select new
                            {
                                Index = pkla.Index,
                                EmployeeATID = pkla.EmployeeATID,
                                EmployeeCode = pkeResult != null ? pkeResult.EmployeeCode : string.Empty,
                                DepartmentIndex = pkwResult != null ? pkwResult.DepartmentIndex : 0,
                                DepartmentName = pkdResult != null ? pkdResult.Name : string.Empty,
                                FullName = pkeResult != null ? pkeResult.FullName : string.Empty,
                                Type = pkla.Type,
                                StatusType = pkla.StatusType,
                                Plate = pkla.Plate,
                                Branch = pkla.Branch,
                                Color = pkla.Color,
                                FromDate = pkla.FromDate,
                                ToDate = pkla.ToDate,
                                FromDateString = pkla.FromDate.ToddMMyyyy(),
                                ToDateString = pkla.ToDate.HasValue ? pkla.ToDate.Value.ToddMMyyyy() : string.Empty
                            };


            if (!string.IsNullOrWhiteSpace(param.Filter))
            {
                dataQuery = dataQuery.Where(x => x.EmployeeATID.Contains(param.Filter) 
                    || (!string.IsNullOrWhiteSpace(x.EmployeeCode) && x.EmployeeCode.Contains(param.Filter)));
            }

            if (param.DepartmentIndexes != null && param.DepartmentIndexes.Count > 0)
            {
                dataQuery = dataQuery.Where(x => param.DepartmentIndexes.Contains(x.DepartmentIndex));
            }

            if (param.EmployeeATIDs != null && param.EmployeeATIDs.Count > 0)
            {
                dataQuery = dataQuery.Where(x => param.EmployeeATIDs.Contains(x.EmployeeATID));
            }

            if (param.PageIndex < 1) param.PageIndex = 1;
            var totalCount = dataQuery.Count();
            var result = await dataQuery.Skip((param.PageIndex - 1) * param.PageSize).Take(param.PageSize).ToListAsync();

            var gridClass = new DataGridClass(totalCount, result);

            return gridClass;
        }

        public async Task<List<EmployeeVehicleImportData>> ImportEmployeeVehicle(List<EmployeeVehicleImportData> result, UserInfo user)
        {
            try
            {
                var employeeATIDs = result.Select(x => x.EmployeeATID).ToHashSet();
                var employees = await _HR_UserService.GetFilterEmployeeTypeUserCompactInfo(user.CompanyIndex, employeeATIDs.ToList());
                string[] formats = { "dd/MM/yyyy" };
                var empVehicleList = _dbContext.GC_EmployeeVehicle.Where(x => employeeATIDs.Contains(x.EmployeeATID) && x.CompanyIndex == user.CompanyIndex).ToList();
                long i = 0;
                result.ForEach(x =>
                {
                    ++i;
                    x.RowIndex = i;
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
                    if (string.IsNullOrWhiteSpace(x.Plate) && x.StatusType == (short)VehicleStatusType.FixedVehicle)
                    {
                        x.ErrorMessage += "Biển số không được để trống\r\n";
                    }
                    else if (!string.IsNullOrWhiteSpace(x.Plate))
                    {
                        x.Plate = x.Plate;
                        //var plateExist = empVehicleList.FirstOrDefault(_ => _.Plate.Trim().ToUpper() == x.Plate.Trim().ToUpper());
                        //if (plateExist != null && x.EmployeeATID != plateExist.EmployeeATID)
                        //{
                        //    x.ErrorMessage += "Biển số đã được khai báo\r\n";
                        //}
                        //else
                        //{
                            
                        //}
                    }
                });
                var listDataExist = new List<GC_EmployeeVehicle>();
                result.ForEach(x =>
                {
                    if (x.ErrorMessage == null)
                    {
                        var employeeExist = empVehicleList.FirstOrDefault(_ => _.EmployeeATID == x.EmployeeATID && _.Plate == x.Plate);
                        var employeeExistExcel = listDataExist.FirstOrDefault(_ => _.EmployeeATID == x.EmployeeATID && _.Plate == x.Plate);
                        if (employeeExistExcel != null)
                        {
                            x.ErrorMessage += "Dữ liệu đăng ký bị trùng trong tập tin\r\n";
                        }
                        else
                        {
                            if (employeeExist != null)
                            {
                                employeeExist.FromDate = DateTime.Now;
                                employeeExist.ToDate = x.ToDate;
                                employeeExist.VehicleImage = x.VehicleImage;
                                employeeExist.RegistrationImage = x.RegistrationImage;
                                employeeExist.Type = x.Type;
                                employeeExist.StatusType = x.StatusType;
                                employeeExist.Branch = x.Branch;
                                employeeExist.Color = x.Color;
                                employeeExist.UpdatedDate = DateTime.Now;
                                employeeExist.UpdatedUser = user.UserName;
                                employeeExist.Plate = x.Plate;
                                _dbContext.GC_EmployeeVehicle.Update(employeeExist);
                                listDataExist.Add(employeeExist);
                            }
                            else
                            {
                                var data = new GC_EmployeeVehicle()
                                {
                                    EmployeeATID = x.EmployeeATID,
                                    Plate = x.Plate,
                                    Type = x.Type,
                                    StatusType = x.StatusType,
                                    FromDate = DateTime.Now,
                                    Branch = x.Branch,
                                    Color = x.Color,
                                    CreatedDate = DateTime.Now,
                                    UpdatedDate = DateTime.Now,
                                    UpdatedUser = user.UserName,
                                    CompanyIndex = user.CompanyIndex
                                };
                                _dbContext.GC_EmployeeVehicle.Add(data);
                                listDataExist.Add(data);
                            }
                        }
                    }
                });
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                //_logger.LogError("AddParkingLotAccessed: " + ex);
            }
            return result;
        }
    }
}
