using EPAD_Backend_Core.Base;
using EPAD_Backend_Core.WebUtilitys;
using EPAD_Common.Types;
using EPAD_Common.Utility;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using EPAD_Logic;
using EPAD_Logic.MainProcess;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Xml;

namespace EPAD_Backend_Core.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("api/Service/[action]")]
    [ApiController]
    public class IC_ServiceController : ApiControllerBase
    {
        private EPAD_Context context;
        private IMemoryCache cache;
        private IIC_ServiceAndDeviceLogic _iC_ServiceAndDeviceLogic;
        public IC_ServiceController(IServiceProvider provider):base(provider)
        {
            context = TryResolve<EPAD_Context>();
            cache = TryResolve<IMemoryCache>();
            _iC_ServiceAndDeviceLogic = TryResolve<IIC_ServiceAndDeviceLogic>();
        }

        [Authorize]
        [ActionName("GetServiceAtPage")]
        [HttpGet]
        public IActionResult GetServiceAtPage(int page, string filter, int limit)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            List<IC_Service> listService = null;
            int countPage = 0;
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            if (string.IsNullOrEmpty(filter))
            {
                if (page <= 1)
                {
                    listService = context.IC_Service.Where(t => t.CompanyIndex == user.CompanyIndex).Take(limit).ToList();
                }
                else
                {
                    int fromRow = limit * (page - 1);
                    listService = context.IC_Service.Where(t => t.CompanyIndex == user.CompanyIndex)
                        .Skip(fromRow).Take(limit).ToList();
                }
            }
            else
            {
                if (page <= 1)
                {
                    var services = context.IC_Service.Where(t => t.CompanyIndex == user.CompanyIndex && (
                       t.Name.Contains(filter)
                    || t.Description.Contains(filter)
                    || t.ServiceType.Contains(filter)
                    ));
                    countPage = services.Count();
                    //listService = services.Take(GlobalParams.ROWS_NUMBER_IN_PAGE).ToList();
                    listService = services.Take(limit).ToList();
                }
                else
                {
                    int fromRow = limit * (page - 1);
                    var services = context.IC_Service.Where(t => t.CompanyIndex == user.CompanyIndex && (
                       t.Name.Contains(filter)
                    || t.Description.Contains(filter)
                    || t.ServiceType.Contains(filter)
                    )).OrderBy(t => t.Name);

                    countPage = services.Count();
                    //listService = services.Skip(fromRow).Take(GlobalParams.ROWS_NUMBER_IN_PAGE).ToList();
                    listService = services.Skip(fromRow).Take(limit).ToList();
                }
            }


            List<IC_Device> listDevice = context.IC_Device.Where(t => t.CompanyIndex == user.CompanyIndex).ToList();
            List<IC_ServiceAndDevices> listServiceDetails = context.IC_ServiceAndDevices.Where(t => t.CompanyIndex == user.CompanyIndex).ToList();
            List<ServiceResult> listData = new List<ServiceResult>();

            for (int i = 0; i < listService.Count; i++)
            {
                ServiceResult data = new ServiceResult();
                data.Index = listService[i].Index;
                data.Name = listService[i].Name;
                data.ServiceType = listService[i].ServiceType;
                data.Description = listService[i].Description;

                List<IC_ServiceAndDevices> serviceDetails = listServiceDetails.FindAll(t => t.ServiceIndex == listService[i].Index).ToList();
                string[] arrSerial = new string[serviceDetails.Count];
                for (int j = 0; j < serviceDetails.Count; j++)
                {
                    arrSerial[j] = serviceDetails[j].SerialNumber;
                }

                List<IC_Device> listDeviceInRange = listDevice.Where(t => arrSerial.Contains(t.SerialNumber)).ToList();
                List<DeviceBasic> listDeviceBasic = new List<DeviceBasic>();
                foreach (IC_Device item in listDeviceInRange)
                {
                    DeviceBasic device = new DeviceBasic();
                    device.SerialNumber = item.SerialNumber;
                    device.AliasName = item.AliasName;
                    device.IPAddress = item.IPAddress;
                    device.Port = item.Port;
                    listDeviceBasic.Add(device);
                }
                data.ListDevice = listDeviceBasic;

                listData.Add(data);
            }

            int record = 0;
            if (string.IsNullOrEmpty(filter))
            {
                record = context.IC_Service.Where(t => t.CompanyIndex == user.CompanyIndex).ToList().Count;
            }
            else
            {
                record = countPage;
            }
            //double totalPage = ConfigObject.CheckDoubleNumber((record / GlobalParams.ROWS_NUMBER_IN_PAGE).ToString());
            DataGridClass dataGrid = new DataGridClass(record, listData);



            result = Ok(dataGrid);
            return result;
        }
        
        [Authorize]
        [ActionName("GetAllService")]
        [HttpGet]
        public IActionResult GetAllService()
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            List<IC_Service> listService = null;

            listService = context.IC_Service.Where(t => t.CompanyIndex == user.CompanyIndex).OrderBy(t => t.Name).ToList();

            result = Ok(listService);
            return result;
        }

        [Authorize]
        [ActionName("GetAllServiceForDownload")]
        [HttpGet]
        public IActionResult GetAllServiceForDownload()
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            List<IC_Service> listService = null;

            listService = context.IC_Service.Where(t => t.CompanyIndex == user.CompanyIndex
                && t.ServiceType != null && t.ServiceType == "SDKInterfaceService").OrderBy(t => t.Name).ToList();

            result = Ok(listService);
            return result;
        }

        [Authorize]
        [ActionName("GetDevicesByService")]
        [HttpGet]
        public IActionResult GetDevicesByService(int serviceIndex)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            IC_Service service = context.IC_Service.Where(t => t.CompanyIndex == user.CompanyIndex && t.Index == serviceIndex).FirstOrDefault();
            if (service == null)
            {
                return Conflict("ServiceNotExists");
            }


            List<IC_ServiceAndDevices> serviceDetails = context.IC_ServiceAndDevices.Where(t => t.CompanyIndex == user.CompanyIndex
                  && t.ServiceIndex == serviceIndex).ToList();
            string[] arrSerial = new string[serviceDetails.Count];
            for (int j = 0; j < serviceDetails.Count; j++)
            {
                arrSerial[j] = serviceDetails[j].SerialNumber;
            }
            List<IC_Device> listDevice = context.IC_Device.Where(t => t.CompanyIndex == user.CompanyIndex && arrSerial.Contains(t.SerialNumber)).ToList();
            List<DeviceByService> listDeviceBasic = new List<DeviceByService>();

            for (int i = 0; i < listDevice.Count; i++)
            {
                DeviceByService device = new DeviceByService();
                device.SerialNumber = listDevice[i].SerialNumber;
                device.AliasName = listDevice[i].AliasName;
                device.IPAddress = listDevice[i].IPAddress;
                device.ConnectionCode = listDevice[i].ConnectionCode;
                device.Port = listDevice[i].Port.ToString();
                device.InService = false;
                if (arrSerial.Contains(device.SerialNumber))
                {
                    device.InService = true;
                }

                if (!string.IsNullOrEmpty(listDevice[i].DeviceModel))
                {
                    int.TryParse(listDevice[i].DeviceModel, out int deviceModel);
                    device.DeviceModel = deviceModel;
                }

                listDeviceBasic.Add(device);
            }


            result = Ok(listDeviceBasic);
            return result;
        }


        [AllowAnonymous]
        [ActionName("GetDevicesByServiceOffline")]
        [HttpGet]
        public IActionResult GetDevicesByServiceOffline(int serviceIndex)
        {
            IActionResult result = Unauthorized();
            IC_Service service = context.IC_Service.Where(t => t.Index == serviceIndex).FirstOrDefault();
            if (service == null)
            {
                return Conflict("ServiceNotExists");
            }


            List<IC_ServiceAndDevices> serviceDetails = context.IC_ServiceAndDevices.Where(t => 
                   t.ServiceIndex == serviceIndex).ToList();
            string[] arrSerial = new string[serviceDetails.Count];
            for (int j = 0; j < serviceDetails.Count; j++)
            {
                arrSerial[j] = serviceDetails[j].SerialNumber;
            }
            List<IC_Device> listDevice = context.IC_Device.Where(t => arrSerial.Contains(t.SerialNumber) && t.IsUsingOffline).ToList();
            var listDeviceBasic = listDevice.Select(x => new Tuple<string, string, int>(x.SerialNumber, x.IPAddress, x.Port ?? 4370)).ToList();
            result = Ok(listDeviceBasic);
            return result;
        }

        [Authorize]
        [ActionName("GetDevicesInOutService")]
        [HttpGet]
        public IActionResult GetDevicesInOutService(int serviceIndex)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            IC_Service service = context.IC_Service.Where(t => t.CompanyIndex == user.CompanyIndex && t.Index == serviceIndex).FirstOrDefault();
            if (service == null)
            {
                return Conflict("ServiceNotExists");
            }

            List<IC_Device> listDevice = new List<IC_Device>();
            if (user.IsAdmin)
            {
                listDevice = _DbContext.IC_Device.Where(x => x.CompanyIndex == user.CompanyIndex).ToList();
            }
            else
            {
                listDevice = (from d in context.IC_Device
                              join pridv in context.IC_PrivilegeDeviceDetails on d.SerialNumber equals pridv.SerialNumber
                              join pri in context.IC_UserPrivilege on pridv.PrivilegeIndex equals pri.Index
                              join ac in context.IC_UserAccount on pri.Index equals ac.AccountPrivilege
                              where ac.UserName.Equals(user.UserName) && (pridv.Role.Equals(Privilege.None.ToString()) == false)
                                          select d).Cast<IC_Device>().ToList();
            }
    

            List<DeviceByService> listDeviceBasic = new List<DeviceByService>();

            List<AddedParam> paramAdded = new List<AddedParam>();
            paramAdded.Add(new AddedParam { Key = "CompanyIndex", Value = user.CompanyIndex });
            var serviceDetails = _iC_ServiceAndDeviceLogic.GetMany(paramAdded);

            for (int i = 0; i < listDevice.Count; i++)
            {
                DeviceByService device = new DeviceByService();
                device.SerialNumber = listDevice[i].SerialNumber;
                device.AliasName = listDevice[i].AliasName;
                device.IPAddress = listDevice[i].IPAddress;
                device.Port = listDevice[i].Port.ToString();
                device.InService = false;
                device.Available = true;

                var inService = serviceDetails.Where(u => u.SerialNumber == device.SerialNumber && u.ServiceIndex == serviceIndex).FirstOrDefault();
                if (inService != null)
                {
                    device.InService = true;
                }
                // Filter remove all device in other service type
                // it mean one device allow add in a service type only
                var serviceType = serviceDetails.Where(u => u.SerialNumber == device.SerialNumber && u.ServiceType != service.ServiceType).FirstOrDefault();
                if (serviceType == null) {
                    listDeviceBasic.Add(device);
                }

            }

            result = Ok(listDeviceBasic);
            return result;
        }

        [Authorize]
        [ActionName("AddService")]
        [HttpPost]
        public IActionResult AddService([FromBody]ServiceParam param)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            param = (ServiceParam)StringHelper.RemoveWhiteSpace(param);

            if (param.Name == "")
            {
                return BadRequest("PleaseFillAllRequiredFields");
            }

            if (param.ServiceType == "PUSHInterfaceService")
            {
                IC_Service checkPUSHInterfaceService = context.IC_Service.Where(t => t.CompanyIndex == user.CompanyIndex && t.ServiceType == param.ServiceType).FirstOrDefault();

                if (checkPUSHInterfaceService != null)
                {
                    return NotFound("ServiceExistPUSHInterfaceService");
                }
            }

            IC_Service checkData = context.IC_Service.Where(t => t.CompanyIndex == user.CompanyIndex && t.Name == param.Name).FirstOrDefault();
            if (checkData != null)
            {
                return Conflict("ServiceNameIsExist");
            }
            DateTime now = DateTime.Now;
            IC_Service data = new IC_Service();
            data.Name = param.Name;
            data.ServiceType = param.ServiceType;
            data.Description = param.Description;
            data.CompanyIndex = user.CompanyIndex;
            data.CreatedDate = now;
            data.UpdatedDate = now;
            data.UpdatedUser = user.UserName;

            context.IC_Service.Add(data);
            context.SaveChanges();

            result = Ok();
            return result;
        }

        [Authorize]
        [ActionName("UpdateService")]
        [HttpPost]
        public IActionResult UpdateService([FromBody]ServiceParam param)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            param = (ServiceParam)StringHelper.RemoveWhiteSpace(param);

            try
            {
                IC_Service updateData = context.IC_Service.Where(t => t.Index == param.Index).FirstOrDefault();

                if (param.ListDeviceSerial != null)
                {
                    foreach (var item in param.ListDeviceSerial)
                    {

                        //Phân quyền
                        IC_Device CheckPriDevice = (from d in context.IC_Device
                                                    join pridv in context.IC_PrivilegeDeviceDetails on d.SerialNumber equals pridv.SerialNumber
                                                    join pri in context.IC_UserPrivilege on pridv.PrivilegeIndex equals pri.Index
                                                    join ac in context.IC_UserAccount on pri.Index equals ac.AccountPrivilege
                                                    where (user.IsAdmin ||  ac.UserName.Equals(user.UserName) && (!pridv.Role.Equals(Privilege.Full.ToString())) && (!pridv.Role.Equals(Privilege.Edit.ToString()))) && (d.SerialNumber.Equals(item))
                                                    select d).Cast<IC_Device>().FirstOrDefault();

                        if (CheckPriDevice != null && !user.IsAdmin)
                        {
                            return BadRequest("MSG_NotPrivilege");
                        }
                    }
                }

                if (updateData == null)
                {
                    return NotFound("ServiceNotExist");
                }
                if (param.ServiceType == "PUSHInterfaceService")
                {
                    IC_Service checkPUSHInterfaceService = context.IC_Service.Where(t => t.CompanyIndex == user.CompanyIndex && t.Index.Equals(param.Index) && t.ServiceType == "PUSHInterfaceService").FirstOrDefault();

                    if (checkPUSHInterfaceService != null)
                    {
                        return NotFound("ServiceExistPUSHInterfaceService");
                    }
                }

                IC_Service checkData = context.IC_Service.Where(t => t.CompanyIndex == user.CompanyIndex && t.Name == param.Name).FirstOrDefault();
                if (checkData != null && checkData.Index != updateData.Index)
                {
                    return Conflict("ServiceNameIsExist");
                }
                if (param.Name != "")
                {
                    updateData.Name = param.Name;
                }
                if (param.Description != null)
                {
                    updateData.Description = param.Description;
                }
                if (string.IsNullOrEmpty(param.ServiceType) == false)
                {
                    updateData.ServiceType = param.ServiceType;
                }

                DateTime now = DateTime.Now;

                updateData.UpdatedDate = now;
                updateData.UpdatedUser = user.UserName;

                //xóa dữ liệu device service
                if (param.ListDeviceSerial != null)
                {
                    context.IC_ServiceAndDevices.RemoveRange(context.IC_ServiceAndDevices.Where(t => t.CompanyIndex == user.CompanyIndex && t.ServiceIndex == param.Index));
                    for (int i = 0; i < param.ListDeviceSerial.Count; i++)
                    {
                        IC_ServiceAndDevices serviceAndDevices = new IC_ServiceAndDevices();
                        serviceAndDevices.ServiceIndex = param.Index;
                        serviceAndDevices.SerialNumber = param.ListDeviceSerial[i];
                        serviceAndDevices.CompanyIndex = user.CompanyIndex;
                        serviceAndDevices.UpdatedDate = now;
                        serviceAndDevices.UpdatedUser = user.UserName;

                        context.IC_ServiceAndDevices.Add(serviceAndDevices);
                    }
                    // update device for service
                    List<IC_Device> listDevice = context.IC_Device.Where(t => t.CompanyIndex == user.CompanyIndex
                          && param.ListDeviceSerial.Contains(t.SerialNumber)).ToList();
                    CompanyInfo companyInfo = CompanyInfo.GetFromCache(cache, user.CompanyIndex.ToString());
                    List<UserInfo> listUser = companyInfo.GetListUserInfoIsService(cache);

                    for (int i = 0; i < listUser.Count; i++)
                    {
                        if (listUser[i].Index == param.Index)
                        {
                            listUser[i].ListDevice = listDevice;

                            //update commands in cache
                            if (companyInfo != null)
                            {
                                CommandProcess.GetCommandsFromGeneralCacheForService(companyInfo, listUser[i]);
                            }
                        }
                    }
                }
                context.SaveChanges();

                result = Ok();
            }
            catch (Exception ex)
            {
                result = StatusCode((int)HttpStatusCode.InternalServerError, ex.ToString());
            }
            return result;
        }

        [Authorize]
        [ActionName("DeleteService")]
        [HttpPost]
        public IActionResult DeleteService([FromBody]List<ServiceParam> lsparam)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            foreach (var param in lsparam)
            {
                IC_Service deleteData = context.IC_Service.Where(t => t.CompanyIndex == user.CompanyIndex && t.Index == param.Index).FirstOrDefault();

                IC_ServiceAndDevices checkServiceHasDevice = context.IC_ServiceAndDevices.Where(t => t.CompanyIndex == user.CompanyIndex && t.ServiceIndex == param.Index).FirstOrDefault();

                if (deleteData == null)
                {
                    return NotFound("ServiceNotExist");
                }
                else if (checkServiceHasDevice != null)
                {
                    return NotFound("ServiceHasDevice");
                }
                else
                {
                    context.IC_Service.Remove(deleteData);
                }
            }
            context.SaveChanges();

            result = Ok();
            return result;
        }

        [Authorize]
        [ActionName("DownloadSettingService")]
        [HttpPost]
        public IActionResult DownloadSettingService([FromBody]ServiceParam param)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            string pathDirectory = "";
            IC_Service data = context.IC_Service.Where(t => t.Index == param.Index).FirstOrDefault();
            if (data == null)
            {
                return NotFound("ServiceNotExists");
            }
            if (data != null)
            {
                var JSON = JsonConvert.SerializeObject(ConfigObject.GetConfig(cache));
                var JObj = JObject.Parse(JSON);
                pathDirectory = JObj[data.ServiceType].ToString();
            }

            IC_Company company = context.IC_Company.Where(t => t.Index == user.CompanyIndex).FirstOrDefault();

            if (company != null)
            {
                XmlDocument doc = new XmlDocument();
                string sFileName = AppDomain.CurrentDomain.BaseDirectory + @"Files\" + pathDirectory;

                string[] filePaths = System.IO.Directory.GetFiles(sFileName, "*.config", System.IO.SearchOption.AllDirectories);

                doc = ReadXmlFile(filePaths[0]);

                if (doc != null)
                {
                    XmlNodeList node = doc.GetElementsByTagName("add");
                    foreach (XmlNode nodeItem in node)
                    {
                        if (nodeItem.Attributes["key"].Value == "username")
                        {
                            nodeItem.Attributes["value"].Value = company.TaxCode;
                        }
                        else if (nodeItem.Attributes["key"].Value == "password")
                        {
                            nodeItem.Attributes["value"].Value = StringHelper.Decrypt(company.Password, GlobalParams.__PASSWORD_SALT);
                        }
                        else if (nodeItem.Attributes["key"].Value == "serviceId")
                        {
                            nodeItem.Attributes["value"].Value = param.Index.ToString();
                        }
                        else if (nodeItem.Attributes["key"].Value == "Host")
                        {
                            nodeItem.Attributes["value"].Value = this.Request.Scheme + "://" + this.Request.Host.Value;
                        }
                        doc.Save(filePaths[0]);
                    }
                }
                //create powershell scripts to control the services
                EditPowershellFile(AppDomain.CurrentDomain.BaseDirectory, data.Index);
            }

            string sourceServicePath = AppDomain.CurrentDomain.BaseDirectory + @"Files\" + pathDirectory.Split('\\')[0];
            // tạo folder mới cho source service
            string sourceServiceTemp = AppDomain.CurrentDomain.BaseDirectory + @"Files\" + pathDirectory.Split('\\')[0] + "_" + data.Index;
            if (Directory.Exists(sourceServiceTemp)==false)
            {
                Directory.CreateDirectory(sourceServiceTemp);
            }
            CopyAllFilesInFolder(sourceServicePath, sourceServiceTemp);
            string zipPath = AppDomain.CurrentDomain.BaseDirectory + $"Files/{pathDirectory.Split('\\')[0]}_{data.Index}.zip";
            if (System.IO.File.Exists(zipPath))
            {
                System.IO.File.Delete(zipPath);
            }
            
            ZipFile.CreateFromDirectory(sourceServiceTemp, zipPath, CompressionLevel.Fastest, true);
            string url = this.Request.Scheme + "://" + this.Request.Host.Value + $"/Files/{pathDirectory.Split('\\')[0]}_{data.Index}.zip";

            result = Ok(url);
            return result;
        }
        
        private void CopyAllFilesInFolder(string pSourcePath,string pDestPath)
        {
            string[] files = System.IO.Directory.GetFiles(pSourcePath);
            foreach (var s in files)
            {
                string fileName = System.IO.Path.GetFileName(s);
                string destFile = System.IO.Path.Combine(pDestPath, fileName);
                System.IO.File.Copy(s, destFile, true);
            }
        }
        
        private void EditPowershellFile(string pSourcePath,int pServiceIndex)
        {
            CopyScriptTemplateToSDKSource(pSourcePath, pServiceIndex, "InstallService.ps1");
            CopyScriptTemplateToSDKSource(pSourcePath, pServiceIndex, "DeleteService.ps1");
            CopyScriptTemplateToSDKSource(pSourcePath, pServiceIndex, "StartService.ps1");
            CopyScriptTemplateToSDKSource(pSourcePath, pServiceIndex, "StopService.ps1");
        }
        
        private void CopyScriptTemplateToSDKSource(string pSourcePath, int pServiceIndex, string pScriptName)
        {
            string scriptFile = $"{pSourcePath}/Files/ScriptFile/{pScriptName}";
            string data = System.IO.File.ReadAllText(scriptFile);
            data = data.Replace("@ServiceName", "SDK_Interface_" + pServiceIndex);
            System.IO.File.WriteAllText($"{pSourcePath}/Files/SDK_Interface/{pScriptName}", data);
        }
        
        private static XmlDocument ReadXmlFile(string sPath)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(sPath);
                return doc;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public class ServiceParam
        {
            public int Index { get; set; }
            public string Name { get; set; }
            public string ServiceType { get; set; }
            public string Description { get; set; }
            public List<string> ListDeviceSerial { get; set; }
        }

        public class ServiceResult
        {
            public int Index { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public string ServiceType { get; set; }
            public List<DeviceBasic> ListDevice { get; set; }
        }

        public class DeviceBasic
        {
            public string SerialNumber { get; set; }
            public string AliasName { get; set; }
            public string IPAddress { get; set; }
            public int? Port { get; set; }
        }

        public class DeviceByService
        {
            public string SerialNumber { get; set; }
            public string AliasName { get; set; }
            public string IPAddress { get; set; }
            public string Port { get; set; }
            public bool Available { get; set; }
            public bool InService { get; set; }
            public int? DeviceModel { get; set; }
            public string ConnectionCode { get; set; }
        }
    }
}
