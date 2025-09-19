using EPAD_Common.Types;
using EPAD_Data;
using EPAD_Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EPAD_Logic
{
    public class IC_DeviceLogic : IIC_DeviceLogic
    {
        private EPAD_Context _dbContext;
        public IC_DeviceLogic(EPAD_Context dbContext)
        {
            _dbContext = dbContext;
        }
        public List<IC_DeviceDTO> GetManyDeviceWithPrivilege(List<AddedParam> addeParams)
        {
            if (addeParams == null)
                return new List<IC_DeviceDTO>();

            var username = addeParams.FirstOrDefault(u => u.Key == "UserName");
            if (username == null)
            {
                return new List<IC_DeviceDTO>();
            }

            var isAdmin = Convert.ToBoolean(addeParams.FirstOrDefault(u => u.Key == "IsAdmin").Value);

            if (isAdmin)
            {
                var query = (from d in _dbContext.IC_Device
                             select new IC_DeviceDTO
                             {
                                 CompanyIndex = d.CompanyIndex,
                                 SerialNumber = d.SerialNumber,
                                 AliasName = d.AliasName,
                                 IPAddress = d.IPAddress,
                                 Port = d.Port,
                                 LastConnection = d.LastConnection,
                                 UserCount = d.UserCount,
                                 FingerCount = d.FingerCount,
                                 FaceCount = d.FaceCount,
                                 AdminCount = d.AdminCount,
                                 DeviceType = d.DeviceType,
                                 DeviceNumber = d.DeviceNumber,
                             }
                         ).AsQueryable();
                if (addeParams != null)
                {
                    foreach (AddedParam param in addeParams)
                    {
                        switch (param.Key)
                        {
                            case "CompanyIndex":
                                if (param.Value != null)
                                {
                                    var companyIndex = Convert.ToInt32(param.Value);
                                    query = query.Where(u => u.CompanyIndex == companyIndex);
                                }
                                break;
                        }
                    }
                }
                return query.ToList();
            }
            else
            {
                var query = (from d in _dbContext.IC_Device
                             join pridv in _dbContext.IC_PrivilegeDeviceDetails on d.SerialNumber equals pridv.SerialNumber
                             join pri in _dbContext.IC_UserPrivilege on pridv.PrivilegeIndex equals pri.Index
                             join ac in _dbContext.IC_UserAccount on pri.Index equals ac.AccountPrivilege
                             where isAdmin || (ac.UserName.Equals(username.Value.ToString()) && pridv.Role.Equals(Privilege.None.ToString()) == false)
                             select new IC_DeviceDTO
                             {
                                 CompanyIndex = d.CompanyIndex,
                                 SerialNumber = d.SerialNumber,
                                 AliasName = d.AliasName,
                                 IPAddress = d.IPAddress,
                                 Port = d.Port,
                                 LastConnection = d.LastConnection,
                                 UserCount = d.UserCount,
                                 FingerCount = d.FingerCount,
                                 FaceCount = d.FaceCount,
                                 AdminCount = d.AdminCount,
                                 DeviceType = d.DeviceType,
                                 DeviceNumber = d.DeviceNumber,
                             }
                          ).AsQueryable();
                if (addeParams != null)
                {
                    foreach (AddedParam param in addeParams)
                    {
                        switch (param.Key)
                        {
                            case "CompanyIndex":
                                if (param.Value != null)
                                {
                                    var companyIndex = Convert.ToInt32(param.Value);
                                    query = query.Where(u => u.CompanyIndex == companyIndex);
                                }
                                break;
                        }
                    }
                }
                return query.ToList();
            }
        }

        public List<IC_DeviceDTO> GetMany(List<AddedParam> addedParams)
        {
            if (addedParams == null) return null;

            var query = _dbContext.IC_Device.AsQueryable();
            if (addedParams != null)
            {
                foreach (AddedParam param in addedParams)
                {
                    switch (param.Key)
                    {
                        case "CompanyIndex":
                            if (param.Value != null)
                            {
                                var companyIndex = Convert.ToInt32(param.Value);
                                query = query.Where(u => u.CompanyIndex == companyIndex);
                            }
                            break;
                    }
                }
            }
            var data = query.Select(e => new IC_DeviceDTO
            {
                CompanyIndex = e.CompanyIndex,
                SerialNumber = e.SerialNumber,
                AliasName = e.AliasName,
                IPAddress = e.IPAddress,
                Port = e.Port,
                LastConnection = e.LastConnection,
                UserCount = e.UserCount,
                FingerCount = e.FingerCount,
                FaceCount = e.FaceCount,
                AttendanceLogCount = e.AttendanceLogCount,
                AdminCount = e.AdminCount,
                UserCapacity = e.UserCapacity,
                FingerCapacity = e.FingerCapacity,
                FaceCapacity = e.FaceCapacity,
                AttendanceLogCapacity = e.AttendanceLogCapacity,
                DeviceType = e.DeviceType,
                DeviceNumber = e.DeviceNumber,
            }).ToList();
            return data;
        }
    }
    public interface IIC_DeviceLogic
    {
        List<IC_DeviceDTO> GetManyDeviceWithPrivilege(List<AddedParam> addeParams);
        List<IC_DeviceDTO> GetMany(List<AddedParam> addedParams);
    }
}
