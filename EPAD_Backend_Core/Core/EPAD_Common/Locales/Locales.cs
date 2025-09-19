using EPAD_Common.Enums;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace EPAD_Common.Locales
{
    public class Locales : ILocales
    {
        private readonly IMemoryCache _Cache;

        public Dictionary<string, string> VI { get; set; }
        public Dictionary<string, string> EN { get; set; }

        public Language Language { get; set; } = Language.VI;
        public Locales(IMemoryCache pCache, IServiceProvider provider)
        {
            _Cache = pCache;
            InitLanguage();
        }
        public bool AddResources(string pKey, string pVI, string pEN, out string pMessage)
        {
            pMessage = "";
            if (!VI.ContainsKey(pKey))
            {
                VI.Add(pKey, pVI);
                LocaleResource localResource = new LocaleResource()
                {
                    resources = VI
                };
                localResource.AddToCache(_Cache, "resources_vi");
                localResource.SaveToFile("vi", out pMessage);
            }

            if (!EN.ContainsKey(pKey))
            {
                EN.Add(pKey, pEN);
                LocaleResource localResource = new LocaleResource()
                {
                    resources = EN
                };
                localResource.AddToCache(_Cache, "resources_en");
                localResource.SaveToFile("en", out pMessage);
            }

            InitLanguage();
            return true;
        }

        public string GetCheckedString(string pKey)
        {
            string result = pKey;

            //if (_Context.Language == GlobalParams.Language.VI)
            //{
            //    if (VI.ContainsKey(pKey))
            //        result = VI[pKey];
            //}
            //else if (_Context.Language == GlobalParams.Language.EN)
            //{
            //    if (EN.ContainsKey(pKey))
            //        result = EN[pKey];
            //}
            return result;
        }

        public string GetCheckedStringWithLang(string pKey, Language plang = Language.VI)
        {

            string response = pKey;
            if (plang == Language.VI)
            {
                if (VI.ContainsKey(pKey))
                    response = VI[pKey];
            }
            else if (plang == Language.EN)
            {
                if (EN.ContainsKey(pKey))
                    response = EN[pKey];
            }
            return response;
            //return pKey;
        }

        public void InitLanguage()
        {
            VI = GetResources(_Cache, Language.VI)?.resources ?? new Dictionary<string, string>();
            EN = GetResources(_Cache, Language.EN)?.resources ?? new Dictionary<string, string>();
        }
        private LocaleResource GetResources(IMemoryCache pCache, Language pLang)
        {
            LocaleResource resource = LocaleResource.GetFromCache(pCache, $"resources_{pLang}");

            if (resource == null)
            {
                string filePath = AppDomain.CurrentDomain.BaseDirectory + $"StaticFiles/Locales/{pLang.ToString().ToLower()}.json";
                if (!File.Exists(filePath)) return resource;
                using (StreamReader r = new StreamReader(filePath))
                {
                    string json = r.ReadToEnd();
                    resource = Newtonsoft.Json.JsonConvert.DeserializeObject<LocaleResource>(json);
                }
                resource.AddToCache(pCache, $"resources_{pLang}");
            }
            return resource;
        }
        public void SetLanguage(Language pLang)
        {
            this.Language = pLang;
        }

        public class LocaleResource
        {
            public Dictionary<string, string> resources { get; set; }
            public void AddToCache(IMemoryCache cache, string pGuid)
            {
                cache.Set(pGuid, this, TimeSpan.FromHours(24));
            }
            public static LocaleResource GetFromCache(IMemoryCache pCache, string pKey)
            {
                LocaleResource resource = null;
                if (pCache.TryGetValue(pKey, out resource) == false)
                {
                    return null;
                }
                return resource;
            }
            public bool SaveToFile(string pLang, out string pMessage)
            {
                pMessage = "";
                try
                {
                    string filePath = AppDomain.CurrentDomain.BaseDirectory + $"StaticFiles/Locales/{pLang.ToString().ToLower()}.json";
                    using (StreamWriter sw = File.CreateText(filePath))
                    {
                        var resourceData = Newtonsoft.Json.JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.Indented);
                        sw.WriteLine(resourceData);
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    pMessage = ex.Message;
                    return false;
                }
            }
        }
    }
}
