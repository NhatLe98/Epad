using EPAD_Backend_Core.Base;
using EPAD_Backend_Core.WebUtilitys;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using EPAD_Services.Impl;
using EPAD_Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EPAD_Backend_Core.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("api/AC_Door/[action]")]
    [ApiController]
    public class AC_DoorController : ApiControllerBase
    {
        private readonly IAC_DoorService _IAC_DoorService;
        private IMemoryCache cache;
        private EPAD_Context context;
        public AC_DoorController(IServiceProvider pProvider) : base(pProvider)
        {
            context = TryResolve<EPAD_Context>();
            _IAC_DoorService = TryResolve<IAC_DoorService>();
            cache = TryResolve<IMemoryCache>();
        }

        [Authorize]
        [ActionName("GetAllDoor")]
        [HttpGet]
        public IActionResult GetAllDoor()
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            IEnumerable<object> dep;

            List<AC_Door> doorList = null;
            doorList = context.AC_Door.Where(t => t.CompanyIndex == user.CompanyIndex).ToList();
            dep = from door in doorList
                  join areaDoor in context.AC_AreaAndDoor.Where(t => t.CompanyIndex == user.CompanyIndex)
                  on door.Index equals areaDoor.DoorIndex into pareadoor
                  from ci in pareadoor.DefaultIfEmpty()
                  orderby door.Name
                  select new
                  {
                      value = door.Index,
                      label = door.Name,
                      areaId = ci != null ? ci.AreaIndex : 0
                  };
            result = Ok(dep);

            return result;
        }

        [Authorize]
        [ActionName("GetDoorAtPage")]
        [HttpGet]
        public IActionResult GetDoorAtPage(int page, string filter, int limit)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }
            var result = _IAC_DoorService.GetDataGrid(user.CompanyIndex, page, limit, filter);
            return ApiOk(result);
        }

        [Authorize]
        [ActionName("AddDoor")]
        [HttpPost]
        public IActionResult AddDoor([FromBody] AC_DoorDTO param)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            var checkName = context.AC_Door.Where(t => t.Name.Equals(param.Name)).FirstOrDefault();

            if (checkName != null)
            {
                return BadRequest("ExistDoorName");
            }

            var door = new AC_Door();
            door.Name = param.Name;
            door.DoorOpenTimezoneUID = param.DoorOpenTimezoneUID;
            door.UpdatedDate = DateTime.Now;
            door.CompanyIndex = user.CompanyIndex;
            door.DoorSettingDescription = param.Description;
            //door.Timezone = param.Timezone;

            context.AC_Door.Add(door);
            context.SaveChanges();
            if (param.AreaIndex != 0)
            {
                var areaDoor = new AC_AreaAndDoor();
                areaDoor.DoorIndex = door.Index;
                areaDoor.AreaIndex = param.AreaIndex;
                areaDoor.CompanyIndex = user.CompanyIndex;
                areaDoor.UpdatedUser = user.UserName;
                areaDoor.UpdatedDate = DateTime.Now;
                context.AC_AreaAndDoor.Add(areaDoor);
            }
            if (param.SerialNumberLst != null && param.SerialNumberLst.Count > 0)
            {
                foreach (var item in param.SerialNumberLst)
                {
                    var device = new AC_DoorAndDevice();
                    device.DoorIndex = door.Index;
                    device.SerialNumber = item;
                    device.CompanyIndex = user.CompanyIndex;
                    device.UpdatedUser = user.UserName;
                    device.UpdatedDate = DateTime.Now;
                    context.AC_DoorAndDevice.Add(device);
                }
            }
            context.SaveChanges();

            return Ok(door);
        }

        [Authorize]
        [ActionName("UpdateDoor")]
        [HttpPost]
        public IActionResult UpdateDoor([FromBody] AC_DoorDTO param)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }
            var updateData = context.AC_Door.Where(t => t.Index == param.Index).FirstOrDefault();
            var checkName = context.AC_Door.Where(t => t.Name.Equals(param.Name)).FirstOrDefault();

            if (checkName != null && checkName.Index != updateData.Index)
            {
                return BadRequest("ExistAreaName");
            }

            var door = context.AC_Door.Where(t => t.CompanyIndex == user.CompanyIndex && t.Index == param.Index).FirstOrDefault();
            door.Name = param.Name;
            door.DoorOpenTimezoneUID = param.DoorOpenTimezoneUID;
            door.UpdatedDate = DateTime.Now;
            door.CompanyIndex = user.CompanyIndex;
            door.DoorSettingDescription = param.Description;
            //door.Timezone = param.Timezone;

            var areaDoor = context.AC_AreaAndDoor.FirstOrDefault(x => x.DoorIndex == param.Index);

            if (param.AreaIndex != 0)
            {
                if (areaDoor == null)
                {
                    areaDoor = new AC_AreaAndDoor();
                    areaDoor.DoorIndex = door.Index;
                    areaDoor.AreaIndex = param.AreaIndex;
                    areaDoor.CompanyIndex = user.CompanyIndex;
                    areaDoor.UpdatedUser = user.UserName;
                    areaDoor.UpdatedDate = DateTime.Now;
                    context.AC_AreaAndDoor.Add(areaDoor);
                }
                else
                {
                    areaDoor.AreaIndex = param.AreaIndex;
                    areaDoor.UpdatedUser = user.UserName;
                    context.AC_AreaAndDoor.Update(areaDoor);
                }
            }
            else
            {
                if (areaDoor != null)
                {
                    context.AC_AreaAndDoor.Remove(areaDoor);
                }

            }
            var serialNumberLst = context.AC_DoorAndDevice.Where(x => x.DoorIndex == door.Index).ToList();
            if (serialNumberLst != null && serialNumberLst.Count > 0)
            {
                context.AC_DoorAndDevice.RemoveRange(serialNumberLst);
            }
            if (param.SerialNumberLst != null && param.SerialNumberLst.Count > 0)
            {
                foreach (var item in param.SerialNumberLst)
                {
                    var device = new AC_DoorAndDevice();
                    device.DoorIndex = door.Index;
                    device.SerialNumber = item;
                    device.CompanyIndex = user.CompanyIndex;
                    device.UpdatedUser = user.UserName;
                    device.UpdatedDate = DateTime.Now;
                    context.AC_DoorAndDevice.Add(device);
                }
            }

            context.SaveChanges();
            return Ok();
        }

        [Authorize]
        [ActionName("DeleteDoor")]
        [HttpPost]
        public IActionResult DeleteGroupDevice([FromBody] List<AC_DoorDTO> lsparam)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }
            foreach (var param in lsparam)
            {
                var deleteData = context.AC_Door.Where(t => t.CompanyIndex == user.CompanyIndex && t.Index == param.Index).FirstOrDefault();

                var checkServiceHasDevice = context.AC_DoorAndDevice.Where(t => t.CompanyIndex == user.CompanyIndex && t.DoorIndex == param.Index).FirstOrDefault();

                if (deleteData == null)
                {
                    return NotFound("DoorNotExist");
                }
                else
                {
                    context.AC_Door.Remove(deleteData);
                    var areaDoor = context.AC_AreaAndDoor.FirstOrDefault(x => x.DoorIndex == param.Index);
                    if (areaDoor != null)
                    {
                        context.AC_AreaAndDoor.Remove(areaDoor);
                    }

                    var doorDevice = context.AC_DoorAndDevice.Where(x => x.DoorIndex == param.Index);
                    if (doorDevice != null)
                    {
                        context.AC_DoorAndDevice.RemoveRange(doorDevice);
                    }
                }
            }
            context.SaveChanges();

            result = Ok();
            return result;
        }
        [Authorize]
        [ActionName("AddDoorSetting")]
        [HttpPost]
        public async Task<IActionResult> AddDoorSetting([FromBody] AC_DoorDTO param)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            if (param.DoorIndexes == null || (param.DoorIndexes != null && param.DoorIndexes.Count == 0))
            {
                return Conflict("PleaseInputDoor");
            }

            if (param.Timezone == 0)
            {
                return Conflict("PleaseInputTimeZone");
            }

            var existedDoor = await _IAC_DoorService.GetExistedDoorSetting(user, param.DoorIndexes);
            if (existedDoor != null && existedDoor.Count > 0 && existedDoor.Any(x => x.Timezone > 0))
            {
                return Ok(new Tuple<string, object>("DoorSettingIsExisted", existedDoor.Where(x => x.Timezone > 0).ToList()));
            }

            await _IAC_DoorService.UpdateDoorSettings(user, param);

            return Ok();
        }

        [Authorize]
        [ActionName("UpdateDoorSetting")]
        [HttpPost]
        public async Task<IActionResult> UpdateDoorSetting([FromBody] AC_DoorDTO param)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            var existedDoor = await _IAC_DoorService.GetByIndex(param.Index);
            if (param.Index <= 0 && existedDoor == null)
            {
                return Conflict("DoorSettingNotExist");
            }

            if (param.Timezone == 0)
            {
                return Conflict("PleaseInputTimeZone");
            }

            await _IAC_DoorService.UpdateDoorSetting(user, param);

            return Ok();
        }

        [Authorize]
        [ActionName("DeleteDoorSetting")]
        [HttpPost]
        public async Task<IActionResult> DeleteDoorSetting([FromBody] List<AC_DoorDTO> lsparam)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            var listIndex = lsparam.Select(x => x.Index).ToList();
            await _IAC_DoorService.DeleteDoorSettings(user, listIndex);

            result = Ok();
            return result;
        }
    }
}
