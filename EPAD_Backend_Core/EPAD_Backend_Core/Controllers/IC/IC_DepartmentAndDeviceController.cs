using EPAD_Backend_Core.Base;
using EPAD_Backend_Core.WebUtilitys;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EPAD_Backend_Core.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("api/DepartmentAndDevice/[action]")]
    [ApiController]
    public class IC_DepartmentAndDeviceController : ApiControllerBase
    {
        private EPAD_Context context;
        private ezHR_Context otherContext;
        private IMemoryCache cache;
        public IC_DepartmentAndDeviceController(IServiceProvider provider) : base(provider)
        {
            context = TryResolve<EPAD_Context>();
            cache = TryResolve<IMemoryCache>();
            otherContext = TryResolve<ezHR_Context>();
        }
        [Authorize]
        [ActionName("GetDepartmentAndDeviceAtPage")]
        [HttpGet]
        public IActionResult GetDepartmentAndDeviceAtPage(int page)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            List<IC_Department> departmentList = null;
            if (page <= 1)
            {
                departmentList = context.IC_Department.Where(t => t.CompanyIndex == user.CompanyIndex).Take(GlobalParams.ROWS_NUMBER_IN_PAGE).ToList();
            }
            else
            {
                int fromRow = GlobalParams.ROWS_NUMBER_IN_PAGE * (page - 1);
                departmentList = context.IC_Department.Where(t => t.CompanyIndex == user.CompanyIndex).OrderBy(t => t.Name)
                    .Skip(fromRow).Take(GlobalParams.ROWS_NUMBER_IN_PAGE).ToList();
            }

            List<IC_DepartmentAndDevice> departmentAndDeviceList = new List<IC_DepartmentAndDevice>();
            departmentAndDeviceList = context.IC_DepartmentAndDevice.Where(t => t.CompanyIndex == user.CompanyIndex).ToList();
            List<IC_Device> deviceList = new List<IC_Device>();
            deviceList = context.IC_Device.Where(t => t.CompanyIndex == user.CompanyIndex).ToList();

            List<DepartmentAndDeviceResult> listData = new List<DepartmentAndDeviceResult>();
            for (int i = 0; i < departmentList.Count; i++)
            {
                DepartmentAndDeviceResult data = new DepartmentAndDeviceResult();
                data.Department = departmentList[i];
                List<IC_DepartmentAndDevice> listDeviceInDepartment = departmentAndDeviceList.Where(t => t.DepartmentIndex == departmentList[i].Index).ToList();
                List<string> listSerial = new List<string>();
                for (int j = 0; j < listDeviceInDepartment.Count; j++)
                {
                    listSerial.Add(listDeviceInDepartment[j].SerialNumber);
                }

                data.ListDevice = deviceList.Where(t => listSerial.Contains(t.SerialNumber)).ToList();

                listData.Add(data);
            }
            result = Ok(listData);
            return result;
        }

        [Authorize]
        [ActionName("UpdateDepartmentAndDevice")]
        [HttpPost]
        public IActionResult UpdateDepartmentAndDevice([FromBody]DepartmentAndDeviceParam param)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }


            ConfigObject config = ConfigObject.GetConfig(cache);

            //if (param.ListDeviceSerial == null || param.ListDeviceSerial.Count < 1)
            //{
            //    return BadRequest("DeviceNotExist");
            //}
            if (param.ListDeviceSerial != null)
            {
                foreach (var item in param.ListDeviceSerial)
                {

                    //Phân quyền
                    IC_Device CheckPriDevice = (from d in context.IC_Device
                                                join pridv in context.IC_PrivilegeDeviceDetails on d.SerialNumber equals pridv.SerialNumber
                                                join pri in context.IC_UserPrivilege on pridv.PrivilegeIndex equals pri.Index
                                                join ac in context.IC_UserAccount on pri.Index equals ac.AccountPrivilege
                                                where ( ( ac.UserName.Equals(user.UserName) && (!pridv.Role.Equals(Privilege.Full.ToString())) && (!pridv.Role.Equals(Privilege.Edit.ToString())))) && (d.SerialNumber.Equals(item))
                                                select d).Cast<IC_Device>().FirstOrDefault();

                    if (CheckPriDevice != null && !user.IsAdmin)
                    {
                        return BadRequest("MSG_NotPrivilege");
                    }
                }
            }
            if (config.IntegrateDBOther == false)
            {
                IC_Department department = context.IC_Department.Where(t => t.CompanyIndex == user.CompanyIndex && t.Index == param.DepartmentIndex).FirstOrDefault();
                if (department == null)
                {
                    return Conflict("DepartmentNotExists");
                }
            }
            else
            {
                HR_Department department = otherContext.HR_Department.Where(t => t.CompanyIndex == config.CompanyIndex && t.Index == param.DepartmentIndex).FirstOrDefault();
                if (department == null)
                {
                    return Conflict("DepartmentNotExists");
                }
            }

            List<IC_Device> listDevice = context.IC_Device.Where(t => t.CompanyIndex == user.CompanyIndex).ToList();
            bool checkDeviceNotExist = false;
            for (int i = 0; i < param.ListDeviceSerial.Count; i++)
            {
                IC_Device dataExist = listDevice.Where(t => t.SerialNumber == param.ListDeviceSerial[i]).FirstOrDefault();
                if (dataExist == null)
                {
                    checkDeviceNotExist = true;
                    break;
                }
            }
            if (checkDeviceNotExist == true)
            {
                return BadRequest("DeviceNotExist");
            }
            //xóa dữ liệu theo phòng ban
            context.IC_DepartmentAndDevice.RemoveRange(context.IC_DepartmentAndDevice.Where(t => t.CompanyIndex == user.CompanyIndex && t.DepartmentIndex == param.DepartmentIndex));
            //insert dữ liệu mới
            DateTime now = DateTime.Now;
            for (int i = 0; i < param.ListDeviceSerial.Count; i++)
            {
                IC_DepartmentAndDevice dataInsert = new IC_DepartmentAndDevice();
                dataInsert.DepartmentIndex = param.DepartmentIndex;
                dataInsert.SerialNumber = param.ListDeviceSerial[i];
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
        [ActionName("GetDepartmentAndDeviceLookup")]
        [HttpGet]
        public IActionResult GetDepartmentAndDeviceLookup()
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }
            ConfigObject config = ConfigObject.GetConfig(cache);

            List<IC_DepartmentDTO> departmentList = new List<IC_DepartmentDTO>();
            if (config.IntegrateDBOther)
            {
                departmentList = otherContext.HR_Department.Where(x => x.CompanyIndex == config.CompanyIndex)
                    .Select(e => new IC_DepartmentDTO()
                    {
                        Index = e.Index,
                        Code = e.Code,
                        Name = e.Name,
                        ParentIndex = e.ParentIndex
                    }).ToList();
            }
            else
            {
                departmentList = context.IC_Department.Where(x => x.CompanyIndex == user.CompanyIndex)
                    .Select(e => new IC_DepartmentDTO()
                    {
                        Index = e.Index,
                        Name = e.Name,
                        Code = e.Code,
                        ParentIndex = e.ParentIndex
                    }).ToList();
            }


            List<IC_DepartmentAndDevice> departmentAndDeviceList = context.IC_DepartmentAndDevice.Where(t => t.CompanyIndex == user.CompanyIndex).ToList();
            List<IC_Device> deviceList = context.IC_Device.Where(t => t.CompanyIndex == user.CompanyIndex).ToList();

            List<DepartmentAndDeviceLookup> listData = new List<DepartmentAndDeviceLookup>();
            for (int i = 0; i < departmentList.Count; i++)
            {
                DepartmentAndDeviceLookup data = new DepartmentAndDeviceLookup();
                data.DepartmentIndex = departmentList[i].Index;
                List<IC_DepartmentAndDevice> listDeviceInDepartment = departmentAndDeviceList.Where(t => t.DepartmentIndex == departmentList[i].Index).ToList();
                List<string> listSerial = new List<string>();
                for (int j = 0; j < listDeviceInDepartment.Count; j++)
                {
                    listSerial.Add(listDeviceInDepartment[j].SerialNumber);
                }

                data.ListDevices = deviceList.Where(t => listSerial.Contains(t.SerialNumber)).ToList();

                listData.Add(data);
            }
            result = Ok(listData);
            return result;

        }

        public class DepartmentAndDeviceResult
        {
            public IC_Department Department { get; set; }
            public List<IC_Device> ListDevice { get; set; }
        }

        public class DepartmentAndDeviceParam
        {
            public int DepartmentIndex { get; set; }
            public List<string> ListDeviceSerial { get; set; }
        }

        public class DepartmentAndDeviceLookup
        {
            public long DepartmentIndex { get; set; }
            public List<IC_Device> ListDevices { get; set; }
        }
    }
}
