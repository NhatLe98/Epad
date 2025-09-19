using EPAD_Common.Extensions;
using EPAD_Common.Services;
using EPAD_Common.Types;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using EPAD_Services.Interface;
using EPAD_Repository.Interface;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace EPAD_Services.Impl
{
    public class IC_DeviceService : BaseServices<IC_Device, EPAD_Context>, IIC_DeviceService
    {
        private readonly IMemoryCache mCache;
        private readonly IIC_PrivilegeDeviceDetailsRepository iC_PrivilegeDeviceDetailsRepository;
        private readonly IIC_GroupDeviceDetailsRepository iC_GroupDeviceDetailsRepository;
        private readonly IIC_ServiceAndDevicesRepository iC_DeviceAndServiceRepository;
        private readonly ILogger _logger;

        public IC_DeviceService(IServiceProvider serviceProvider, ILoggerFactory loggerFactory) : base(serviceProvider)
        {
            mCache = serviceProvider.GetService<IMemoryCache>();
            iC_PrivilegeDeviceDetailsRepository = serviceProvider.GetService<IIC_PrivilegeDeviceDetailsRepository>();
            iC_GroupDeviceDetailsRepository = serviceProvider.GetService<IIC_GroupDeviceDetailsRepository>();
            iC_DeviceAndServiceRepository = serviceProvider.GetService<IIC_ServiceAndDevicesRepository>();
            _logger = loggerFactory.CreateLogger<IC_DeviceService>();
        }

        public async Task<DataGridClass> GetDataGrid(string pFilter, UserInfo pUser, int pPage = 1, int pPageSize = 50)
        {
            if (string.IsNullOrEmpty(pFilter)) pFilter = "";
            var isCheckOnOff = false;
            var isOnline = true;

            if(pFilter.ToLower() == "online" || pFilter.ToLower() == "offline")
            {
                isCheckOnOff = true;
                if(pFilter.ToLower() == "offline")
                {
                    isOnline = false;
                }
                pFilter = "";
            }

            List<DeviceType> deviceTypes = new List<DeviceType>() {
                new DeviceType(){ Id = 0, InfoDeviceType = "Thẻ" },
                new DeviceType(){ Id = 1, InfoDeviceType = "Vân tay" },
                new DeviceType(){ Id = 2, InfoDeviceType = "Khuôn mặt" },
                new DeviceType(){ Id = 3, InfoDeviceType = "Thẻ và vân tay" },
                new DeviceType(){ Id = 4, InfoDeviceType = "Thẻ vân tay khuôn mặt" }
            };

            List<DeviceModule> deviceModules = new List<DeviceModule>() {
                new DeviceModule(){ Key = "TA", Value = "Chấm công" },
                new DeviceModule(){ Key = "GCS", Value = "Quản lý cổng" },
                new DeviceModule(){ Key = "AC", Value = "Kiểm soát truy cập" },
                new DeviceModule(){ Key = "PA", Value = "Bãi xe" },
                new DeviceModule(){ Key = "ICMS", Value = "Nhà ăn" }
            };

            var listdevieProcerss = DbContext.IC_SystemCommand.Where(x => x.CompanyIndex == pUser.CompanyIndex && x.Excuted == false && x.ExcutingServiceIndex > 0)
                .AsEnumerable().GroupBy(x => x.SerialNumber);
            var deviceProcessing = listdevieProcerss.ToDictionary(x => x.Key, x => x.ToList());
            var devPrivileges = new List<string>();
            if (pUser.IsAdmin)
            {
                devPrivileges = DbContext.IC_Device.Where(x => x.CompanyIndex == pUser.CompanyIndex).Select(x => x.SerialNumber).AsEnumerable().ToList();
            }
            else
            {
                devPrivileges = DbContext.IC_PrivilegeDeviceDetails.Where(x => x.PrivilegeIndex == pUser.PrivilegeIndex && x.Role != "None").Select(x => x.SerialNumber).AsEnumerable().ToList();
            }

             
            var listSevice = DbContext.IC_Service.ToList();
            var listServiceAndDevice = DbContext.IC_ServiceAndDevices.ToList();

            var listGroupDevice = DbContext.IC_GroupDevice.ToList();
            var listGroupDeviceDetails = DbContext.IC_GroupDeviceDetails.ToList();

            var obj = from dev in devPrivileges
                      join d in DbContext.IC_Device.Where(x => x.CompanyIndex == pUser.CompanyIndex)
                        on dev equals d.SerialNumber
                      join t in deviceTypes on d.DeviceType equals t.Id into dvt
                      from dvtResult in dvt.DefaultIfEmpty()
                      where
                      (
                     
                                d.SerialNumber.Contains(pFilter, StringComparison.OrdinalIgnoreCase)
                            || (d.AliasName == null ? "" : d.AliasName.ToString()).Contains(pFilter, StringComparison.OrdinalIgnoreCase)
                            || (d.IPAddress == null ? "" : d.IPAddress.ToString()).Contains(pFilter, StringComparison.OrdinalIgnoreCase)
                            || (d.Port.HasValue ? "" : d.Port.ToString()).Contains(pFilter, StringComparison.OrdinalIgnoreCase)
                            || (d.AttendanceLogCount.HasValue && d.AttendanceLogCount.Value.ToString().Contains(pFilter, StringComparison.OrdinalIgnoreCase))
                            || (d.UserCount.HasValue && d.UserCount.Value.ToString().Contains(pFilter, StringComparison.OrdinalIgnoreCase))
                            || (d.FingerCount.HasValue && d.FingerCount.Value.ToString().Contains(pFilter, StringComparison.OrdinalIgnoreCase))
                            || (dvtResult != null &&  dvtResult.InfoDeviceType.Contains(pFilter, StringComparison.OrdinalIgnoreCase))
                            || (d.LastConnection.GetValueOrDefault().ToString("d/M/yyyy HH:mm:ss tt").Contains(pFilter, StringComparison.OrdinalIgnoreCase))
                      )

                      select new { Dev = d, DeviceType = dvtResult };

            var rs = obj.AsEnumerable().Select(x =>
            {
                IC_Device device = x.Dev;

                int serviceIndexOfDevice = listServiceAndDevice
                    .FirstOrDefault(sd => sd.SerialNumber == device.SerialNumber)?.ServiceIndex ?? 0;
                IC_Service serviceOfDevice = listSevice.FirstOrDefault(s => s.Index == serviceIndexOfDevice);

                int groupIndexOfDevice = listGroupDeviceDetails.FirstOrDefault(gd => gd.SerialNumber == device.SerialNumber)?.GroupDeviceIndex ?? 0;
                IC_GroupDevice groupOfDevice = listGroupDevice.FirstOrDefault(g => g.Index == groupIndexOfDevice);

                return new
                {
                    AliasName = !string.IsNullOrWhiteSpace(x?.Dev.AliasName) ? x?.Dev.AliasName : x?.Dev.SerialNumber,
                    SerialNumber = x?.Dev.SerialNumber,
                    IPAddress = x?.Dev.IPAddress,
                    Port = x?.Dev.Port,
                    DeviceType = x?.Dev.DeviceType,
                    UseSDK = x?.Dev.UseSDK,
                    UsePush = x?.Dev.UsePush,
                    DeviceName = SetNameForDevice(x.Dev.DeviceType),
                    IsSDK = x.Dev.UseSDK == true ? "Có" : "Không",
                    IsPush = x.Dev.UsePush == true ? "Có" : "Không",
                    UserCount = x?.Dev.UserCount,
                    FingerCount = x?.Dev.FingerCount,
                    AdminCount = x?.Dev.AdminCount,
                    FaceCount = x?.Dev.FaceCount,
                    AttendanceLogCount = x?.Dev.AttendanceLogCount,
                    AttendanceLogCapacity = x?.Dev.AttendanceLogCapacity,
                    UserCapacity = x?.Dev.UserCapacity,
                    FingerCapacity = x?.Dev.FingerCapacity,
                    FaceCapacity = x?.Dev.FaceCapacity,
                    HardWareLicense = mCache.HaveHWLicense(x.Dev.SerialNumber, 1) ? "HwLicense" : "NotLicense",
                    HardWareLicenseExpireDate = mCache.CheckHWLicenseExpireDate(x.Dev.SerialNumber, 1).Item1,
                    HardWareLicenseStatus = mCache.CheckHWLicenseExpireDate(x.Dev.SerialNumber, 1).Item2,
                    LastConnection = x.Dev.LastConnection.HasValue ? x.Dev.LastConnection.Value.ToddMMyyyyHHmmss() : "",
                    Status = GetDeviceStatus(x.Dev, mCache, deviceProcessing),
                    ListRunningCommand = GetRunningCommand(x.Dev, mCache, deviceProcessing),
                    ConnectionCode = x?.Dev.ConnectionCode,
                    DeviceId = x?.Dev.DeviceId,
                    DeviceModel = x?.Dev.DeviceModel,
                    DeviceStatus = x?.Dev.DeviceStatus,
                    DeviceModule = x?.Dev.DeviceModule,
                    DeviceModuleName = x?.Dev.DeviceModule,
                    ServiceName = serviceOfDevice?.Name,
                    ServiceID = serviceOfDevice?.Index,
                    GroupDeviceID = groupOfDevice?.Index,
                    GroupDeviceName = groupOfDevice?.Name,
                    Note = x?.Dev.Note,
                    Account = x?.Dev.Account,
                    ParkingType = x?.Dev.ParkingType,
                    IsUsingOffline = x?.Dev.IsUsingOffline
                };
            });

            if (isCheckOnOff)
            {
                if (isOnline)
                {
                    rs = rs.Where(x => x.Status != "Offline");
                }
                else
                {
                    rs = rs.Where(x => x.Status == "Offline");
                }
            }

            if (pPage <= 1) pPage = 1;
            int fromRow = pPageSize * (pPage - 1);
            int record = rs.Count();
            var lsDevice = rs.OrderBy(t => t.AliasName).Skip(fromRow).Take(pPageSize).ToList();
            DataGridClass dataGrid = new DataGridClass(record, lsDevice);
            return await Task.FromResult(dataGrid);
        }

        public async Task<DataGridClass> GetDeviceInfo(UserInfo pUser)
        {

            List<DeviceType> deviceTypes = new List<DeviceType>() {
                new DeviceType(){ Id = 0, InfoDeviceType = "Thẻ" },
                new DeviceType(){ Id = 1, InfoDeviceType = "Vân tay" },
                new DeviceType(){ Id = 2, InfoDeviceType = "Khuôn mặt" },
                new DeviceType(){ Id = 3, InfoDeviceType = "Thẻ và vân tay" },
                new DeviceType(){ Id = 4, InfoDeviceType = "Thẻ vân tay khuôn mặt" }
            };

            List<DeviceModule> deviceModules = new List<DeviceModule>() {
                new DeviceModule(){ Key = "TA", Value = "Chấm công" },
                new DeviceModule(){ Key = "GCS", Value = "Quản lý cổng" },
                new DeviceModule(){ Key = "AC", Value = "Kiểm soát truy cập" },
                new DeviceModule(){ Key = "PA", Value = "Bãi xe" },
                new DeviceModule(){ Key = "ICMS", Value = "Nhà ăn" }
            };

            var listDeviceProcess = DbContext.IC_SystemCommand.Where(x => x.CompanyIndex == pUser.CompanyIndex && x.Excuted == false && x.ExcutingServiceIndex > 0)
                .AsEnumerable().GroupBy(x => x.SerialNumber);
            var deviceProcessing = listDeviceProcess.ToDictionary(x => x.Key, x => x.ToList());

            var devPrivileges = new List<string>();
            if (pUser.IsAdmin)
            {
                devPrivileges = DbContext.IC_Device.Where(x => x.CompanyIndex == pUser.CompanyIndex).Select(x => x.SerialNumber).AsEnumerable().ToList();
            }
            else
            {
                devPrivileges = DbContext.IC_PrivilegeDeviceDetails.Where(x => x.PrivilegeIndex == pUser.PrivilegeIndex && x.Role != "None").Select(x => x.SerialNumber).AsEnumerable().ToList();
            }
            var listSevice = DbContext.IC_Service.ToList();
            var listServiceAndDevice = DbContext.IC_ServiceAndDevices.ToList();

            var listGroupDevice = DbContext.IC_GroupDevice.ToList();
            var listGroupDeviceDetails = DbContext.IC_GroupDeviceDetails.ToList();

            var obj = from dev in devPrivileges
                      join d in DbContext.IC_Device.Where(x => x.CompanyIndex == pUser.CompanyIndex)
                        on dev equals d.SerialNumber
                      join t in deviceTypes on d.DeviceType equals t.Id into dvt
                      from dvtResult in dvt.DefaultIfEmpty()

                      select new { Dev = d, DeviceType = dvtResult };

            var rs = obj.AsEnumerable().Select(x =>
            {
                IC_Device device = x.Dev;

                int serviceIndexOfDevice = listServiceAndDevice
                    .FirstOrDefault(sd => sd.SerialNumber == device.SerialNumber)?.ServiceIndex ?? 0;
                IC_Service serviceOfDevice = listSevice.FirstOrDefault(s => s.Index == serviceIndexOfDevice);

                int groupIndexOfDevice = listGroupDeviceDetails.FirstOrDefault(gd => gd.SerialNumber == device.SerialNumber)?.GroupDeviceIndex ?? 0;
                IC_GroupDevice groupOfDevice = listGroupDevice.FirstOrDefault(g => g.Index == groupIndexOfDevice);

                return new
                {
                    AliasName = x?.Dev.AliasName,
                    SerialNumber = x?.Dev.SerialNumber,
                    IPAddress = x?.Dev.IPAddress,
                    Port = x?.Dev.Port,
                    DeviceType = x?.Dev.DeviceType,
                    UseSDK = x?.Dev.UseSDK,
                    UsePush = x?.Dev.UsePush,
                    DeviceName = SetNameForDevice(x.Dev.DeviceType),
                    IsSDK = x.Dev.UseSDK == true ? "Có" : "Không",
                    IsPush = x.Dev.UsePush == true ? "Có" : "Không",
                    UserCount = x?.Dev.UserCount,
                    FingerCount = x?.Dev.FingerCount,
                    AdminCount = x?.Dev.AdminCount,
                    FaceCount = x?.Dev.FaceCount,
                    AttendanceLogCount = x?.Dev.AttendanceLogCount,
                    AttendanceLogCapacity = x?.Dev.AttendanceLogCapacity,
                    UserCapacity = x?.Dev.UserCapacity,
                    FingerCapacity = x?.Dev.FingerCapacity,
                    FaceCapacity = x?.Dev.FaceCapacity,
                    HardWareLicense = mCache.HaveHWLicense(x.Dev.SerialNumber, 1) ? "HwLicense" : "NotLicense",
                    HardWareLicenseExpireDate = mCache.CheckHWLicenseExpireDate(x.Dev.SerialNumber, 1).Item1,
                    HardWareLicenseStatus = mCache.CheckHWLicenseExpireDate(x.Dev.SerialNumber, 1).Item2,
                    LastConnection = x.Dev.LastConnection.HasValue ? x.Dev.LastConnection.Value.ToddMMyyyyHHmmss() : "",
                    Status = GetDeviceStatus(x.Dev, mCache, deviceProcessing),
                    ListRunningCommand = GetRunningCommand(x.Dev, mCache, deviceProcessing),
                    ConnectionCode = x?.Dev.ConnectionCode,
                    DeviceId = x?.Dev.DeviceId,
                    DeviceModel = x?.Dev.DeviceModel,
                    DeviceStatus = x?.Dev.DeviceStatus,
                    DeviceModule = x?.Dev.DeviceModule,
                    DeviceModuleName = x?.Dev.DeviceModule,
                    ServiceName = serviceOfDevice?.Name,
                    ServiceID = serviceOfDevice?.Index,
                    GroupDeviceID = groupOfDevice?.Index,
                    GroupDeviceName = groupOfDevice?.Name,
                    Note = x?.Dev.Note,
                    Account = x?.Dev.Account,
                    ParkingType = x?.Dev.ParkingType,
                };
            });

            var lsDevice = rs.ToList();
            var recordCount = lsDevice.Count;
            DataGridClass dataGrid = new DataGridClass(recordCount, lsDevice);
            return await Task.FromResult(dataGrid);
        }

        public Task<List<IC_Device>> GetListDeviceByDeviceModule(List<string> pListDeviceModule, int pCompanyIndex) 
        {
            var result = DbContext.IC_Device.Where(x => x.CompanyIndex == pCompanyIndex && (pListDeviceModule.Contains(x.DeviceModule)
                || string.IsNullOrWhiteSpace(x.DeviceModule))).ToList();

            return Task.FromResult(result);
        }

        public async Task<IC_Device> CreateNewDeviceAsync(IC_Device device,
            int userPrivilegeIndex, int? serviceIndex, int? groupDeviceIndex)
        {
            IC_Device entity;
            try
            {
                entity = this.Repository.Insert(device);

                void InsertPrivilegeDetails(int userPrivilegeIndex)
                {
                    iC_PrivilegeDeviceDetailsRepository.Repository.Insert(new IC_PrivilegeDeviceDetails
                    {
                        CompanyIndex = device.CompanyIndex,
                        PrivilegeIndex = userPrivilegeIndex,
                        Role = "Full",
                        SerialNumber = device.SerialNumber,
                        UpdatedDate = device.UpdatedDate,
                        UpdatedUser = device.UpdatedUser,
                    });
                }

                InsertPrivilegeDetails(userPrivilegeIndex);

                if (groupDeviceIndex.HasValue)
                    this.iC_GroupDeviceDetailsRepository.Repository.Insert(new IC_GroupDeviceDetails
                    {
                        CompanyIndex = device.CompanyIndex,
                        GroupDeviceIndex = groupDeviceIndex.Value,
                        SerialNumber = device.SerialNumber,
                        UpdatedUser = device.UpdatedUser,
                        UpdatedDate = DateTime.Now,
                    });
                if (serviceIndex.HasValue)
                {
                    this.iC_DeviceAndServiceRepository.Repository.Insert(new IC_ServiceAndDevices
                    {
                        CompanyIndex = device.CompanyIndex,
                        SerialNumber = device.SerialNumber,
                        ServiceIndex = serviceIndex.Value,
                        UpdatedDate = DateTime.Now,
                        UpdatedUser = device.UpdatedUser
                    });
                }

                await this.UnitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError("CreateNewDeviceAsync: " + ex.Message);
                throw ex;
            }
            return entity;
        }



        private string SetNameForDevice(int? type)
        {
            string name = "";
            switch (type)
            {
                case 0:
                    name = "Thẻ";
                    break;
                case 1:
                    name = "Vân tay";
                    break;
                case 2:
                    name = "Khuôn mặt";
                    break;
                case 3:
                    name = "Thẻ và vân tay";
                    break;
                case 4:
                    name = "Thẻ vân tay khuôn mặt";
                    break;
                default:
                    break;
            }
            return name;
        }

        private double CaculateTime(DateTime? time1, DateTime time2)
        {
            DateTime temp = new DateTime();
            if (time1.HasValue)
            {
                temp = time1.Value;
            }
            else
            {
                temp = new DateTime(2000, 1, 1, 0, 0, 0);
            }
            TimeSpan time = new TimeSpan();
            time = time2 - temp;
            return time.TotalMinutes;
        }

        private string GetDeviceStatus(IC_Device pDevice, IMemoryCache pCache, Dictionary<string, List<IC_SystemCommand>> pProcessingDevice)
        {
            if (CaculateTime(pDevice.LastConnection, DateTime.Now) < ConfigObject.GetConfig(pCache).LimitedTimeConnection)
            {
                if (pProcessingDevice.ContainsKey(pDevice.SerialNumber))
                {
                    return "Đang xử lý";
                }
                else
                {
                    return "Online";
                }
            }
            else
            {
                return "Offline";
            }
        }

       

        // Chỉ lấy ra command Name cho device đang xử lý
        private List<string> GetRunningCommand(IC_Device pDevice, IMemoryCache pCache, Dictionary<string, List<IC_SystemCommand>> pProcessingDevice)
        {
            List<IC_SystemCommand> listSystemCommand = new List<IC_SystemCommand>();
            pProcessingDevice.TryGetValue(pDevice.SerialNumber, out listSystemCommand);
            if (listSystemCommand != null)
            {
                var deviceStatus = GetDeviceStatus(pDevice, pCache, pProcessingDevice);
                if (deviceStatus == "Đang xử lý")
                {
                    if (pProcessingDevice.ContainsKey(pDevice.SerialNumber))
                    {
                        // group by command name and convert to string
                        return listSystemCommand.GroupBy(u => new { u.CommandName }).Select(u => u.Key.CommandName).ToList();
                    }
                }

                return null;
            }
            else
            {
                return null;
            }

        }

        public async Task<IEnumerable<ComboboxItem>> GetDeviceAll(UserInfo pUser)
        {
            var devPrivileges = new List<string>();
            if (pUser.IsAdmin)
            {
                devPrivileges = DbContext.IC_Device.Where(x => x.CompanyIndex == pUser.CompanyIndex).Select(x => x.SerialNumber).AsEnumerable().ToList();
            }
            else
            {
                devPrivileges = DbContext.IC_PrivilegeDeviceDetails.Where(x => x.PrivilegeIndex == pUser.PrivilegeIndex && x.Role != "None" && x.Role != "ReadOnly").Select(x => x.SerialNumber).AsEnumerable().ToList();
            }
            var result = from dev in devPrivileges
                         join d in DbContext.IC_Device.Where(x => x.CompanyIndex == pUser.CompanyIndex)
                         on dev equals d.SerialNumber
                         select new ComboboxItem { label = d.AliasName == null ? d.SerialNumber : d.AliasName, value = d.SerialNumber };
            return await Task.FromResult(result);
        }

        public async Task<IEnumerable<IC_Device>> GetAllWithPrivilegeFull(UserInfo pUser)
        {
            var devPrivileges = new List<string>();
            if (pUser.IsAdmin)
            {
                devPrivileges = DbContext.IC_Device.Where(x => x.CompanyIndex == pUser.CompanyIndex).Select(x => x.SerialNumber).AsEnumerable().ToList();
            }
            else
            {
                devPrivileges = DbContext.IC_PrivilegeDeviceDetails.Where(x => x.PrivilegeIndex == pUser.PrivilegeIndex && x.Role != "None" && x.Role != "ReadOnly").Select(x => x.SerialNumber).AsEnumerable().ToList();
            }
                        var result = from dev in devPrivileges
                         join d in DbContext.IC_Device.Where(x => x.CompanyIndex == pUser.CompanyIndex).ToList()
                         on dev equals d.SerialNumber
                         select d;
            return await Task.FromResult(result);
        }

        public async Task<IC_Device> GetBySerialNumber(string pSerialNumber, int pCompanyIndex)
        {
            var dummy = await FirstOrDefaultAsync(x => x.SerialNumber == pSerialNumber && x.CompanyIndex == pCompanyIndex);
            return dummy;
        }

        public async Task<List<IC_Device>> GetBySerialNumbers(string[] pSerialNumber, int pCompanyIndex)
        {
            var serialLookup = pSerialNumber.ToHashSet();
            var dummy = Where(x => serialLookup.Contains(x.SerialNumber) && x.CompanyIndex == pCompanyIndex).ToList();
            return await Task.FromResult(dummy);
        }

        public async Task<IC_Device> GetCanEditableBySerialNumber(string pSerialNumber, UserInfo pUser)
        {
            var devPrivileges = new List<string>();
            if (pUser.IsAdmin)
            {
                devPrivileges = DbContext.IC_Device.Where(x => x.CompanyIndex == pUser.CompanyIndex && x.SerialNumber == pSerialNumber).Select(x => x.SerialNumber).ToList();
            }
            else
            {
                devPrivileges = DbContext.IC_PrivilegeDeviceDetails.Where(x => x.PrivilegeIndex == pUser.PrivilegeIndex && x.SerialNumber == pSerialNumber && (x.Role == "Full" || x.Role == "Edit")).Select(x => x.SerialNumber).AsEnumerable().ToList();
            } 
            var dummy = await FirstOrDefaultAsync(x => x.CompanyIndex == pUser.CompanyIndex && x.SerialNumber == pSerialNumber && devPrivileges.Contains(x.SerialNumber));
            return dummy;
        }

        public async Task<IC_Device> GetCanFullPrivilegeBySerialNumber(string pSerialNumber, UserInfo pUser)
        {
            var devPrivileges = new List<string>();
            if (pUser.IsAdmin)
            {
                devPrivileges = DbContext.IC_Device.Where(x => x.CompanyIndex == pUser.CompanyIndex && x.SerialNumber == pSerialNumber).Select(x => x.SerialNumber).ToList();
            }
            else
            {
                devPrivileges = DbContext.IC_PrivilegeDeviceDetails.Where(x => x.PrivilegeIndex == pUser.PrivilegeIndex && x.SerialNumber == pSerialNumber && x.Role == "Full" ).Select(x => x.SerialNumber).AsEnumerable().ToList();
            }

            var dummy = await FirstOrDefaultAsync(x => x.CompanyIndex == pUser.CompanyIndex && x.SerialNumber == pSerialNumber && devPrivileges.Contains(x.SerialNumber));
            return dummy;
        }

        public async Task<List<IC_Device>> GetListCanFullPrivilegeBySerialNumber(string[] pSerialNumbers, UserInfo pUser)
        {
            var serialLookup = pSerialNumbers.ToHashSet();
            var devPrivileges = new List<string>();
            if (pUser.IsAdmin)
            {
                devPrivileges = DbContext.IC_Device.Where(x => x.CompanyIndex == pUser.CompanyIndex && serialLookup.Contains(x.SerialNumber)).Select(x => x.SerialNumber).ToList();
            }
            else
            {
                devPrivileges = DbContext.IC_PrivilegeDeviceDetails.Where(x => x.PrivilegeIndex == pUser.PrivilegeIndex && serialLookup.Contains(x.SerialNumber) && x.Role == "Full").Select(x => x.SerialNumber).AsEnumerable().ToList();
            }
           
            var dummy = Where(x => x.CompanyIndex == pUser.CompanyIndex && devPrivileges.Contains(x.SerialNumber)).ToList();
            return await Task.FromResult(dummy);
        }

        public async Task<string> TryDeleteDevices(string[] pSerialNumbers, UserInfo pUser)
        {
            var serialLookup = pSerialNumbers.ToHashSet();
            var deleteFlag = new List<IC_PrivilegeDeviceDetails>();

            var allDeviceHavePrivilege = await GetListCanFullPrivilegeBySerialNumber(pSerialNumbers, pUser);
            var devPrivilegeLookup = allDeviceHavePrivilege.ToDictionarySafe(x => x.SerialNumber);

            if (allDeviceHavePrivilege.Select(x => x.SerialNumber).ToHashSet().Count != serialLookup.Count)
                return "MSG_NotPrivilege";

            var allDeleteDev = await GetBySerialNumbers(pSerialNumbers, pUser.CompanyIndex);

            var existsDevDept = DbContext.IC_DepartmentAndDevice.Any(x => x.CompanyIndex == pUser.CompanyIndex && serialLookup.Contains(x.SerialNumber));
            if (existsDevDept) return "DeviceExistDepartment";

            var existsDevServ = DbContext.IC_ServiceAndDevices.Any(x => x.CompanyIndex == pUser.CompanyIndex && serialLookup.Contains(x.SerialNumber));
            if (existsDevServ) return "DeviceExistService";

            var deviceExistsDepartment = DbContext.IC_DepartmentAndDevice.Where(x => x.CompanyIndex == pUser.CompanyIndex && serialLookup.Contains(x.SerialNumber));
            var deviceDeptLookup = deviceExistsDepartment.ToDictionarySafe(x => x.SerialNumber);

            DbContext.IC_Device.RemoveRange(allDeleteDev);


            var allPrivilege = DbContext.IC_PrivilegeDeviceDetails.Where(x => x.PrivilegeIndex == pUser.PrivilegeIndex && x.CompanyIndex == pUser.CompanyIndex && serialLookup.Contains(x.SerialNumber));
            DbContext.IC_PrivilegeDeviceDetails.RemoveRange(allPrivilege);

            await DbContext.SaveChangesAsync();

            return "";
        }

        public async Task UpdateDataDevice(DeviceParamInfo param, int pCompanyIndex)
        {
            var device = await GetBySerialNumber(param.SerialNumber, pCompanyIndex);

            if (device != null)
            {
                device.UserCount = param.UserCount;
                device.FingerCount = param.FingerCount;
                device.AttendanceLogCount = param.AttendanceLogCount;
                device.OperationLogCount = param.OperationLogCount;
                device.LastConnection = DateTime.Now;
                device.FaceCount = param.FaceCount;
                device.AdminCount = param.AdminCount;
                device.AttendanceLogCapacity = param.AttendanceLogCapacity;
                device.UserCapacity =  param.UserCapacity;
                device.FingerCapacity = param.FingerCapacity;
                device.FaceCapacity = param.FaceCapacity;
                device.Note = param.Note;
                DbContext.Update(device);
            }

            await DbContext.SaveChangesAsync();
        }

        public async Task UpdateTransactionCount(DeviceParamInfo param, int pCompanyIndex)
        {
            var device = await GetBySerialNumber(param.SerialNumber, pCompanyIndex);

            if (device != null)
            {
                device.AttendanceLogCount = param.AttendanceLogCount;
                DbContext.Update(device);
            }

            await DbContext.SaveChangesAsync();
        }
    }
}
