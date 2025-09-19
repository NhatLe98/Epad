using EPAD_Common.Types;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Threading.Tasks;
using EPAD_Common.Extensions;
using EPAD_Common.Utility;

namespace EPAD_Background.Schedule.Job
{
    // [DisallowConcurrentExecution]
    public class CheckLicense : BaseJob
    {
        readonly ILogger _logger;
        public CheckLicense(IServiceScopeFactory provider, ILoggerFactory loggerFactory) : base(provider)
        {
            _logger = loggerFactory.CreateLogger<CheckLicense>();
        }

        public override async Task Execute(IJobExecutionContext context)
        {
        #if !DEBUG
            await DoWorkAsync();
        #endif
        }

        private async Task DoWorkAsync()
        {

            string computerIdentity = _cache.GetComputerIdentify();
            var lstLicense = await _db.IC_AppLicense.ToListAsync();
            if (lstLicense.Count == 0)
            {
                LogoutAllSession();
            }

            if (computerIdentity == null)
            {
                _logger.LogWarning("Cannot get ID2");
                return;
            }

            foreach (var lic in lstLicense)
            {
                string licCacheKey = $"{StringHelper.UrnAppLicensePrefix}{lic.CompanyIndex}";
                if (lic.PublicKey == "") continue;

                var lcData = await _licenseClient.GetAppLicenseInfoAsync(lic.PublicKey, computerIdentity);

                if (lcData.Error == "")
                {
                    // get license online
                    AppLicenseInfoPure appLicenseInfo = StringHelper.GetLicenseInfoPure(lic.PublicKey, lcData.LicenseInfo.EncryptedData);

                    if (appLicenseInfo == null)
                    {
                        _logger.LogInformation("Get license offline");
                        // get license online fail, do get license offline
                        appLicenseInfo = StringHelper.GetLicenseInfoPure(lic.PublicKey, lic.Data);
                        if (appLicenseInfo != null && appLicenseInfo.ValidTo < DateTime.Now)
                        {
                            appLicenseInfo = null;
                        }

                        if (appLicenseInfo != null && appLicenseInfo.ComputerIdentify != computerIdentity)
                        {
                            appLicenseInfo = null;
                        }
                    }
                    else
                    {
                        lic.Data = lcData.LicenseInfo.EncryptedData;
                    }

                    // applicense null khi sai publickey hoặc encrypt data
                    if (appLicenseInfo == null)
                    {
                        _logger.LogError("License invalid");
                        LogoutAllSession();
                    }
                    else if (appLicenseInfo.ExpiredDate < DateTime.Now.Date)
                    {
                        _logger.LogError("License expired");
                        LogoutAllSession();
                    }


                    if (appLicenseInfo == null)
                    {
                        _cache.Remove(licCacheKey);
                    }
                    else
                    {
                        ReInitLicenseCache(licCacheKey, appLicenseInfo);
                        lic.UpdatedDate = DateTime.Now;
                        lic.UpdatedUser = "LicenseSystem";
                        _db.Update(lic);
                        _db.SaveChanges();
                    }
                }
                else if (lcData.Error == "NoConnection")
                {
                    _logger.LogInformation("No connection, start check license offline");
                    CheckLicenseOffline(lic.PublicKey, lic.Data, licCacheKey, computerIdentity);
                }
                else
                {
                    _logger.LogError($"Check license failed: CompanyIndex - {lic.CompanyIndex}, Error - {lcData.Error}");
                    CheckLicenseOffline(lic.PublicKey, lic.Data, licCacheKey, computerIdentity);
                }
            }
        }

        private void LogoutAllSession()
        {
            var allAuthKey = _cache.GetAllCacheStartWith(StringHelper.UrnAuthInfoPrefix);
            _cache.RemoveAll(allAuthKey);
            _logger.LogInformation("Logout all session");
        }

       

        internal void CheckLicenseOffline(string pPublicKey, string pEncryptData, string pLicCacheKey, string pComputerIdentity)
        {
            AppLicenseInfoPure appLicenseInfo = StringHelper.GetLicenseInfoPure(pPublicKey, pEncryptData);
            if (appLicenseInfo != null && appLicenseInfo.ValidTo >= DateTime.Now && appLicenseInfo.ExpiredDate >= DateTime.Now && appLicenseInfo.ComputerIdentify == pComputerIdentity)
            {
                ReInitLicenseCache(pLicCacheKey, appLicenseInfo);
            }
            else
            {
                _logger.LogError("License invalid or expired"); 
                _cache.Remove(pLicCacheKey);
                LogoutAllSession();
            }
        }

        private void ReInitLicenseCache(string pCachekey, AppLicenseInfoPure appLicenseInfo)
        {
            //Console.ForegroundColor = ConsoleColor.DarkGreen;
            //Console.WriteLine();
            //Console.WriteLine("Check license success");
            //Console.ResetColor();
            //var tempCache = _cache.Get<AppLicenseInfoPure>(pCachekey);
            _cache.Set(pCachekey, appLicenseInfo);
        }
    }
}
