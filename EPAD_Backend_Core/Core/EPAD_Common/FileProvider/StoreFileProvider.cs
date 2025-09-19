using EPAD_Common.Extensions;
using EPAD_Data.Models;
using EPAD_Data.Models.Other;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace EPAD_Common.FileProvider
{
    public class StoreFileProvider : IStoreFileProvider
    {
        private readonly ILogger Logger;
        private readonly ILoggerFactory LoggerFactory;

        IMemoryCache _Cache;
        ConfigObject _Config;

        public StoreFileProvider(ILoggerFactory loggerFactory, IConfiguration appConfig, IServiceProvider provider)
        {
            LoggerFactory = loggerFactory;
            Logger = LoggerFactory.CreateLogger<StoreFileProvider>();
            _Cache = provider.GetService<IMemoryCache>();
            _Config = ConfigObject.GetConfig(_Cache);
        }

        public string getPath(string internalPath)
        {
            return _Config.WarningSoundRoot + "/" + _Config.CompanyIndex + "/" + internalPath;
        }

        public string getPath(string internalPath, int pCompanyIndex)
        {
            return _Config.WarningSoundRoot + "/" + pCompanyIndex + "/" + internalPath;
        }

        public bool SaveImageBase64(string imagBase64, string folder, string fileName, ref string error)
        {
            try
            {
                var base64string = imagBase64;
                if (imagBase64.StartsWith("data:image/png;base64,"))
                {
                    base64string = imagBase64.Replace("data:image/png;base64,", "");
                }
                if (imagBase64.StartsWith("data:image/jpeg;base64,"))
                {
                    base64string = imagBase64.Replace("data:image/jpeg;base64,", "");
                }
                var base64array = Convert.FromBase64String(base64string);
                var folderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "StaticFiles", folder);
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }
                var filePath = Path.Combine(folderPath, fileName);
                var fileCheck = fileName;
                if (fileName.EndsWith(".png"))
                {
                    fileCheck = fileName.Replace(".png", ".jpg");
                }
                else if (fileName.EndsWith(".jpg"))
                {
                    fileCheck = fileName.Replace(".jpg", ".png");
                }
                string filePathCheck = Path.Combine(folderPath, fileCheck);
                if (File.Exists(filePathCheck))
                {
                    File.Delete(filePathCheck);
                }
                System.IO.File.WriteAllBytes(filePath, base64array);
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogError("Save Image Base64 error: " + ex.StackTrace);
                error = ex.Message;
                return false;
            }

        }

        public EzFile uploadAndUpdateUrl(EzFile ezFile, string parentPath)
        {
            if (!ezFile.Url.StartsWith("data:") || ezFile.Url.Length <= 5)
                return ezFile;
            FileHelper.WriteAndEncodeFileFromBase64(
                            getPath(parentPath),
                            ezFile.Name,
                            ezFile.Url.Split(new string[] { ";base64," }, StringSplitOptions.None)[1]);
            ezFile.Url = parentPath;
            return ezFile;
        }
        public string uploadAndUpdateUrlBase(byte[] data, string parentPath, string fileName)
        {
            return FileHelper.WriteAndEncodeFileFromByte(
                            getPath(parentPath),
                            fileName,
                            data);
        }
    }
}
