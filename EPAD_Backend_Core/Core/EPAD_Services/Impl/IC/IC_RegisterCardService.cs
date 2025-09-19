using EPAD_Common.Services;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using EPAD_Data.Models.Other;
using EPAD_Services.Interface;
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
    public class IC_RegisterCardService : BaseServices<GC_TimeLog, EPAD_Context>, IIC_RegisterCardService
    {
        protected readonly IConfiguration _Configuration;
        protected readonly VinParkingSetting _VinParkingSetting;
        private readonly ILogger _logger;
        protected readonly IHR_UserService _HR_UserService;

        public IC_RegisterCardService(IServiceProvider serviceProvider, IConfiguration Configuration, ILogger<IC_RegisterCardService> logger) : base(serviceProvider)
        {
            _Configuration = Configuration;
            _VinParkingSetting = _Configuration.GetSection("VinParkingSetting").Get<VinParkingSetting>();
            _logger = logger;
            _HR_UserService = serviceProvider.GetService<IHR_UserService>();
        }

        public async Task RegisterMonthCard(List<IC_RegisterCard> lstCard)
        {
            var register = MapEmployeeVehicleToRegisterCard(lstCard);
            var cards = DbContext.IC_RegisterCard.ToList();
            DbContext.RemoveRange(cards);
            await DbContext.SaveChangesAsync();
            DbContext.AddRange(lstCard);
            await DbContext.SaveChangesAsync();
            HttpClient httpClient = new HttpClient();
            var json = JsonConvert.SerializeObject(register);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await httpClient.PostAsync(_VinParkingSetting.Host + "/api/registerlist?TenDangNhap=" + _VinParkingSetting.Account + "&MatKhau=" + _VinParkingSetting.Password, content);
            string data = await response.Content.ReadAsStringAsync();

            _logger.LogError($"{data}");
        }

        public async Task<List<IC_RegisterCard>> GetRegisterCardInfo()
        {
            var card = (from pla in DbContext.GC_ParkingLotAccessed
                        join emp in DbContext.GC_EmployeeVehicle
                        on pla.EmployeeATID equals emp.EmployeeATID into lstEmp
                        from d in lstEmp.DefaultIfEmpty()
                        select new GC_EmployeeVehicleInfo()
                        {
                            EmployeeATID = pla.EmployeeATID,
                            Branch = d.Branch,
                            FromDate = pla.FromDate,
                            ToDate = pla.ToDate,
                            Note = pla.Description,
                            CategoryId = 1,
                            Plate = d.Plate
                        }).ToList();

            var guest = DbContext.GC_Customer.Select(x => new GC_EmployeeVehicleInfo()
            {
                EmployeeATID = x.EmployeeATID,
                Branch = x.BikeModel,
                FromDate = x.FromTime,
                ToDate = x.ToTime,
                Note = x.BikeDescription,
                Plate = x.BikePlate,
                CategoryId = 1,
            }).ToList();

            card.AddRange(guest);

            var employeeLst = card.Select(x => x.EmployeeATID).Distinct().ToList();

            var lstEmployee = await _HR_UserService.GetEmployeeCompactInfoByEmployeeATID(employeeLst,
                          DateTime.Now, 2);
            var lstguest = lstEmployee.Where(x => x.Department == "All Visitor").Select(x => x.EmployeeATID).ToList();
            card = card.Where(x => !string.IsNullOrEmpty(x.Plate) || (lstguest.Contains(x.EmployeeATID))).ToList();
            var result = card.Select(x => new IC_RegisterCard
            {
                EmployeeATID = x.EmployeeATID,
                Branch = lstEmployee.FirstOrDefault(y => y.EmployeeATID == x.EmployeeATID)?.Department == "All Visitor" ? "" : x.Branch ?? "",
                FromDate = x.FromDate,
                ToDate = x.ToDate,
                Note = x.Note ?? "",
                Plate = lstEmployee.FirstOrDefault(y => y.EmployeeATID == x.EmployeeATID)?.Department == "All Visitor" ? "" : x.Plate ?? "",
                Account = _VinParkingSetting.Account,
                Password = _VinParkingSetting.Password,
                Address = "",
                Email = lstEmployee.FirstOrDefault(y => y.EmployeeATID == x.EmployeeATID)?.Email ?? "",
                EmployeeName = lstEmployee.FirstOrDefault(y => y.EmployeeATID == x.EmployeeATID)?.FullName ?? "",
                Phone = lstEmployee.FirstOrDefault(y => y.EmployeeATID == x.EmployeeATID)?.Phone ?? "",
                VehicleTypeId = lstEmployee.FirstOrDefault(y => y.EmployeeATID == x.EmployeeATID)?.Department == "All Visitor" ? 2 : x.CategoryId,
                CategoryId = 1,
                CardNumber = lstEmployee.FirstOrDefault(y => y.EmployeeATID == x.EmployeeATID)?.CardNumber ?? "",
            }).ToList();
            return result;
        }

        private List<IC_RegisterCardIntegrate> MapEmployeeVehicleToRegisterCard(List<IC_RegisterCard> lstEmployee)
        {
            var result = new List<IC_RegisterCardIntegrate>();
            foreach (var item in lstEmployee)
            {
                result.Add(new IC_RegisterCardIntegrate
                {
                    BienSo = item.Plate,
                    DanhMucId = item.CategoryId,
                    DiaChi = item.Address,
                    DienThoai = item.Phone,
                    Email = item.Email,
                    ChuXe = item.EmployeeName,
                    GhiChu = item.Note,
                    LoaiVeId = item.VehicleTypeId,
                    MaThe = item.CardNumber,
                    NgayHetHan = item.ToDate != null ? item.ToDate.Value.ToString("MM/dd/yyyy") : "",
                    NgayKichHoat = item.FromDate.ToString("MM/dd/yyyy"),
                    NhanHieu = item.Branch,
                    EnrollNumber = Int32.Parse(item.EmployeeATID)
                });
            }
            return result;
        }

        public async Task IntegrateAttendanceLog()
        {
            var fromDate = new DateTime(2000, 1, 1);
            var toDate = DateTime.Now.AddDays(1);
            _logger.LogError(fromDate.ToString("yyyy-MM-dd"));
            HttpClient httpClient = new HttpClient();
            HttpResponseMessage response = await httpClient.PostAsync(_VinParkingSetting.Host + "/api/attendanceLogs?TenDangNhap=admin&MatKhau=123&Tu=" +
                fromDate.ToString("yyyy-MM-dd") + "&Den=" + toDate.ToString("yyyy-MM-dd"), null);

            string data = await response.Content.ReadAsStringAsync();

            var result = JsonConvert.DeserializeObject<VehicleLogLogResult>(data);
            if (result != null && result.data != null && result.data.Count > 0)
            {
                var lstLog = DbContext.IC_VehicleLog.Where(x => x.FromDate >= fromDate).ToList();

                foreach (var item in result.data)
                {
                    var itemDb = lstLog.FirstOrDefault(x => x.CardNumber == item.MaThe && x.FromDate == item.ThoiGianVao);
                    if (itemDb != null && item.ThoiGianRa == DateTime.MinValue)
                    {
                        continue;
                    }
                    else if (itemDb != null)
                    {
                        itemDb.FromDate = item.ThoiGianVao;
                        itemDb.ToDate = item.ThoiGianRa;
                        itemDb.Status = item.TrangThai;
                        itemDb.ComputerIn = item.MayTinhVao;
                        itemDb.ComputerOut = item.MayTinhRa;
                    }
                    else
                    {
                        itemDb = new IC_VehicleLog();
                        itemDb.IntegrateId = item.Id;
                        itemDb.CardNumber = item.MaThe;
                        itemDb.Plate = item.BienSo;
                        itemDb.VehicleTypeId = item.LoaiVeId;
                        itemDb.CategoryId = item.VeThangId;
                        itemDb.FromDate = item.ThoiGianVao;
                        itemDb.ToDate = item.ThoiGianRa;
                        itemDb.Status = item.TrangThai;
                        itemDb.ComputerIn = item.MayTinhVao;
                        itemDb.ComputerOut = item.MayTinhRa;
                        DbContext.IC_VehicleLog.Add(itemDb);
                    }
                }
                await DbContext.SaveChangesAsync();
            }

        }
    }
}
