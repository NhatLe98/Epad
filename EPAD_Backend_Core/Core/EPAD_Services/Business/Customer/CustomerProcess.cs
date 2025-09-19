using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using EPAD_Data.Models.TimeLog;
using EPAD_Services.Interface;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EPAD_Services.Business
{
    public class CustomerProcess : MonitoringProcess
    {
        IGC_Rules_WarningService _GC_Rules_WarningService;
        IGC_GatesService _GC_GatesService;
        IGC_Gates_LinesService _GC_Gates_LinesService;
        IIC_ControllerService _IC_ControllerService;
        IGC_BlackListService _GC_BlackListService;

        //private readonly CheckWarningViolation _CheckWarningViolation;
        public CustomerProcess(IServiceProvider pServiceProvider, IConfiguration configuration, ILoggerFactory loggerFactory)
           : base(pServiceProvider, configuration, loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<CustomerProcess>();

            _GC_Rules_WarningService = TryResolve<IGC_Rules_WarningService>();
            _GC_GatesService = TryResolve<IGC_GatesService>();
            _GC_Gates_LinesService = TryResolve<IGC_Gates_LinesService>();
            _IC_ControllerService = TryResolve<IIC_ControllerService>();
            _GC_BlackListService = TryResolve<IGC_BlackListService>();

            //_CheckWarningViolation = TryResolve<CheckWarningViolation>();
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

            var result = new CheckRuleResult();

            //check object access type 
            mAccessType = CheckObjectAccess();
            param.InOutMode = inOutByLine.ToString();

            if (mAccessType == ObjectAccessType.Customer)
            {
                result = CheckRuleForCustomer(param.EmployeeATID, param.CheckTime, companyIndex);
            }
            else if (mAccessType == ObjectAccessType.Employee)
            {
                var blackList = _GC_BlackListService.FirstOrDefault(x => ((!string.IsNullOrWhiteSpace(param.EmployeeATID) && param.EmployeeATID == x.EmployeeATID) || (!string.IsNullOrEmpty(param.Nric) && x.Nric == param.Nric)) 
                                                                                                                          && x.FromDate.Date <= now.Date
                                                                                                                          && (x.ToDate == null || (x.ToDate != null && now.Date.Date <= x.ToDate.Value.Date)));
                if (blackList != null)
                {
                    mArrViolation.Add((int)EMonitoringError.EmployeeInBlackList);
                }
                else
                {
                    var ruleData = _GC_Employee_AccessedGroupService.GetInfoByEmpATIDAndFromDate(param.EmployeeATID, param.CheckTime, companyIndex);
                    if (ruleData != null)
                    {
                        if (ruleData.GeneralAccessRuleIndex != 0)
                        {
                            CheckIsLobbyLog(param, ruleData, companyIndex);
                            CheckAreaGroup(param, ruleData, companyIndex);
                            CheckAreaAccess(ruleData, companyIndex);

                            result = CheckRuleForEmployee(ruleData, param.EmployeeATID, param.CheckTime, companyIndex, LogType.Walker);
                        }
                        else
                        {
                            mArrViolation.Add((int)EMonitoringError.EmployeeNotInAccessGroup);
                        }
                    }
                    else
                    {
                        var ruleDataByDepartmentAccessedGroup = _GC_Department_AccessedGroupService.GetInfoDepartmentAccessedGroup(param.EmployeeATID, param.CheckTime, companyIndex);
                        if (ruleDataByDepartmentAccessedGroup != null && param.EmployeeType != (int)EmployeeType.Driver && param.EmployeeType != (int)EmployeeType.Guest)
                        {
                            if (ruleDataByDepartmentAccessedGroup.GeneralAccessRuleIndex != 0)
                            {
                                //if (_useOtherDb)
                                //{
                                //    GetStartTimeByShift(param.EmployeeATID, param.CheckTime);
                                //}

                                //CheckAreaGroupLog(param, ruleData); // Kiểm tra log nhóm khu vực trực thuộc
                                CheckIsLobbyLog(param, ruleDataByDepartmentAccessedGroup, companyIndex);
                                CheckAreaGroup(param, ruleDataByDepartmentAccessedGroup, companyIndex);
                                CheckAreaAccess(ruleDataByDepartmentAccessedGroup, companyIndex);

                                result = CheckRuleForEmployee(ruleDataByDepartmentAccessedGroup, param.EmployeeATID, param.CheckTime, companyIndex, LogType.Walker);
                            }
                            else
                            {
                                mArrViolation.Add((int)EMonitoringError.EmployeeNotInAccessGroup);
                            }
                          
                        }
                        else
                        {
                            if (_configClientName == ClientName.MONDELEZ.ToString() && (param.EmployeeType == (int)EmployeeType.Driver || param.EmployeeType == (int)EmployeeType.Guest))
                            {
                                var ruleDataEmp = new EmployeeAccessRule();
                                if (param.EmployeeType == (int)EmployeeType.Guest)
                                {
                                    ruleDataEmp = _GC_Employee_AccessedGroupService.GetInfoByGuest(companyIndex, param.EmployeeATID);
                                }
                                else
                                {
                                    ruleDataEmp = _GC_Employee_AccessedGroupService.GetInfoByDriver(companyIndex, param.EmployeeATID);
                                }
                                if (ruleDataEmp != null)
                                {
                                    if (ruleDataEmp.GeneralAccessRuleIndex != 0)
                                    {
                                        CheckIsLobbyLog(param, ruleDataEmp, companyIndex);
                                        CheckAreaGroup(param, ruleDataEmp, companyIndex);
                                        CheckAreaAccess(ruleDataEmp, companyIndex);

                                        result = CheckRuleForEmployee(ruleDataEmp, param.EmployeeATID, param.CheckTime, companyIndex, LogType.Walker);
                                    }
                                    else
                                    {
                                        mArrViolation.Add((int)EMonitoringError.EmployeeNotInAccessGroup);
                                    }
                                }
                                else
                                {
                                    mArrViolation.Add((int)EMonitoringError.EmployeeNotRegisterAccessGroup);
                                }
                            }
                            else
                            {
                                mArrViolation.Add((int)EMonitoringError.EmployeeNotRegisterAccessGroup);
                            }

                        }
                        //mArrViolation.Add((int)EMonitoringError.NotFoundRule);

                    }
                }
                result.SetStatus(GetpriorityViolation(mArrViolation)); // Lấy vi phạm có ưu tiên cao nhất
            }
            return result;
        }

        protected void CheckAreaAccess(EmployeeAccessRule rule, int companyIndex)
        {
            // lấy ds khu vực dc truy cập
            var listGates = _GC_Rules_GeneralAccess_GatesService.Where(t => t.CompanyIndex == companyIndex
                && t.RulesGeneralIndex == rule.GeneralAccessRuleIndex).ToList();

            bool allow = false;
            for (int i = 0; i < listGates.Count; i++)
            {
                string[] arrLines = listGates[i].LineIndexs.Split(',');
                foreach (string item in arrLines)
                {
                    if (item != "" && item == mLineIndex.ToString())
                    {
                        allow = true;
                        break;
                    }
                }
            }

            if (allow == false)
            {
                mArrViolation.Add((int)EMonitoringError.EmployeeNotInAccessGroup);
            }
        }

        private void CheckIsLobbyLog(AttendanceLogRealTime param, EmployeeAccessRule rule, int companyIndex)
        {
            // get shift info

            var listLobbyMachines = GetLobbyMachines();


            GC_Rules_General_Log rules_General_Log = null;

            //Get Quy định chung
            var listRuleGenerals = _GC_Rules_GeneralService.Where(e => e.CompanyIndex == param.CompanyIndex
            && DateTime.Now >= e.FromDate && e.ToDate <= DateTime.Now).OrderByDescending(e => e.FromDate);
            var listRuleGeneralsLogs = _GC_Rules_General_LogService.Where(e => e.AreaGroupIndex.Split(',').ToList().Contains(mAreaGroup?.ToString()));
            foreach (var item in listRuleGeneralsLogs)
            {
                var lastestAppliedRule = listRuleGenerals.FirstOrDefault(e => item.RuleGeneralIndex == e.Index);
                if (lastestAppliedRule != null)
                {
                    rules_General_Log = item;
                }
            }


            if (listLobbyMachines != null)
            {
                #region Set InOut mode by rule general
                //if (CustomerVersionControl.GetVersionMondelez())
                //{
                //    if (listLobbyMachines.Contains(param.SerialNumber))
                //    {
                //        var logInLobbies = GetTimeLogInByParams(param.EmployeeATID, approveStatus: 1, param.CheckTime, listLobbyMachines, rule);
                //        //if (logInLobbies == null || !logInLobbies.Any() || (logInLobbies.Count() % 2) == 0) // Check log đầu tiên
                //        if (logInLobbies == null || !logInLobbies.Any()) // Check log đầu tiên
                //        {
                //            mInOutMode = 1;
                //        }
                //        //else if ((logInLobbies.Count() % 2) != 0) // Lấy Log lần lượt: => Log này là log ra
                //        else if (logInLobbies.Count() < 2) // Lấy Log đầu cuối: => Log này là log ra
                //        {
                //            mInOutMode = 2;
                //        }
                //        mIsLobbyLog = true;
                //    }
                //}
                //else
                //{
                if (listLobbyMachines.Contains(param.SerialNumber))
                {
                    var logInLobbies = GetTimeLogInByParams(param.EmployeeATID, approveStatus: 1, param.CheckTime, listLobbyMachines, rule, companyIndex);
                    if (rules_General_Log == null)
                    {
                        //Default DeviceMode
                        mInOutMode = Convert.ToInt16(param.InOutMode);
                    }
                    else
                    {
                        switch (rules_General_Log.UseMode)
                        {
                            case 0: //UseDeviceMode
                                mInOutMode = Convert.ToInt16(param.InOutMode);
                                break;
                            case 1: //UseSequenceLog
                                if (logInLobbies == null || !logInLobbies.Any() || (logInLobbies.Count() % 2) == 0) // Check log đầu tiên
                                {
                                    mInOutMode = 1;
                                }
                                else if ((logInLobbies.Count() % 2) != 0) // Lấy Log lần lượt: => Log này là log ra
                                {
                                    mInOutMode = 2;
                                }
                                break;
                            case 2: //UseMinimumLog
                                if (logInLobbies == null || !logInLobbies.Any()) // Check log đầu tiên
                                {
                                    mInOutMode = 1;
                                }
                                else if (logInLobbies.Count() < 2) // Lấy Log đầu cuối: => Log này là log ra
                                {
                                    mInOutMode = 2;
                                }
                                break;
                            case 3: // UseTimeLog
                                var baseDate = new DateTime(2020, 1, 1, param.CheckTime.Hour, param.CheckTime.Minute, param.CheckTime.Second);
                                if (rules_General_Log.FromEarlyDate <= baseDate && baseDate <= rules_General_Log.FromDate)
                                {
                                    mInOutMode = 1;
                                }
                                else if (rules_General_Log.ToDate <= baseDate && baseDate <= rules_General_Log.ToLateDate)
                                {
                                    mInOutMode = 2;
                                }
                                break;
                            default:
                                break;
                        }
                    }
                    mIsLobbyLog = true;
                    //}
                }
                #endregion

                if (!listLobbyMachines.Contains(param.SerialNumber)) //Nếu ko phải log ở sảnh thì check tiếp thiếu log hay ko
                {
                    if (mInOutMode != 1) // Log ra
                    {
                        var logInGate = GetTimeLogByParams(param.EmployeeATID, approveStatus: 1, 1, param.CheckTime, companyIndex);
                        if (logInGate != null)
                        {
                            var logInLobbies = GetTimeLogInByParams(param.EmployeeATID, approveStatus: 1, param.CheckTime, listLobbyMachines, rule, companyIndex);
                            //_logger.LogError("Start: if (rule.CheckLogByShift && listDailyShiftInfo != null)");
                            if (!rule.CheckLogByShift)
                            {

                                CheckViolationLobby(rules_General_Log, logInLobbies);

                            }
                        }
                    }
                }
            }
        }

        protected IEnumerable<GC_TimeLog> GetTimeLogInByParams(string employeeATID, int approveStatus, DateTime endTime, List<string> listMachines, EmployeeAccessRule ruleData, int companyIndex)
        {
            DateTime? startTime = null;
            if (ruleData.CheckInByShift == false)
            {
                if (ruleData.CheckInTime != null)
                {
                    startTime = new DateTime(endTime.Year, endTime.Month, endTime.Day, ruleData.CheckInTime.Value.Hour, ruleData.CheckInTime.Value.Minute, 0);
                }
            }

            var logs = _GC_TimeLogService.Where(t => t.CompanyIndex == companyIndex && listMachines.Contains(t.MachineSerial) && t.LogType == LogType.Walker.ToString() && t.EmployeeATID == employeeATID
                            && t.ApproveStatus == approveStatus && t.Time >= startTime && t.Time < endTime);
            if (logs != null && logs.Any())
            {
                return logs;
            }

            logs = _GC_TimeLogService.Where(t => t.CompanyIndex == companyIndex && listMachines.Contains(t.MachineSerial) && t.LogType == LogType.Parking.ToString() && t.EmployeeATID == employeeATID
                            && t.ApproveStatus == approveStatus && t.Time >= startTime && t.Time < endTime);
            return logs;
        }

        private void CheckViolationLobby(GC_Rules_General_Log rules_General_Log, IEnumerable<GC_TimeLog> logInLobbies)
        {
            if (rules_General_Log == null)
            {
                //Default DeviceMode
                if ((logInLobbies == null || !logInLobbies.Any() || !logInLobbies.Any(e => e.InOutMode == 1))) // Thiếu log vào ở sảnh
                {
                    mArrViolation.Add((int)EMonitoringError.CheckInLobbyLogNotExist);
                }
                else if (!logInLobbies.Any(e => e.InOutMode == 2)) //Thiếu log ra ở sảnh
                {
                    mArrViolation.Add((int)EMonitoringError.CheckOutLobbyLogNotExist);
                }
            }
            else
            {
                switch (rules_General_Log.UseMode)
                {
                    case 0: //UseDeviceMode
                        if ((logInLobbies == null || !logInLobbies.Any() || !logInLobbies.Any(e => e.InOutMode == 1))) // Thiếu log vào ở sảnh
                        {
                            mArrViolation.Add((int)EMonitoringError.CheckInLobbyLogNotExist);
                        }
                        else if (!logInLobbies.Any(e => e.InOutMode == 2)) //Thiếu log ra ở sảnh
                        {
                            mArrViolation.Add((int)EMonitoringError.CheckOutLobbyLogNotExist);
                        }
                        break;
                    case 1: //UseSequenceLog
                        if ((logInLobbies == null || !logInLobbies.Any())) // Thiếu log vào ở sảnh
                        {
                            mArrViolation.Add((int)EMonitoringError.CheckInLobbyLogNotExist);
                        }
                        else if (logInLobbies.Count() % 2 != 0) //Thiếu log ra ở sảnh
                        {
                            mArrViolation.Add((int)EMonitoringError.CheckOutLobbyLogNotExist);
                        }
                        break;
                    case 2: //UseMinimumLog
                        if ((logInLobbies == null || !logInLobbies.Any() || logInLobbies.Count() < rules_General_Log.MinimumLog)) // Thiếu log vào ở sảnh
                        {
                            mArrViolation.Add((int)EMonitoringError.CheckInLobbyLogNotExist);
                        }
                        //else if (logInLobbies.Count() < 2) //Thiếu log ra ở sảnh
                        //{
                        //    mArrViolation.Add((int)GCSEnum.EMonitoringError.CheckOutLobbyLogNotExist);
                        //}
                        break;
                    case 3: // UseTimeLog
                        if (!logInLobbies.Any(e => CompareDateWithRule(e.Time, rules_General_Log) == 1))// Check thiếu log vào
                        {
                            mArrViolation.Add((int)EMonitoringError.CheckInLobbyLogNotExist);
                        }
                        else if (!logInLobbies.Any(e => CompareDateWithRule(e.Time, rules_General_Log) == 2))// Check thiếu log ra
                        {
                            mArrViolation.Add((int)EMonitoringError.CheckOutLobbyLogNotExist);
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        private int CompareDateWithRule(DateTime time, GC_Rules_General_Log rules_General_Log)
        {
            var baseDate = new DateTime(2020, 1, 1, time.Hour, time.Minute, time.Second);
            if (rules_General_Log.FromEarlyDate <= baseDate && baseDate <= rules_General_Log.FromDate)
            {
                return 1;
            }
            else if (rules_General_Log.ToDate <= baseDate && baseDate <= rules_General_Log.ToLateDate)
            {
                return 2;
            }
            return 0;
        }

        public List<ViolationShift> CheckShiftByList(string[] empATIDs, DateTime checkTime)
        {
            return null;
        }

        internal override GC_TimeLog CreateTimeLogObject(AttendanceLogRealTime param, DateTime now, int companyIndex)
        {
            var log = new GC_TimeLog();
            log.EmployeeATID = param.EmployeeATID;
            log.Time = param.CheckTime;
            log.CompanyIndex = companyIndex;
            log.MachineSerial = param.SerialNumber;
            log.Action = "ADD";
            log.SystemTime = now;
            log.GateIndex = mGateIndex;
            log.LineIndex = mLineIndex;
            log.LogType = LogType.Walker.ToString();
            log.Status = (short)mResult.GetStatus();
            log.LeaveType = mResult.LeaveType;
            log.Error = mResult.GetError();
            log.ApproveStatus = (short)ApproveStatus.Waiting;
            log.VerifyMode = param.VerifyMode;
            log.CardNumber = param.CardNumber;
            var rules_Generals = _GC_Rules_GeneralService.Where(e => e.CompanyIndex == param.CompanyIndex
                && DateTime.Now >= e.FromDate && (DateTime.Now <= e.ToDate || e.ToDate == null)).OrderByDescending(e => e.FromDate).ToList(); // lay quy dinh chung con hieu luc
            var areaGroup = GetAreaGroups(param.CompanyIndex).FirstOrDefault(x => x.DeviceGroups.Contains(mdeviceIndex)); // lay khu vuc theo thiet bi

            log.InOutMode = mInOutMode;

            if (rules_Generals != null && rules_Generals.Any() && areaGroup != null)
            {
                var rules_General_Log = _GC_Rules_General_LogService.Where(x => rules_Generals.Any(y => y.Index == x.RuleGeneralIndex)
                    && x.AreaGroupIndex.Split(',').Contains(areaGroup.Index.ToString())).FirstOrDefault();

                var logModeIn = GetListTimeLogByParams(param.EmployeeATID, approveStatus: 1, inOutMode: 1, param.CheckTime, companyIndex); // kiem tra log vao co ton tai chua
                var logModeOut = GetListTimeLogByParams(param.EmployeeATID, approveStatus: 1, inOutMode: 2, param.CheckTime, companyIndex); // kiem tra log ra co ton tai chua

                var logexist = _GC_TimeLogService.Where(t => t.CompanyIndex == companyIndex && t.LogType == LogType.Walker.ToString() && t.EmployeeATID == param.EmployeeATID
                            && t.ApproveStatus == (short)ApproveStatus.Approved && t.Time.Date >= param.CheckTime.Date.AddDays(-1) && t.Time < param.CheckTime
                            && (t.InOutMode == (short)InOutMode.Input || t.InOutMode == (short)InOutMode.Output)).GroupBy(t => t.InOutMode, (key, g) => new { key, Count = g.ToList() }).ToList();

                if (rules_General_Log != null)
                {
                    switch (rules_General_Log.UseMode)
                    {
                        case 0:
                            log.InOutMode = Convert.ToInt16(param.InOutMode);
                            break;
                        case 1:
                            if (logModeIn == null)
                                log.InOutMode = (short)InOutMode.Input;
                            else if (logModeOut == null)
                                log.InOutMode = (short)InOutMode.Output;
                            else if (logModeIn.Count <= logModeOut.Count)
                                log.InOutMode = (short)InOutMode.Input;
                            else
                                log.InOutMode = (short)InOutMode.Output;
                            break;
                        default:
                            log.InOutMode = mInOutMode;
                            break;
                    }
                }
            }

            if (mIsLobbyLog && mResult.GetSuccess())
                log.ApproveStatus = (short)ApproveStatus.Approved;

            log.ObjectAccessType = mAccessType.ToString();
            log.ExtendData = "";
            log.UpdatedDate = DateTime.Now;
            log.UpdatedUser = GlobalParams.UpdatedUser;

            if (mAccessType == ObjectAccessType.Customer && mCustomer != null)
                log.CustomerIndex = mCustomer.Index;

            return log;
        }

        internal override List<string> GetImages(short pInOut, UserInfo user)
        {
            if (mLineIndex == 0) return new List<string>();
            // get cameras in this line
            List<string> listData = new List<string>();
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
            for (int i = 0; i < listCameraIndex.Count; i++)
            {
                var pictureInfo = _IC_CameraService.GetCameraPictureByCameraIndex(listCameraIndex[i], "120", user);
                if (pictureInfo == null)
                {
                    continue;
                }
                if (pictureInfo.Success == true)
                {
                    listData.Add(pictureInfo.Link);
                }
                else
                {
                    _logger.LogError($"Error get camera picture. Index: {listCameraIndex[i]}. Detail: {pictureInfo.Error}");
                }
            }

            return listData;
        }

        internal override List<string> GetInfos(short pInOut)
        {
            return null;
        }

        internal override bool PushDataToClient(AttendanceLogRealTime param, GC_TimeLog log, DateTime now, string link, List<string> verifyImages, List<string> verifyInfos)
        {
            try
            {
                var info = new WalkerInfo();
                info.ObjectType = mAccessType.ToString();
                info.SetBasicInfo(param.EmployeeATID, mInOutMode, mResult.GetSuccess(),
                    mResult.GetError(), param.CheckTime, _Config.CompanyIndex, log.Index, mLineIndex, param.Avatar);
                if (mAccessType == ObjectAccessType.Customer)
                {
                    info.CustomerIndex = mCustomer == null ? 0 : mCustomer.Index;
                    SetCustomerInfo(info, param, log.Index, verifyImages.Count == 0 ? "" : verifyImages[0], now);
                }
                else if (mAccessType == ObjectAccessType.Employee)
                {
                    SetEmployeeInfo(info, param, log.Index, verifyImages.Count == 0 ? "" : verifyImages[0], now);
                }

                if (mIsLobbyLog) // Nếu là log ở sảnh thì ko gửi data về client
                {
                    if (!mResult.GetSuccess())
                    {
                        var warningRule = _GC_Rules_WarningService.GetRulesWarningByCompanyIndexAndCode(mResult.GetError(), param.CompanyIndex);
                        if (warningRule != null)
                        {
                            if ((warningRule.UseSpeaker ?? false) || (warningRule.UseLed ?? false))
                            {
                                var list = _GC_Rules_WarningService.GetRulesWarningControllerChannel(warningRule.Index, param.CompanyIndex).Result;
                                var group = list.GroupBy(e => e.ControllerIndex);

                                var dummy = new List<CotrollerWarningRequestModel>();
                                var checkDuplicateList = new List<CotrollerWarningRequestModel>();
                                foreach (var item in group)
                                {
                                    var listLines = item.Select(e => e.ChannelIndex).Distinct().ToList();

                                    var controllerParam = new RelayControllerParam();
                                    controllerParam.ControllerIndex = item.Key;
                                    controllerParam.ListChannel = listLines;
                                    controllerParam.AutoOff = true;
                                    controllerParam.SetOn = true;
                                    var success = _IC_ControllerService.SetOnAndAutoOffController(controllerParam);
                                }
                            }
                            if (warningRule.UseEmail ?? false)
                            {
                                CreateAndSendMail(warningRule.Email, log);
                            }
                        }
                    }
                    else
                    {
                        string error = Enum.GetName(typeof(EMonitoringError), (int)EMonitoringError.CheckInOutLobbySuccess);
                        var warningRule = _GC_Rules_WarningService.GetRulesWarningByCompanyIndexAndCode(error, param.CompanyIndex);

                        if (warningRule != null)
                        {
                            if ((warningRule.UseSpeaker ?? false) || (warningRule.UseLed ?? false))
                            {
                                var list = _GC_Rules_WarningService.GetRulesWarningControllerChannel(warningRule.Index, param.CompanyIndex).Result;
                                var group = list.GroupBy(e => e.ControllerIndex);

                                var dummy = new List<CotrollerWarningRequestModel>();
                                var checkDuplicateList = new List<CotrollerWarningRequestModel>();
                                foreach (var item in group)
                                {
                                    var listLines = item.Select(e => e.ChannelIndex).Distinct().ToList();

                                    var controllerParam = new RelayControllerParam();
                                    controllerParam.ControllerIndex = item.Key;
                                    controllerParam.ListChannel = listLines;
                                    controllerParam.AutoOff = true;
                                    controllerParam.SetOn = true;
                                    var success = _IC_ControllerService.SetOnAndAutoOffController(controllerParam);
                                }
                            }
                            if (warningRule.UseEmail ?? false)
                            {
                                CreateAndSendMail(warningRule.Email, log);
                            }
                        }
                    }

                    return true;
                }
                else
                {
                    // bool success = _RealtimeServer_Client.AddWalkerMonitoringLog(info).Result;
                    var send = SendWalkerData(info);
                    send.Wait();
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"PushDataToClient: {ex}");
                return false;
            }
        }
        public void SetCustomerInfo(WalkerInfo info, AttendanceLogRealTime param, long logIndex, string verifyImage, DateTime now)
        {
            info.ListInfo.Add(new InfoDetail("CustomerName", mCustomer == null ? "" : mCustomer.CustomerName));
            info.ListInfo.Add(new InfoDetail("CompanyName", mCustomer == null ? "" : mCustomer.CustomerCompany));
            info.ListInfo.Add(new InfoDetail("WorkContent", mCustomer == null ? "" : mCustomer.WorkContent));
            info.ListInfo.Add(new InfoDetail("RegisterTime", mCustomer == null ? "" : mCustomer.RegisterTime.ToString("dd/MM/yyyy HH:mm:ss")));
            string workingTime = "";
            string contactPerson = "";
            if (mCustomer != null)
            {
                if (mCustomer.ExtensionTime == null)
                {
                    workingTime = mCustomer.FromTime.ToString("dd/MM/yyyy HH:mm:ss") + " - " + mCustomer.ToTime.ToString("dd/MM/yyyy HH:mm:ss");
                }
                else
                {
                    workingTime = mCustomer.FromTime.ToString("dd/MM/yyyy HH:mm:ss") + " - " + mCustomer.ExtensionTime.Value.ToString("dd/MM/yyyy HH:mm:ss");
                }
                //Custom Mondelez
                List<EmployeeFullInfo> listEmp = GetInfoByListEmployeeATIDAndDateAsync(new List<string>() { mCustomer.ContactPersonATIDs }, _Config.CompanyIndex, now).Result;/*_HR_EmployeeFullInfoService.GetInfoByListEmployeeATIDAndDateAsync(new List<string>() { mCustomer.ContactPersonATIDs }, now, _AdminConfiguration.CompanyIndex).Result;*/
                if (listEmp.Count > 0)
                {
                    contactPerson = $"{listEmp[0].FullName} - {listEmp[0].Department} - {listEmp[0].Phone}";
                }
            }
            info.ListInfo.Add(new InfoDetail("WorkingDuration", workingTime));
            info.ListInfo.Add(new InfoDetail("ContactPerson", contactPerson));
            info.RegisterImage = mCustomer == null ? "" : mCustomer.CustomerImage;
            info.VerifyImage = verifyImage;
        }

        public async Task<List<EmployeeFullInfo>> GetInfoByListEmployeeATIDAndDateAsync(List<string> listEmp, int pCompanyIndex, DateTime pDate)
        {
            var data = new List<EmployeeFullInfo>();

            data = await _HR_UserService.GetEmployeeCompactInfoByEmployeeATID(new List<string>() { mCustomer.ContactPersonATIDs }, pDate, _Config.CompanyIndex);

            data = data.Where(e => listEmp.Contains(e.EmployeeATID)).ToList();
            return data;
        }

        public void SetEmployeeInfo(WalkerInfo info, AttendanceLogRealTime param, long logIndex, string verifyImage, DateTime now)
        {
            info.ListInfo.Add(new InfoDetail("FullName", mEmpInfo.FullName));
            info.ListInfo.Add(new InfoDetail("EmployeeCode", mEmpInfo.EmployeeCode));
            info.ListInfo.Add(new InfoDetail("Department", mEmpInfo.Department));
            info.ListInfo.Add(new InfoDetail("Position", mEmpInfo.Position));
            info.VerifyImage = verifyImage;
            if (mEmpInfo.Avatar != null)
            {
                info.RegisterImage = Convert.ToBase64String(mEmpInfo.Avatar);
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
