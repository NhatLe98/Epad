using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Bibliography;
using EPAD_Backend_Core.Base;
using EPAD_Backend_Core.Provider;
using EPAD_Common.Clients;
using EPAD_Common.Extensions;
using EPAD_Common.Types;
using EPAD_Common.Utility;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace EPAD_Backend_Core.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("api/License/[action]")]
    [ApiController]
    public class IC_LicenseController : ApiControllerBase
    {
        private EPAD_Context db;
        private IMemoryCache cache;
        protected readonly LicenseClient _licenseClient;
        protected readonly IServiceProvider serviceProvider;
        public IC_LicenseController(IServiceProvider provider) : base(provider)
        {
            db = TryResolve<EPAD_Context>();
            cache = TryResolve<IMemoryCache>();
            _licenseClient = TryResolve<LicenseClient>();
            serviceProvider = provider;
        }

        [Authorize]
        [ActionName("AddHardwareLicense")]
        [HttpPost]
        public async Task<IActionResult> UploadHardwareLicenseAsync([FromBody] List<HWLicenseRequest> request)
        {
            var user = UserInfo.GetFromCache(cache, User.Identity.Name);
            if (user == null) return Unauthorized("TokenExpired");

            var lstAppLicense = await db.IC_AppLicense.ToListAsync();
            var lstHWLicense = await db.IC_HardwareLicense.ToListAsync();
            var hwLCCache = cache.GetAllHWLicense();

            foreach (var appLicense in lstAppLicense)
            {
                foreach (var req in request)
                {
                    var hwLicenseInfo = StringHelper.GetHWLicenseInfo(appLicense.PublicKey, req.Key);
                    if (hwLicenseInfo == null) continue;

                    var hardwareLicense = lstHWLicense.FirstOrDefault(x => x.CompanyIndex == appLicense.CompanyIndex && x.Data == req.Key);
                    if (hardwareLicense == null)
                    {
                        db.Add(new IC_HardwareLicense()
                        {
                            CompanyIndex = appLicense.CompanyIndex,
                            Data = req.Key,
                            FileName = req.FileName,
                            Note = "",
                            UpdatedDate = DateTime.Now,
                            UpdatedUser = user.FullName
                        });
                    }
                }
            }
            db.SaveChanges();

            var checkHardwareLicense = serviceProvider.GetService<EPAD_Background.Schedule.Job.CheckHardwareLicense>();
            await checkHardwareLicense.DoWorkAsync();
            return Ok();
        }


        [AllowAnonymous]
        [ActionName("TestLicense")]
        [HttpPost]
        public async Task<IActionResult> TestLicense(string publicKey, string key)
        {
            publicKey = "61722fa4-2bec-45cd-97fa-9fc6dbbd72d9";
            key = "o+JQcpcWvAQWIHRH5YJxdVNNnlt9zfIRSk7MbVcYdGbpvN4WxMyawv/j73zWgFpeuWEssIPYUgIq+JiUDxtwcKy82pInHziWwoLMrNSU021LwK587Nl31Tg0PcfkU4gRj7mVC1cZqYJE+GEKa4RxC0IppVurMQPmnAJ+WnHmXDg=";
            var hwLicenseInfo = StringHelper.GetHWLicenseInfo(publicKey, key);

            return Ok();
        }
        [ActionName("ActivateLicense")]
        [HttpPost]
        public async Task<IActionResult> ActivateLicense([FromBody] ActivateRequest request)
        {
            var checkLock = cache.Get<LockActivate>("epad.cache.checkLockLicense");
            if (checkLock != null && checkLock.Count > 5)
            {
                if (checkLock.LockTo > DateTime.Now)
                {
                    return NotFound("MSG_LockedActivate");
                }
                else
                {
                    LockLicense(true);
                }
            }

            string computerIdentity = cache.GetComputerIdentify();
            if (computerIdentity == null)
            {
                return NotFound("CannotGetID2");
            }
            // kiểm tra active offline => cho phép add license thủ công, rồi chạy job checklicense, trong job có tự động check offline
            if (request.IsOffline && request.LicenseData != "")
            {
                try
                {
                    string decryptStr = AbriLicenseDecryptor.AbriLicenseDecryptor.Decrypt(request.LicenseData, request.LicenseKey);
                    AppLicenseInfoPure appLicenseInfo = JsonSerializer.Deserialize<AppLicenseInfoPure>(decryptStr);
                    if (appLicenseInfo.ComputerIdentify != computerIdentity)
                    {
                        LockLicense();
                        return NotFound("ServerComputerNotMatch");
                    }

                    if (appLicenseInfo.ValidTo < DateTime.Now)
                    {
                        LockLicense();
                        return NotFound("LicenseFileExpired");
                    }

                    if (appLicenseInfo.ExpiredDate < DateTime.Now)
                    {
                        LockLicense();
                        return NotFound("LicenseExpired");
                    }
                }
                catch
                {
                    LockLicense();
                    return NotFound("InvalidLicenseFile");
                }
                AddLicense(request.LicenseKey, request.LicenseData);
            }
            else
            {
                // khi check online hợp lệ thì add license và chạy job check license, ngược lại thì báo lỗi
                var lcData = await _licenseClient.GetAppLicenseInfoAsync(request.LicenseKey, computerIdentity);
                if (lcData.Error != "")
                {
                    LockLicense();
                    return NotFound(lcData.Error);
                }

                AddLicense(request.LicenseKey, lcData.LicenseInfo.EncryptedData);
            }

            LockLicense(true);
            var scheduler = serviceProvider.GetService<IScheduler>();

            Console.WriteLine("call check license manual");
            JobKey jobKey = new JobKey($"epad.job.checkLicense.{typeof(EPAD_Background.Schedule.Job.CheckLicense).Name}", "checkLicense");
            await scheduler.TriggerJob(jobKey);

            return Ok();
        }


        [Authorize]
        [ActionName("GetVersion")]
        [HttpGet]
        public IActionResult GetVersion()
        {
            string version = string.Empty;
            FileStream fileStream = new FileStream("version.txt", FileMode.Open);
            using (StreamReader reader = new StreamReader(fileStream))
            {
                version = reader.ReadLine();
            }

            var companyIndex = db.IC_Company.FirstOrDefault()?.Index ?? 2;
            var cacheKey = $"urn:license-key-company-{companyIndex}";
            var licenseInfo = cache.Get<AppLicenseInfoPure>(cacheKey);
            licenseInfo = null;

            if (licenseInfo == null)
            {
                var appLicense = db.IC_AppLicense.FirstOrDefault();
                if (appLicense != null)
                {
                    licenseInfo = StringHelper.GetLicenseInfoPure(appLicense.PublicKey, appLicense.Data);
                }
            }

            if (licenseInfo != null)
            {
                version += "<br/>License: " + licenseInfo.ExpiredDate.ToddMMyyyy();
            }

            return Ok(version);
        }

        private void AddLicense(string pKey, string pData)
        {
            var appLicenses = db.IC_AppLicense.ToList();
            if (appLicenses.Count == 0)
            {
                var lstCompany = db.IC_Company.ToList();
                foreach (var com in lstCompany)
                {
                    db.Add(new IC_AppLicense()
                    {
                        CompanyIndex = com.Index,
                        PublicKey = pKey,
                        UpdatedDate = System.DateTime.Now,
                        Data = pData
                    });
                }
                db.SaveChanges();
            }
            else
            {
                foreach (var license in appLicenses)
                {
                    license.PublicKey = pKey;
                    license.Data = pData;
                }
                db.UpdateRange(appLicenses);
                db.SaveChanges();
            }
        }
        private void LockLicense(bool success = false)
        {
            var checkLock = cache.Get<LockActivate>("epad.cache.checkLockLicense");
            if (checkLock == null) checkLock = new LockActivate() { Count = 0 };

            if (success)
            {
                checkLock.Count = 0;
                checkLock.LockTo = null;
            }
            else
            {
                checkLock.Count += 1;
                if (checkLock.Count >= 5)
                {
                    checkLock.LockTo = DateTime.Now.AddMinutes(10);
                }
            }


            cache.Set("epad.cache.checkLockLicense", checkLock);
        }

        [ActionName("GetID2")]
        [HttpGet]
        public IActionResult GetComputerIdentity()
        {
            string computerIdentity = cache.GetComputerIdentify();
            if (string.IsNullOrEmpty(computerIdentity))
            {
                computerIdentity = "CAN'T GET ID2";
            }
            return Ok(computerIdentity);
        }
    }

    public class HWLicenseRequest
    {
        public string Key { get; set; }
        public string FileName { get; set; }
    }

    public class ActivateRequest
    {
        public string LicenseKey { get; set; }
        public string LicenseData { get; set; }
        public bool IsOffline { get; set; }
    }

    public class LockActivate
    {
        public int Count { get; set; }
        public DateTime? LockTo { get; set; }
    }
}