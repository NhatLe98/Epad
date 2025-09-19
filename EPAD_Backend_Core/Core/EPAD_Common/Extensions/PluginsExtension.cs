using AbriDeviceHelper;
using AbriDeviceHelper.Extensions;
using EPAD_Common.Types;
using EPAD_Common.Utility;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace EPAD_Common.Extensions
{
    public static class PluginsExtension
    {
        public static string GetComputerIdentify(this IMemoryCache cache)
        {
            var rs = cache.Get<string>(StringHelper.ComputerIdentifyKey);
            if (string.IsNullOrEmpty(rs) == true)
            {
                rs = new DeviceIDBuilder()
                    .AddMacAddress()
                    .ToString();

                if(rs != "")
                    cache.Set(StringHelper.ComputerIdentifyKey, rs);
            }
            if(rs == "" || rs == null)
            {
                rs = null;
            }

            return rs;
        }
        public static List<string> GetAllCacheStartWith(this IMemoryCache cache, string pattern)
        {
            var field = typeof(MemoryCache).GetProperty("EntriesCollection", BindingFlags.NonPublic | BindingFlags.Instance);
            var collection = field.GetValue(cache) as ICollection;
            List<string> result = new List<string>();
            if (collection != null)
            {
                foreach (var item in collection)
                {
                    var methodInfo = item.GetType().GetProperty("Key");
                    var val = methodInfo.GetValue(item).ToString();
                    if (val.StartsWith(pattern))
                    {
                        result.Add(val);
                    }
                }
            }
            return result;
        }

        public static void RemoveAll(this IMemoryCache cache, List<string> keys)
        {
            foreach (var key in keys)
            {
                cache.Remove(key);
            }
        }

        public static Dictionary<string, HWLicenseInfo> GetAllHWLicense(this IMemoryCache cache, int companyIndex = 1)
        {
            var lcData = cache.Get<Dictionary<string, HWLicenseInfo>>($"{StringHelper.UrnHWLicensePrefix}{companyIndex}");
            if(lcData == null)
            {
                lcData = new Dictionary<string, HWLicenseInfo>();
                cache.Set($"{StringHelper.UrnHWLicensePrefix}{companyIndex}", lcData);
            }
            return lcData;
        }

        public static bool HaveHWLicense(this IMemoryCache cache, string serialNumber, int companyIndex = 1)
        {
#if DEBUG
            return true;
#endif
            var lcData = cache.Get<Dictionary<string, HWLicenseInfo>>($"{StringHelper.UrnHWLicensePrefix}{companyIndex}");
            if (lcData == null || !lcData.ContainsKey(serialNumber)) return false;
            if (lcData[serialNumber].ExpiredDate <= DateTime.Now) return false;
            return true;
        }

        public static Tuple<string, string> CheckHWLicenseExpireDate(this IMemoryCache cache, string serialNumber, int companyIndex = 1)
        {
            var result = new Tuple<string, string>(string.Empty, "LongTerm");
#if DEBUG
            return result;
#endif
            var lcData = cache.Get<Dictionary<string, HWLicenseInfo>>($"{StringHelper.UrnHWLicensePrefix}{companyIndex}");
            if (lcData == null || !lcData.ContainsKey(serialNumber))
            {
                result = new Tuple<string, string>(string.Empty, "NoLicense");
            }
            else if (lcData[serialNumber].ExpiredDate.Date < DateTime.Now.Date)
            {
                result = new Tuple<string, string>(lcData[serialNumber].ExpiredDate.ToddMMyyyy(), "Expired");
            }
            else if (lcData[serialNumber].ExpiredDate.Date >= DateTime.Now.Date 
                && lcData[serialNumber].ExpiredDate.Date <= DateTime.Now.AddDays(30).Date) 
            {
                result = new Tuple<string, string>(lcData[serialNumber].ExpiredDate.ToddMMyyyy(), "NearExpire");
            }
            else if (lcData[serialNumber].ExpiredDate.Date >= DateTime.Now.Date 
                && lcData[serialNumber].ExpiredDate.Date > DateTime.Now.AddDays(30).Date)
            {
                result = new Tuple<string, string>(lcData[serialNumber].ExpiredDate.ToddMMyyyy(), "LongTerm");
            }
            return result;
        }

        public static string IsValidHWLicense(this HWLicenseInfo value)
        {
#if DEBUG
            return "";
#endif
            if (value == null) return "MSG_InvalidHWLicense";
            if (value.ExpiredDate < DateTime.Now) return "MSG_HWLicenseExpired";
            return "";
        }
    }
}
