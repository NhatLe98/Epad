using EPAD_Common.Services;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using EPAD_Services.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EPAD_Services.Impl
{
    public class IC_PlanDockService : BaseServices<IC_PlanDock, EPAD_Context>, IIC_PlanDockService
    {
        private readonly EPAD_Context _context;
        private readonly ConfigObject _Config;
        public IC_PlanDockService(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _context = serviceProvider.GetService<EPAD_Context>();
            _Config = ConfigObject.GetConfig(_Cache);
        }

        public async Task AddDataToPlanDock(List<IC_PlanDockIntegrate> planDocks)
        {
            var lstTripId = planDocks.Select(x => x.TripId).Distinct().ToList();
            var lstVc = planDocks.Select(x => x.Vc).Distinct().ToList();
            lstTripId.AddRange(lstVc);
            var lstDocks = await _context.IC_PlanDock.Where(x => lstTripId.Contains(x.TripId)).ToListAsync();
            var lstDepartment = await _context.IC_Department.Where(x => x.IsInactive != true).ToListAsync();
            var lstDeparmentImport = planDocks.Select(x => x.Supplier).Where(x => !string.IsNullOrEmpty(x)).ToList();

            var save = false;
            foreach (var supplier in lstDeparmentImport)
            {
                if (!lstDepartment.Any(x => x.Name == supplier))
                {
                    var suppli = new IC_Department()
                    {
                        Name = supplier,
                        Code = DateTime.Now.Ticks.ToString(),
                        CompanyIndex = _Config.CompanyIndex,
                        ParentIndex = 0,
                        ParentCode = string.Empty,
                        UpdatedDate = DateTime.Now,
                        UpdatedUser = UpdatedUser.AutoIntegrateEmployee.ToString(),
                        CreatedDate = DateTime.Now,
                        IsDriverDepartment = true
                    };
                    await _context.IC_Department.AddAsync(suppli);
                    save = true;
                }
            }
            if (save)
            {
                await _context.SaveChangesAsync();
                lstDepartment = await _context.IC_Department.Where(x => x.IsInactive != true).ToListAsync();
            }

            foreach (var planDock in planDocks)
            {
                int department = 0;
                if (!string.IsNullOrEmpty(planDock.Supplier))
                {
                    department = lstDepartment.FirstOrDefault(x => x.Name == planDock.Supplier)?.Index ?? 0;
                }
                if (!string.IsNullOrEmpty(planDock.TripId))
                {
                    if (!lstDocks.Any(x => x.TripId == planDock.TripId))
                    {

                        var trip = new IC_PlanDock()
                        {
                            TripId = planDock.TripId,
                            CompanyIndex = _Config.CompanyIndex,
                            DriverCode = planDock.DriverCode,
                            DriverName = planDock.DriverName,
                            DriverPhone = planDock.DriverPhone,
                            Eta = planDock.Eta,
                            LocationFrom = planDock.LocationFrom,
                            OrderCode = planDock.OrderCode,
                            StatusDock = planDock.StatusDock,
                            Status = planDock.Status,
                            TimesDock = planDock.StatusDock == "0001" ? planDock.TimesDock : null,
                            Supplier = planDock.Supplier,
                            TrailerNumber = planDock.TrailerNumber,
                            Type = planDock.Type,
                            Vc = false,
                            CreatedDate = DateTime.Now,
                            UpdatedUser = UpdatedUser.AutoIntegrateEmployee.ToString(),
                            Operation = planDock.Operation,
                            SupplierId = department
                        };
                        await _context.IC_PlanDock.AddAsync(trip);
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(planDock.TripId))
                        {
                            var trip = lstDocks.FirstOrDefault(x => x.TripId == planDock.TripId);
                            if (trip != null)
                            {
                                trip.CompanyIndex = _Config.CompanyIndex;
                                trip.UpdatedUser = UpdatedUser.AutoIntegrateEmployee.ToString();
                                trip.UpdatedDate = DateTime.Now;
                                trip.TimesDock = planDock.StatusDock == "0001" ? planDock.TimesDock : trip.TimesDock;
                                trip.Status = planDock.Status;
                                trip.StatusDock = planDock.StatusDock;
                                trip.Supplier = planDock.Supplier;
                                trip.Operation = planDock.Operation;
                                trip.SupplierId = department;
                                trip.DriverCode = planDock.DriverCode;
                                trip.DriverPhone = planDock.DriverPhone;
                                trip.Eta = planDock.Eta;
                                trip.LocationFrom= planDock.LocationFrom;
                                trip.OrderCode= planDock.OrderCode;
                                trip.TrailerNumber = planDock.TrailerNumber;
                                trip.DriverName = planDock.DriverName;
                                trip.Type = planDock.Type;
                                _context.IC_PlanDock.Update(trip);
                            }
                        }
                        else
                        {
                            var trip = lstDocks.FirstOrDefault(x => x.TripId == planDock.Vc);
                            if (trip != null)
                            {
                                trip.CompanyIndex = _Config.CompanyIndex;
                                trip.UpdatedUser = UpdatedUser.AutoIntegrateEmployee.ToString();
                                trip.UpdatedDate = DateTime.Now;
                                trip.TimesDock = planDock.StatusDock == "0001" ? planDock.TimesDock : trip.TimesDock;
                                trip.Status = planDock.Status;
                                trip.StatusDock = planDock.StatusDock;
                                trip.Supplier = planDock.Supplier;
                                trip.Operation = planDock.Operation;
                                trip.SupplierId = department;
                                trip.DriverCode = planDock.DriverCode;
                                trip.DriverPhone = planDock.DriverPhone;
                                trip.Eta = planDock.Eta;
                                trip.LocationFrom = planDock.LocationFrom;
                                trip.OrderCode = planDock.OrderCode;
                                trip.TrailerNumber = planDock.TrailerNumber;
                                trip.Type = planDock.Type;
                                trip.DriverName = planDock.DriverName;
                                _context.IC_PlanDock.Update(trip);
                            }
                        }
                    }
                }
                else
                {
                    if (!lstDocks.Any(x => x.TripId == planDock.TripId))
                    {
                        var trip = new IC_PlanDock()
                        {
                            TripId = planDock.Vc,
                            CompanyIndex = _Config.CompanyIndex,
                            DriverCode = planDock.DriverCode,
                            DriverName = planDock.DriverName,
                            DriverPhone = planDock.DriverPhone,
                            Eta = planDock.Eta,
                            LocationFrom = planDock.LocationFrom,
                            OrderCode = planDock.OrderCode,
                            StatusDock = planDock.StatusDock,
                            Status = planDock.Status,
                            TimesDock = planDock.StatusDock == "0001" ? planDock.TimesDock : null,
                            Supplier = planDock.Supplier,
                            TrailerNumber = planDock.TrailerNumber,
                            Type = planDock.Type,
                            Vc = true,
                            CreatedDate = DateTime.Now,
                            UpdatedUser = UpdatedUser.AutoIntegrateEmployee.ToString(),
                            Operation = planDock.Operation,
                            SupplierId = department
                        };
                        await _context.IC_PlanDock.AddAsync(trip);
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(planDock.TripId))
                        {
                            var trip = lstDocks.FirstOrDefault(x => x.TripId == planDock.TripId);
                            if (trip != null)
                            {
                                trip.CompanyIndex = _Config.CompanyIndex;
                                trip.UpdatedUser = UpdatedUser.AutoIntegrateEmployee.ToString();
                                trip.UpdatedDate = DateTime.Now;
                                trip.TimesDock = planDock.StatusDock == "0001" ? planDock.TimesDock : trip.TimesDock; 
                                trip.Status = planDock.Status;
                                trip.StatusDock = planDock.StatusDock;
                                trip.Supplier = planDock.Supplier;
                                trip.Operation = planDock.Operation;
                                trip.SupplierId = department;
                                trip.DriverCode = planDock.DriverCode;
                                trip.DriverPhone = planDock.DriverPhone;
                                trip.Eta = planDock.Eta;
                                trip.LocationFrom = planDock.LocationFrom;
                                trip.OrderCode = planDock.OrderCode;
                                trip.TrailerNumber = planDock.TrailerNumber;
                                trip.Type = planDock.Type;
                                trip.DriverName = planDock.DriverName;
                                _context.IC_PlanDock.Update(trip);
                            }
                        }
                        else
                        {
                            var trip = lstDocks.FirstOrDefault(x => x.TripId == planDock.Vc);
                            if (trip != null)
                            {
                                trip.CompanyIndex = _Config.CompanyIndex;
                                trip.UpdatedUser = UpdatedUser.AutoIntegrateEmployee.ToString();
                                trip.UpdatedDate = DateTime.Now;
                                trip.TimesDock = planDock.StatusDock == "0001" ? planDock.TimesDock : trip.TimesDock;
                                trip.Status = planDock.Status;
                                trip.StatusDock = planDock.StatusDock;
                                trip.Supplier = planDock.Supplier;
                                trip.Operation = planDock.Operation;
                                trip.SupplierId = department;
                                trip.DriverCode = planDock.DriverCode;
                                trip.DriverPhone = planDock.DriverPhone;
                                trip.Eta = planDock.Eta;
                                trip.LocationFrom = planDock.LocationFrom;
                                trip.OrderCode = planDock.OrderCode;
                                trip.TrailerNumber = planDock.TrailerNumber;
                                trip.Type = planDock.Type;
                                trip.DriverName = planDock.DriverName;
                                _context.IC_PlanDock.Update(trip);
                            }
                        }
                    }
                }
                await _context.SaveChangesAsync();
            }
        }
        public async Task UpdateDataPlanDock(List<IC_PlanDockIntegrate> planDocks)
        {
            var lstDepartment = await _context.IC_Department.Where(x => x.IsInactive != true).ToListAsync();
            var lstDeparmentImport = planDocks.Select(x => x.Supplier).Where(x => !string.IsNullOrEmpty(x)).ToList();

            var save = false;
            foreach (var supplier in lstDeparmentImport)
            {
                if (!lstDepartment.Any(x => x.Name == supplier))
                {
                    var suppli = new IC_Department()
                    {
                        Name = supplier,
                        Code = DateTime.Now.Ticks.ToString(),
                        CompanyIndex = _Config.CompanyIndex,
                        ParentIndex = 0,
                        ParentCode = string.Empty,
                        UpdatedDate = DateTime.Now,
                        UpdatedUser = UpdatedUser.AutoIntegrateEmployee.ToString(),
                        CreatedDate = DateTime.Now,
                        IsDriverDepartment = true
                    };
                    await _context.IC_Department.AddAsync(suppli);
                    save = true;
                }
            }
            if (save)
            {
                await _context.SaveChangesAsync();
                lstDepartment = await _context.IC_Department.Where(x => x.IsInactive != true).ToListAsync();
            }

            foreach (var planDock in planDocks)
            {
                int department = 0;
                if (!string.IsNullOrEmpty(planDock.Supplier))
                {
                    department = lstDepartment.FirstOrDefault(x => x.Name == planDock.Supplier)?.Index ?? 0;
                }
                if (!string.IsNullOrEmpty(planDock.TripId))
                {
                    var trip = _context.IC_PlanDock.FirstOrDefault(x => x.TripId == planDock.TripId);
                    if (trip != null)
                    {
                        trip.CompanyIndex = _Config.CompanyIndex;
                        trip.UpdatedUser = UpdatedUser.AutoIntegrateEmployee.ToString();
                        trip.UpdatedDate = DateTime.Now;
                        trip.TimesDock = planDock.StatusDock == "0001" ? planDock.TimesDock : trip.TimesDock;
                        trip.Status = planDock.Status;
                        trip.StatusDock = planDock.StatusDock;
                        trip.Supplier = planDock.Supplier;
                        trip.Operation = planDock.Operation;
                        trip.SupplierId = department;
                        trip.DriverCode = planDock.DriverCode;
                        trip.DriverPhone = planDock.DriverPhone;
                        trip.Eta = planDock.Eta;
                        trip.LocationFrom = planDock.LocationFrom;
                        trip.OrderCode = planDock.OrderCode;
                        trip.TrailerNumber = planDock.TrailerNumber;
                        trip.Type = planDock.Type;
                        trip.DriverName = planDock.DriverName;
                        _context.IC_PlanDock.Update(trip);
                    }
                    else
                    {
                        await AddDataToPlanDock(new List<IC_PlanDockIntegrate>() { planDock });
                    }
                }
                else
                {
                    var trip = _context.IC_PlanDock.FirstOrDefault(x => x.TripId == planDock.Vc);
                    if (trip != null)
                    {
                        trip.CompanyIndex = _Config.CompanyIndex;
                        trip.UpdatedUser = UpdatedUser.AutoIntegrateEmployee.ToString();
                        trip.UpdatedDate = DateTime.Now;
                        trip.TimesDock = planDock.StatusDock == "0001" ? planDock.TimesDock : trip.TimesDock; ;
                        trip.Status = planDock.Status;
                        trip.StatusDock = planDock.StatusDock;
                        trip.Supplier = planDock.Supplier;
                        trip.Operation = planDock.Operation;
                        trip.SupplierId = department;
                        trip.DriverCode = planDock.DriverCode;
                        trip.DriverPhone = planDock.DriverPhone;
                        trip.Eta = planDock.Eta;
                        trip.LocationFrom = planDock.LocationFrom;
                        trip.OrderCode = planDock.OrderCode;
                        trip.TrailerNumber = planDock.TrailerNumber;
                        trip.Type = planDock.Type;
                        trip.DriverName = planDock.DriverName;
                        _context.IC_PlanDock.Update(trip);
                    }
                    else
                    {
                        await AddDataToPlanDock(new List<IC_PlanDockIntegrate>() { planDock });
                    }
                }
            }
            await _context.SaveChangesAsync();
        }

        public async Task<IC_PlanDock> GetPlanDockByDriverCode(string driverCode)
        {
            return await DbContext.IC_PlanDock.AsNoTracking().FirstOrDefaultAsync(x => x.DriverCode == driverCode);
        }

        public async Task<List<IC_PlanDock>> GetListPlanDockByDriverCode(string driverCode)
        {
            return await DbContext.IC_PlanDock.AsNoTracking().Where(x => x.DriverCode == driverCode).ToListAsync();
        }

        public async Task<List<IC_PlanDock>> GetPlanDockByListTripCode(List<string> tripCode)
        {
            return await DbContext.IC_PlanDock.AsNoTracking().Where(x => tripCode.Contains(x.TripId)).ToListAsync();
        }
    }
}
