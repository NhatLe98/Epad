using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using EPAD_Backend_Core.Base;
using EPAD_Data;
using EPAD_Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace EPAD_Backend_Core.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("api/File/[action]")]
    [ApiController]
    public class IC_FileController : ApiControllerBase
    {
        static private EPAD_Context context;
        private IMemoryCache cache;
        public IC_FileController(IServiceProvider provider) : base(provider)
        {
            context = TryResolve<EPAD_Context>();
            cache = TryResolve<IMemoryCache>();
        }

        [Authorize]
        [ActionName("UserAddFile")]
        [HttpPost]
        public async Task<IActionResult> ImportAttendanceLog([FromForm] IFormFile file)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized("TokenExpired");
            }
            UploadFileResult uploadResult = new UploadFileResult();
            if (file == null || file.FileName == "")
            {
                uploadResult.Success = false; uploadResult.Error = "FileNotExist";
                return Ok(uploadResult);
            }
            DateTime now = DateTime.Now;
            string rootPath = AppDomain.CurrentDomain.BaseDirectory;
            string pathSaveFolder = "";
            string fileName = "";
            try
            {
                pathSaveFolder = $"Files/UserUpload/{user.UserName}/{now.ToString("yyyyMMdd")}";

                if (Directory.Exists(rootPath + pathSaveFolder) == false)
                {
                    Directory.CreateDirectory(rootPath + pathSaveFolder);
                }
                pathSaveFolder += "/";
                fileName = file.FileName;
                using (var stream = new FileStream(rootPath + pathSaveFolder + fileName, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
                uploadResult.Success = true;
                uploadResult.Path = pathSaveFolder + fileName;
            }
            catch(Exception ex)
            {
                uploadResult.Success = false; 
                uploadResult.Error = ex.Message;
            }
            
            return Ok(uploadResult);
        }
        [Authorize]
        [ActionName("ImportProcess")]
        [HttpPost]
        public async Task<IActionResult> ImportProcess([FromBody] ImportProcessParam param)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized("TokenExpired");
            }

            string rootPath = AppDomain.CurrentDomain.BaseDirectory;
            List<string> listFilePath = new List<string>();
            for (int i = 0; i < param.ListFilePath.Count; i++)
            {
                if(System.IO.File.Exists(rootPath+param.ListFilePath[i]) == true)
                {
                    listFilePath.Add(rootPath+param.ListFilePath[i]);
                }
            }
            string data = "";
            if (param.ProcessClass == "Log")
            {
                MainProcess.ImportProcess.IImportProcess logProcess = new MainProcess.ImportProcess.ImportLog();
                data = logProcess.Process(context, listFilePath,user.CompanyIndex,user.UserName);
            }

            return Ok(data);
        }
        [Authorize]
        [ActionName("UserRemoveFile")]
        [HttpPost]
        public async Task<IActionResult> UserRemoveFile([FromBody] UploadFileResult param)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized("TokenExpired");
            }

            string rootPath = AppDomain.CurrentDomain.BaseDirectory;
            try
            {
                if (param.Path != "" && System.IO.File.Exists(rootPath + param.Path) == true)
                {
                    System.IO.File.Delete(rootPath + param.Path);
                }
            }
            catch(Exception ex)
            {

            }
            return Ok();
        }

        public class UploadFileResult
        {
            public bool Success { get; set; }
            public string Error { get; set; }
            public string Path { get; set; }
            public UploadFileResult()
            {
                Error = "";
                Path = "";
            }
        }
        public class ImportProcessParam
        {
            public List<string> ListFilePath { get; set; }
            public string ProcessClass { get; set; }
        }
    }
}
