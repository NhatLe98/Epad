using System;
using System.Collections.Generic;
using System.Linq;
using EPAD_Common.Types;
using EPAD_Common.Utility;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using Microsoft.Extensions.Caching.Memory;

namespace EPAD_Logic
{
    public class IC_UserInfoLogic : IIC_UserInfoLogic
    {
        private EPAD_Context _dbContext;
        private ConfigObject _config;
        private IMemoryCache _iCache;
        public IC_UserInfoLogic(EPAD_Context dbContext, IMemoryCache cache)
        {
            _dbContext = dbContext;
            _iCache = cache;
            _config = ConfigObject.GetConfig(_iCache);
        }
        public bool UpdateListUserPrivilege(List<IC_UserInfoDTO> listEmployee)
        {
            try
            {
                if (listEmployee.Count > 0)
                {
                    string[] listids = listEmployee.Select(u => u.EmployeeATID).ToArray();
                    foreach (var emp in listEmployee)
                    {
                        var emloyees = _dbContext.IC_UserInfo.Where(u => u.EmployeeATID == emp.EmployeeATID && u.SerialNumber == emp.SerialNumber).ToList();
                        if (emloyees != null && emloyees.Count > 0)
                        {
                            foreach (var em in emloyees)
                            {
                                em.Privilege = emp.Privilege;
                            }
                            _dbContext.IC_UserInfo.UpdateRange(emloyees);
                        }
                    }
                    _dbContext.SaveChanges();
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
        public List<IC_UserInfoDTO> GetMany(List<AddedParam> addedParams)
        {
            var query = _dbContext.IC_UserInfo.AsQueryable();
            if (addedParams != null)
            {
                foreach (AddedParam p in addedParams)
                {
                    switch (p.Key)
                    {
                        case "Filter":
                            if (p.Value != null)
                            {
                            }
                            break;
                        case "ListSerialNumber":
                            if (p.Value != null)
                            {
                                IList<string> serialNumbers = (IList<string>)p.Value;
                                query = query.Where(u => serialNumbers.Contains(u.SerialNumber));
                            }
                            break;
                        case "SerialNumber":
                            if (p.Value != null)
                            {
                                string serialNumber = p.Value.ToString();
                                query = query.Where(u => u.SerialNumber == serialNumber);
                            }
                            break;
                        case "ListEmployeeATID":
                            if (p.Value != null)
                            {
                                IList<string> employeeATIDs = (IList<string>)p.Value;

                                query = query.Where(u => employeeATIDs.Contains(u.EmployeeATID));
                            }
                            break;

                        case "CompanyIndex":
                            if (p.Value != null)
                            {
                                int companyIndex = Convert.ToInt32(p.Value);
                                query = query.Where(u => u.CompanyIndex == companyIndex);
                            }
                            break;
                    }
                }
            }
            query = query.OrderBy(u => u.EmployeeATID);
            // var count = query.Count();
            var data = query.Select(u => new IC_UserInfoDTO
            {
                EmployeeATID = u.EmployeeATID,
                CompanyIndex = u.CompanyIndex,
                UserName = u.UserName,
                SerialNumber = u.SerialNumber,
                AuthenMode = u.AuthenMode,
                Privilege = u.Privilege,
                CardNumber = u.CardNumber,
                Reserve1 = u.Reserve1,
                Reserve2 = u.Reserve2,
                Password = u.Password,
                CreatedDate = u.CreatedDate,
                UpdatedDate = u.UpdatedDate,
                UpdatedUser = u.UpdatedUser

            }).ToList();
            return data;
        }

        // Using SQL query command for using DATALENGHT to get FACE Lenght
        public List<IC_EmployeeDTO> GetUserInfoMany(List<AddedParam> addedParams)
        {
            if (addedParams == null)
                return new List<IC_EmployeeDTO>();
            var query = _dbContext.IC_UserInfo.AsQueryable();

            if (addedParams != null)
            {
                foreach (AddedParam param in addedParams)
                {
                    switch (param.Key)
                    {
                        case "Filter":
                            if (param.Value != null && !string.IsNullOrWhiteSpace(param.Value.ToString()))
                            {
                                string filter = param.Value.ToString();
                                query = query.Where(e => e.EmployeeATID.Contains(filter)
                                || e.SerialNumber.Contains(filter)
                                || e.CardNumber.Contains(filter)
                                || e.UserName.Contains(filter)
                                );
                            }
                            break;
                        case "CompanyIndex":
                            if (param.Value != null)
                            {
                                int companyIndex = Convert.ToInt32(param.Value);
                                query = query.Where(e => e.CompanyIndex == companyIndex);
                            }

                            break;
                        case "ListDevice":
                            if (param.Value != null)
                            {
                                IList<string> listDevice = (IList<string>)param.Value;
                                query = query.Where(e => listDevice.Contains(e.SerialNumber));
                            }
                            break;
                        case "ListEmployeeATID":
                            if (param.Value != null && !string.IsNullOrWhiteSpace(param.Value.ToString()))
                            {
                                IList<string> listEmployeeID = (IList<string>)param.Value;
                                if (listEmployeeID != null && listEmployeeID.Count > 0)
                                {
                                    query = query.Where(e => listEmployeeID.Contains(e.EmployeeATID));
                                }
                            }
                            break;
                        case "EmployeeATID":
                            if (param.Value != null)
                            {
                                string employeeID = param.Value.ToString();
                                query = query.Where(e => e.EmployeeATID == employeeID);
                            }
                            break;
                    }
                }
            }

            var listResultDevice = _dbContext.IC_Device.Where(x => query.AsEnumerable().Any(y => y.SerialNumber == x.SerialNumber)).ToList();
            var resutlData = query.AsEnumerable().Select(e => new IC_EmployeeDTO
            {
                CompanyIndex = e.CompanyIndex,
                EmployeeATID = e.EmployeeATID,
                Privilege = e.Privilege,
                PrivilegeName = GetPrivilegeName(e.Privilege.HasValue ? e.Privilege.Value : 0),
                NameOnMachine = e.UserName,
                CardNumber = e.CardNumber,
                SerialNumber = e.SerialNumber,
                IPAddress = listResultDevice.FirstOrDefault(x => x.SerialNumber == e.SerialNumber)?.IPAddress ?? string.Empty,
                Password = e.Password,
                Finger1 = e.FingerData0,
                Finger2 = e.FingerData1,
                Finger3 = e.FingerData2,
                Finger4 = e.FingerData3,
                Finger5 = e.FingerData4,
                Finger6 = e.FingerData5,
                Finger7 = e.FingerData6,
                Finger8 = e.FingerData7,
                Finger9 = e.FingerData8,
                Finger10 = e.FingerData9,
                FaceTemplate = e.FaceTemplate.HasValue ? e.FaceTemplate.Value != 0 ? e.FaceTemplate : e.FaceTemplateV2 : e.FaceTemplateV2,
                DepartmentName = e.DepartmentName
            }).ToList();
            return resutlData;
        }
        public IC_UserInfoDTO GetExist(string employeeID, int companyIndex, string serialNumber)
        {

            var data = _dbContext.IC_UserInfo.FirstOrDefault(e => e.EmployeeATID == employeeID && e.CompanyIndex == companyIndex && e.SerialNumber == serialNumber);
            if (data != null)
                return ConvertToDTO(data);
            else
                return null;
        }
        public IC_UserInfoDTO UpdateField(List<AddedParam> addParams)
        {
            AddedParam addParamEmployeeATID = addParams.FirstOrDefault(a => a.Key == "EmployeeATID");
            AddedParam addParamCompany = addParams.FirstOrDefault(a => a.Key == "CompanyIndex");
            AddedParam addParamSerialNumber = addParams.FirstOrDefault(a => a.Key == "SerialNumber");
            if (addParamEmployeeATID == null || addParamCompany == null || addParamSerialNumber == null)
                return null;
            addParams.Remove(addParamEmployeeATID);
            addParams.Remove(addParamCompany);
            addParams.Remove(addParamSerialNumber);
            var companyIndex = Convert.ToInt32(addParamCompany.Value);
            var employeeATID = addParamEmployeeATID.Value.ToString();
            var serialNumber = addParamSerialNumber.Value.ToString();

            var dataItem = _dbContext.IC_UserInfo.FirstOrDefault(e => e.CompanyIndex == companyIndex && e.EmployeeATID == employeeATID && e.SerialNumber == serialNumber);
            if (dataItem != null)
            {
                foreach (AddedParam p in addParams)
                {
                    switch (p.Key)
                    {

                        case "UserName":
                            string userName = string.Empty;
                            if (p.Value != null)
                            {
                                userName = p.Value.ToString();
                            }
                            dataItem.UserName = userName;
                            break;
                        case "CardNumber":
                            string cardNumber = string.Empty;
                            if (p.Value != null)
                            {
                                cardNumber = p.Value.ToString();
                            }
                            dataItem.CardNumber = cardNumber;
                            break;
                        case "Privilege":
                            short? privilege = null;
                            if (p.Value != null)
                            {
                                privilege = Convert.ToInt16(p.Value);
                            }
                            dataItem.Privilege = privilege;
                            break;
                        case "Password":
                            string password = string.Empty;
                            if (p.Value != null)
                            {
                                password = p.Value.ToString();
                            }
                            dataItem.Password = password;
                            break;
                        case "Reserve1":
                            string reserve1 = string.Empty;
                            if (p.Value != null)
                            {
                                reserve1 = p.Value.ToString();
                            }
                            dataItem.Reserve1 = reserve1;
                            break;
                        case "reserve2":
                            int? reserve2 = null;
                            if (p.Value != null)
                            {
                                reserve2 = Convert.ToInt32(p.Value);
                            }
                            dataItem.Reserve2 = reserve2;
                            break;
                        case "AuthenMode":
                            string authenMode = string.Empty;
                            if (p.Value != null)
                            {
                                authenMode = p.Value.ToString();
                            }
                            dataItem.AuthenMode = authenMode;
                            break;


                    }
                }
                _dbContext.IC_UserInfo.Update(dataItem);
            }
            _dbContext.SaveChanges();
            var dto = ConvertToDTO(dataItem);
            return dto;
        }

        public List<IC_UserInfoDTO> UpdateList(List<IC_UserInfoDTO> listUserInfo)
        {

            var listExisted = _dbContext.IC_UserInfo.Where(e => listUserInfo.Where(u => u.EmployeeATID == e.EmployeeATID && u.CompanyIndex == e.CompanyIndex).Count() > 0).ToList();

            foreach (var user in listUserInfo)
            {
                var data = listExisted.FirstOrDefault(e => e.EmployeeATID == user.EmployeeATID);
                if (data != null)
                {
                    ConvertDTOToData(user, data);
                    data.UpdatedDate = DateTime.Today;
                    _dbContext.Update(data);
                }
            }
            _dbContext.SaveChanges();
            return listUserInfo;
        }
        public List<IC_UserInfoDTO> SaveOrUpdateList(List<IC_UserInfoDTO> listUserInfo)
        {
            List<IC_UserInfoDTO> returnList = new List<IC_UserInfoDTO>();
            var listExisted = _dbContext.IC_UserInfo.Where(e => listUserInfo.Where(u => u.EmployeeATID == e.EmployeeATID && u.CompanyIndex == e.CompanyIndex && e.SerialNumber == u.SerialNumber).Count() > 0).ToList();
            foreach (var user in listUserInfo)
            {
                var data = listExisted.FirstOrDefault(e => e.EmployeeATID == user.EmployeeATID);
                if (data != null)
                {
                    ConvertDTOToData(user, data);
                    _dbContext.IC_UserInfo.Update(data);
                }
                else
                {
                    data = new IC_UserInfo();
                    ConvertDTOToData(user, data);
                    _dbContext.IC_UserInfo.Add(data);
                }
            }
            _dbContext.SaveChanges();
            return listUserInfo;
        }
        public List<IC_UserInfoDTO> SaveOrUpdateListField(List<IC_UserInfoDTO> listUserInfo)
        {

            var listExisted = _dbContext.IC_UserInfo.Where(e => listUserInfo.Where(u => u.EmployeeATID == e.EmployeeATID && u.CompanyIndex == e.CompanyIndex && e.SerialNumber == u.SerialNumber).Count() > 0).ToList();

            foreach (var user in listUserInfo)
            {
                var data = listExisted.FirstOrDefault(e => e.EmployeeATID == user.EmployeeATID);
                if (data != null)
                {
                    ConvertDTOToDataWithoutNull(user, data);
                    _dbContext.IC_UserInfo.Update(data);
                }
                else
                {
                    data = new IC_UserInfo();
                    ConvertDTOToData(user, data);
                    _dbContext.IC_UserInfo.Add(data);
                }
            }
            _dbContext.SaveChanges();
            return listUserInfo;
        }

        public bool Delete(List<string> listDevice, int companyIndex)
        {
            if (listDevice == null)
                return false;

            var listUserinfo = _dbContext.IC_UserInfo.Where(e => listDevice.Contains(e.SerialNumber) && e.CompanyIndex == companyIndex).ToList();

            _dbContext.IC_UserInfo.RemoveRange(listUserinfo);
            _dbContext.SaveChanges();
            return true;
        }
        public IC_UserInfoDTO Create(IC_UserInfoDTO item)
        {
            IC_UserInfo dataItem = _dbContext.IC_UserInfo.FirstOrDefault(e => e.EmployeeATID == item.EmployeeATID && e.CompanyIndex == item.CompanyIndex && e.SerialNumber == item.SerialNumber);
            if (dataItem != null)
            {
                dataItem = new IC_UserInfo();
                ConvertDTOToData(item, dataItem);
                //set logic when update UserMaster
                dataItem.UpdatedDate = DateTime.Now;
                _dbContext.IC_UserInfo.Add(dataItem);
                _dbContext.SaveChanges();
            }

            item = ConvertToDTO(dataItem);
            return item;
        }
        public string GetPrivilegeName(int privilege)
        {
            switch (privilege)
            {
                case GlobalParams.DevicePrivilege.PUSHAdminRole:
                    return "Quản trị viên";
                case GlobalParams.DevicePrivilege.SDKAdminRole:
                    return "Quản trị viên";
            }
            return "Người dùng";
        }
        // favorites function
        public UserInfoPram CheckExistedOrCreate(UserInfoPram listUserInfo, UserInfo userInfo)
        {

            var listEmployeeATID = listUserInfo.ListUserInfo.Select(e => e.UserID).ToList();
            var listUserMasterInDB = _dbContext.IC_UserMaster.Where(e => listEmployeeATID.Contains(e.EmployeeATID) && e.CompanyIndex == userInfo.CompanyIndex).ToList();
            foreach (var userMaster in listUserInfo.ListUserInfo)
            {
                // add user master
                var existUserMaster = listUserMasterInDB.FirstOrDefault(e => e.EmployeeATID == userMaster.UserID);
                if (existUserMaster == null)
                {
                    existUserMaster = new IC_UserMaster();
                    existUserMaster.CardNumber = userMaster.CardNumber;
                    existUserMaster.Privilege = (short)userMaster.Privilege;
                    existUserMaster.NameOnMachine = userMaster.NameOnDevice;
                    existUserMaster.CompanyIndex = userInfo.CompanyIndex;
                    existUserMaster.EmployeeATID = userMaster.UserID.PadLeft(_config.MaxLenghtEmployeeATID, '0');
                    existUserMaster.CreatedDate = DateTime.Now;
                    existUserMaster.UpdatedUser = userInfo.UserName;
                    _dbContext.IC_UserMaster.Add(existUserMaster);
                }

            }
            _dbContext.SaveChanges();
            return listUserInfo;
        }
        public UserInfoPram CheckCreateOrUpdate(UserInfoPram listUserInfo, UserInfo userInfo)
        {
            var listEmployeeATID = listUserInfo.ListUserInfo.Select(e => e.UserID.PadLeft(_config.MaxLenghtEmployeeATID, '0')).ToList();
            var listUserInfoInDB = _dbContext.IC_UserInfo.Where(e => listEmployeeATID.Contains(e.EmployeeATID) && e.CompanyIndex == userInfo.CompanyIndex).ToList();

            foreach (var userMaster in listUserInfo.ListUserInfo)
            {
                var existUserInfo = listUserInfoInDB.FirstOrDefault(e => e.EmployeeATID == userMaster.UserID.PadLeft(_config.MaxLenghtEmployeeATID,'0') && e.SerialNumber == listUserInfo.SerialNumber);

                if (existUserInfo == null)
                {
                    existUserInfo = new IC_UserInfo();
                    existUserInfo.CompanyIndex = userInfo.CompanyIndex;
                    existUserInfo.EmployeeATID = userMaster.UserID.PadLeft(_config.MaxLenghtEmployeeATID, '0');
                    existUserInfo.SerialNumber = listUserInfo.SerialNumber;

                    existUserInfo.UserName = string.IsNullOrWhiteSpace(userMaster.NameOnDevice) ? "" : userMaster.NameOnDevice;
                    if (existUserInfo.UserName.Length > 20)
                    {
                        existUserInfo.UserName = userMaster.NameOnDevice.Substring(0, 20);
                    }
                    existUserInfo.CardNumber = userMaster.CardNumber;
                    existUserInfo.Password = userMaster.PasswordOndevice;
                    existUserInfo.Privilege = (short)userMaster.Privilege;
                    existUserInfo.AuthenMode = AuthenMode.FullAccessRight.ToString();
                    existUserInfo.Reserve1 = "";
                    existUserInfo.Reserve2 = 0;
                    if (userMaster.FingerPrints != null && userMaster.FingerPrints.Count() > 0)
                    {
                        GetFingerDataLength(userMaster.FingerPrints, existUserInfo);
                    }
                    if (userMaster.Face != null)
                    {
                        existUserInfo.FaceTemplate = userMaster.Face != null ? userMaster.Face.FaceTemplate.Length : 0;
                    }
                    if (userMaster.FaceInfoV2 != null)
                    {
                        //if (userMaster.FaceInfoV2.TemplateBIODATA != null)
                        //{
                        //    existUserInfo.FaceTemplateV2 = userMaster.FaceInfoV2 != null ? userMaster.FaceInfoV2.TemplateBIODATA.Length : 0;
                        //}
                        //else
                        //{
                        existUserInfo.FaceTemplateV2 = userMaster.FaceInfoV2 != null ? userMaster.FaceInfoV2.Content.Length : 0;
                        //}
                    }


                    existUserInfo.CreatedDate = DateTime.Now;
                    existUserInfo.UpdatedUser = userInfo.UserName;
                    _dbContext.IC_UserInfo.Add(existUserInfo);
                }
                else
                {
                    if ((userMaster.NameOnDevice == null && userMaster.CardNumber == null && userMaster.PasswordOndevice == null) == false)
                    {
                        existUserInfo.UserName = userMaster.NameOnDevice;
                        existUserInfo.CardNumber = userMaster.CardNumber;
                        existUserInfo.Password = userMaster.PasswordOndevice;
                    }

                    if (userMaster.FingerPrints != null && userMaster.FingerPrints.Count() > 0)
                    {
                        GetFingerDataLength(userMaster.FingerPrints, existUserInfo);
                    }
                    if (userMaster.Face != null)
                    {
                        existUserInfo.FaceTemplate = userMaster.Face != null ? userMaster.Face.FaceTemplate.Length : 0;
                    }
                    if (userMaster.FaceInfoV2 != null)
                    {
                        //if (userMaster.FaceInfoV2.TemplateBIODATA != null)
                        //{
                        //    existUserInfo.FaceTemplateV2 = userMaster.FaceInfoV2 != null ? userMaster.FaceInfoV2.TemplateBIODATA.Length : 0;
                        //}
                        //else
                        //{
                        existUserInfo.FaceTemplateV2 = userMaster.FaceInfoV2 != null ? userMaster.FaceInfoV2.Content.Length : 0;
                        //}
                    }
                    existUserInfo.UpdatedDate = DateTime.Now;
                    existUserInfo.UpdatedUser = userInfo.UserName;
                    _dbContext.IC_UserInfo.Update(existUserInfo);
                }
            }
            _dbContext.SaveChanges();
            return listUserInfo;
        }
        public UserInfoParamV2 CheckCreateOrUpdateV2(UserInfoParamV2 listUserInfo, UserInfo userInfo)
        {

            var listEmployeeATID = listUserInfo.ListUserInfo.Select(e => e.UserID.PadLeft(_config.MaxLenghtEmployeeATID, '0')).ToList();
            var listUserInfoInDB = _dbContext.IC_UserInfo.Where(e => listEmployeeATID.Contains(e.EmployeeATID) && e.CompanyIndex == userInfo.CompanyIndex).ToList();
            foreach (var userMaster in listUserInfo.ListUserInfo)
            {
                var existUserInfo = listUserInfoInDB.FirstOrDefault(e => e.EmployeeATID == userMaster.UserID.PadLeft(_config.MaxLenghtEmployeeATID,'0') && e.SerialNumber == listUserInfo.SerialNumber);

                if (existUserInfo != null)
                {
                    if (userMaster.Face != null)
                    {
                        existUserInfo.FaceTemplateV2 = userMaster.Face != null ? userMaster.Face.Content != null ? userMaster.Face.Content.Length : 0 : 0;
                        existUserInfo.UpdatedUser = userInfo.UserName;
                        existUserInfo.UpdatedDate = DateTime.Now;
                        _dbContext.IC_UserInfo.Update(existUserInfo);
                    }
                }
                else
                {
                    if (userMaster.Face != null)
                    {
                        existUserInfo.FaceTemplateV2 = userMaster.Face != null ? userMaster.Face.Content != null ? userMaster.Face.Content.Length : 0 : 0;
                        existUserInfo.UpdatedUser = userInfo.UserName;
                        existUserInfo.UpdatedDate = DateTime.Now;
                        _dbContext.IC_UserInfo.Add(existUserInfo);
                    }
                }
            }
            _dbContext.SaveChanges();
            return listUserInfo;
        }
        //
        private void GetFingerDataLength(List<FingerInfo> listFinger , IC_UserInfo currentUserInfo)
        {
            foreach (var finger in listFinger) {
                switch (finger.FingerIndex) {
                    case 0:
                        currentUserInfo.FingerData0 = finger.FingerTemplate.Length;
                        break;
                    case 1:
                        currentUserInfo.FingerData1 = finger.FingerTemplate.Length;
                        break;
                    case 2:
                        currentUserInfo.FingerData2 = finger.FingerTemplate.Length;
                        break;
                    case 3:
                        currentUserInfo.FingerData3 = finger.FingerTemplate.Length;
                        break;
                    case 4:
                        currentUserInfo.FingerData4 = finger.FingerTemplate.Length;
                        break;
                    case 5:
                        currentUserInfo.FingerData5 = finger.FingerTemplate.Length;
                        break;
                    case 6:
                        currentUserInfo.FingerData6 = finger.FingerTemplate.Length;
                        break;
                    case 7:
                        currentUserInfo.FingerData7 = finger.FingerTemplate.Length;
                        break;
                    case 8:
                        currentUserInfo.FingerData8 = finger.FingerTemplate.Length;
                        break;
                    case 9:
                        currentUserInfo.FingerData9 = finger.FingerTemplate.Length;
                        break;
                }
            }
        }
        private IC_UserInfoDTO ConvertToDTO(IC_UserInfo data)
        {
            IC_UserInfoDTO dto = new IC_UserInfoDTO();
            dto.EmployeeATID = data.EmployeeATID;
            dto.CompanyIndex = data.CompanyIndex;
            dto.SerialNumber = data.SerialNumber;
            dto.UserName = data.UserName;
            dto.CardNumber = data.CardNumber;
            dto.Privilege = data.Privilege;
            dto.Password = data.Password;
            dto.Reserve1 = data.Reserve1;
            dto.Reserve2 = data.Reserve2;
            dto.AuthenMode = data.AuthenMode;
            dto.CreatedDate = data.CreatedDate;
            dto.UpdatedDate = data.UpdatedDate;
            dto.UpdatedUser = data.UpdatedUser;
            return dto;
        }
        private void ConvertDTOToData(IC_UserInfoDTO dto, IC_UserInfo data)
        {
            data.EmployeeATID = dto.EmployeeATID.PadLeft(_config.MaxLenghtEmployeeATID, '0');
            data.CompanyIndex = dto.CompanyIndex;
            data.SerialNumber = dto.SerialNumber;
            data.UserName = dto.UserName == null ? "User" : dto.UserName;
            data.CardNumber = dto.CardNumber == null ? "0" : dto.CardNumber;
            data.Privilege = dto.Privilege;
            data.Password = dto.Password;
            data.Reserve1 = dto.Reserve1;
            data.Reserve2 = dto.Reserve2;
            data.AuthenMode = dto.AuthenMode;
            data.CreatedDate = dto.CreatedDate;
            data.UpdatedDate = dto.UpdatedDate;
            data.UpdatedUser = dto.UpdatedUser;
        }
        private void ConvertDTOToDataWithoutNull(IC_UserInfoDTO dto, IC_UserInfo data)
        {
            if (!string.IsNullOrWhiteSpace(dto.UserName))
                data.UserName = dto.UserName;
            if (!string.IsNullOrWhiteSpace(dto.CardNumber))
                data.CardNumber = dto.CardNumber;
            if (dto.Privilege.HasValue)
                data.Privilege = dto.Privilege;
            if (!string.IsNullOrWhiteSpace(dto.Password))
                data.Password = dto.Password;
            if (!string.IsNullOrWhiteSpace(dto.Reserve1))
                data.Reserve1 = dto.Reserve1;
            if (dto.Reserve2.HasValue)
                data.Reserve2 = dto.Reserve2;
            if (!string.IsNullOrWhiteSpace(dto.AuthenMode))
                data.AuthenMode = dto.AuthenMode;

            data.CreatedDate = dto.CreatedDate;
            data.UpdatedDate = dto.UpdatedDate;
            data.UpdatedUser = dto.UpdatedUser;
        }
    }
    public interface IIC_UserInfoLogic
    {
        List<IC_UserInfoDTO> UpdateList(List<IC_UserInfoDTO> listUserInfo);
        List<IC_UserInfoDTO> SaveOrUpdateList(List<IC_UserInfoDTO> listUserInfo);
        List<IC_UserInfoDTO> SaveOrUpdateListField(List<IC_UserInfoDTO> listUserInfo);
        IC_UserInfoDTO UpdateField(List<AddedParam> addParams);
        bool UpdateListUserPrivilege(List<IC_UserInfoDTO> listEmployee);
        List<IC_UserInfoDTO> GetMany(List<AddedParam> addedParams);
        IC_UserInfoDTO Create(IC_UserInfoDTO item);
        IC_UserInfoDTO GetExist(string employeeID, int companyIndex, string serialNumber);
        bool Delete(List<string> listDevice, int companyIndex);
        string GetPrivilegeName(int privilege);
        UserInfoPram CheckCreateOrUpdate(UserInfoPram listUserInfo, UserInfo userInfo);
        UserInfoParamV2 CheckCreateOrUpdateV2(UserInfoParamV2 listUserInfo, UserInfo userInfo);
        List<IC_EmployeeDTO> GetUserInfoMany(List<AddedParam> addedParams);
    }
}