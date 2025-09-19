using EPAD_Backend_Core.Base;
using EPAD_Backend_Core.WebUtilitys;
using EPAD_Common.Types;
using EPAD_Common.Utility;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using EPAD_Data.Models.Other;
using EPAD_Logic;
using EPAD_Services.Impl;
using EPAD_Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IHR_EmployeeInfoService = EPAD_Services.Interface.IHR_EmployeeInfoService;

namespace EPAD_Backend_Core.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("api/UserMaster/[action]")]
    [ApiController]
    public class IC_UserMasterController : ApiControllerBase
    {
        private EPAD_Context context;
        private IMemoryCache cache;
        IConfiguration configuration;
        private IIC_ServiceAndDeviceLogic _iC_ServiceAndDeviceLogic;
        private IIC_EmployeeLogic _IIC_EmployeeLogic;
        private IIC_EmployeeTransferLogic _iC_EmployeeTransferLogic;
        private IHR_EmployeeLogic _IHR_EmployeeLogic;
        private IIC_WorkingInfoLogic _IIC_WorkingInfoLogic;
        private IIC_CommandLogic _IIC_CommandLogic;
        private IIC_UserMasterLogic _IIC_UserMasterLogic;
        private IIC_ConfigLogic _iC_ConfigLogic;
        private IHR_UserService _iHR_UserService;
        private IHR_EmployeeInfoService _iHR_EmployeeInfoService;
        private IHR_CardNumberInfoService _iHR_CardNumberInfoService;
        private IIC_UserMasterService _IIC_UserMasterService;
        private IIC_UserAuditService _IIC_UserAuditService;
        public IC_UserMasterController(IServiceProvider provider) : base(provider)
        {
            context = TryResolve<EPAD_Context>();
            cache = TryResolve<IMemoryCache>();
            _Logger = _LoggerFactory.CreateLogger<IC_UserMasterController>();
            configuration = TryResolve<IConfiguration>();
            _iC_ServiceAndDeviceLogic = TryResolve<IIC_ServiceAndDeviceLogic>();
            _IIC_EmployeeLogic = TryResolve<IIC_EmployeeLogic>();
            _IHR_EmployeeLogic = TryResolve<IHR_EmployeeLogic>();
            _IIC_WorkingInfoLogic = TryResolve<IIC_WorkingInfoLogic>();
            _IIC_CommandLogic = TryResolve<IIC_CommandLogic>();
            _IIC_UserMasterLogic = TryResolve<IIC_UserMasterLogic>();
            _iC_EmployeeTransferLogic = TryResolve<IIC_EmployeeTransferLogic>();
            _iC_ConfigLogic = TryResolve<IIC_ConfigLogic>();
            _iHR_UserService = TryResolve<IHR_UserService>();
            _iHR_EmployeeInfoService = TryResolve<IHR_EmployeeInfoService>();
            _iHR_CardNumberInfoService = TryResolve<IHR_CardNumberInfoService>();
            _IIC_UserMasterService = TryResolve<IIC_UserMasterService>();
            _IIC_UserAuditService = TryResolve<IIC_UserAuditService>();
        }

        // This Function using for PUSH Service call when add new or update employee info on device
        [Authorize]
        [ActionName("AddOrUpdateUserMaster")]
        [HttpPost]
        public async Task<IActionResult> AddOrUpdate([FromBody] UserInfoPram Pram)
        {
            
            ConfigObject config = ConfigObject.GetConfig(cache);
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }
            List<AddedParam> addedParams1 = new List<AddedParam>();
            addedParams1.Add(new AddedParam { Key = "CompanyIndex", Value = user.CompanyIndex });
            addedParams1.Add(new AddedParam { Key = "SerialNumber", Value = Pram.SerialNumber });
            var serviceAndDevice = _iC_ServiceAndDeviceLogic.GetMany(addedParams1);
            if (serviceAndDevice != null)
            {
                var service = serviceAndDevice.FirstOrDefault();
                if (service != null)
                {
                    if (service.ServiceType != GlobalParams.ServiceType.PUSHInterfaceService)
                    {
                        return Ok();
                    }
                }
                else { return Ok(); }
            }
            else { return Ok(); }
            

            if (Pram.ListUserInfo != null && Pram.ListUserInfo.Count > 0)
            {
                List<IC_UserMasterDTO> listUserMaster = new List<IC_UserMasterDTO>();
                List<IC_EmployeeDTO> listEmployee = new List<IC_EmployeeDTO>();
                List<IC_WorkingInfoDTO> listWorkingInfo = new List<IC_WorkingInfoDTO>();
                List<HR_User> listHRUser = new List<HR_User>();
                List<HR_EmployeeInfo> listHREmpInfo = new List<HR_EmployeeInfo>();
                List<HR_CardNumberInfo> listCardInfo = new List<HR_CardNumberInfo>();

                foreach (var item in Pram.ListUserInfo)
                {
                    // user info
                    //item
                    HR_User hrUser = new HR_User();
                    hrUser.EmployeeATID = item.UserID.PadLeft(config.MaxLenghtEmployeeATID, '0');
                    hrUser.CompanyIndex = user.CompanyIndex;
                    hrUser.CreatedDate = DateTime.Now;
                    hrUser.UpdatedUser = user.UserName;
                    listHRUser.Add(hrUser);

                    //hr employee info
                    HR_EmployeeInfo hrEmpInfo = new HR_EmployeeInfo();
                    hrEmpInfo.EmployeeATID = hrUser.EmployeeATID;
                    hrEmpInfo.CompanyIndex = hrUser.CompanyIndex;
                    hrEmpInfo.JoinedDate = hrUser.CreatedDate;
                    hrEmpInfo.UpdatedDate = hrUser.CreatedDate;
                    hrEmpInfo.UpdatedUser = hrUser.UpdatedUser;
                    listHREmpInfo.Add(hrEmpInfo);

                    // card info
                    HR_CardNumberInfo cardInfo = new HR_CardNumberInfo();
                    cardInfo.EmployeeATID = hrUser.EmployeeATID;
                    cardInfo.CompanyIndex = hrEmpInfo.CompanyIndex;
                    cardInfo.CardNumber = item.CardNumber;
                    cardInfo.IsActive = true;
                    cardInfo.CreatedDate = DateTime.Now;
                    cardInfo.UpdatedDate = hrUser.UpdatedDate;
                    cardInfo.UpdatedUser = hrUser.UpdatedUser;

                    // user master
                    IC_UserMasterDTO userMaster = new IC_UserMasterDTO();
                    userMaster.EmployeeATID = hrUser.EmployeeATID;
                    userMaster.CompanyIndex = hrUser.CompanyIndex;
                    userMaster.CardNumber = item.CardNumber;
                    userMaster.Password = item.PasswordOndevice;
                    userMaster.NameOnMachine = item.NameOnDevice;
                    if (item.FingerPrints.Count == 0 && item.Face == null && item.FaceInfoV2 == null)
                    {
                        userMaster.Privilege = (short)item.Privilege;
                    }

                    // finger info
                    userMaster.FingerData0 = _IIC_UserMasterLogic.GetFingerData(item.FingerPrints, 0);
                    userMaster.FingerData1 = _IIC_UserMasterLogic.GetFingerData(item.FingerPrints, 1);
                    userMaster.FingerData2 = _IIC_UserMasterLogic.GetFingerData(item.FingerPrints, 2);
                    userMaster.FingerData3 = _IIC_UserMasterLogic.GetFingerData(item.FingerPrints, 3);
                    userMaster.FingerData4 = _IIC_UserMasterLogic.GetFingerData(item.FingerPrints, 4);
                    userMaster.FingerData5 = _IIC_UserMasterLogic.GetFingerData(item.FingerPrints, 5);
                    userMaster.FingerData6 = _IIC_UserMasterLogic.GetFingerData(item.FingerPrints, 6);
                    userMaster.FingerData7 = _IIC_UserMasterLogic.GetFingerData(item.FingerPrints, 7);
                    userMaster.FingerData8 = _IIC_UserMasterLogic.GetFingerData(item.FingerPrints, 8);
                    userMaster.FingerData9 = _IIC_UserMasterLogic.GetFingerData(item.FingerPrints, 9);

                    //face info
                    userMaster.FaceIndex = 50;
                    if (item.Face != null)
                    {
                        userMaster.FaceIndex = 50;
                        userMaster.FaceTemplate = item.Face.FaceTemplate;
                    }

                    // face 2 info
                    if (item.FaceInfoV2 != null)
                    {
                        userMaster.FaceV2_Index = item.FaceInfoV2.Index;
                        userMaster.FaceV2_No = item.FaceInfoV2.No;
                        userMaster.FaceV2_Duress = item.FaceInfoV2.Duress;
                        userMaster.FaceV2_Format = item.FaceInfoV2.Format;
                        userMaster.FaceV2_MajorVer = item.FaceInfoV2.MajorVer;
                        userMaster.FaceV2_MinorVer = item.FaceInfoV2.MinorVer;
                        userMaster.FaceV2_Type = item.FaceInfoV2.Type;
                        userMaster.FaceV2_Valid = item.FaceInfoV2.Valid;
                        userMaster.FaceV2_TemplateBIODATA = item.FaceInfoV2.TemplateBIODATA;
                        userMaster.FaceV2_Content = item.FaceInfoV2.Content;
                    }

                    listUserMaster.Add(userMaster);

                    IC_WorkingInfoDTO workingInfo = new IC_WorkingInfoDTO();
                    workingInfo.EmployeeATID = hrUser.EmployeeATID;
                    workingInfo.CompanyIndex = hrUser.CompanyIndex;
                    workingInfo.IsManager = false;
                    workingInfo.IsSync = null;
                    workingInfo.DepartmentIndex = 0;
                    workingInfo.Status = (short)TransferStatus.Approve;
                    workingInfo.UpdatedDate = hrUser.CreatedDate;
                    workingInfo.UpdatedUser = hrUser.UpdatedUser;
                    workingInfo.ApprovedDate = hrUser.CreatedDate;
                    workingInfo.ApprovedUser = hrUser.UpdatedUser;
                    listWorkingInfo.Add(workingInfo);
                    // }
                }

                if (Pram.IsOverwriteData)
                {
                    await _IIC_UserMasterLogic.SaveAndOverwriteList(listUserMaster);
                }
                else
                {
                    await _IIC_UserMasterLogic.SaveAndAddMoreList(listUserMaster);
                }
                var res = await _iHR_UserService.CheckExistedOrCreateList(listHRUser, user.CompanyIndex);
                await _iHR_EmployeeInfoService.CheckExistedOrCreateList(listHREmpInfo, user.CompanyIndex);
                await _iHR_CardNumberInfoService.CheckCardActivedOrCreateList(listCardInfo, user.CompanyIndex);
                await _IIC_WorkingInfoLogic.CheckExistedOrCreateList(listWorkingInfo);
                await SaveChangeAsync();

                if(res.Count > 0)
                {
                    await _IHR_EmployeeLogic.IntegrateUserToOfflineEmployee(res);
                    await _IIC_UserAuditService.InsertAudit(res);
                }
            

                var listToBeSync = listUserMaster.Where(e => !string.IsNullOrWhiteSpace(e.FaceTemplate)
                || !string.IsNullOrWhiteSpace(e.FingerData0)
                || !string.IsNullOrWhiteSpace(e.FingerData1)
                || !string.IsNullOrWhiteSpace(e.FingerData2)
                || !string.IsNullOrWhiteSpace(e.FingerData3)
                || !string.IsNullOrWhiteSpace(e.FingerData4)
                || !string.IsNullOrWhiteSpace(e.FingerData5)
                || !string.IsNullOrWhiteSpace(e.FingerData6)
                || !string.IsNullOrWhiteSpace(e.FingerData7)
                || !string.IsNullOrWhiteSpace(e.FingerData8)
                || !string.IsNullOrWhiteSpace(e.FingerData9)
                || !string.IsNullOrWhiteSpace(e.FaceV2_Content)
                ).ToList();
                if (listToBeSync != null && listToBeSync.Count > 0)
                {
                    List<AddedParam> addedParams = new List<AddedParam>();
                    addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = user.CompanyIndex });
                    addedParams.Add(new AddedParam { Key = "EventType", Value = ConfigAuto.ADD_OR_DELETE_USER.ToString() });
                    var systemconfigs = await _iC_ConfigLogic.GetMany(addedParams);
                    if (systemconfigs != null)
                    {
                        var sysconfig = systemconfigs.FirstOrDefault();
                        if (sysconfig != null)
                        {
                            if (sysconfig.IntegrateLogParam.AutoIntegrate)
                            {
                                await _IIC_CommandLogic.SyncWithEmployee(listToBeSync.Select(e => e.EmployeeATID).ToList(), user.CompanyIndex, Pram.SerialNumber);
                            }
                        }
                    }
                }
            }

            result = Ok();
            return result;

        }
        // This Function using for PUSH Service call when add new or update employee info on device
        [Authorize]
        [ActionName("DownloadUserMasterPUSH")]
        [HttpPost]
        public async Task<IActionResult> DownloadUserMasterPUSH([FromBody] UserInfoPram Pram)
        {
            ConfigObject config = ConfigObject.GetConfig(cache);
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }
            var companyIndex = config.IntegrateDBOther == true ? config.CompanyIndex : user.CompanyIndex;
            var res = new List<string>();
            if (Pram.ListUserInfo != null && Pram.ListUserInfo.Count > 0)
            {
                List<IC_UserMasterDTO> listUserMaster = new List<IC_UserMasterDTO>();
                List<IC_EmployeeDTO> listEmployee = new List<IC_EmployeeDTO>();
                List<IC_WorkingInfoDTO> listWorkingInfo = new List<IC_WorkingInfoDTO>();
                List<HR_User> listHRUser = new List<HR_User>();
                List<HR_EmployeeInfo> listHREmpInfo = new List<HR_EmployeeInfo>();
                List<HR_CardNumberInfo> listCardInfo = new List<HR_CardNumberInfo>();

              
                foreach (var item in Pram.ListUserInfo)
                {
                    // user info
                    HR_User hrUser = new HR_User();
                    hrUser.EmployeeATID = item.UserID.PadLeft(config.MaxLenghtEmployeeATID, '0');
                    hrUser.CompanyIndex = user.CompanyIndex;
                    hrUser.CreatedDate = DateTime.Now;
                    hrUser.UpdatedUser = user.UserName;
                    //hrUser.EmployeeType = (int)EmployeeType.Employee;
                    listHRUser.Add(hrUser);

                    // hr employee info
                    HR_EmployeeInfo hrEmpInfo = new HR_EmployeeInfo();
                    hrEmpInfo.EmployeeATID = hrUser.EmployeeATID;
                    hrEmpInfo.CompanyIndex = hrUser.CompanyIndex;
                    hrEmpInfo.JoinedDate = hrUser.CreatedDate;
                    hrEmpInfo.UpdatedDate = hrUser.UpdatedDate;
                    hrEmpInfo.UpdatedUser = hrUser.UpdatedUser;
                    listHREmpInfo.Add(hrEmpInfo);

                    // hr card info
                    HR_CardNumberInfo cardInfo = new HR_CardNumberInfo();
                    cardInfo.EmployeeATID = hrUser.EmployeeATID;
                    cardInfo.CompanyIndex = hrUser.CompanyIndex;
                    cardInfo.CardNumber = item.CardNumber;
                    cardInfo.IsActive = true;
                    cardInfo.CreatedDate = DateTime.Now;
                    cardInfo.UpdatedDate = hrUser.UpdatedDate;
                    cardInfo.UpdatedUser = hrUser.UpdatedUser;
                    listCardInfo.Add(cardInfo);


                    // user master 
                    IC_UserMasterDTO userMaster = new IC_UserMasterDTO();
                    userMaster.EmployeeATID = hrUser.EmployeeATID;
                    userMaster.CompanyIndex = hrUser.CompanyIndex;
                    userMaster.CardNumber = item.CardNumber;
                    userMaster.Password = item.PasswordOndevice;
                    userMaster.NameOnMachine = item.NameOnDevice;
                    // finger info
                    userMaster.FingerData0 = _IIC_UserMasterLogic.GetFingerData(item.FingerPrints, 0);
                    userMaster.FingerData1 = _IIC_UserMasterLogic.GetFingerData(item.FingerPrints, 1);
                    userMaster.FingerData2 = _IIC_UserMasterLogic.GetFingerData(item.FingerPrints, 2);
                    userMaster.FingerData3 = _IIC_UserMasterLogic.GetFingerData(item.FingerPrints, 3);
                    userMaster.FingerData4 = _IIC_UserMasterLogic.GetFingerData(item.FingerPrints, 4);
                    userMaster.FingerData5 = _IIC_UserMasterLogic.GetFingerData(item.FingerPrints, 5);
                    userMaster.FingerData6 = _IIC_UserMasterLogic.GetFingerData(item.FingerPrints, 6);
                    userMaster.FingerData7 = _IIC_UserMasterLogic.GetFingerData(item.FingerPrints, 7);
                    userMaster.FingerData8 = _IIC_UserMasterLogic.GetFingerData(item.FingerPrints, 8);
                    userMaster.FingerData9 = _IIC_UserMasterLogic.GetFingerData(item.FingerPrints, 9);
                    if((item.FingerPrints == null || item.FingerPrints.Count == 0) && item.Face == null && item.FaceInfoV2 == null)
                    {
                        userMaster.Privilege = (short)item.Privilege;
                    }

                    //face info
                    userMaster.FaceIndex = 50;
                    if (item.Face != null)
                    {
                        userMaster.FaceIndex = 50;
                        userMaster.FaceTemplate = item.Face.FaceTemplate;
                    }

                    // face 2 info
                    if (item.FaceInfoV2 != null)
                    {
                        userMaster.FaceV2_Index = item.FaceInfoV2.Index;
                        userMaster.FaceV2_No = item.FaceInfoV2.No;
                        userMaster.FaceV2_Duress = item.FaceInfoV2.Duress;
                        userMaster.FaceV2_Format = item.FaceInfoV2.Format;
                        userMaster.FaceV2_MajorVer = item.FaceInfoV2.MajorVer;
                        userMaster.FaceV2_MinorVer = item.FaceInfoV2.MinorVer;
                        userMaster.FaceV2_Type = item.FaceInfoV2.Type;
                        userMaster.FaceV2_Valid = item.FaceInfoV2.Valid;
                        userMaster.FaceV2_TemplateBIODATA = item.FaceInfoV2.TemplateBIODATA;
                        userMaster.FaceV2_Content = item.FaceInfoV2.Content;
                    }

                    listUserMaster.Add(userMaster);
                    //if (!string.IsNullOrWhiteSpace(item.NameOnDevice) || !string.IsNullOrWhiteSpace(item.CardNumber) || !string.IsNullOrWhiteSpace(item))
                    //{
                    //IC_EmployeeDTO employee = new IC_EmployeeDTO();
                    //employee.EmployeeATID = userMaster.EmployeeATID.PadLeft(config.MaxLenghtEmployeeATID, '0');
                    //employee.CompanyIndex = userMaster.CompanyIndex;
                    //employee.CardNumber = userMaster.CardNumber;
                    //employee.Privilege = userMaster.Privilege;
                    //employee.NameOnMachine = userMaster.NameOnMachine;
                    //employee.Gender = 0;
                    //employee.JoinedDate = DateTime.Now;
                    //employee.CreatedDate = DateTime.Now;
                    //employee.UpdatedUser = userMaster.UpdatedUser;
                    //employee.UpdatedDate = DateTime.Now;
                    //employee.DepartmentIndex = 0;
                    //listEmployee.Add(employee);


                    IC_WorkingInfoDTO workingInfo = new IC_WorkingInfoDTO();
                    workingInfo.EmployeeATID = hrUser.EmployeeATID;
                    workingInfo.CompanyIndex = hrUser.CompanyIndex;
                    workingInfo.IsManager = false;
                    workingInfo.IsSync = true;
                    workingInfo.DepartmentIndex = 0;
                    workingInfo.Status = (short)TransferStatus.Approve;
                    workingInfo.UpdatedDate = hrUser.UpdatedDate;
                    workingInfo.UpdatedUser = hrUser.UpdatedUser;
                    workingInfo.ApprovedDate = hrUser.CreatedDate;
                    workingInfo.ApprovedUser = hrUser.UpdatedUser;
                    listWorkingInfo.Add(workingInfo);
                    // }
                }

                if (Pram.IsOverwriteData)
                {
                    await _IIC_UserMasterLogic.SaveAndOverwriteList(listUserMaster);
                }
                else
                {
                    await _IIC_UserMasterLogic.SaveAndAddMoreList(listUserMaster);
                }
                 res = await _iHR_UserService.CheckExistedOrCreateList(listHRUser, user.CompanyIndex);
                await _iHR_EmployeeInfoService.CheckExistedOrCreateList(listHREmpInfo, user.CompanyIndex);
                await _iHR_CardNumberInfoService.CheckCardActivedOrCreateList(listCardInfo, user.CompanyIndex, Pram.IsOverwriteData);
                await _IIC_WorkingInfoLogic.CheckExistedOrCreateList(listWorkingInfo);
            }
            await SaveChangeAsync();
            if(res.Count > 0)
            {
                await _IHR_EmployeeLogic.IntegrateUserToOfflineEmployee(res);
                await _IIC_UserAuditService.InsertAudit(res);
            }
            result = Ok();
            return result;

        }



        // This Function using for SDK Service call when add new or update employee info on device
        [Authorize]
        [ActionName("DownloadUserMasterSDK")]
        [HttpPost]
        public async Task<IActionResult> DownloadUserMasterSDK([FromBody] UserInfoPram Pram)
        {
            ConfigObject config = ConfigObject.GetConfig(cache);
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            await _IIC_UserMasterService.DownloadUserMasterSDK(Pram, user, config);
            await SaveChangeAsync();
            result = Ok();
            return result;

        }

        // This Function using for PUSH Service call when add new or update employee info on device
        [Authorize]
        [ActionName("AddOrUpdateUserMasterV2")]
        [HttpPost]
        public async Task<IActionResult> AddOrUpdateUserMasterV2([FromBody] UserInfoParamV2 param)
        {
            ConfigObject config = ConfigObject.GetConfig(cache);
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            var companyIndex = config.IntegrateDBOther == true ? config.CompanyIndex : user.CompanyIndex;

            List<IC_UserMasterDTO> listUserMaster = new List<IC_UserMasterDTO>();

            foreach (var item in param.ListUserInfo)
            {

                if (item.Face != null)
                {
                    var userfacev2 = new IC_UserMasterDTO();
                    if (item.Face != null)
                    {
                        userfacev2.EmployeeATID = item.UserID.PadLeft(config.MaxLenghtEmployeeATID, '0');
                        userfacev2.CompanyIndex = user.CompanyIndex;
                        userfacev2.FaceV2_TemplateBIODATA = item.Face.TemplateBIODATA;
                        userfacev2.UpdatedDate = DateTime.Now;
                        userfacev2.UpdatedUser = user.UserName;
                        //userfacev2.FaceV2_MinorVer = item.Face.MajorVer;
                        //userfacev2.FaceV2_MajorVer = item.Face.MajorVer;
                        //userfacev2.FaceV2_Duress = item.Face.Duress;
                        //userfacev2.FaceV2_Index = item.Face.Index;
                        //}
                        //else
                        //{
                        //userfacev2.EmployeeATID = item.UserID.PadLeft(config.MaxLenghtEmployeeATID, '0');
                        //userfacev2.CompanyIndex = user.CompanyIndex;
                        userfacev2.FaceV2_Type = item.Face.Type;
                        userfacev2.FaceV2_Size = item.Face.Size;
                        userfacev2.FaceV2_Content = item.Face.Content;
                        userfacev2.UpdatedDate = DateTime.Now;
                        userfacev2.UpdatedUser = user.UserName;
                    }
                    listUserMaster.Add(userfacev2);
                }

            }

            if (param.IsOverwriteData)
            {
                await _IIC_UserMasterLogic.SaveAndOverwriteList(listUserMaster);
            }
            else
            {
                await _IIC_UserMasterLogic.SaveAndAddMoreList(listUserMaster);
                List<AddedParam> addedParams = new List<AddedParam>();
                addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = user.CompanyIndex });
                addedParams.Add(new AddedParam { Key = "EventType", Value = ConfigAuto.ADD_OR_DELETE_USER.ToString() });
                var systemconfigs = await _iC_ConfigLogic.GetMany(addedParams);
                if (systemconfigs != null)
                {
                    var sysconfig = systemconfigs.FirstOrDefault();
                    if (sysconfig != null)
                    {
                        if (sysconfig.IntegrateLogParam.AutoIntegrate)
                        {
                            await _IIC_CommandLogic.SyncWithEmployee(listUserMaster.Select(e => e.EmployeeATID).ToList(), user.CompanyIndex, null);
                        }
                    }
                }

            }


            result = Ok();
            return result;
        }
        [Authorize]
        [ActionName("DownloadUserMasterV2")]
        [HttpPost]
        public async Task<IActionResult> DownloadUserMasterV2([FromBody] UserInfoParamV2 param)
        {
            ConfigObject config = ConfigObject.GetConfig(cache);
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            var companyIndex = config.IntegrateDBOther == true ? config.CompanyIndex : user.CompanyIndex;

            List<IC_UserMasterDTO> listUserMaster = new List<IC_UserMasterDTO>();

            foreach (var item in param.ListUserInfo)
            {

                if (item.Face != null)
                {
                    var userfacev2 = new IC_UserMasterDTO();
                    if (item.Face != null)
                    {
                        userfacev2.EmployeeATID = item.UserID.PadLeft(config.MaxLenghtEmployeeATID, '0');
                        userfacev2.CompanyIndex = user.CompanyIndex;
                        userfacev2.FaceV2_TemplateBIODATA = item.Face.TemplateBIODATA;
                        userfacev2.UpdatedDate = DateTime.Now;
                        userfacev2.UpdatedUser = user.UserName;
                        //userfacev2.FaceV2_MinorVer = item.Face.MajorVer;
                        //userfacev2.FaceV2_MajorVer = item.Face.MajorVer;
                        //userfacev2.FaceV2_Duress = item.Face.Duress;
                        //userfacev2.FaceV2_Index = item.Face.Index;
                        //}
                        //else
                        //{
                        //userfacev2.EmployeeATID = item.UserID.PadLeft(config.MaxLenghtEmployeeATID, '0');
                        //userfacev2.CompanyIndex = user.CompanyIndex;
                        userfacev2.FaceV2_Type = item.Face.Type;
                        userfacev2.FaceV2_Size = item.Face.Size;
                        userfacev2.FaceV2_Content = item.Face.Content;
                        userfacev2.UpdatedDate = DateTime.Now;
                        userfacev2.UpdatedUser = user.UserName;
                    }
                    listUserMaster.Add(userfacev2);
                }

            }

            if (param.IsOverwriteData)
            {
                await _IIC_UserMasterLogic.SaveAndOverwriteList(listUserMaster);
            }
            else
            {
                await _IIC_UserMasterLogic.SaveAndAddMoreList(listUserMaster);
            }


            result = Ok();
            return result;
        }
        [Authorize]
        [ActionName("GetUserMachineInfo")]
        [HttpPost]
        public IActionResult GetUserMachineInfo([FromBody] ListUserAndFilter param)
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
                ConfigObject config = ConfigObject.GetConfig(cache);
                List<IC_EmployeeDTO> listData = new List<IC_EmployeeDTO>();
                if (config.IntegrateDBOther == true)
                {
                    //listData = _IHR_EmployeeLogic.GetUserMasterMachineInfo(param.listUser, param.filter, user); //GetUserInfo_IntegrateDB(param.listUser, config, user);
                    //return Ok(listData);

                    List<AddedParam> addedParamss = new List<AddedParam>();
                    addedParamss.Add(new AddedParam { Key = "Filter", Value = param.filter });
                    addedParamss.Add(new AddedParam { Key = "CompanyIndex", Value = config.CompanyIndex });
                    addedParamss.Add(new AddedParam { Key = "ListDepartment", Value = user.ListDepartmentAssigned });
                    addedParamss.Add(new AddedParam { Key = "ListEmployeeATID", Value = param.listUser });
                    addedParamss.Add(new AddedParam { Key = "IsCurrentWorking", Value = true });

                    listData = _IHR_EmployeeLogic.GetMany(addedParamss);
                    return Ok(listData);
                }

                List<AddedParam> addedParams = new List<AddedParam>();
                addedParams.Add(new AddedParam { Key = "IsCurrentTransfer", Value = true });
                addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = user.CompanyIndex });
                addedParams.Add(new AddedParam { Key = "ListDepartment", Value = user.ListDepartmentAssigned });

                addedParams = new List<AddedParam>();
                List<IC_EmployeeTransferDTO> listEmTransfer = _iC_EmployeeTransferLogic.GetMany(addedParams);
                if (listEmTransfer != null && listEmTransfer.Count > 0)
                {
                    addedParams.Add(new AddedParam { Key = "ListEmployeeTransferATID", Value = listEmTransfer.Select(e => e.EmployeeATID).ToList() });
                }

                addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = user.CompanyIndex });
                addedParams.Add(new AddedParam { Key = "ListEmployeeATID", Value = param.listUser });
                addedParams.Add(new AddedParam { Key = "ListDepartment", Value = user.ListDepartmentAssigned });
                addedParams.Add(new AddedParam { Key = "Filter", Value = param.filter });
                addedParams.Add(new AddedParam { Key = "IsCurrentWorkingAndNoDepartment", Value = true });
                listData = _IIC_UserMasterLogic.GetUserMasterInfoMany(addedParams);
                listData = _IIC_EmployeeLogic.CheckCurrentDepartment(listData);

                result = Ok(listData);

            }
            catch (Exception ex) { throw ex; }
            return result;
        }

        [Authorize]
        [ActionName("GetAllUserMachineInfo")]
        [HttpPost]
        public IActionResult GetAllUserMachineInfo([FromBody] ListDepartmentAndFilter filter)
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
                ConfigObject config = ConfigObject.GetConfig(cache);
                List<IC_EmployeeDTO> listData = new List<IC_EmployeeDTO>();
                List<AddedParam> addedParams = new List<AddedParam>();
                if (!filter.departmentIndexs.Any())
                    filter.departmentIndexs = user.ListDepartmentAssigned;

                if (config.IntegrateDBOther == true)
                {
                    addedParams.Add(new AddedParam { Key = "Filter", Value = filter.filter });
                    addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = config.CompanyIndex });
                    addedParams.Add(new AddedParam { Key = "ListDepartment", Value = filter.departmentIndexs });
                    addedParams.Add(new AddedParam { Key = "IsCurrentWorking", Value = true });
                    addedParams.Add(new AddedParam { Key = "IsWorking", Value = filter.ListWorkingStatus });

                    listData = _IHR_EmployeeLogic.GetMany(addedParams);
                    return Ok(listData);
                }
                else
                {
                    addedParams.Add(new AddedParam { Key = "IsCurrentTransfer", Value = true });
                    addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = user.CompanyIndex });
                    addedParams.Add(new AddedParam { Key = "ListDepartment", Value = filter.departmentIndexs });
                    addedParams.Add(new AddedParam { Key = "UserType", Value = (int)EmployeeType.Employee });

                    List<IC_EmployeeTransferDTO> listEmTransfer = _iC_EmployeeTransferLogic.GetMany(addedParams);
                    addedParams = new List<AddedParam>();
                    if (listEmTransfer != null && listEmTransfer.Count > 0)
                    {
                        addedParams.Add(new AddedParam { Key = "ListEmployeeTransferATID", Value = listEmTransfer.Select(e => e.EmployeeATID).ToList() });
                    }
                    addedParams.Add(new AddedParam { Key = "IsWorking", Value = filter.ListWorkingStatus });
                    addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = user.CompanyIndex });
                    addedParams.Add(new AddedParam { Key = "ListDepartment", Value = filter.departmentIndexs });
                    addedParams.Add(new AddedParam { Key = "Filter", Value = filter.filter });
                    addedParams.Add(new AddedParam { Key = "UserType", Value = (int)EmployeeType.Employee });
                    //addedParams.Add(new AddedParam { Key = "IsCurrentWorkingAndNoDepartment", Value = true });
                    listData = _IIC_UserMasterLogic.GetUserMasterInfoMany(addedParams);
                }
                listData = _IIC_EmployeeLogic.CheckCurrentDepartment(listData);

                result = Ok(listData);

            }
            catch (Exception ex)
            {
                _Logger.LogError($"{ex}");
                throw ex;
            }
            return result;
        }

        [Authorize]
        [ActionName("GetAllStudentMachineInfo")]
        [HttpGet]
        public IActionResult GetAllStudentMachineInfo([FromQuery] string filter, [FromQuery] string[] classIndex)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }
            try
            {
                ConfigObject config = ConfigObject.GetConfig(cache);
                List<IC_EmployeeDTO> listData = new List<IC_EmployeeDTO>();


                List<AddedParam> addedParams = new List<AddedParam>();
                addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = user.CompanyIndex });
                addedParams.Add(new AddedParam { Key = "ListClassIndex", Value = classIndex });
                addedParams.Add(new AddedParam { Key = "Filter", Value = filter });
                listData = _IIC_UserMasterLogic.GetStudentMasterInfoMany(addedParams);

                result = Ok(listData);

            }
            catch (Exception ex) { throw ex; }
            return result;
        }

        [Authorize]
        [ActionName("GetAllParentMachineInfo")]
        [HttpGet]
        public IActionResult GetAllParentMachineInfo([FromQuery] string filter)
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
                ConfigObject config = ConfigObject.GetConfig(cache);
                List<IC_EmployeeDTO> listData = new List<IC_EmployeeDTO>();


                List<AddedParam> addedParams = new List<AddedParam>();
                addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = user.CompanyIndex });
                addedParams.Add(new AddedParam { Key = "Filter", Value = filter });
                listData = _IIC_UserMasterLogic.GetParentMasterInfoMany(addedParams);

                result = Ok(listData);

            }
            catch (Exception ex) { throw ex; }
            return result;
        }

        [Authorize]
        [ActionName("GetAllCustomerMachineInfo")]
        [HttpPost]
        public async Task<IActionResult> GetAllCustomerMachineInfo(ListDepartmentAndFilter filter)
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
                ConfigObject config = ConfigObject.GetConfig(cache);
                List<IC_EmployeeDTO> listData = new List<IC_EmployeeDTO>();


                List<AddedParam> addedParams = new List<AddedParam>();
                addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = user.CompanyIndex });
                addedParams.Add(new AddedParam { Key = "ListDepartment", Value = filter.departmentIndexs == null || filter.departmentIndexs.Count == 0 ? null : filter.departmentIndexs });
                addedParams.Add(new AddedParam { Key = "Filter", Value = filter.filter });
                addedParams.Add(new AddedParam { Key = "EmployeeType", Value = filter.EmployeeType });
                listData = await _IIC_UserMasterLogic.GetCustomerMasterInfoMany(addedParams);

                result = Ok(listData);

            }
            catch (Exception ex) { throw ex; }
            return result;
        }

        [Authorize]
        [ActionName("GetAllCustomerMachineInfoByMultipleType")]
        [HttpPost]
        public async Task<IActionResult> GetAllCustomerMachineInfoByMultipleType(ListDepartmentAndFilter filter)
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
                ConfigObject config = ConfigObject.GetConfig(cache);
                List<IC_EmployeeDTO> listData = new List<IC_EmployeeDTO>();


                List<AddedParam> addedParams = new List<AddedParam>();
                addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = user.CompanyIndex });
                addedParams.Add(new AddedParam { Key = "ListDepartment", Value = filter.departmentIndexs == null || filter.departmentIndexs.Count == 0 ? null : filter.departmentIndexs });
                addedParams.Add(new AddedParam { Key = "Filter", Value = filter.filter });
                addedParams.Add(new AddedParam { Key = "ListEmployeeType", Value = filter.ListEmployeeType });
                listData = await _IIC_UserMasterLogic.GetCustomerMasterInfoMany(addedParams);

                result = Ok(listData);

            }
            catch (Exception ex) { throw ex; }
            return result;
        }

        [Authorize]
        [ActionName("GetUserMachineInfoCompare")]
        [HttpPost]
        public IActionResult GetUserMachineInfoCompare([FromBody] ListUserAndFilter param)
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
                ConfigObject config = ConfigObject.GetConfig(cache);
                List<IC_EmployeeDTO> listData = new List<IC_EmployeeDTO>();
                if (config.IntegrateDBOther == true)
                {
                    listData = _IHR_EmployeeLogic.GetUserMasterMachineInfoCompare(param.listUser, param.filter, user); //GetUserInfo_IntegrateDB(param.listUser, config, user);
                    return Ok(listData);
                }

                List<AddedParam> addedParams = new List<AddedParam>();
                addedParams.Add(new AddedParam { Key = "IsCurrentTransfer", Value = true });
                addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = user.CompanyIndex });
                addedParams.Add(new AddedParam { Key = "ListDepartment", Value = user.ListDepartmentAssigned });

                addedParams = new List<AddedParam>();
                List<IC_EmployeeTransferDTO> listEmTransfer = _iC_EmployeeTransferLogic.GetMany(addedParams);
                if (listEmTransfer != null && listEmTransfer.Count > 0)
                {
                    addedParams.Add(new AddedParam { Key = "ListEmployeeTransferATID", Value = listEmTransfer.Select(e => e.EmployeeATID).ToList() });
                }

                addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = user.CompanyIndex });
                addedParams.Add(new AddedParam { Key = "ListEmployeeATID", Value = param.listUser });
                addedParams.Add(new AddedParam { Key = "ListDepartment", Value = user.ListDepartmentAssigned });
                addedParams.Add(new AddedParam { Key = "Filter", Value = param.filter });
                addedParams.Add(new AddedParam { Key = "IsCurrentWorkingAndNoDepartment", Value = true });
                listData = _IIC_UserMasterLogic.GetUserMasterInfoMany(addedParams);
                listData = _IIC_EmployeeLogic.CheckCurrentDepartment(listData);

                result = Ok(listData);


            }
            catch (Exception ex) { throw ex; }
            return result;
        }


    }
}
