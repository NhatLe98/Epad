using EPAD_Common.Services;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using EPAD_Logic;
using EPAD_Services.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EPAD_Services.Impl
{
    public class IC_UserMasterService : BaseServices<IC_UserMaster, EPAD_Context>, IIC_UserMasterService
    {
        private readonly IIC_UserMasterLogic _IIC_UserMasterLogic;
        private readonly IHR_UserService _iHR_UserService;
        private readonly IHR_EmployeeInfoService _iHR_EmployeeInfoService;
        private readonly IHR_CardNumberInfoService _iHR_CardNumberInfoService;
        private readonly IIC_WorkingInfoLogic _IIC_WorkingInfoLogic;
        public IC_UserMasterService(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _IIC_UserMasterLogic = serviceProvider.GetService<IIC_UserMasterLogic>();
            _iHR_UserService = serviceProvider.GetService<IHR_UserService>();
            _iHR_EmployeeInfoService = serviceProvider.GetService<IHR_EmployeeInfoService>();
            _iHR_CardNumberInfoService = serviceProvider.GetService<IHR_CardNumberInfoService>();
            _IIC_WorkingInfoLogic = serviceProvider.GetService<IIC_WorkingInfoLogic>();
        }

        public async Task DownloadUserMasterSDK(UserInfoPram Pram, UserInfo user, ConfigObject config)
        {
            if (Pram.ListUserInfo?.Count > 0)
            {
                List<IC_UserMasterDTO> listUserMaster = new List<IC_UserMasterDTO>();
                List<IC_WorkingInfoDTO> listWorkingInfo = new List<IC_WorkingInfoDTO>();
                List<HR_User> listHRUser = new List<HR_User>();
                List<HR_EmployeeInfo> listHREmpInfo = new List<HR_EmployeeInfo>();
                List<HR_CardNumberInfo> listCardInfo = new List<HR_CardNumberInfo>();

                int groupIndex = DbContext.IC_SystemCommand.Single(x => x.Index == Pram.SystemCommandIndex).GroupIndex;
                string authModesStr = DbContext.IC_CommandSystemGroup.Single(x => x.Index == groupIndex).ExternalData;
                List<UserSyncAuthMode> authModes = null;
                TargetDownloadUser targetDownloadUser = TargetDownloadUser.AllUser;
                if (!string.IsNullOrEmpty(authModesStr))
                {
                    var externalDataDefinition = new
                    {
                        AuthModes = "",
                        TargetUser = "",
                    };
                    var externalData = JsonConvert.DeserializeAnonymousType(authModesStr, externalDataDefinition);

                    if (!string.IsNullOrEmpty(externalData.AuthModes))
                    {
                        List<string> authModeStrList = externalData.AuthModes.Split(", ").ToList();
                        authModes = authModeStrList.ConvertAll(x => (UserSyncAuthMode)Enum.Parse(typeof(UserSyncAuthMode), x));
                        if (authModes.Count == 0)
                            authModes = null;
                    }

                    if (!string.IsNullOrEmpty(externalData.TargetUser))
                        targetDownloadUser = (TargetDownloadUser)Enum.Parse(typeof(TargetDownloadUser), externalData.TargetUser);
                }

                foreach (var item in Pram.ListUserInfo)
                {
                    // user info
                    HR_User hrUser = new HR_User();
                    hrUser.EmployeeATID = item.UserID.PadLeft(config.MaxLenghtEmployeeATID, '0');
                    hrUser.CompanyIndex = user.CompanyIndex;
                    hrUser.CreatedDate = DateTime.Now;
                    hrUser.UpdatedUser = user.UserName;
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
                    userMaster.Privilege = (short)item.Privilege;
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
                    workingInfo.IsSync = true;
                    workingInfo.DepartmentIndex = 0;
                    workingInfo.Status = (short)TransferStatus.Approve;
                    workingInfo.UpdatedDate = hrUser.UpdatedDate;
                    workingInfo.UpdatedUser = hrUser.UpdatedUser;
                    workingInfo.ApprovedDate = hrUser.CreatedDate;
                    workingInfo.ApprovedUser = hrUser.UpdatedUser;
                    listWorkingInfo.Add(workingInfo);
                }

                if (Pram.IsOverwriteData)
                {
                    await _IIC_UserMasterLogic.SaveAndOverwriteList(listUserMaster, authModes, targetDownloadUser);
                }
                else
                {
                    await _IIC_UserMasterLogic.SaveAndAddMoreList(listUserMaster, authModes, targetDownloadUser);
                }
                await _iHR_UserService.CheckExistedOrCreateList(listHRUser, user.CompanyIndex);
                await _iHR_EmployeeInfoService.CheckExistedOrCreateList(listHREmpInfo, user.CompanyIndex);
                await _iHR_CardNumberInfoService.CheckCardActivedOrCreateList(listCardInfo, user.CompanyIndex, Pram.IsOverwriteData);
                await _IIC_WorkingInfoLogic.CheckExistedOrCreateList(listWorkingInfo);

            }
        }

        public async Task<byte[]> GetFaceByEmployeeATID(string employeeATID)
        {
            var usermaster = await DbContext.IC_UserMaster.FirstOrDefaultAsync(x => x.EmployeeATID == employeeATID);
            if(usermaster != null && !string.IsNullOrEmpty(usermaster.FaceV2_Content))
            {
                return Convert.FromBase64String(usermaster.FaceV2_Content);
            }
            return null;
        }
    }
}
