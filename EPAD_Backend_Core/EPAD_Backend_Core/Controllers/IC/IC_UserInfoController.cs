using ClosedXML.Excel;
using EPAD_Backend_Core.Base;
using EPAD_Backend_Core.WebUtilitys;
using EPAD_Common.Types;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using EPAD_Data.Models.Other;
using EPAD_Logic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace EPAD_Backend_Core.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("api/UserInfo/[action]")]
    [ApiController]
    public class IC_UserInfoController : ApiControllerBase
    {
        private EPAD_Context context;
        private ezHR_Context otherContext;
        private IMemoryCache cache;
        IConfiguration configuration;
        private readonly IHostingEnvironment _hostingEnvironment;
        private IIC_UserInfoLogic _iIC_UserInfoLogic;
        private IIC_UserMasterLogic _iIC_UserMasterLogic;
        private IIC_ServiceAndDeviceLogic _iC_ServiceAndDeviceLogic;
        private IIC_EmployeeLogic _IIC_EmployeeLogic;
        private IHR_EmployeeLogic _IHR_EmployeeLogic;
        private IIC_WorkingInfoLogic _IIC_WorkingInfoLogic;
        private IIC_CommandLogic _IIC_CommandLogic;
        public IC_UserInfoController(IServiceProvider provider):base(provider)
        {
            context = TryResolve<EPAD_Context>();
            cache = TryResolve<IMemoryCache>();
            configuration = TryResolve<IConfiguration>();
            otherContext = TryResolve<ezHR_Context>();
            _hostingEnvironment = TryResolve<IHostingEnvironment>();
            _iIC_UserInfoLogic = TryResolve<IIC_UserInfoLogic>();
            _iIC_UserMasterLogic = TryResolve<IIC_UserMasterLogic>();
            _iC_ServiceAndDeviceLogic = TryResolve<IIC_ServiceAndDeviceLogic>();
            _IIC_EmployeeLogic = TryResolve<IIC_EmployeeLogic>();
            _IHR_EmployeeLogic = TryResolve<IHR_EmployeeLogic>();
            _IIC_WorkingInfoLogic = TryResolve<IIC_WorkingInfoLogic>();
            _IIC_CommandLogic = TryResolve<IIC_CommandLogic>();
            _Logger = _LoggerFactory.CreateLogger<IC_UserInfoController>();
        }
        [Authorize]
        [ActionName("GetUserInfo")]
        [HttpGet]
        public IActionResult GetUserInfo()
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            result = Ok(user);
            return result;
        }

        // This Function using for PUSH Service call when add new or update employee info on device
        [Authorize]
        [ActionName("AddOrUpdateUserInfo")]
        [HttpPost]
        public async Task<IActionResult> AddOrUpdateUserInfo([FromBody] UserInfoPram Pram)
        {
            ConfigObject config = ConfigObject.GetConfig(cache);
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }


            if (Pram.ListUserInfo != null && Pram.ListUserInfo.Count > 0)
            {
                _Logger.LogInformation("AddOrUpdateUserInfo: " + Pram.ListUserInfo.Count);
                _iIC_UserInfoLogic.CheckCreateOrUpdate(Pram, user);
               
            }

            result = Ok();
            return result;
        }

        // This Function using for PUSH Service call when add new or update employee info on device
        [Authorize]
        [ActionName("AddOrUpdateUserInfoV2")]
        [HttpPost]
        public IActionResult AddOrUpdateUserInfoV2([FromBody] UserInfoParamV2 param)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }
            _Logger.LogInformation("AddOrUpdateUserInfo: " + param.ListUserInfo.Count);
            _iIC_UserInfoLogic.CheckCreateOrUpdateV2(param, user);
            //foreach (var item in param.ListUserInfo)
            //{
            //    var userfacev2 = context.IC_UserFaceTemplate_v2.Where(t => t.EmployeeATID.Equals(item.UserID) && t.CompanyIndex.Equals(user.CompanyIndex) && t.SerialNumber.Equals(param.SerialNumber)).FirstOrDefault();

            //    if (item.Face != null)
            //    {
            //        if (item.Face.TemplateBIODATA != null)
            //        {
            //            userfacev2.EmployeeATID = item.UserID;
            //            userfacev2.CompanyIndex = user.CompanyIndex;
            //            userfacev2.SerialNumber = param.SerialNumber;
            //            userfacev2.TemplateBIODATA = item.Face.TemplateBIODATA;
            //            userfacev2.UpdatedDate = DateTime.Now;
            //            userfacev2.UpdatedUser = user.UserName;
            //        }
            //        else
            //        {
            //            userfacev2.EmployeeATID = item.UserID;
            //            userfacev2.CompanyIndex = user.CompanyIndex;
            //            userfacev2.SerialNumber = param.SerialNumber;
            //            userfacev2.Size = item.Face.Size;
            //            userfacev2.Content = item.Face.Content;
            //            userfacev2.UpdatedDate = DateTime.Now;
            //            userfacev2.UpdatedUser = user.UserName;
            //        }
            //    }
            //}
            //context.SaveChanges();

            result = Ok();
            return result;
        }

        // TODO
        // if you want to using this api please retest
        [Authorize]
        [ActionName("UpdateUserPrivilege")]
        [HttpPost]
        public IActionResult UpdateUserPrivilege([FromBody] List<AddedParam> addedParam)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = null;
            if (user == null)
            {
                return Unauthorized("TokenExpired");
            }
            List<AddedParam> addedListParam = new List<AddedParam>();

            if (addedParam != null)
            {
                var paramServiceType = addedParam.FirstOrDefault(p => p.Key == "ServiceType");
                var paramPrivilege = addedParam.FirstOrDefault(p => p.Key == "Privilege");
                var paramEmployeeIDs = addedParam.FirstOrDefault(p => p.Key == "EmployeeATIDs");

                if (paramServiceType != null && paramServiceType.Value != null && paramPrivilege != null && paramPrivilege.Value != null && paramEmployeeIDs != null && paramEmployeeIDs.Value != null)
                {

                    // get all device of service type
                    addedListParam.Add(new AddedParam { Key = "ServiceType", Value = paramServiceType.Value });
                    var listServiceAndDevice = _iC_ServiceAndDeviceLogic.GetMany(addedListParam);

                    // get all device with list serial number of the service
                    addedListParam = new List<AddedParam>();
                    addedListParam.Add(new AddedParam { Key = "ListSerialNumber", Value = listServiceAndDevice.Select(u => u.SerialNumber).ToList() });
                    addedListParam.Add(new AddedParam { Key = "ListEmployeeATID", Value = paramEmployeeIDs });
                    var listUserInfo = _iIC_UserInfoLogic.GetMany(addedListParam);


                    var privilege = Convert.ToInt16(paramPrivilege.Value);
                    if (paramServiceType.Value.ToString() == GlobalParams.ServiceType.PUSHInterfaceService && privilege == GlobalParams.DevicePrivilege.PUSHAdminRole)
                    {
                        privilege = GlobalParams.DevicePrivilege.PUSHAdminRole; // 14 is admin role using for push service update to device
                    }

                    if (listUserInfo != null)
                    {
                        foreach (var userInfo in listUserInfo)
                        {
                            userInfo.Privilege = privilege;
                        }
                        _iIC_UserInfoLogic.UpdateListUserPrivilege(listUserInfo);
                        result = Ok();
                    }
                }
                else
                {
                    result = BadRequest("UpdateUserPrivilegeError");
                }
            }
            else
            {
                result = BadRequest("UpdateUserPrivilegeError");
            }
            return result;
        }
        [Authorize]
        [ActionName("DeleteUserInfo")]
        [HttpPost]
        public IActionResult DeleteUserInfo([FromBody] ListUserAndFilter param)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }
            string conn = configuration.GetConnectionString("connectionString");
            try
            {
                _iIC_UserInfoLogic.Delete(param.listDevice, user.CompanyIndex);
                result = Ok();
            }
            catch (Exception ex) { throw ex; }
            return result;
        }

        [Authorize]
        [ActionName("GetUserMachineInfo")]
        [HttpPost]
        public IActionResult GetUserMachineInfo([FromBody] List<AddedParam> addedParams)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }
            var listEmployeeATID = addedParams.FirstOrDefault(e => e.Key == "ListEmployeeATID")?.Value.ToString() ?? "";
            var listUser = JsonConvert.DeserializeObject<List<string>>(listEmployeeATID);
            var listDevice = JsonConvert.DeserializeObject<List<string>>(addedParams.FirstOrDefault(e => e.Key == "ListDevice").Value.ToString());
            var filter = addedParams.FirstOrDefault(e => e.Key == "Filter").Value.ToString();
            string conn = configuration.GetConnectionString("connectionString");
            if(listDevice != null && listDevice.Contains("SelectAll"))
            {
                listDevice = _DbContext.IC_Device.Where(x => x.CompanyIndex == user.CompanyIndex).Select(x => x.SerialNumber).ToList();
            }
            try
            {
                var listData = new List<IC_EmployeeDTO>();
                var config = ConfigObject.GetConfig(cache);
                if (config.IntegrateDBOther == true)
                {
                    
                    //List<string> listUser = (List<string>)addedParams.FirstOrDefault(e => e.Key == "ListEmployeeATID").Value;
                    //List<string> listDevice = (List<string>)addedParams.FirstOrDefault(e => e.Key == "ListDevice").Value;
                    //var filter = addedParams.FirstOrDefault(e => e.Key == "ListDevice").Value.ToString();
                    //var baseOnCompare = addedParams.FirstOrDefault(e => e.Key == "BaseOn").Value.ToString();
                    listData = _IHR_EmployeeLogic.GetUserMachineInfo(listUser, listDevice, filter, user); //GetUserInfo_IntegrateDB(param.listUser, config, user);
                    return Ok(listData);
                }

                addedParams = new List<AddedParam>();
                addedParams.Add(new AddedParam { Key = "ListEmployeeATID", Value = listUser });
                addedParams.Add(new AddedParam { Key = "ListDevice", Value = listDevice });
                addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = user.CompanyIndex });
                addedParams.Add(new AddedParam { Key = "Filter", Value = filter });

                listData = _iIC_UserInfoLogic.GetUserInfoMany(addedParams);
                //if (listData != null && listData.Count > 0)
                //{
                //    listData = _IIC_EmployeeLogic.CheckCurrentDepartment(listData);
                //}
                result = Ok(listData);
            }
            catch (Exception ex) { throw ex; }
            return result;
        }

        [Authorize]
        [ActionName("ExportToExcel")]
        [HttpPost]
        public IActionResult ExportToExcel([FromBody] List<AddedParam> addedParams)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }
            List<string> listUser = JsonConvert.DeserializeObject<List<string>>(addedParams.FirstOrDefault(e => e.Key == "ListEmployeeATID").Value.ToString());
            List<string> listDevice = JsonConvert.DeserializeObject<List<string>>(addedParams.FirstOrDefault(e => e.Key == "ListDevice").Value.ToString());
            var filter = addedParams.FirstOrDefault(e => e.Key == "Filter").Value.ToString();
            try
            {
                var listData = new List<IC_EmployeeDTO>();
                ConfigObject config = ConfigObject.GetConfig(cache);
                if (config.IntegrateDBOther == true)
                {
                    listData = _IHR_EmployeeLogic.GetUserMachineInfo(listUser, listDevice, filter, user); //GetUserInfo_IntegrateDB(param.listUser, config, user);
                    return Ok(listData);
                }

                addedParams = new List<AddedParam>
                {
                    new AddedParam { Key = "ListEmployeeATID", Value = listUser },
                    new AddedParam { Key = "ListDevice", Value = listDevice },
                    new AddedParam { Key = "CompanyIndex", Value = user.CompanyIndex },
                    new AddedParam { Key = "Filter", Value = filter }
                };

                listData = _iIC_UserInfoLogic.GetUserInfoMany(addedParams);

                string sWebRootFolder = _hostingEnvironment.ContentRootPath;
                string URL = string.Format("{0}://{1}/{2}", Request.Scheme, Request.Host, @"Files/UserOnDevice.xlsx");
                FileInfo file = new FileInfo(Path.Combine(sWebRootFolder, @"Files/UserOnDevice.xlsx"));

                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Userinfo");
                    var currentRow = 1;
                    worksheet.Cell(currentRow, 1).Value = "Mã chấm công (*)";
                    worksheet.Cell(currentRow, 2).Value = "Tên trên máy";
                    worksheet.Cell(currentRow, 3).Value = "Mã thẻ";
                    worksheet.Cell(currentRow, 4).Value = "Mật khẩu";
                    worksheet.Cell(currentRow, 5).Value = "Vân tay 1";
                    worksheet.Cell(currentRow, 6).Value = "Vân tay 2";
                    worksheet.Cell(currentRow, 7).Value = "Vân tay 3";
                    worksheet.Cell(currentRow, 8).Value = "Vân tay 4";
                    worksheet.Cell(currentRow, 9).Value = "Vân tay 5";
                    worksheet.Cell(currentRow, 10).Value = "Vân tay 6";
                    worksheet.Cell(currentRow, 11).Value = "Vân tay 7";
                    worksheet.Cell(currentRow, 12).Value = "Vân tay 8";
                    worksheet.Cell(currentRow, 13).Value = "Vân tay 9";
                    worksheet.Cell(currentRow, 14).Value = "Vân tay 10";
                    worksheet.Cell(currentRow, 15).Value = "Khuân mặt";

                    for (int i = 1; i < 16; i++)
                    {
                        worksheet.Cell(1, i).Style.Fill.BackgroundColor = XLColor.Yellow;
                        worksheet.Cell(1, i).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        worksheet.Column(i).Width = 20;
                    }

                    foreach (var users in listData)
                    {
                        currentRow++;
                        worksheet.Cell(currentRow, 1).Value = users.EmployeeATID;
                        worksheet.Cell(currentRow, 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        worksheet.Cell(currentRow, 1).Style.NumberFormat.Format = "0".PadLeft(users.EmployeeATID.Length, '0');

                        worksheet.Cell(currentRow, 2).Value = users.NameOnMachine;
                        worksheet.Cell(currentRow, 2).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                        worksheet.Cell(currentRow, 3).Value = users.CardNumber;
                        worksheet.Cell(currentRow, 3).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        if (!string.IsNullOrWhiteSpace(users.CardNumber))
                            worksheet.Cell(currentRow, 3).Style.NumberFormat.Format = "0".PadLeft(users.CardNumber.Length, '0');

                        worksheet.Cell(currentRow, 4).Value = users.Password;
                        worksheet.Cell(currentRow, 4).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        if (!string.IsNullOrWhiteSpace(users.Password))
                            worksheet.Cell(currentRow, 4).Style.NumberFormat.Format = "0".PadLeft(users.Password.Length, '0');

                        worksheet.Cell(currentRow, 5).Value = users.Finger1;
                        worksheet.Cell(currentRow, 5).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                        worksheet.Cell(currentRow, 6).Value = users.Finger2;
                        worksheet.Cell(currentRow, 6).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                        worksheet.Cell(currentRow, 7).Value = users.Finger3;
                        worksheet.Cell(currentRow, 7).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                        worksheet.Cell(currentRow, 8).Value = users.Finger4;
                        worksheet.Cell(currentRow, 8).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                        worksheet.Cell(currentRow, 9).Value = users.Finger5;
                        worksheet.Cell(currentRow, 9).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                        worksheet.Cell(currentRow, 10).Value = users.Finger6;
                        worksheet.Cell(currentRow, 10).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                        worksheet.Cell(currentRow, 11).Value = users.Finger7;
                        worksheet.Cell(currentRow, 11).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                        worksheet.Cell(currentRow, 12).Value = users.Finger8;
                        worksheet.Cell(currentRow, 12).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                        worksheet.Cell(currentRow, 13).Value = users.Finger9;
                        worksheet.Cell(currentRow, 13).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                        worksheet.Cell(currentRow, 14).Value = users.Finger10;
                        worksheet.Cell(currentRow, 14).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                        worksheet.Cell(currentRow, 15).Value = users.FaceTemplate;
                        worksheet.Cell(currentRow, 15).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    }

                    //var workbookBytes = new byte[0];
                    //using (var ms = new MemoryStream())
                    //{
                    //    workbook.SaveAs(ms);
                    //    return workbookBytes = ms.ToArray();
                    //}
                    workbook.SaveAs(file.FullName);
                    return Ok(URL);
                }

               
            }
            catch (Exception ex) { return NotFound("TemplateError"); }
        }

        private List<UserMachineInfo> GetUserInfo_IntegrateDB(List<string> pListEmployeeATID, ConfigObject config, UserInfo user)
        {
            List<HR_EmployeeReport> listEmployee = IC_EmployeeInfoController.GetEmployeeReportByEmps(otherContext, config, pListEmployeeATID);

            List<IC_UserInfo> listUser = context.IC_UserInfo.Where(t => t.CompanyIndex == user.CompanyIndex && pListEmployeeATID.Contains(t.EmployeeATID)).ToList();
            List<IC_UserFinger> listFinger = context.IC_UserFinger.Where(t => t.CompanyIndex == user.CompanyIndex && pListEmployeeATID.Contains(t.EmployeeATID)).ToList();
            List<IC_UserFaceTemplate> listFace = context.IC_UserFaceTemplate.Where(t => t.CompanyIndex == user.CompanyIndex && pListEmployeeATID.Contains(t.EmployeeATID)).ToList();

            List<UserMachineInfo> listData = new List<UserMachineInfo>();
            int index = 0;
            for (int i = 0; i < listEmployee.Count; i++)
            {
                UserMachineInfo data = null;
                List<IC_UserInfo> listUserByEmployeeATID = listUser.Where(t => t.EmployeeATID == listEmployee[i].EmployeeATID).ToList();
                if (listUserByEmployeeATID.Count > 0)
                {
                    foreach (IC_UserInfo item in listUserByEmployeeATID)
                    {
                        List<IC_UserFinger> listFingerByEmpAndSerial = listFinger.Where(t => t.EmployeeATID == listEmployee[i].EmployeeATID
                             && t.SerialNumber == item.SerialNumber).ToList();
                        List<IC_UserFaceTemplate> listFaceByEmpAndSerial = listFace.Where(t => t.EmployeeATID == listEmployee[i].EmployeeATID
                             && t.SerialNumber == item.SerialNumber).ToList();

                        data = new UserMachineInfo();
                        data.Index = index + 1;

                        CreateBasicInfo(ref data, listEmployee[i]);

                        data.SerialNumber = item.SerialNumber;
                        data.CardNumber = item.CardNumber;
                        data.Privilege = int.Parse(item.Privilege.ToString());
                        data.Password = item.Password;

                        CreateListFingerData(listFingerByEmpAndSerial, ref data);
                        data.FaceTemplate = listFaceByEmpAndSerial.Count == 0 ? 0 : listFaceByEmpAndSerial[i].FaceTemplate.Length;

                        listData.Add(data);
                        index++;
                    }
                }
                else
                {
                    data = new UserMachineInfo();
                    data.Index = index;

                    CreateBasicInfo(ref data, listEmployee[i]);
                    listData.Add(data);
                    index++;
                }

            }
            return listData;
        }
        private void CreateBasicInfo(ref UserMachineInfo data, HR_EmployeeReport pEmployee)
        {
            data.EmployeeATID = pEmployee.EmployeeATID;
            data.EmployeeCode = pEmployee.EmployeeCode;
            data.FullName = pEmployee.FullName;
            data.DepartmentName = pEmployee.DepartmentName;
        }
        private void CreateListFingerData(List<IC_UserFinger> listFingerByEmp, ref UserMachineInfo data)
        {
            for (int i = 0; i < listFingerByEmp.Count; i++)
            {
                switch (listFingerByEmp[i].FingerIndex)
                {
                    case 0:
                        data.Finger1 = listFingerByEmp[i].FingerData.Length;
                        break;
                    case 1:
                        data.Finger2 = listFingerByEmp[i].FingerData.Length;
                        break;
                    case 2:
                        data.Finger3 = listFingerByEmp[i].FingerData.Length;
                        break;
                    case 3:
                        data.Finger4 = listFingerByEmp[i].FingerData.Length;
                        break;
                    case 4:
                        data.Finger5 = listFingerByEmp[i].FingerData.Length;
                        break;
                    case 5:
                        data.Finger6 = listFingerByEmp[i].FingerData.Length;
                        break;
                    case 6:
                        data.Finger7 = listFingerByEmp[i].FingerData.Length;
                        break;
                    case 7:
                        data.Finger8 = listFingerByEmp[i].FingerData.Length;
                        break;
                    case 8:
                        data.Finger9 = listFingerByEmp[i].FingerData.Length;
                        break;
                    case 9:
                        data.Finger10 = listFingerByEmp[i].FingerData.Length;
                        break;
                }

            }
        }

    }
}
