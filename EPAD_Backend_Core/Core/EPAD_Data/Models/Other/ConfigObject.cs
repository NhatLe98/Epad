using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace EPAD_Data.Models
{
    public class ConfigObject
    {
        public string MongoDBConnectionString { get; set; }
        public string RealTimeServerLink { get; set; }
        public string ServerLink { get; set; }
        public string PushNotificatioinLink { get; set; }
        public string IntegrateEmployeeLink { get; set; }
        public double LimitedTimeConnection { get; set; }
        public string SMTP_SERVER { get; set; }
        public string SMTP_USERNAME { get; set; }
        public string SMTP_PASSWORD { get; set; }

        public int SMTP_PORT { get; set; }
        public string SMTP_SENDER_NAME { get; set; }
        public bool SMTP_ENABLE_SSL { get; set; }
        public string SDKInterfaceService { get; set; }

        public bool IntegrateDBOther { get; set; }
        public int CompanyIndex { get; set; }
        public string WarningSoundRoot { get; set; }
        public string WisenetWaveServerLocalAddress { get; set; }
        public string WisenetWaveServerCloudAddress { get; set; }
        public string WisenetWaveServerUsername { get; set; }
        public string WisenetWaveServerPassword { get; set; }
        public int TimesResendMail { get; set; }
        public int MaxLenghtEmployeeATID { get; set; }
        public string AutoGenerateIDPrefix { get; set; }
        public string AutoGenerateCustomerIDPrefix { get; set; }
        public string EmergencyPrefixMachineName { get; set; }

        public static ConfigObject GetConfig(IMemoryCache pCache)
        {
            ConfigObject config = GetFromCache(pCache, "Config");

            if (config == null)
            {
                string filePath = AppDomain.CurrentDomain.BaseDirectory + "Config.json";
                using (StreamReader r = new StreamReader(filePath, Encoding.UTF8))
                {
                    string json = r.ReadToEnd();
                    config = JsonConvert.DeserializeObject<ConfigObject>(json);
                }

                config.AddToCache(pCache, "Config");
            }
            return config;
        }

        public void AddToCache(IMemoryCache cache, string pGuid)
        {
            cache.Set(pGuid, this);
        }
        /// <summary>
        /// Get user login object from cache with guid key
        /// </summary>
        /// <param name="pGuid"></param>
        /// <returns></returns>
        static public ConfigObject GetFromCache(IMemoryCache cache, string pGuid)
        {
            ConfigObject user = null;
            if (cache.TryGetValue(pGuid, out user) == false)
            {
                return null;
            }
            return user;
        }
        /// <summary>
        /// Remove user login object from cache with guid key
        /// </summary>
        /// <param name="pGuid"></param>
        static public void RemoveFromCache(IMemoryCache cache, string pGuid)
        {
            cache.Remove(pGuid);
        }

        static public double CheckDoubleNumber(string value)
        {
            double number, result;
            bool isDouble = Double.TryParse(value, out number);
            if (isDouble)
            {
                string[] values = value.Split(',').ToArray();
                result = (double.Parse(values[0]) + 1) * 10;
            }
            else
            {
                result = double.Parse(value) * 10;
            }
            return result;
        }
    }
}