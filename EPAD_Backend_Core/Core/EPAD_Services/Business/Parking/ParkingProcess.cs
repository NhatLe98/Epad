using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using EPAD_Data.Models.TimeLog;
using EPAD_Services.Interface;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace EPAD_Services.Business.Parking
{
    public class ParkingProcess : MonitoringProcess
    {
        IGC_ParkingLotAccessedService _GC_ParkingLotAccessedService;

        IGC_Lines_CheckInCameraService _GC_Lines_CheckInCameraService;
        IGC_Lines_CheckOutCameraService _GC_Lines_CheckOutCameraService;
        IGC_AccessedGroupService _GC_AccessedGroupService;
        IGC_TimeLogService _GC_TimeLogService;

        private static IMemoryCache cache;
        private ConfigObject config;
        string[] mArrInfos;
        bool mIsRequiredEmployeeVehicle = false;
        GC_ParkingLotAccessed mParkingAccess;
        List<GC_EmployeeVehicle> mListEmployeeVehicle;
        List<string> mDependentEmployees;
        List<GC_TimeLog> mListTimeLog;
        public ParkingProcess(IServiceProvider pServiceProvider, IMemoryCache pCache, IConfiguration configuration, ILoggerFactory loggerFactory)
           : base(pServiceProvider, configuration, loggerFactory)
        {
            cache = pCache;
            _logger = loggerFactory.CreateLogger<ParkingProcess>();
            config = ConfigObject.GetConfig(cache);

            _GC_ParkingLotAccessedService = TryResolve<IGC_ParkingLotAccessedService>();

            _GC_Lines_CheckInCameraService = TryResolve<IGC_Lines_CheckInCameraService>();
            _GC_Lines_CheckOutCameraService = TryResolve<IGC_Lines_CheckOutCameraService>();

            _GC_AccessedGroupService = TryResolve<IGC_AccessedGroupService>();
            _GC_TimeLogService = TryResolve<IGC_TimeLogService>();
        }

        internal override CheckRuleResult CheckByPassRules(AttendanceLogRealTime param, short inOutByLine, DateTime now)
        {
            var result = new CheckRuleResult();
            param.InOutMode = inOutByLine.ToString();
            result.SetStatus(GetpriorityViolation(mArrViolation));
            return result;
        }

        internal override CheckRuleResult CheckRules(AttendanceLogRealTime param, short inOutByLine, DateTime now, int companyIndex)
        {
            mParkingProcess = true;
            CheckRuleResult result = new CheckRuleResult();

            mInOutMode = inOutByLine;
            //check object access type 
            mAccessType = CheckObjectAccess();
            if (mAccessType == ObjectAccessType.Customer)
            {
                result = CheckRuleForCustomer(param.EmployeeATID, param.CheckTime, companyIndex);
            }
            else if (mAccessType == ObjectAccessType.Employee)
            {
                //if(CustomerVersionControl.GetVersionMondelez())
                //{
                var ruleData = _GC_Employee_AccessedGroupService.GetInfoByEmpATIDAndFromDate(param.EmployeeATID, param.CheckTime, _Config.CompanyIndex);
                if (ruleData != null)
                {
                    CheckAreaAccess(ruleData);
                    ////var parkingResult = CheckParkingRules(param, inOutByLine, now);
                    //result = CheckRuleForEmployee(param.EmployeeATID, param.CheckTime, companyIndex, LogType.Parking);
                    result = CheckRuleForEmployee(ruleData, param.EmployeeATID, param.CheckTime, companyIndex, LogType.Parking);
                }
                else
                {
                    var ruleDataByDepartmentAccessedGroup = _GC_Department_AccessedGroupService.GetInfoDepartmentAccessedGroup(param.EmployeeATID, param.CheckTime, _Config.CompanyIndex);
                    if (ruleDataByDepartmentAccessedGroup != null)
                    {
                        CheckAreaAccess(ruleDataByDepartmentAccessedGroup);
                        ////var parkingResult = CheckParkingRules(param, inOutByLine, now);
                        //result = CheckRuleForEmployee(param.EmployeeATID, param.CheckTime, companyIndex, LogType.Parking);
                        result = CheckRuleForEmployee(ruleDataByDepartmentAccessedGroup, param.EmployeeATID, param.CheckTime, companyIndex, LogType.Parking);
                    }
                    else
                    {
                        mArrViolation.Add((int)EMonitoringError.NotFoundRule);
                    }
                }
                //}
                //else
                //{
                //    GetStartTimeByShift(param.EmployeeATID, param.CheckTime);
                //    var parkingResult = CheckParkingRules(param, inOutByLine, now);
                //    result = CheckRuleForEmployee(param.EmployeeATID, param.CheckTime);
                //}
            }

            result.SetStatus(GetpriorityViolation(mArrViolation)); // Lấy vi phạm có ưu tiên cao nhất
            return result;
        }

        internal override GC_TimeLog CreateTimeLogObject(AttendanceLogRealTime param, DateTime now, int companyIndex)
        {
            var log = new GC_TimeLog();
            log.EmployeeATID = param.EmployeeATID;
            log.Time = param.CheckTime;
            log.CompanyIndex = companyIndex;
            log.MachineSerial = param.SerialNumber;
            log.InOutMode = mInOutMode;
            log.Action = "ADD";
            log.SystemTime = now;
            log.GateIndex = mGateIndex;
            log.LineIndex = mLineIndex;
            log.LogType = LogType.Parking.ToString();
            log.Status = short.Parse(mResult.GetStatus().ToString());
            log.Error = mResult.GetError();/*string.Join(";", mArrViolation); */
            log.ApproveStatus = (short)ApproveStatus.Waiting;
            log.VerifyMode = param.VerifyMode;
            if (mIsLobbyLog && mResult.GetSuccess())
                log.ApproveStatus = (short)ApproveStatus.Approved;

            log.ObjectAccessType = mAccessType.ToString();
            if (mAccessType == ObjectAccessType.Customer && mCustomer != null)
            {
                log.CustomerIndex = mCustomer.Index;
            }
            log.UpdatedDate = DateTime.Now;
            log.UpdatedUser = GlobalParams.UpdatedUser;
            log.ExtendData = "";

            return log;
        }

        internal override List<string> GetImages(short pInOut, UserInfo user)
        {
            if (mLineIndex == 0) return new List<string>();
            Dictionary<int, CameraPublicResult> dicCamera = new Dictionary<int, CameraPublicResult>();
            _cache.TryGetValue("ListCamera", out dicCamera);
            if (dicCamera == null)
            {
                _logger.LogInformation("Cannot get list camera from cache");
                return new List<string>();
            }
            // get cameras in this line

            List<int> listCameraIndex = new List<int>();
            if (mInOutMode == 1)
            {
                listCameraIndex = _GC_Lines_CheckInCameraService.GetDataByLineIndex(mLineIndex, _Config.CompanyIndex)
                    .Result.Select(t => t.CameraIndex).ToList();
            }
            else
            {
                listCameraIndex = _GC_Lines_CheckOutCameraService.GetDataByLineIndex(mLineIndex, _Config.CompanyIndex)
                    .Result.Select(t => t.CameraIndex).ToList();
            }

            string[] listData = new string[3]; // [FaceIn(Out), ImageNormal(Out), ImagePlate(Out)]
            mArrInfos = new string[2];
            for (int i = 0; i < listCameraIndex.Count; i++)
            {
                CameraPublicResult cameraInfo = null;
                dicCamera.TryGetValue(listCameraIndex[i], out cameraInfo);
                //if (cameraInfo == null || cameraInfo.Type == "Picture")
                //{
                var pictureInfo = _IC_CameraService.GetCameraPictureByCameraIndex(listCameraIndex[i], "15", user);
                if (pictureInfo == null)
                {
                    continue;
                }

                if (pictureInfo.Success == true)
                {
                    if (cameraInfo.Type == "Picture")
                    {
                        listData[0] = pictureInfo.Link;
                    }
                    else
                    {
                        listData[1] = pictureInfo.Link;
                    }

                    return listData.ToList();
                }
                else
                {
                    _logger.LogError($"Error get camera picture. Index: {listCameraIndex[i]}. Detail: {pictureInfo.Error}");
                }
            }
            return new List<string>();
        }

        internal override List<string> GetInfos(short pInOut)
        {
            if (mArrInfos == null)
            {
                return null;
            }
            return mArrInfos.ToList();
        }

        internal override bool PushDataToClient(AttendanceLogRealTime param, GC_TimeLog log, DateTime now, string link, List<string> verifyImages, List<string> verifyInfos)
        {
            var info = new ParkingMonitoringInfo();
            info.SetBasicInfo(param.EmployeeATID, mInOutMode, mResult.GetSuccess(), mResult.GetError(),
                param.CheckTime, _Config.CompanyIndex, log.Index, mLineIndex, param.Avatar);
            info.IsRequiredEmployeeVehicle = mIsRequiredEmployeeVehicle;
            info.CustomerIndex = 0;
            if (mAccessType == ObjectAccessType.Customer)
            {
                info.CustomerIndex = mCustomer.Index;
            }
            if (mParkingAccess == null)
            {
                mParkingAccess = new GC_ParkingLotAccessed();
                mParkingAccess.AccessType = (short)(mCustomer != null ? 1 : mEmpInfo != null ? 0 : 0);
            }
            if (mParkingAccess != null)
            {
                // customer
                if (mParkingAccess.AccessType == 1)
                {
                    info.EmployeeName = mCustomer == null ? "" : mCustomer.CustomerName;
                    info.EmployeeCode = "";
                    info.DepartmentIndex = (long)mEmpInfo.DepartmentIndex;
                    info.DepartmentName = mEmpInfo.Department;
                    info.CompanyName = mCustomer.CustomerCompany;

                    if (!string.IsNullOrEmpty(mCustomer.ContactPersonATIDs))
                    {
                        var contactPerson = _HR_EmployeeInfoService.GetEmployeeInfo(mCustomer.ContactPersonATIDs, _Config.CompanyIndex).Result;
                        if (contactPerson != null)
                        {
                            info.ContactPersonName = contactPerson.FullName;
                        }
                    }
                    info.FromTime = mCustomer.FromTime;
                    info.ToTime = mCustomer.ToTime;

                    if (mCustomer != null)
                    {
                        info.BikeModels.Add(mCustomer.BikeModel);
                        info.BikePlatesRegister.Add(mCustomer.BikePlate);
                    }
                    info.CardNumber = mCustomer == null ? "" : mCustomer.CardNumber;
                }
                else
                {

                    info.EmployeeName = mEmpInfo.FullName;
                    info.EmployeeCode = mEmpInfo.EmployeeATID; //change MNV thành MCC
                    info.DepartmentIndex = (long)mEmpInfo.DepartmentIndex;
                    info.DepartmentName = mEmpInfo.Department;
                    //info.CardNumber = mEmpInfo.EmployeeATID; bỏ
                    if (mListEmployeeVehicle != null)
                    {
                        for (int i = 0; i < mListEmployeeVehicle.Count; i++)
                        {
                            info.BikePlatesRegister.Add(mListEmployeeVehicle[i].Plate);
                            info.BikeModels.Add(mListEmployeeVehicle[i].Type.ToString());
                        }
                    }

                }
                info.AccessObject = mParkingAccess.AccessType == 1 ? "Customer" : "Employee";
            }
            info.InOut = mInOutMode;

            if (mInOutMode == 1)
            {
                info.ImageFaceIn = verifyImages == null ? "" : verifyImages[0];
                info.ImagePlateIn = verifyImages == null ? "" : verifyImages[1];
                info.TimeIn = param.CheckTime;

                info.BikePlateIn = verifyInfos == null ? "" : verifyInfos[0];
            }
            else
            {
                info.ImageFaceOut = verifyImages == null ? "" : verifyImages[0];
                info.ImagePlateOut = verifyImages == null ? "" : verifyImages[1];

                mDependentEmployees = new List<string> { param.EmployeeATID };


                mListTimeLog = _GC_TimeLogService.Where(t => t.CompanyIndex == _Config.CompanyIndex
                                    && t.EmployeeATID == param.EmployeeATID && mLineIndex == t.LineIndex).OrderBy(t => t.Time).ToList();

                if (mListTimeLog != null && mListTimeLog.Count > 0)
                {
                    var parkingLog = mListTimeLog.Where(t => t.InOutMode.ToString() == (verifyInfos == null ? "" : verifyInfos[0])).FirstOrDefault();
                    if (parkingLog == null)
                    {
                        parkingLog = mListTimeLog[0];
                    }

                    // 1 nv có thể có nhiều xe, lấy thông tin xe vào sớm nhất
                    info.TimeIn = parkingLog.Time;
                    info.TimeOut = param.CheckTime;

                    //GC_TimeLog_Image plateInfo = _GC_TimeLog_ImageService.Where(t => t.TimeLogIndex == mListParkingLog[0].TimeLogIn_Index).FirstOrDefault();
                    //info.BikePlateIn = parkingLog.PlateNumberIn;
                    info.BikePlateOut = verifyInfos == null ? "" : verifyInfos[0];
                }
            }
            // get number of bikes in parking
            info.NumberOfBikeInParking = mListTimeLog == null ? 0 : mListTimeLog.Count;

            // save external data for log table
            /*
            ParkingExternalData externalData = new ParkingExternalData();
            externalData.PlateNumbersRegistered = info.BikePlatesRegister;
            externalData.PlateNumberIn = info.BikePlateIn;
            externalData.PlateNumberOut = info.BikePlateOut;
            string jsonData = JsonConvert.SerializeObject(externalData);

            log.ExtendData = jsonData;*/
            log.PlatesRegistered = "";
            for (int i = 0; i < info.BikePlatesRegister.Count; i++)
            {
                log.PlatesRegistered += info.BikePlatesRegister[i] + ";";
            }

            // if the result is a success then will compare plate
            if (mResult.GetSuccess() == true)
            {
                ComparePlateAndChangeResult(info, log);
            }
            _GC_TimeLogService.SaveChangeAsync();

            try
            {
                // bool success = _RealtimeServer_Client.AddParkingMonitoringLog(info).Result;
                var client = new HttpClient();
                client.Timeout = TimeSpan.FromSeconds(3);
                client.BaseAddress = new Uri(_Config.RealTimeServerLink);
                var json = JsonConvert.SerializeObject(info);

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var request = client.PostAsync("/api/PushAttendanceLog/SendParkingData", content).Result;
                request.EnsureSuccessStatusCode();


                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"SendProgressData: {ex}");
                return false;
            }
        }

        private void ComparePlateAndChangeResult(ParkingMonitoringInfo info, GC_TimeLog log)
        {
            if (info.InOut == 1)
            {
                // compare verify plate and register plate
                if (info.BikePlatesRegister.Contains(info.BikePlateIn) == false && mIsRequiredEmployeeVehicle)
                {
                    var error = GetpriorityViolation(EMonitoringError.VerifyPlateAndRegisterPlateDifferent, GetEMonitoringErrorByName(mResult.GetError()));
                    mResult.SetStatus(error);
                    info.Success = mResult.GetSuccess();
                    info.Error = mResult.GetError();

                    log.Status = short.Parse(mResult.GetStatus().ToString());
                    log.Error = mResult.GetError();
                }
            }
            else
            {
                // compare verify plate in and verify plate out
                if (string.IsNullOrEmpty(info.BikePlateIn) || info.BikePlateIn != info.BikePlateOut && mIsRequiredEmployeeVehicle)
                {
                    var error = GetpriorityViolation(EMonitoringError.VerifyPlateInAndVerifyPlateOut, GetEMonitoringErrorByName(mResult.GetError()));
                    mResult.SetStatus(error);
                    info.Success = mResult.GetSuccess();
                    info.Error = mResult.GetError();

                    log.Status = short.Parse(mResult.GetStatus().ToString());
                    log.Error = mResult.GetError();
                }
            }
        }

        internal override void UpdateLocalData(long timeLogIndex, AttendanceLogRealTime param, DateTime now)
        {
            if (mCustomer != null && mCustomer.GoInSystem == null)
            {
                mCustomer.GoInSystem = true;
                _GC_CustomerService.SaveChanges();
            }
        }
    }
}
