using EPAD_Backend_Core.Base;
using EPAD_Backend_Core.WebUtilitys;
using EPAD_Common.Types;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using EPAD_Logic;
using EPAD_Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using static EPAD_Backend_Core.Controllers.IC_ServiceController;

namespace EPAD_Backend_Core.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("api/GroupDevice/[action]")]
    [ApiController]
    public class IC_GroupDeviceController : ApiControllerBase
    {
        private EPAD_Context context;
        private IMemoryCache cache;
        private IIC_DeviceLogic _iIC_DeviceLogic;
        private IIC_GroupDeviceService _IC_GroupDeviceService;
        private IIC_GroupDeviceDetailLogic _IIC_GroupDeviceDetailLogic;
        public IC_GroupDeviceController(IServiceProvider provider) : base(provider)
        {
            context = TryResolve<EPAD_Context>();
            cache = TryResolve<IMemoryCache>();
            _iIC_DeviceLogic = TryResolve<IIC_DeviceLogic>();
            _IC_GroupDeviceService = TryResolve<IIC_GroupDeviceService>();
            _IIC_GroupDeviceDetailLogic = TryResolve<IIC_GroupDeviceDetailLogic>();
        }

        [Authorize]
        [ActionName("GetGroupDeviceAtPage")]
        [HttpGet]
        public IActionResult GetGroupDeviceAtPage(int page, string filter, int limit)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }
            DataGridClass dataGrid = null;
            int countPage = 0;
            IEnumerable<object> dep;

            List<IC_GroupDevice> employeeList = null;
            employeeList = context.IC_GroupDevice.Where(t => t.CompanyIndex == user.CompanyIndex
                && (string.IsNullOrWhiteSpace(filter) || t.Name.Contains(filter) || t.Description.Contains(filter))).ToList();
            dep = from groupdevice in employeeList
                  orderby groupdevice.Name
                  select new
                  {
                      value = groupdevice.Index.ToString(),
                      label = groupdevice.Name
                  };
            countPage = employeeList.Count();
            dataGrid = new DataGridClass(countPage, employeeList);
            if (page <= 1)
            {
                var lsDevice = employeeList.OrderBy(t => t.Name).Take(limit).ToList();
                dataGrid = new DataGridClass(countPage, lsDevice);
            }
            else
            {
                int fromRow = limit * (page - 1);
                var lsDevice = employeeList.OrderBy(t => t.Name).Skip(fromRow).Take(limit).ToList();
                dataGrid = new DataGridClass(countPage, lsDevice);
            }
            //dataGrid = new DataGridClass(record, employeeList);
            result = Ok(dataGrid);
            return result;

        }

        [Authorize]
        [ActionName("GetGroupDevice")]
        [HttpGet]
        public IActionResult GetGroupDevice()
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            IEnumerable<object> dep;

            List<IC_GroupDevice> employeeList = null;
            employeeList = context.IC_GroupDevice.Where(t => t.CompanyIndex == user.CompanyIndex).ToList();
            dep = from groupdevice in employeeList
                  orderby groupdevice.Name
                  select new
                  {
                      value = groupdevice.Index.ToString(),
                      label = groupdevice.Name
                  };
            result = Ok(dep);

            return result;
        }

        [Authorize]
        [ActionName("AddGroupDevice")]
        [HttpPost]
        public IActionResult AddGroupDevice([FromBody]GroupDevice param)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            IC_GroupDevice checkName = context.IC_GroupDevice.Where(t => t.Name.Equals(param.Name)).FirstOrDefault();

            if (checkName != null)
            {
                return BadRequest("ExistNameGroupDevice");
            }

            IC_GroupDevice groupDevice = new IC_GroupDevice();
            groupDevice.Name = param.Name;
            groupDevice.Description = param.Description;
            groupDevice.CreatedDate = DateTime.Now;
            groupDevice.UpdatedDate = DateTime.Now;
            groupDevice.CompanyIndex = user.CompanyIndex;

            context.IC_GroupDevice.Add(groupDevice);
            context.SaveChanges();
            return Ok();
        }

        [Authorize]
        [ActionName("UpdateGroupDevice")]
        [HttpPost]
        public IActionResult UpdateGroupDevice([FromBody]GroupDevice param)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }
            IC_GroupDevice updateData = context.IC_GroupDevice.Where(t => t.Index == param.Index).FirstOrDefault();
            IC_GroupDevice checkName = context.IC_GroupDevice.Where(t => t.Name.Equals(param.Name)).FirstOrDefault();

            if (checkName != null && checkName.Index != updateData.Index)
            {
                return BadRequest("ExistNameGroupDevice");
            }

            IC_GroupDevice groupDevice = context.IC_GroupDevice.Where(t => t.CompanyIndex == user.CompanyIndex && t.Index == param.Index).FirstOrDefault();
            groupDevice.Name = param.Name;
            groupDevice.Description = param.Description;
            groupDevice.CreatedDate = DateTime.Now;
            groupDevice.UpdatedDate = DateTime.Now;
            groupDevice.CompanyIndex = user.CompanyIndex;

            context.SaveChanges();
            return Ok();
        }

        [Authorize]
        [ActionName("GetDevicesInOutGroupDevice")]
        [HttpGet]
        public IActionResult GetDevicesInOutGroupDevice(int groupIndex)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            List<IC_Device> listDevice = new List<IC_Device>();
            if (user.IsAdmin)
            {
                listDevice = context.IC_Device.Where(t => t.CompanyIndex == user.CompanyIndex).ToList();
            }
            else
            {


                listDevice = (from d in context.IC_Device
                              join pridv in context.IC_PrivilegeDeviceDetails on d.SerialNumber equals pridv.SerialNumber
                              join pri in context.IC_UserPrivilege on pridv.PrivilegeIndex equals pri.Index
                              join ac in context.IC_UserAccount on pri.Index equals ac.AccountPrivilege
                              where (ac.UserName.Equals(user.UserName) && (pridv.Role.Equals(Privilege.None.ToString()) == false))
                              select d).Cast<IC_Device>().ToList();
            }

            List<DeviceByService> listDeviceBasic = new List<DeviceByService>();

            List<IC_GroupDeviceDetails> serviceDetails = context.IC_GroupDeviceDetails.Where(t => t.CompanyIndex == user.CompanyIndex
                  && t.GroupDeviceIndex == groupIndex).ToList();
            string[] arrSerial = new string[serviceDetails.Count];
            for (int j = 0; j < serviceDetails.Count; j++)
            {
                arrSerial[j] = serviceDetails[j].SerialNumber;
            }

            for (int i = 0; i < listDevice.Count; i++)
            {
                DeviceByService device = new DeviceByService();
                device.SerialNumber = listDevice[i].SerialNumber;
                device.AliasName = listDevice[i].AliasName;
                device.IPAddress = listDevice[i].IPAddress;
                device.Port = listDevice[i].Port.ToString();
                device.InService = false;
                if (arrSerial.Contains(device.SerialNumber))
                {
                    device.InService = true;
                }

                listDeviceBasic.Add(device);
            }

            result = Ok(listDeviceBasic);
            return result;
        }

        [Authorize]
        [ActionName("GetGroupDeviceResult")]
        [HttpGet]
        public IActionResult GetGroupDeviceResult()
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            var result = _IC_GroupDeviceService.GetGroupDeviceResult(user);
            return ApiOk(result);
        }

        [Authorize]
        [ActionName("GetDeviceByGroup")]
        [HttpGet]
        public IActionResult GetDeviceByGroup( int groupIndex)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            List<AddedParam> addedParams1 = new List<AddedParam>();
            addedParams1.Add(new AddedParam { Key = "CompanyIndex", Value = user.CompanyIndex });
            addedParams1.Add(new AddedParam { Key = "UserName", Value = user.UserName });
            addedParams1.Add(new AddedParam { Key = "IsAdmin", Value = user.IsAdmin });
            List<IC_DeviceDTO> listDevice = _iIC_DeviceLogic.GetManyDeviceWithPrivilege(addedParams1);


            addedParams1 = new List<AddedParam>();
            addedParams1.Add(new AddedParam { Key = "CompanyIndex", Value = user.CompanyIndex });
            addedParams1.Add(new AddedParam { Key = "GroupDeviceIndex", Value = groupIndex });
            List<IC_GroupDeviceDetailDTO> serviceDetails = _IIC_GroupDeviceDetailLogic.GetMany(addedParams1);

            List<DeviceByService> listDeviceBasic = new List<DeviceByService>();
            
            for (int i = 0; i < listDevice.Count; i++)
            {
                DeviceByService device = new DeviceByService();
                device.SerialNumber = listDevice[i].SerialNumber;
                device.AliasName = listDevice[i].AliasName;
                device.IPAddress = listDevice[i].IPAddress;
                device.Port = listDevice[i].Port.ToString();
                device.InService = false;
                if (groupIndex == 0 )
                {
                    listDeviceBasic.Add(device);
                }
                else if(serviceDetails.Where(u => u.SerialNumber == device.SerialNumber).Count() > 0) 
                {
                    device.InService = true;
                    listDeviceBasic.Add(device);
                }
            }

            result = Ok(listDeviceBasic);
            return result;
        }

        [Authorize]
        [ActionName("UpdateGroupDeviceDetail")]
        [HttpPost]
        public IActionResult UpdateGroupDeviceDetail([FromBody]GroupDeviceParam param)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            if (param.ListDeviceSerial != null)
            {
                foreach (var item in param.ListDeviceSerial)
                {

                    //Phân quyền
                    IC_Device CheckPriDevice = (from d in context.IC_Device
                                                join pridv in context.IC_PrivilegeDeviceDetails on d.SerialNumber equals pridv.SerialNumber
                                                join pri in context.IC_UserPrivilege on pridv.PrivilegeIndex equals pri.Index
                                                join ac in context.IC_UserAccount on pri.Index equals ac.AccountPrivilege
                                                where (user.IsAdmin || (ac.UserName.Equals(user.UserName) && (!pridv.Role.Equals(Privilege.Full.ToString())) && (!pridv.Role.Equals(Privilege.Edit.ToString())))) && (d.SerialNumber.Equals(item))
                                                select d).Cast<IC_Device>().FirstOrDefault();

                    if (CheckPriDevice != null && !user.IsAdmin)
                    {
                        return BadRequest("MSG_NotPrivilege");
                    }
                }
            }
            context.IC_GroupDeviceDetails.RemoveRange(context.IC_GroupDeviceDetails.Where(t => t.CompanyIndex == user.CompanyIndex && t.GroupDeviceIndex == param.GroupDeviceIndex));
            //insert dữ liệu mới
            DateTime now = DateTime.Now;
            for (int i = 0; i < param.ListDeviceSerial.Count; i++)
            {
                IC_GroupDeviceDetails dataInsert = new IC_GroupDeviceDetails();
                dataInsert.GroupDeviceIndex = param.GroupDeviceIndex;
                dataInsert.SerialNumber = param.ListDeviceSerial[i];
                dataInsert.CompanyIndex = user.CompanyIndex;
                dataInsert.UpdatedDate = now;
                dataInsert.UpdatedUser = user.UserName;

                context.IC_GroupDeviceDetails.Add(dataInsert);
            }

            context.SaveChanges();
            result = Ok();
            return result;
        }

        [Authorize]
        [ActionName("DeleteGroupDevice")]
        [HttpPost]
        public IActionResult DeleteGroupDevice([FromBody]List<GroupDevice> lsparam)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }
            foreach (var param in lsparam)
            {
                IC_GroupDevice deleteData = context.IC_GroupDevice.Where(t => t.CompanyIndex == user.CompanyIndex && t.Index == param.Index).FirstOrDefault();

                IC_GroupDeviceDetails checkServiceHasDevice = context.IC_GroupDeviceDetails.Where(t => t.CompanyIndex == user.CompanyIndex && t.GroupDeviceIndex == param.Index).FirstOrDefault();

                if (deleteData == null)
                {
                    return NotFound("GroupDeviceNotExist");
                }
                else if (checkServiceHasDevice != null)
                {
                    return NotFound("GroupDeviceHasDevice");
                }
                else
                {
                    context.IC_GroupDevice.Remove(deleteData);
                }
            }
            context.SaveChanges();

            result = Ok();
            return result;
        }

        public class GroupDeviceResult
        {
            public IC_GroupDevice GroupDevice { get; set; }
            public List<IC_GroupDeviceDetails> GroupDeviceDetails { get; set; }
        }

        public class GroupDeviceParam
        {
            public int GroupDeviceIndex { get; set; }
            public List<string> ListDeviceSerial { get; set; }
        }
        public class GroupDevice
        {
            public int Index { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
        }
    }
}