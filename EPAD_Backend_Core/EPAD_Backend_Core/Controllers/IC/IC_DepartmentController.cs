using ClosedXML.Excel;
using EPAD_Backend_Core.Base;
using EPAD_Backend_Core.WebUtilitys;
using EPAD_Common.Types;
using EPAD_Common.Utility;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using EPAD_Data.Models.IC;
using EPAD_Logic;
using EPAD_Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using static EPAD_Backend_Core.Controllers.IC_ServiceController;

namespace EPAD_Backend_Core.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("api/Department/[action]")]
    [ApiController]
    public class IC_DepartmentController : ApiControllerBase
    {
        private EPAD_Context context;
        private ezHR_Context otherContext;
        private IMemoryCache cache;
        private IIC_DepartmentLogic _iIC_DepartmentLogic;
        private readonly IIC_DepartmentService _IC_DepartmentService;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IIC_AuditLogic _iIC_AuditLogic;
        private readonly IConfiguration _configuration;
        private readonly string _configClientName;
        private readonly IHR_EmployeeLogic _IHR_EmployeeLogic;
        private List<long> Ids { get; set; }

        public IC_DepartmentController(IServiceProvider provider) : base(provider)
        {
            context = TryResolve<EPAD_Context>();
            cache = TryResolve<IMemoryCache>();
            otherContext = TryResolve<ezHR_Context>();
            _configuration = TryResolve<IConfiguration>();
            _iIC_DepartmentLogic = TryResolve<IIC_DepartmentLogic>();
            _IC_DepartmentService = TryResolve<IIC_DepartmentService>();
            _hostingEnvironment = TryResolve<IHostingEnvironment>();
            _iIC_AuditLogic = TryResolve<IIC_AuditLogic>();
            _configClientName = _configuration.GetValue<string>("ClientName").ToUpper();
            _IHR_EmployeeLogic = TryResolve<IHR_EmployeeLogic>();
        }

        [Authorize]
        [ActionName("GetDepartmentAtPage")]
        [HttpGet]
        public IActionResult GetDepartmentAtPage(int page, string filter, int limit)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }
            ConfigObject config = ConfigObject.GetConfig(cache);
            DataGridClass dataGrid = null;
            if (config.IntegrateDBOther == false)
            {
                int countPage = 0;

                var obj = from d in context.IC_Department
                          join c in context.IC_Department on d.ParentIndex equals c.Index
                          into gc
                          from c in gc.DefaultIfEmpty()
                          where d.CompanyIndex.Equals(user.CompanyIndex) && d.IsInactive != true &&
                                  string.IsNullOrEmpty(filter) ? d.Name.Contains("") :
                                     (d.Index.ToString().Contains(filter)
                                    || d.Name.Contains(filter)
                                    || d.Code.Contains(filter)
                                    || d.Description.Contains(filter)
                                    || c.Name.Contains(filter)
                                    || d.Location.Contains(filter))

                          select new
                          {
                              Index = d.Index,
                              Location = d.Location,
                              Name = d.Name,
                              Description = d.Description,
                              Code = d.Code,
                              ParentIndex = d.ParentIndex,
                              ParentName = c.Name,
                              d.IsContractorDepartment,
                              d.IsDriverDepartment,
                              IsContractorDepartmentName = d.IsContractorDepartment == true ? "Có" : "Không",
                              IsDriverDepartmentName = d.IsDriverDepartment == true ? "Có" : "Không"
                          };

                if (user.ListDepartmentAssigned != null && user.ListDepartmentAssigned.Count > 0)
                {
                    obj = obj.Where(x => user.ListDepartmentAssigned.Contains(x.Index));
                }

                if (page <= 1)
                {
                    //countPage = obj.Count();
                    //var lsdepartment = obj.Take(GlobalParams.ROWS_NUMBER_IN_PAGE).ToList();
                    var lsdepartment = obj.OrderBy(t => t.Name).Where(t => user.ListDepartmentAssigned.Contains(t.Index)).ToList();
                    countPage = lsdepartment.Count;
                    lsdepartment = lsdepartment.Take(limit).ToList();
                    dataGrid = new DataGridClass(countPage, lsdepartment);
                }
                else
                {
                    //countPage = obj.Count();
                    int fromRow = limit * (page - 1);
                    //var lsdepartment = obj.OrderBy(t => t.Name).Skip(fromRow).Take(GlobalParams.ROWS_NUMBER_IN_PAGE).ToList();
                    var lsdepartment = obj.OrderBy(t => t.Name)
                        .Where(t => user.ListDepartmentAssigned.Contains(t.Index)).ToList();
                    countPage = lsdepartment.Count;
                    lsdepartment = lsdepartment.Skip(fromRow).Take(limit).ToList();
                    dataGrid = new DataGridClass(countPage, lsdepartment);
                }
            }
            else
            {
                dataGrid = GetDepartmentFromOtherDB(config, filter, page);
            }
            result = Ok(dataGrid);
            return result;
        }

        [Authorize]
        [ActionName("GetActiveDepartmentByPermission")]
        [HttpGet]
        public async Task<IActionResult> GetActiveDepartmentByPermission()
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            var result = await _IC_DepartmentService.GetActiveDepartmentByPermission(user);
            return ApiOk(result);
        }

        [Authorize]
        [ActionName("GetActiveDepartmentAndDeviceByPermission")]
        [HttpGet]
        public async Task<IActionResult> GetActiveDepartmentAndDeviceByPermission()
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            var result = await _IC_DepartmentService.GetActiveDepartmentAndDeviceByPermission(user);
            return ApiOk(result);
        }

        [Authorize]
        [ActionName("Get_RegularDepartment")]
        [HttpGet]
        public async Task<IActionResult> Get_RegularDepartment()
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }
            var departmentData = await _IC_DepartmentService.GetDepartmentChildrentByName(DepartmentKeyVstar.RegularDepartment, user.CompanyIndex);

            return Ok(departmentData);
        }
        [Authorize]
        [ActionName("Get_OfficeDepartment")]
        [HttpGet]
        public async Task<IActionResult> Get_OfficeDepartment()
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }
            var departmentData = await _IC_DepartmentService.GetDepartmentChildrentByName(DepartmentKeyVstar.OfficeDepartment, user.CompanyIndex);

            return Ok(departmentData);
        }
        [Authorize]
        [ActionName("Get_AllClassInfo")]
        [HttpGet]
        public async Task<IActionResult> Get_AllClassInfo()
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }
            var departmentData = await _IC_DepartmentService.GetDepartmentChildrentByName(DepartmentKeyVstar.AllClassInfo, user.CompanyIndex);

            return Ok(departmentData);
        }

        private DataGridClass GetDepartmentFromOtherDB(ConfigObject config, string filter, int page)
        {
            List<HR_Department> listDepartment;
            if (page <= 1)
            {
                if (string.IsNullOrEmpty(filter))
                {

                    listDepartment = otherContext.HR_Department.Where(t => t.CompanyIndex == config.CompanyIndex).Take(GlobalParams.ROWS_NUMBER_IN_PAGE).ToList();
                }
                else
                {
                    listDepartment = otherContext.HR_Department.Where(t => t.CompanyIndex == config.CompanyIndex &&
                            (t.Index.ToString().Contains(filter) || t.Name.Contains(filter) || t.Code.Contains(filter) || t.Description.Contains(filter)
                            || t.Location.Contains(filter))).Take(GlobalParams.ROWS_NUMBER_IN_PAGE).ToList();
                }
            }
            else
            {
                int fromRow = GlobalParams.ROWS_NUMBER_IN_PAGE * (page - 1);
                if (string.IsNullOrEmpty(filter))
                {
                    listDepartment = otherContext.HR_Department.Where(t => t.CompanyIndex == config.CompanyIndex).OrderBy(t => t.Name)
                        .Skip(fromRow).Take(GlobalParams.ROWS_NUMBER_IN_PAGE).ToList();
                }
                else
                {
                    listDepartment = otherContext.HR_Department.Where(t => t.CompanyIndex == config.CompanyIndex &&
                            (t.Index.ToString().Contains(filter) || t.Name.Contains(filter) || t.Code.Contains(filter) || t.Description.Contains(filter)
                            || t.Location.Contains(filter))).OrderBy(t => t.Name)
                                .Skip(fromRow).Take(GlobalParams.ROWS_NUMBER_IN_PAGE).ToList();
                }

            }
            List<HR_Department> listParent = otherContext.HR_Department.Where(t => t.CompanyIndex == config.CompanyIndex).ToList();
            var obj = from d in listDepartment
                      join c in listParent.DefaultIfEmpty() on d.ParentIndex equals c.Index into dep
                      from emps in dep.DefaultIfEmpty(new HR_Department())
                      select new
                      {
                          Index = d.Index,
                          Location = d.Location,
                          Name = d.Name,
                          Description = d.Description,
                          Code = d.Code,
                          ParentIndex = d.ParentIndex,
                          ParentName = emps.Name
                      };
            DataGridClass dataGrid = new DataGridClass(obj.Count(), obj);
            return dataGrid;
        }

        [Authorize]
        [ActionName("GetDepartment")]
        [HttpGet]
        public IActionResult GetDepartment()
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            ConfigObject config = ConfigObject.GetConfig(cache);
            if (config.IntegrateDBOther == false)
            {
                IEnumerable<object> dep;

                List<IC_Department> deparmentList = null;
                var userDepartmentAssignedList = user.ListDepartmentAssigned.ToHashSet();
                //deparmentList = context.IC_Department
                //    .Where(t => t.CompanyIndex == user.CompanyIndex && user.ListDepartmentAssigned.Contains(t.Index) && t.IsInactive != true).ToList();
                deparmentList = context.IC_Department
                    .Where(t => t.CompanyIndex == user.CompanyIndex && userDepartmentAssignedList.Contains(t.Index)
                        && t.IsInactive != true).ToList();
                deparmentList.Add(new IC_Department { Index = 0, Name = "Không có phòng ban" });
                if (user.ListDepartmentAssigned != null && user.ListDepartmentAssigned.Count > 0)
                {
                    deparmentList = deparmentList.Where(x => user.ListDepartmentAssigned.Contains(x.Index)).ToList();
                }
                dep = from department in deparmentList
                      orderby department.Name
                      select new
                      {
                          value = department.Index.ToString(),
                          label = department.Name,
                          parent = _IC_DepartmentService.GetDeparmentNameByIdFromList(department.ParentIndex ?? 0, deparmentList)
                      };
                result = Ok(dep);
            }
            else
            {
                var deparmentList = otherContext.HR_Department.Where(t => t.CompanyIndex == config.CompanyIndex).ToList();
                deparmentList.Add(new HR_Department { Index = 0, Name = "Không có phòng ban" });
                if (user.ListDepartmentAssigned != null && user.ListDepartmentAssigned.Count > 0)
                {
                    deparmentList = deparmentList.Where(x => user.ListDepartmentAssigned.Contains(x.Index)).ToList();
                }
                IEnumerable<object> dep = from department in deparmentList
                                          orderby department.Name
                                          select new
                                          {
                                              value = department.Index.ToString(),
                                              label = department.Name,
                                          };
                result = Ok(dep);
            }

            return result;
        }


        [Authorize]
        [ActionName("GetDepartmentByIDs")]
        [HttpPost]
        public IActionResult GetDepartmentByIDs([FromBody] List<int> ids)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }
            var departmentIds = ids.ToHashSet();

            ConfigObject config = ConfigObject.GetConfig(cache);
            if (config.IntegrateDBOther == false)
            {
                var deparmentList = context.IC_Department
                    .Where(t => t.CompanyIndex == user.CompanyIndex && user.ListDepartmentAssigned.Contains(t.Index) && t.IsInactive != true
                                && departmentIds.Contains(t.Index)
                    ).ToList();
                deparmentList.Add(new IC_Department { Index = 0, Name = "Không có phòng ban" });
                IEnumerable<object> dep = from department in deparmentList
                                          orderby department.Name
                                          select new
                                          {
                                              value = department.Index.ToString(),
                                              label = department.Name,
                                              parent = _IC_DepartmentService.GetDeparmentNameByIdFromList(department.ParentIndex ?? 0, deparmentList)
                                          };
                result = Ok(dep);
            }
            else
            {
                var longs = ids.ToList();
                var longLst = longs.ConvertAll(i => (long)i);
                var deparmentList = otherContext.HR_Department.Where(t => t.CompanyIndex == config.CompanyIndex && longLst.Contains(t.Index)).ToList();
                deparmentList.Add(new HR_Department { Index = 0, Name = "Không có phòng ban" });
                IEnumerable<object> dep = from department in deparmentList
                                          orderby department.Name
                                          select new
                                          {
                                              value = department.Index.ToString(),
                                              label = department.Name,
                                          };
                result = Ok(dep);
            }

            return result;
        }

        [Authorize]
        [ActionName("GetDevicesInOutDepartment")]
        [HttpGet]
        public IActionResult GetDevicesInOutDepartment(int departmentIndex)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }
            ConfigObject config = ConfigObject.GetConfig(cache);
            if (config.IntegrateDBOther == false)
            {
                var department = context.IC_Department.FirstOrDefault(t => t.CompanyIndex == user.CompanyIndex && t.Index == departmentIndex && t.IsInactive != true);
                if (department == null)
                {
                    return Conflict("DepartmentNotExists");
                }
            }
            else
            {
                var department = otherContext.HR_Department.FirstOrDefault(t => t.CompanyIndex == config.CompanyIndex && t.Index == departmentIndex);
                if (department == null)
                {
                    return Conflict("DepartmentNotExists");
                }
            }

            //List<Models.IC_Device> listDevice = context.IC_Device.Where(t => t.CompanyIndex == user.CompanyIndex).ToList();
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

            List<IC_DepartmentAndDevice> serviceDetails = context.IC_DepartmentAndDevice.Where(t => t.CompanyIndex == user.CompanyIndex
                  && t.DepartmentIndex == departmentIndex).ToList();
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
        [ActionName("AddDepartment")]
        [HttpPost]
        public IActionResult AddDepartment([FromBody] DepartmentParam param)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            param = (DepartmentParam)StringHelper.RemoveWhiteSpace(param);

            if (param.Name == "" || param.Code == "")
            {
                return BadRequest("PleaseFillAllRequiredFields");
            }

            // 10/04/2020: QC xac nhan khong cho trung Code, van co the trung Name
            // Models.IC_Department checkData = context.IC_Department.Where(t => t.CompanyIndex == user.CompanyIndex && t.Name == param.Name).FirstOrDefault();
            IC_Department checkData = context.IC_Department
                .FirstOrDefault(t => t.CompanyIndex == user.CompanyIndex && t.Code == param.Code && t.IsInactive != true);
            if (checkData != null)
            {
                return Conflict("DepartmentCodeIsExist");
            }

            var existedDepartment = context.IC_Department.FirstOrDefault(x
                => x.CompanyIndex == user.CompanyIndex && x.Name == param.Name && x.IsInactive != true
                && x.ParentIndex == param.ParentIndex);
            if (existedDepartment != null)
            {
                return Conflict("DepartmentExists");
            }

            var department = new IC_DepartmentDTO();
            department.Name = param.Name;
            department.Location = param.Location;
            department.Description = param.Description;
            //department.Code = param.Code;
            department.Code = DateTime.Now.Ticks.ToString();

            department.ParentIndex = param.ParentIndex;
            department.CompanyIndex = user.CompanyIndex;
            department.CreatedDate = DateTime.Now;

            department.UpdatedDate = DateTime.Now;
            department.UpdatedUser = user.UserName;
            department.IsContractorDepartment = param.IsContractorDepartment;
            department.IsDriverDepartment = param.IsDriverDepartment;

            if (param.ParentIndex != null && param.ParentIndex != 0 && _configClientName == ClientName.MONDELEZ.ToString())
            {
                var parent = context.IC_Department.FirstOrDefault(x => x.Index == param.ParentIndex);
                if (parent != null && (parent.IsContractorDepartment == true || parent.IsDriverDepartment == true))
                {
                    department.IsContractorDepartment = parent.IsContractorDepartment;
                    department.IsDriverDepartment = param.IsDriverDepartment;
                }
            }


            var depart = _iIC_DepartmentLogic.CheckExistedOrCreate(department, "");
            _IHR_EmployeeLogic.IntegrateDepartmentToOffline(new List<long> { depart.Index }, null).GetAwaiter().GetResult();

            // Phân quyền theo phòng ban
            user.InitDepartmentAssignedAndParent(context, otherContext, cache);

            return Ok();
        }

        [Authorize]
        [ActionName("UpdateDepartment")]
        [HttpPost]
        public IActionResult UpdateDepartment([FromBody] DepartmentParam param)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            param = (DepartmentParam)StringHelper.RemoveWhiteSpace(param);
            if (param.Name == "" || param.Code == "")
            {
                return BadRequest("PleaseFillAllRequiredFields");
            }
            IC_Department updateData = context.IC_Department.FirstOrDefault(t => t.CompanyIndex == user.CompanyIndex && t.Index == param.Index);
            if (updateData == null)
            {
                return NotFound("DepartmentNotExist");
            }

            //Models.IC_Department checkData = context.IC_Department.Where(t => t.CompanyIndex == user.CompanyIndex && t.Name == param.Name).FirstOrDefault();
            IC_Department checkData = context.IC_Department.Where(t => t.CompanyIndex == user.CompanyIndex && t.Code == param.Code).FirstOrDefault();
            if (checkData != null && checkData.Index != updateData.Index)
            {
                return Conflict("DepartmentCodeIsExist");
            }

            var existedDepartment = context.IC_Department.FirstOrDefault(x
                => x.CompanyIndex == user.CompanyIndex && x.Name == param.Name && x.IsInactive != true
                && x.ParentIndex == param.ParentIndex && x.Index != param.Index);
            if (existedDepartment != null)
            {
                return Conflict("DepartmentExists");
            }

            var isDriverDepartment = updateData.IsDriverDepartment;
            var isContractorDepartment = updateData.IsContractorDepartment;

            updateData.Name = param.Name;
            updateData.Location = param.Location;
            updateData.Description = param.Description;
            updateData.Code = param.Code;

            updateData.ParentIndex = param.ParentIndex;

            updateData.UpdatedDate = DateTime.Now;
            updateData.UpdatedUser = user.UserName;


            if (_configClientName == ClientName.MONDELEZ.ToString())
            {
                updateData.IsDriverDepartment = param.IsDriverDepartment;
                updateData.IsContractorDepartment = param.IsContractorDepartment;
                if (param.ParentIndex != null && param.ParentIndex != 0)
                {
                    var parent = context.IC_Department.FirstOrDefault(x => x.Index == param.ParentIndex);
                    if (parent != null && (parent.IsContractorDepartment == true || parent.IsDriverDepartment == true))
                    {
                        updateData.IsContractorDepartment = parent.IsContractorDepartment;
                        updateData.IsDriverDepartment = param.IsDriverDepartment;
                    }
                }

                if (updateData.IsDriverDepartment != isDriverDepartment || updateData.IsContractorDepartment != isContractorDepartment)
                {
                    var departments = context.IC_Department.Where(x => x.CompanyIndex == user.CompanyIndex).ToList();
                    var children = GetChildren(departments, param.Index);
                    foreach (var item in children)
                    {
                        item.IsContractorDepartment = updateData.IsContractorDepartment;
                        item.IsDriverDepartment = updateData.IsDriverDepartment;
                    }
                }

            }
            _IHR_EmployeeLogic.IntegrateDepartmentToOffline(new List<long> { param.Index }, null).GetAwaiter().GetResult();

            context.SaveChanges();

            result = Ok();
            return result;
        }

        private List<IC_Department> GetChildren(List<IC_Department> foos, long id)
        {
            return foos
                .Where(x => x.ParentIndex == id)
                .Union(foos.Where(x => x.ParentIndex == id)
                    .SelectMany(y => GetChildren(foos, y.Index))
                ).ToList();
        }


        [Authorize]
        [ActionName("UpdateDepartmentAndDevice")]
        [HttpPost]
        public IActionResult UpdateDepartmentAndDevice([FromBody] IC_DepartmentAndDeviceController.DepartmentAndDeviceParam param)
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
            DateTime now = DateTime.Now;
            try
            {
                //xóa dữ liệu device service
                if (param.ListDeviceSerial != null)
                {
                    context.IC_DepartmentAndDevice.RemoveRange(context.IC_DepartmentAndDevice.Where(t => t.CompanyIndex == user.CompanyIndex && t.DepartmentIndex == param.DepartmentIndex));
                    for (int i = 0; i < param.ListDeviceSerial.Count; i++)
                    {
                        IC_DepartmentAndDevice departmentAndDevices = new IC_DepartmentAndDevice();
                        departmentAndDevices.DepartmentIndex = param.DepartmentIndex;
                        departmentAndDevices.SerialNumber = param.ListDeviceSerial[i];
                        departmentAndDevices.CompanyIndex = user.CompanyIndex;
                        departmentAndDevices.UpdatedDate = now;
                        departmentAndDevices.UpdatedUser = user.UserName;

                        context.IC_DepartmentAndDevice.Add(departmentAndDevices);
                    }
                }
                context.SaveChanges();
                result = Ok();
            }
            catch (Exception ex)
            {
                result = StatusCode((int)HttpStatusCode.InternalServerError, ex.ToString());

                return result;
            }

            return result;
        }

        [Authorize]
        [ActionName("DeleteDepartment")]
        [HttpPost]
        public IActionResult DeleteDepartment([FromBody] List<DepartmentParam> lsparam)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            var lstdepartment = new List<string>();
            foreach (var param in lsparam)
            {
                IC_Department deleteData = context.IC_Department.Where(t => t.CompanyIndex == user.CompanyIndex && t.Index == param.Index).FirstOrDefault();
                lstdepartment.Add(deleteData.Code);
                IC_DepartmentAndDevice checkDataExistDepartment = context.IC_DepartmentAndDevice.Where(t => t.CompanyIndex == user.CompanyIndex && t.CompanyIndex == user.CompanyIndex && t.DepartmentIndex == param.Index).FirstOrDefault();

                IC_WorkingInfo checkDataExistEmployee = context.IC_WorkingInfo.Where(t => t.CompanyIndex == user.CompanyIndex && t.DepartmentIndex == param.Index).FirstOrDefault();
                if (deleteData == null)
                {
                    return NotFound("DepartmentNotExist");
                }
                else if (checkDataExistEmployee != null)
                {
                    return NotFound("EmployeeExistDepartment");
                }
                else if (checkDataExistDepartment != null)
                {
                    return NotFound("DeviceExistInDepartment");
                }
                else
                {
                    context.IC_Department.Remove(deleteData);
                }
            }
            _IHR_EmployeeLogic.DeleteDepartmentToOffline(lstdepartment).GetAwaiter().GetResult();
            context.SaveChanges();

            result = Ok();
            return result;
        }

        [Authorize]
        [ActionName("GetDepartmentTree")]
        [HttpGet]
        public IActionResult GetDepartmentTree()
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }
            ConfigObject config = ConfigObject.GetConfig(cache);
            int id = 1, level = 1;
            List<IC_EmployeeTreeDTO> listData = new List<IC_EmployeeTreeDTO>();
            IC_EmployeeTreeDTO companyData = new IC_EmployeeTreeDTO()
            {
                ID = -1,
                Level = level,
                Type = "Company",
                ListChildrent = new List<IC_EmployeeTreeDTO>()
            };


            List<IC_DepartmentDTO> lstDept = new List<IC_DepartmentDTO>();

            if (config.IntegrateDBOther)
            {
                var company = otherContext.HR_Company.FirstOrDefault(x => x.Index == config.CompanyIndex);
                companyData.Name = company?.Name ?? "Công ty";
                companyData.IDLocal = company == null ? "" : company.Index.ToString();
                lstDept = otherContext.HR_Department.Where(x => x.CompanyIndex == config.CompanyIndex)
                    .Select(x => new IC_DepartmentDTO() { Index = x.Index, Name = x.Name, Code = x.Code, ParentIndex = x.ParentIndex }).ToList();
            }
            else
            {
                var company = context.IC_Company.FirstOrDefault(x => x.Index == user.CompanyIndex);
                companyData.Name = company?.Name ?? "Công ty";
                companyData.IDLocal = company == null ? "" : company.Index.ToString();
                lstDept = context.IC_Department.Where(x => x.CompanyIndex == user.CompanyIndex && x.IsInactive != true)
                    .Select(x => new IC_DepartmentDTO() { Index = x.Index, Name = x.Name, Code = x.Code, ParentIndex = x.ParentIndex }).ToList();
            }

            //if (user.ListDepartmentAssigned != null && user.ListDepartmentAssigned.Count > 0)
            //{
            //    lstDept = lstDept.Where(x => user.ListDepartmentAssigned.Contains(x.Index)).ToList();
            //}
            var lstDeptLevel1 = lstDept.Where(x => x.ParentIndex == null || x.ParentIndex == 0).ToList();
            //if (user.ListDepartmentAssigned != null && user.ListDepartmentAssigned.Count > 0)
            //{
            //    lstDeptLevel1 = lstDeptLevel1.Where(x => user.ListDepartmentAssigned.Contains(x.Index)).ToList();
            //}
            for (int i = 0; i < lstDeptLevel1.Count; i++)
            {
                IC_EmployeeTreeDTO deptTree = new IC_EmployeeTreeDTO()
                {
                    ID = lstDeptLevel1[i].Index,
                    IDLocal = lstDeptLevel1[i].Index.ToString(),
                    Code = lstDeptLevel1[i].Code,
                    Name = lstDeptLevel1[i].Name,
                    Level = level,
                    Type = "Department"
                };
                deptTree.ListChildrent = RecursiveGetChildrentDepartment(lstDept, lstDeptLevel1[i].Index, id, level + 1);

                companyData.ListChildrent.Add(deptTree);
            }
            listData.Add(companyData);

            return Ok(listData);
        }

        [Authorize]
        [ActionName("GetDepartmentAEONTree")]
        [HttpGet]
        public IActionResult GetDepartmentAEONTree()
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }
            ConfigObject config = ConfigObject.GetConfig(cache);
            int id = 1, level = 1;
            List<IC_EmployeeTreeDTO> listData = new List<IC_EmployeeTreeDTO>();
            IC_EmployeeTreeDTO companyData = new IC_EmployeeTreeDTO()
            {
                ID = -1,
                Level = level,
                Type = "Company",
                ListChildrent = new List<IC_EmployeeTreeDTO>()
            };


            List<IC_DepartmentDTO> lstDept = new List<IC_DepartmentDTO>();
            Ids = new List<long>();
            if (config.IntegrateDBOther)
            {
                var company = otherContext.HR_Company.FirstOrDefault(x => x.Index == config.CompanyIndex);
                companyData.Name = company?.Name ?? "Công ty";
                companyData.IDLocal = company == null ? "" : company.Index.ToString();
                lstDept = otherContext.HR_Department.Where(x => x.CompanyIndex == config.CompanyIndex)
                    .Select(x => new IC_DepartmentDTO() { Index = x.Index, Name = x.Name, Code = x.Code, ParentIndex = x.ParentIndex }).ToList();
            }
            else
            {
                var company = context.IC_Company.FirstOrDefault(x => x.Index == user.CompanyIndex);
                companyData.Name = company?.Name ?? "Công ty";
                companyData.IDLocal = company == null ? "" : company.Index.ToString();
                lstDept = context.IC_Department.AsNoTracking().Where(x => x.CompanyIndex == user.CompanyIndex && x.IsInactive != true)
                    .Select(x => new IC_DepartmentDTO() { Index = x.Index, Name = x.Name, Code = x.Code, ParentIndex = x.ParentIndex, JobGrade = x.JobGradeGrade, IsStore = x.IsStore, Type = x.Type }).ToList();
            }

            //if (user.ListDepartmentAssigned != null && user.ListDepartmentAssigned.Count > 0)
            //{
            //    lstDept = lstDept.Where(x => user.ListDepartmentAssigned.Contains(x.Index)).ToList();
            //}
            var lstDeptLevel1 = lstDept.Where(x => x.IsStore == true && x.JobGrade == 5).ToList();
            //if (user.ListDepartmentAssigned != null && user.ListDepartmentAssigned.Count > 0)
            //{
            //    lstDeptLevel1 = lstDeptLevel1.Where(x => user.ListDepartmentAssigned.Contains(x.Index)).ToList();
            //}

            var lstDept1 = lstDept.Where(x => x.JobGrade >= 2).ToList();

            for (int i = 0; i < lstDeptLevel1.Count; i++)
            {
                IC_EmployeeTreeDTO deptTree = new IC_EmployeeTreeDTO()
                {
                    ID = lstDeptLevel1[i].Index,
                    IDLocal = lstDeptLevel1[i].Index.ToString(),
                    Code = lstDeptLevel1[i].Code,
                    Name = lstDeptLevel1[i].Name,
                    Level = level,
                    Type = "Department"
                };
                deptTree.ListChildrent = RecursiveGetChildrentDepartment(lstDept1, lstDeptLevel1[i].Index, id, level + 1);

                companyData.ListChildrent.Add(deptTree);

            }
            Ids.AddRange(HandleDepartmentFromJson(companyData));
            lstDept = lstDept.Where(x => !Ids.Contains(x.Index) && x.IsStore == false && x.Type == 2).ToList();
            lstDeptLevel1 = lstDept.Where(x => x.ParentIndex == null || x.ParentIndex == 0).ToList();
            IC_EmployeeTreeDTO deptTreee = new IC_EmployeeTreeDTO()
            {
                ID = -2,
                IDLocal = null,
                Name = "HQ",
                Level = 1,
                Type = "Department"
            };


            for (int i = 0; i < lstDeptLevel1.Count; i++)
            {
                IC_EmployeeTreeDTO deptTree = new IC_EmployeeTreeDTO()
                {
                    ID = lstDeptLevel1[i].Index,
                    IDLocal = lstDeptLevel1[i].Index.ToString(),
                    Code = lstDeptLevel1[i].Code,
                    Name = lstDeptLevel1[i].Name,
                    Level = level + 1,
                    Type = "Department"
                };
                deptTree.ListChildrent = RecursiveGetChildrentDepartment(lstDept, lstDeptLevel1[i].Index, id, level + 2);

                deptTreee.ListChildrent.Add(deptTree);
                Ids.Add(lstDeptLevel1[i].Index);
            }

            companyData.ListChildrent.Add(deptTreee);


            listData.Add(companyData);

            return Ok(listData);
        }

        private List<long> HandleDepartmentFromJson(IC_EmployeeTreeDTO deptLst)
        {
            var itemDepartmentLst = new List<long>();
            Queue<IC_EmployeeTreeDTO> q = new Queue<IC_EmployeeTreeDTO>();
            q.Enqueue(deptLst);
            while (q.Count != 0)
            {
                int n = q.Count;

                while (n > 0)
                {
                    IC_EmployeeTreeDTO p = q.Peek();
                    q.Dequeue();

                    itemDepartmentLst.Add(p.ID.Value);

                    for (int i = 0; i < p.ListChildrent.Count; i++)
                    {
                        q.Enqueue(p.ListChildrent[i]);
                    }
                    n--;
                }
            }


            return itemDepartmentLst;

        }

        [Authorize]
        [ActionName("GetDepartmentTreeEmployeeScreen")]
        [HttpGet]
        public IActionResult GetDepartmentTreeEmployeeScreen(string? userType)
        {


            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }
            var config = ConfigObject.GetConfig(cache);
            int id = 1, level = 1;
            var listData = new List<IC_EmployeeTreeDTO>();
            var companyData = new IC_EmployeeTreeDTO()
            {
                ID = -1,
                Level = level,
                Type = "Company",
                ListChildrent = new List<IC_EmployeeTreeDTO>()
            };


            var lstDept = new List<IC_DepartmentDTO>();

            if (config.IntegrateDBOther)
            {
                var company = otherContext.HR_Company.FirstOrDefault(x => x.Index == config.CompanyIndex);
                companyData.Name = company?.Name ?? "Công ty";
                companyData.IDLocal = company == null ? "" : company.Index.ToString();
                lstDept = otherContext.HR_Department.Where(x => x.CompanyIndex == config.CompanyIndex)
                    .Select(x => new IC_DepartmentDTO() { Index = x.Index, Name = x.Name, Code = x.Code, ParentIndex = x.ParentIndex }).ToList();
            }
            else
            {
                var company = context.IC_Company.FirstOrDefault(x => x.Index == user.CompanyIndex);
                companyData.Name = company?.Name ?? "Công ty";
                companyData.IDLocal = company == null ? "" : company.Index.ToString();
                if (!string.IsNullOrEmpty(userType))
                {
                   var listUserType = userType.Split(',')
                                       .Select(int.Parse)
                                       .ToList();


                    if (listUserType.Contains((int)EmployeeType.Employee) || listUserType.Contains((int)EmployeeType.Guest))
                    {
                        lstDept.AddRange(context.IC_Department.Where(x => x.CompanyIndex == user.CompanyIndex && x.IsInactive != true && x.IsContractorDepartment != true && x.IsDriverDepartment != true)
                  .Select(x => new IC_DepartmentDTO() { Index = x.Index, Name = x.Name, Code = x.Code, ParentIndex = x.ParentIndex }).ToList());
                    }

                    if (listUserType.Contains((int)EmployeeType.Contractor))
                    {
                        lstDept.AddRange(context.IC_Department.Where(x => x.CompanyIndex == user.CompanyIndex && x.IsInactive != true && x.IsContractorDepartment == true && x.IsDriverDepartment != true)
                  .Select(x => new IC_DepartmentDTO() { Index = x.Index, Name = x.Name, Code = x.Code, ParentIndex = x.ParentIndex }).ToList());
                    }

                    if (listUserType.Contains((int)EmployeeType.Driver))
                    {
                        lstDept.AddRange(context.IC_Department.Where(x => x.CompanyIndex == user.CompanyIndex && x.IsInactive != true && x.IsContractorDepartment != true && x.IsDriverDepartment == true)
                  .Select(x => new IC_DepartmentDTO() { Index = x.Index, Name = x.Name, Code = x.Code, ParentIndex = x.ParentIndex }).ToList());
                    }
                    if (listUserType.Contains((int)DepartmentType.EmployeeAndContractor))
                    {
                        lstDept.AddRange(context.IC_Department.Where(x => x.CompanyIndex == user.CompanyIndex && x.IsInactive != true && x.IsDriverDepartment != true)
                  .Select(x => new IC_DepartmentDTO() { Index = x.Index, Name = x.Name, Code = x.Code, ParentIndex = x.ParentIndex }).ToList());
                    }
                }
                else
                {
                    lstDept = context.IC_Department.Where(x => x.CompanyIndex == user.CompanyIndex && x.IsInactive != true)
                  .Select(x => new IC_DepartmentDTO() { Index = x.Index, Name = x.Name, Code = x.Code, ParentIndex = x.ParentIndex, IsContractorDepartment = x.IsContractorDepartment, IsDriverDepartment = x.IsDriverDepartment }).ToList();
                }

            }

            if (user.ListDepartmentAssigned != null && user.ListDepartmentAssigned.Count > 0)
            {
                lstDept = lstDept.Where(x => user.ListDepartmentAssigned.Contains(x.Index)).ToList();
            }
            var lstDeptLevel1 = lstDept.Where(x => x.ParentIndex == null || x.ParentIndex == 0).ToList();
            if (user.ListDepartmentAssigned != null && user.ListDepartmentAssigned.Count > 0)
            {
                lstDeptLevel1 = lstDeptLevel1.Where(x => user.ListDepartmentAssigned.Contains(x.Index)).ToList();
            }
            for (int i = 0; i < lstDeptLevel1.Count; i++)
            {
                var deptTree = new IC_EmployeeTreeDTO()
                {
                    ID = lstDeptLevel1[i].Index,
                    IDLocal = lstDeptLevel1[i].Index.ToString(),
                    Code = lstDeptLevel1[i].Code,
                    Name = lstDeptLevel1[i].Name,
                    Level = level,
                    Type = "Department",
                    IsContractorDepartment = lstDeptLevel1[i].IsContractorDepartment,
                    IsDriverDepartment = lstDeptLevel1[i].IsDriverDepartment
                };
                deptTree.ListChildrent = RecursiveGetChildrentDepartment(lstDept, lstDeptLevel1[i].Index, id, level + 1, user);

                companyData.ListChildrent.Add(deptTree);
            }

            companyData.ListChildrent.Add(new IC_EmployeeTreeDTO
            {
                ID = 0,
                IDLocal = "0",
                Code = "NoDepartment",
                Name = "Không có phòng ban",
                Level = 1,
                Type = "Department"
            });

            listData.Add(companyData);

            return Ok(listData);
        }

        [Authorize]
        [ActionName("GetAllDoor")]
        [HttpGet]
        public IActionResult GetAllDoor()
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }
            ConfigObject config = ConfigObject.GetConfig(cache);
            var door = _DbContext.AC_Door.ToList();

            return Ok(door);
        }

        [Authorize]
        [ActionName("GetAllTimeZone")]
        [HttpGet]
        public IActionResult GetAllTimeZone()
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }
            ConfigObject config = ConfigObject.GetConfig(cache);
            var door = _DbContext.AC_TimeZone.ToList();

            return Ok(door);
        }

        [Authorize]
        [ActionName("GetAllGroup")]
        [HttpGet]
        public IActionResult GetAllGroup()
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }
            ConfigObject config = ConfigObject.GetConfig(cache);
            var door = _DbContext.AC_AccGroup.ToList();

            return Ok(door);
        }

        [Authorize]
        [ActionName("GetAll")]
        [HttpGet]
        public IActionResult GetAll()
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }
            ConfigObject config = ConfigObject.GetConfig(cache);
            if (config.IntegrateDBOther == false)
            {

                IEnumerable<object> dep;

                var departmentDTOs = _iIC_DepartmentLogic.GetAll(user.CompanyIndex);
                departmentDTOs.Add(new IC_DepartmentDTO { Index = 0, Name = "Không có phòng ban" });
                if (user.ListDepartmentAssigned != null && user.ListDepartmentAssigned.Count > 0)
                {
                    departmentDTOs = departmentDTOs.Where(x => user.ListDepartmentAssigned.Contains(x.Index)).ToList();
                }
                dep = departmentDTOs.Select(u => new
                {
                    value = u.Index.ToString(),
                    label = u.Name
                });
                result = Ok(dep);
            }
            else
            {
                List<HR_Department> departmentDTOs = otherContext.HR_Department.Where(t => t.CompanyIndex == config.CompanyIndex).ToList();
                departmentDTOs.Add(new HR_Department { Index = 0, Name = "Không có phòng ban" });
                if (user.ListDepartmentAssigned != null && user.ListDepartmentAssigned.Count > 0)
                {
                    departmentDTOs = departmentDTOs.Where(x => user.ListDepartmentAssigned.Contains(x.Index)).ToList();
                }
                IEnumerable<object> dep = departmentDTOs.Select(u => new
                {
                    value = u.Index.ToString(),
                    label = u.Name
                });
                result = Ok(dep);
            }

            return Ok(result);
        }

        private List<IC_EmployeeTreeDTO> RecursiveGetChildrentDepartment(List<IC_DepartmentDTO> lstDept, long pCurrentIndex, decimal pId, int pLevel)
        {
            var lstChild = lstDept.Where(x => x.ParentIndex == pCurrentIndex).ToList();
            var output = new List<IC_EmployeeTreeDTO>();
            for (int i = 0; i < lstChild.Count(); i++)
            {
                var deptTree = new IC_EmployeeTreeDTO()
                {
                    ID = lstChild[i].Index,
                    IDLocal = lstChild[i].Index.ToString(),
                    Code = lstChild[i].Code,
                    Name = lstChild[i].Name,
                    Level = pLevel,
                    Type = "Department"
                };
                deptTree.ListChildrent = RecursiveGetChildrentDepartment(lstDept, lstChild[i].Index, deptTree.ID.Value, pLevel + 1);

                output.Add(deptTree);
            }
            return output;
        }
        private List<IC_EmployeeTreeDTO> RecursiveGetChildrentDepartment(List<IC_DepartmentDTO> lstDept, long pCurrentIndex, decimal pId, int pLevel, UserInfo user)
        {
            var lstChild = lstDept.Where(x => x.ParentIndex == pCurrentIndex).ToList();
            if (user != null && user.ListDepartmentAssigned != null && user.ListDepartmentAssigned.Count > 0)
            {
                lstChild = lstChild.Where(x => user.ListDepartmentAssigned.Contains(x.Index)).ToList();
            }
            var output = new List<IC_EmployeeTreeDTO>();
            for (int i = 0; i < lstChild.Count(); i++)
            {
                var deptTree = new IC_EmployeeTreeDTO()
                {
                    ID = lstChild[i].Index,
                    IDLocal = lstChild[i].Index.ToString(),
                    Code = lstChild[i].Code,
                    Name = lstChild[i].Name,
                    Level = pLevel,
                    Type = "Department",
                    IsContractorDepartment = lstChild[i].IsContractorDepartment,
                    IsDriverDepartment = lstChild[i].IsDriverDepartment
                };
                deptTree.ListChildrent = RecursiveGetChildrentDepartment(lstDept, lstChild[i].Index, deptTree.ID.Value, pLevel + 1, user);

                output.Add(deptTree);
            }
            return output;
        }

        [Authorize]
        [ActionName("Get_IC_Departments")]
        [HttpGet]
        public IActionResult Get_IC_Departments()
        {
            UserInfo user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            var allDepartment = _IC_DepartmentService.GetAllDepartment(user.CompanyIndex);

            return ApiOk(allDepartment);
        }

        [Authorize]
        [ActionName("AddDepartmentFromExcel")]
        [HttpPost]
        public IActionResult AddDepartmentFromExcel(List<IC_DepartmentImportDTO> lstImport)
        {
            try
            {
                UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
                ConfigObject config = ConfigObject.GetConfig(cache);
                IActionResult result = Unauthorized();
                if (user == null)
                {
                    return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
                }
                // validation data
                List<IC_DepartmentImportDTO> listError = new List<IC_DepartmentImportDTO>();

                foreach (var item in lstImport)
                {
                    item.Code = DateTime.Now.Ticks.ToString();
                    item.ParentName = string.IsNullOrWhiteSpace(item.ParentName) ? string.Empty : item.ParentName;
                    Thread.Sleep(10);
                }

                listError = _iIC_DepartmentLogic.ValidationImportDepartment(lstImport);
                var message = "";
                string sWebRootFolder = _hostingEnvironment.ContentRootPath;
                string URL = string.Format("{0}://{1}/{2}", Request.Scheme, Request.Host, @"Files/DepartmentImportError.xlsx");
                FileInfo file = new FileInfo(Path.Combine(sWebRootFolder, @"Files/DepartmentImportError.xlsx"));
                if (listError != null && listError.Count() > 0)
                {
                    var listDepartmentCodeError = listError.Select(e => e.Code).ToList();
                    lstImport = lstImport.Where(e => !listDepartmentCodeError.Contains(e.Code)).ToList();

                }

                var lstParentIndex = new List<IC_DepartmentParentDTO>();
                bool isFirst = true;
                do
                {
                    if (lstParentIndex.Count > 0)
                    {
                        var listImportToDb = (from x in lstImport
                                              join y in lstParentIndex
                                              on x.ParentName.ToUpper().Trim() equals y.Name.ToUpper().Trim()
                                              where y is not null
                                              select new IC_Department
                                              {
                                                  Code = x.Code,
                                                  Name = x.Name.Trim(),
                                                  CompanyIndex = user.CompanyIndex,
                                                  CreatedDate = DateTime.Now,
                                                  Location = x.Location,
                                                  ParentIndex = y.Index,
                                                  Description = x.Description,
                                                  IsContractorDepartment = y.IsContractorDepartment,
                                                  IsDriverDepartment = y.IsDriverDepartment
                                              }).ToList();

                        lstImport = lstImport.Where(x => !listImportToDb.Select(y => y.Code).Contains(x.Code)).ToList();
                        if (listImportToDb.Count > 0)
                        {
                            context.IC_Department.AddRange(listImportToDb);
                            context.SaveChanges();
                        }
                        lstParentIndex = _IC_DepartmentService.Where(x => x.CompanyIndex == user.CompanyIndex && x.ParentIndex.HasValue && lstParentIndex.Select(x => x.Index).Contains(x.ParentIndex.Value)).Select(x => new IC_DepartmentParentDTO { Index = x.Index, Name = x.Name, IsContractorDepartment = x.IsContractorDepartment, IsDriverDepartment = x.IsDriverDepartment }).ToList();
                    }
                    else if (isFirst)
                    {
                        isFirst = false;
                        var listImportToDb = lstImport.Where(x => string.IsNullOrEmpty(x.ParentName)).ToList();
                        lstImport = lstImport.Except(listImportToDb).ToList();
                        listImportToDb = listImportToDb.Select(x => new IC_DepartmentImportDTO
                        {
                            Code = x.Code,
                            ParentName = x.ParentName,
                            CompanyIndex = user.CompanyIndex,
                            CreatedDate = DateTime.Now,
                            Description = x.Description,
                            ErrorMessage = x.ErrorMessage,
                            Location = x.Location,
                            Name = x.Name.Trim(),
                            IsContractorDepartment = x.IsContractorDepartment,
                            IsDriverDepartment = x.IsDriverDepartment
                        }).ToList();
                        var listParentImport = _Mapper.Map<List<IC_Department>>(listImportToDb);
                        if (listParentImport.Count > 0)
                        {
                            context.IC_Department.AddRange(listParentImport);
                            context.SaveChanges();
                        }
                        lstParentIndex = _IC_DepartmentService.Where(x => x.CompanyIndex == user.CompanyIndex && (x.ParentIndex is null || x.ParentIndex == 0)).Select(x => new IC_DepartmentParentDTO { Index = x.Index, Name = x.Name, IsContractorDepartment = x.IsContractorDepartment, IsDriverDepartment = x.IsDriverDepartment }).ToList();

                    }

                } while (lstParentIndex.Count > 0);

                if (lstImport.Count > 0 || (listError != null && listError.Count() > 0))
                {
                    if (lstImport.Count > 0)
                    {
                        lstImport.ForEach(x => x.ErrorMessage = "Phòng ban cha không tồn tại");
                        listError.AddRange(lstImport);
                    }

                    message = listError.Count().ToString();

                    using (var workbook = new XLWorkbook())
                    {
                        var worksheet = workbook.Worksheets.Add("DepartmentError");
                        var currentRow = 1;
                        //worksheet.Cell(currentRow, 1).Value = "Mã phòng ban (*)";
                        int rownumber = 6;
                        worksheet.Cell(currentRow, 1).Value = "Tên phòng ban (*)";
                        worksheet.Cell(currentRow, 2).Value = "Vị trí";
                        worksheet.Cell(currentRow, 3).Value = "Phòng ban cha";
                        if (_configClientName == ClientName.MONDELEZ.ToString())

                        {
                            rownumber = 8;
                            worksheet.Cell(currentRow, 4).Value = "Phòng ban nhà thầu";
                            worksheet.Cell(currentRow, 5).Value = "Phòng ban tài xế";
                            worksheet.Cell(currentRow, 6).Value = "Diễn giải";
                            worksheet.Cell(currentRow, 7).Value = "Lỗi";
                        }
                        else
                        {
                            worksheet.Cell(currentRow, 4).Value = "Diễn giải";
                            worksheet.Cell(currentRow, 5).Value = "Lỗi";
                        }


                        for (int i = 1; i < rownumber; i++)
                        {
                            worksheet.Cell(1, i).Style.Fill.BackgroundColor = XLColor.Yellow;
                            worksheet.Cell(1, i).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                            worksheet.Column(i).Width = 20;
                        }

                        foreach (var department in listError)
                        {
                            currentRow++;
                            //New template
                            //worksheet.Cell(currentRow, 1).Value = department.Code;
                            //worksheet.Cell(currentRow, 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 1).Value = department.Name;
                            worksheet.Cell(currentRow, 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 2).Value = department.Location;
                            worksheet.Cell(currentRow, 2).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 3).Value = department.ParentName;
                            worksheet.Cell(currentRow, 3).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            if (_configClientName == ClientName.MONDELEZ.ToString())

                            {
                                worksheet.Cell(currentRow, 4).Value = department.IsContractorDepartment == true ? "X" : "";
                                worksheet.Cell(currentRow, 4).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                                worksheet.Cell(currentRow, 5).Value = department.IsDriverDepartment == true ? "X" : "";
                                worksheet.Cell(currentRow, 5).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                                worksheet.Cell(currentRow, 6).Value = department.Description;
                                worksheet.Cell(currentRow, 6).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                                worksheet.Cell(currentRow, 7).Value = department.ErrorMessage;
                                worksheet.Cell(currentRow, 7).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;


                            }
                            else
                            {
                                worksheet.Cell(currentRow, 4).Value = department.Description;
                                worksheet.Cell(currentRow, 4).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                                worksheet.Cell(currentRow, 5).Value = department.ErrorMessage;
                                worksheet.Cell(currentRow, 5).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            }


                        }
                        workbook.SaveAs(file.FullName);
                    }
                }

                // Add audit log
                IC_AuditEntryDTO audit = new IC_AuditEntryDTO(null);
                audit.TableName = "IC_Department";
                audit.UserName = user.UserName;
                audit.CompanyIndex = user.CompanyIndex;
                audit.State = AuditType.Added;
                //audit.Description = AuditType.Added.ToString() + " Import From Excel " + lstImport.Count().ToString() + " Department";
                audit.Description = AuditType.Added.ToString() + "DepartmentFromExcel:/:" + lstImport.Count().ToString();
                audit.DateTime = DateTime.Now;
                _iIC_AuditLogic.Create(audit);

                user.InitDepartmentAssignedAndParent(context, otherContext, cache);
                _IHR_EmployeeLogic.IntegrateDepartmentToOffline(null, null).GetAwaiter().GetResult();
                result = Ok(message);
                return result;
            }
            catch (Exception ex)
            {
                return ApiError(ex.Message);

            }

        }
    }

    public class DepartmentParam
    {
        public int Index { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public string Description { get; set; }
        public string Code { get; set; }
        public int? ParentIndex { get; set; }
        public bool? IsContractorDepartment { get; set; }
        public bool? IsDriverDepartment { get; set; }
    }
}
