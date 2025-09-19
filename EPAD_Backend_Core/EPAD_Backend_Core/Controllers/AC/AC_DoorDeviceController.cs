using EPAD_Backend_Core.WebUtilitys;
using EPAD_Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using EPAD_Backend_Core.Base;
using System;
using EPAD_Data.Models;
using Microsoft.Extensions.Caching.Memory;
using EPAD_Data;

namespace EPAD_Backend_Core.Controllers
{

    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("api/AC_DoorDevice/[action]")]
    [ApiController]
    public class AC_DoorDeviceController : ApiControllerBase
    {
        private IMemoryCache cache;
        private EPAD_Context context;
        public AC_DoorDeviceController(IServiceProvider pProvider) : base(pProvider)
        {
            cache = TryResolve<IMemoryCache>();
            context = TryResolve<EPAD_Context>();
        }

        [Authorize]
        [ActionName("GetDeviceInOutDoor")]
        [HttpGet]
        public IActionResult GetDeviceInOutDoor(int doorIndex)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            var doorDevice = context.AC_DoorAndDevice.Where(x => x.CompanyIndex == user.CompanyIndex).ToList();
            var notInDoor = doorDevice.Where(x => x.DoorIndex != doorIndex).Select(x => x.SerialNumber).ToList();
            var inDoor = doorDevice.Where(x => x.DoorIndex == doorIndex).Select(x => x.SerialNumber).ToList();

            var listDevice = context.IC_Device.Where(x => x.CompanyIndex == user.CompanyIndex && !notInDoor.Contains(x.SerialNumber));

            var listDeviceRe = listDevice.Select(x => new DeviceByDoor
            {
                SerialNumber = x.SerialNumber,
                DeviceName = x.AliasName,
                InDoor = inDoor.Contains(x.SerialNumber),
                IPAddress = x.IPAddress
            });
            result = Ok(listDeviceRe);
            return result;
        }


        [Authorize]
        [ActionName("UpdateDoorDeviceDetail")]
        [HttpPost]
        public IActionResult UpdateDoorDeviceDetail([FromBody] GroupDoorParam param)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            context.AC_DoorAndDevice.RemoveRange(context.AC_DoorAndDevice.Where(t => t.CompanyIndex == user.CompanyIndex && t.DoorIndex == param.DoorIndex));
            //insert dữ liệu mới
            DateTime now = DateTime.Now;
            for (int i = 0; i < param.ListDevice.Count; i++)
            {
                var dataInsert = new AC_DoorAndDevice();
                dataInsert.DoorIndex = param.DoorIndex;
                dataInsert.SerialNumber = param.ListDevice[i];
                dataInsert.CompanyIndex = user.CompanyIndex;
                dataInsert.UpdatedDate = now;
                dataInsert.UpdatedUser = user.UserName;

                context.AC_DoorAndDevice.Add(dataInsert);
            }

            context.SaveChanges();
            result = Ok();
            return result;
        }

        public class DeviceByDoor
        {
            public string SerialNumber { get; set; }
            public string DeviceName { get; set; }
            public string IPAddress { get; set; }
            public bool InDoor { get; set; }
        }

        public class GroupDoorParam
        {
            public int DoorIndex { get; set; }
            public List<string> ListDevice { get; set; }
        }

    }
}
