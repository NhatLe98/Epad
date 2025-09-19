using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EPAD_Data;
using EPAD_Data.Models;
using EPAD_Common.Types;
using EPAD_Data.Entities;
using EPAD_Common.Utility;
using EPAD_Common.Extensions;
using EPAD_Common;
using EPAD_Backend_Core.Base;
using EPAD_Services.Interface;
using EPAD_Data.Models.Other;
using Chilkat;
using EPAD_Backend_Core.WebUtilitys;

namespace EPAD_Backend_Core.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("api/Device/[action]")]
    [ApiController]
    public class IC_DeviceController : ApiControllerBase
    {
        private readonly RabbitMQHelper _rabbitMQ_Helper;
        private readonly IIC_DeviceService _IC_DeviceService;
        private readonly IIC_ServiceAndDevicesService _IC_ServiceAndDevicesService;
        private readonly IIC_GroupDeviceDetailsService _IC_GroupDeviceDetailsService;
        private readonly IIC_DepartmentAndDeviceService _IC_DepartmentAndDeviceService;
        private readonly IIC_PrivilegeDeviceDetailsService _IC_PrivilegeDeviceDetailsService;
        private EPAD_Context context;

        public IC_DeviceController(IServiceProvider provider) : base(provider)
        {
            _rabbitMQ_Helper = TryResolve<RabbitMQHelper>();
            _IC_DeviceService = TryResolve<IIC_DeviceService>();
            _IC_ServiceAndDevicesService = TryResolve<IIC_ServiceAndDevicesService>();
            _IC_GroupDeviceDetailsService = TryResolve<IIC_GroupDeviceDetailsService>();
            context = TryResolve<EPAD_Context>();
        }

        [Authorize]
        [ActionName("GetDeviceAtPage")]
        [HttpGet]
        public async Task<IActionResult> GetDeviceAtPageAsync(int page = 1, string filter = "", int limit = 50)
        {
            UserInfo user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            DataGridClass dataGrid = await _IC_DeviceService.GetDataGrid(filter, user, page, limit);
            return ApiOk(dataGrid);
        }

        [Authorize]
        [ActionName("GetDeviceAll")]
        [HttpGet]
        public async Task<IActionResult> GetDeviceAllAsync()
        {
            UserInfo user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            var dummy = await _IC_DeviceService.GetAllWithPrivilegeFull(user);
            var result = dummy.Select(dev => _Mapper.Map<IC_Device, ComboboxDeviceItem>(dev));

            return ApiOk(result);
        }

        [Authorize]
        [ActionName("GetAllDevice")]
        [HttpGet]
        public async Task<IActionResult> GetAllDevice()
        {
            UserInfo user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            var result = await _IC_DeviceService.GetAllWithPrivilegeFull(user);

            return ApiOk(result.ToList());
        }

        [Authorize]
        [ActionName("GetListDeviceInfo")]
        [HttpGet]
        public async Task<IActionResult> GetListDeviceInfo()
        {
            UserInfo user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            var dataGrid = await _IC_DeviceService.GetDeviceInfo(user);
            return ApiOk(dataGrid);
        }

        [Authorize]
        [ActionName("GetIPAddressBySerialNumbers")]
        [HttpGet]
        public async Task<IActionResult> GetIPAddressBySerialNumbersAsync(string serialNumbers)
        {
            UserInfo user = GetUserInfo();
            if (user == null) return ApiUnauthorized();
            int count = serialNumbers.Count(f => f == ',');
            var serialLookup = new HashSet<string>();
            if (count > 0)
            {
                serialLookup = serialNumbers.Split(",").ToHashSet();
            }
            else
            {
                serialLookup.Add(serialNumbers);
            }

            IEnumerable<IC_Device> result = await _IC_DeviceService.GetAllWithPrivilegeFull(user);

            List<string> res = result.Where(i => serialLookup.Contains(i.SerialNumber)).Select(i => i.IPAddress).ToList();
            return ApiOk(res);
        }


        [Authorize]
        [ActionName("GetDeviceByID")]
        [HttpGet]
        public async Task<IActionResult> GetDeviceByID(string serialNumbers)
        {
            UserInfo user = GetUserInfo();
            if (user == null) return ApiUnauthorized();
            IC_Device checkData = await _IC_DeviceService.GetBySerialNumber(serialNumbers, user.CompanyIndex);
            return ApiOk(checkData);
        }

        [Authorize]
        [ActionName("GetDeviceAllPrivilege")]
        [HttpGet]
        public async Task<IActionResult> GetDeviceAllPrivilegeAsync()
        {
            UserInfo user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            var devs = await _IC_DeviceService.GetAllAsync(x => x.CompanyIndex == user.CompanyIndex);
            var result = devs.Select(dev => _Mapper.Map<IC_Device, ComboboxItem>(dev));

            return ApiOk(result);
        }

        [Authorize]
        [ActionName("GetDeviceAuthenMode")]
        [HttpGet]
        public IActionResult GetDeviceAuthenMode()
        {
            UserInfo user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            List<object> authenMode = new List<object>();
            authenMode.Add(new { value = AuthenMode.Password.ToString(), label = "AuthenPassword" });
            authenMode.Add(new { value = AuthenMode.CardNumber.ToString(), label = "AuthenCardNumber" });
            authenMode.Add(new { value = AuthenMode.Finger.ToString(), label = "AuthenFinger" });
            authenMode.Add(new { value = AuthenMode.Face.ToString(), label = "AuthenFace" });
            return ApiOk(authenMode);
        }

        [Authorize]
        [ActionName("AddDevice")]
        [HttpPost]
        public async Task<IActionResult> AddDeviceAsync([FromBody] DeviceParam param)
        {
            UserInfo user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            param = (DeviceParam)StringHelper.RemoveWhiteSpace(param);
            if (param.SerialNumber == "") return ApiError("PleaseFillAllRequiredFields");
            IC_Device checkData = await _IC_DeviceService.GetBySerialNumber(param.SerialNumber, user.CompanyIndex);
            if (checkData != null) return ApiConflict("SerialNumberIsExists");
            var device = _Mapper.Map<IC_Device>(param);
            device.CompanyIndex = user.CompanyIndex;
            await _IC_DeviceService.CreateNewDeviceAsync(device, user.PrivilegeIndex, param.ServiceID, param.GroupDeviceID);

            //remove device cache
            _Cache.Remove("urn:Dictionary_IC_Device");
            await SaveChangeAsync();

            return ApiOk();
        }

        [Authorize]
        [ActionName("UpdateDevice")]
        [HttpPost]
        public async Task<IActionResult> UpdateDeviceAsync([FromBody] DeviceParam param)
        {
            UserInfo user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            param = (DeviceParam)StringHelper.RemoveWhiteSpace(param);
            if (param.SerialNumber == "") return ApiError("PleaseFillAllRequiredFields");
            IC_Device checkCanEdit = await _IC_DeviceService.GetCanEditableBySerialNumber(param.SerialNumber, user);
            if (checkCanEdit == null) return ApiError("MSG_NotPrivilege");
            IC_Device updateData = await _IC_DeviceService.GetBySerialNumber(param.SerialNumber, user.CompanyIndex);
            if (updateData == null) return Conflict("DeviceIsExists");
            //updateData = _Mapper.Map<IC_Device>(param);
            _Mapper.Map(param, updateData);
            _IC_DeviceService.Update(updateData);
            await SaveChangeAsync();

            if (param.ServiceID.HasValue)
            {
                var updateServiceAndDevices = await _IC_ServiceAndDevicesService.InsertOrUpdateServiceAndDevices(new List<string> { param.SerialNumber }, param.ServiceID.Value, user);
                if (!updateServiceAndDevices)
                {
                    return ApiError("UpdateServiceAndDevicesFalse");
                }
            }

            if (param.GroupDeviceID.HasValue)
            {
                var updateGroupDevicesDetail = await _IC_GroupDeviceDetailsService.InsertOrUpdateGroupDevicesDetail(new List<string> { param.SerialNumber }, param.GroupDeviceID.Value, user);
                if (!updateGroupDevicesDetail)
                {
                    return ApiError("UpdateGroupDevicesDetailFalse");
                }
            }

            // send device change message for service
            var serviceAndDevices = await _IC_ServiceAndDevicesService.GetAllBySerialNumber(param.SerialNumber, user.CompanyIndex);
            for (int i = 0; i < serviceAndDevices.Count; i++)
            {
                _rabbitMQ_Helper.CreateMessageInfo<string>(serviceAndDevices[i].ServiceIndex, "DeviceChanged", "Info");
            }
            _Cache.Remove("urn:Dictionary_IC_Device");

            return ApiOk();
        }

        [Authorize]
        [ActionName("DeleteDevice")]
        [HttpPost]
        public async Task<IActionResult> DeleteDeviceAsync([FromBody] List<DeviceParam> lsparam)
        {
            UserInfo user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            var rs = await _IC_DeviceService.TryDeleteDevices(lsparam.Select(x => x.SerialNumber).ToArray(), user);
            _Cache.Remove("urn:Dictionary_IC_Device");
            if (rs != "") return ApiError(rs);

            return ApiOk();
        }

        [Authorize]
        [ActionName("UpdateDataDevice")]
        [HttpPost]
        public async Task<IActionResult> UpdateDataDevice([FromBody] DeviceParamInfo param)
        {
            UserInfo user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            await _IC_DeviceService.UpdateDataDevice(param, user.CompanyIndex);
            _Cache.Remove("urn:Dictionary_IC_Device");

            return ApiOk();
        }


        [Authorize]
        [ActionName("UpdateCountTransaction")]
        [HttpPost]
        public async Task<IActionResult> UpdateCountTransaction([FromBody] DeviceParamInfo param)
        {
            UserInfo user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            await _IC_DeviceService.UpdateTransactionCount(param, user.CompanyIndex);

            return ApiOk();
        }


        [Authorize]
        [ActionName("UpdateCapacityInfo")]
        [HttpPost]
        public async Task<IActionResult> UpdateCapacityInfoAsync([FromBody] DeviceCapacityParam param)
        {
            UserInfo user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            IC_Device device = await _IC_DeviceService.GetBySerialNumber(param.SerialNumber, user.CompanyIndex);
            if (device != null)
            {
                //device = _Mapper.Map(param, device);
                //device = _Mapper.Map<IC_Device>(param);
                _Mapper.Map(param, device);
                _IC_DeviceService.Update(device);
            }
            await SaveChangeAsync();
            return ApiOk();
        }

        [Authorize]
        [ActionName("InsertOrUpdateDataDevice")]
        [HttpPost]
        public async Task<IActionResult> InsertOrUpdateDataDeviceAsync([FromBody] DeviceParamInfo param)
        {
            UserInfo user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            if (param != null)
            {
                var device = await _IC_DeviceService.GetBySerialNumber(param.SerialNumber, user.CompanyIndex);

                if (device != null)
                {
                    //device = _Mapper.Map<IC_Device>(param, opt => opt.ExcludeMembers("DeviceType", "FaceCount", "AdminCount", 
                    //"AttendanceLogCount", "FingerCount", "UserCount", "OperationLogCount","AliasName","IPAddress","Port"));
                    device.SerialNumber = param.SerialNumber;
                    device.CompanyIndex = user.CompanyIndex;
                    device.FirmwareVersion = param.FirmwareVersion;
                    device.Stamp = param.Stamp;
                    device.OpStamp = param.OpStamp;
                    device.PhotoStamp = param.PhotoStamp;
                    device.ErrorDelay = param.ErrorDelay;
                    device.Delay = param.Delay;
                    device.TransTimes = param.TransTimes;
                    device.TransInterval = param.TransInterval;
                    device.TransFlag = param.TransFlag;
                    device.Realtime = param.Realtime;
                    device.Encrypt = param.Encrypt;
                    device.TimeZoneclock = param.TimeZoneclock;
                    device.Reserve1 = param.Reserve1;
                    device.Reserve2 = param.Reserve2;
                    device.Reserve3 = param.Reserve3;
                    device.ATTLOGStamp = param.ATTLOGStamp;
                    device.OPERLOGStamp = param.OPERLOGStamp;
                    device.ATTPHOTOStamp = param.ATTPHOTOStamp;
                    device.SMSStamp = param.SMSStamp;
                    device.USER_SMSStamp = param.USER_SMSStamp;
                    device.USERINFOStamp = param.USERINFOStamp;
                    device.FINGERTMPStamp = param.FINGERTMPStamp;
                    device.DeviceNumber = param.DeviceNumber;
                    //device.DeviceType = param.DeviceType;
                    device.UseSDK = param.UseSDK == "true" ? true : false;
                    device.UsePush = param.UsePush == "true" ? true : false;
                    device.LastConnection = DateTime.Now;
                    device.UpdatedDate = DateTime.Now;
                    device.UpdatedUser = user.UserName;

                    _IC_DeviceService.Update(device);

                }
                else
                {
                    // device = _Mapper.Map<DeviceParamInfo, IC_Device>(param);
                    device = _Mapper.Map<DeviceParamInfo, IC_Device>(param);
                    await _IC_DeviceService.InsertAsync(device);
                }
                _IC_DeviceService.AddOrUpdate(device);
                await SaveChangeAsync();
                _Cache.Remove("urn:Dictionary_IC_Device");
            }
            return ApiOk();
        }

        [Authorize]
        [ActionName("GetDepartmentsBySerialNumber")]
        [HttpGet]
        public IActionResult GetDepartmentsBySerialNumber(string serialNumber)
        {
            UserInfo user = GetUserInfo();
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }
            var privilegeCheck = context.IC_Device.Where(t => t.SerialNumber == serialNumber).FirstOrDefault();
            if (privilegeCheck == null)
            {
                return BadRequest("DeviceNotExist");
            }
            var listPrivilegeDepartment = context.IC_DepartmentAndDevice.Where(t => t.CompanyIndex == user.CompanyIndex && t.SerialNumber == serialNumber).ToList();

            List<int> listData = listPrivilegeDepartment.Select(x => x.DepartmentIndex).ToList();
            return Ok(listData);
        }

        [Authorize]
        [ActionName("UpdateDeviceAndDepartments")]
        [HttpPost]
        public IActionResult UpdateDeviceAndDepartments([FromBody] DeviceAndDepartmentsParam param)
        {
            UserInfo user = GetUserInfo();
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            var privilegeCheck = context.IC_Device.Where(t => t.SerialNumber == param.SerialNumber).FirstOrDefault();
            if (privilegeCheck == null)
            {
                return BadRequest("DeviceNotExist");
            }
         
            //xóa dữ liệu theo phòng ban
            context.IC_DepartmentAndDevice.RemoveRange(context.IC_DepartmentAndDevice.Where(t => t.CompanyIndex == user.CompanyIndex && t.SerialNumber == param.SerialNumber));
            //insert dữ liệu mới
            DateTime now = DateTime.Now;
            for (int i = 0; i < param.ListDepartmentIndexs.Count; i++)
            {
                IC_DepartmentAndDevice dataInsert = new IC_DepartmentAndDevice();
                dataInsert.DepartmentIndex = param.ListDepartmentIndexs[i];
                dataInsert.SerialNumber = param.SerialNumber;
                dataInsert.CompanyIndex = user.CompanyIndex;
                dataInsert.UpdatedDate = now;
                dataInsert.UpdatedUser = user.UserName;

                context.IC_DepartmentAndDevice.Add(dataInsert);
            }

            context.SaveChanges();
            result = Ok();
            return result;
        }

        [Authorize]
        [ActionName("ImportDevice")]
        [HttpPost]
        public async Task<IActionResult> ImportDeviceAsync([FromBody] DeviceParam[] request)
        {
            UserInfo user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            var devices = await _IC_DeviceService.GetAllAsync();
            var devsLookup = devices.Select(x => x.SerialNumber).ToHashSet();

            foreach (var item in request)
            {
                var param = item;
                param = (DeviceParam)StringHelper.RemoveWhiteSpace(param);
                if (param.SerialNumber == "") return ApiError("PleaseFillAllRequiredFields");
                if (devsLookup.Contains(param.SerialNumber)) continue;
                devsLookup.Add(param.SerialNumber);
                IC_Device device = _Mapper.Map<DeviceParam, IC_Device>(param);
                await _IC_DeviceService.InsertAsync(device);
            }
            await SaveChangeAsync();
            _Cache.Remove("urn:Dictionary_IC_Device");
            return ApiOk();
        }

        [Authorize]
        [ActionName("GetProducerEnumList")]
        public List<EnumModel> GetProducerEnumList()
        {
            return ((ProducerEnum[])Enum.GetValues(typeof(ProducerEnum))).Select(c => new EnumModel() { Value = ((int)c).ToString(), Name = c.ToString() }).ToList();

        }

        public class DeviceAndDepartmentsParam
        {
            public string SerialNumber { get; set; }
            public List<int> ListDepartmentIndexs { get; set; }
        }
    }
}
