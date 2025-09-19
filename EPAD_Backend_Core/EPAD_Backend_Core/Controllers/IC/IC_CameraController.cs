using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EPAD_Backend_Core.Base;
using EPAD_Backend_Core.WebUtilitys;
using EPAD_Common.Types;
using EPAD_Common.Utility;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using EPAD_Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace EPAD_Backend_Core.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("api/Camera/[action]")]
    [ApiController]
    public class IC_CameraController : ApiControllerBase
    {
        private EPAD_Context _context;
        private IMemoryCache _cache;
        private IIC_CameraService _IC_CameraService;
        public IC_CameraController(IServiceProvider provider) : base(provider)
        {
            _context = TryResolve<EPAD_Context>();
            _cache = TryResolve<IMemoryCache>();
            _IC_CameraService = TryResolve<IIC_CameraService>();
        }

        [Authorize]
        [ActionName("GetAllCamera")]
        [HttpGet]
        public async Task<IActionResult> GetAllCamera()
        {
            UserInfo user = UserInfo.GetFromCache(_cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            var dataResult = await _IC_CameraService.GetAllCamera(user.CompanyIndex);
            return Ok(dataResult);
        }

        [Authorize]
        [ActionName("GetCameraAtPage")]
        [HttpGet]
        public IActionResult GetCameraAtPage(int page, string filter, int limit)
        {
            UserInfo user = UserInfo.GetFromCache(_cache, User.Identity.Name);
            IActionResult result = Unauthorized();
           
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }
            int countPage = 0;
            List<IC_Camera> listCamera = new List<IC_Camera>();
            IC_Camera cameraModel = new IC_Camera();
            if (filter==null || filter == "")
            {
                countPage = _context.IC_Camera.Where(t => t.CompanyIndex == user.CompanyIndex).Count();
                if (page <= 1)
                {
                    //change 'GlobalParams.ROWS_NUMBER_IN_PAGE' to 'limit'
                    listCamera = _context.IC_Camera.Where(t => t.CompanyIndex == user.CompanyIndex).OrderBy(t => t.CreatedDate)
                        .Take(limit).ToList();
                }
                else
                {
                    int fromRow = limit * (page - 1);
                    listCamera = _context.IC_Camera.Where(t => t.CompanyIndex == user.CompanyIndex).OrderBy(t => t.CreatedDate)
                        .Skip(fromRow).Take(limit).ToList();
                }
            }
            else
            {
                countPage = _context.IC_Camera.FromSqlRaw(PublicFunctions.CreateQueryFilterAllColumn(cameraModel, user.CompanyIndex, filter)).Count();
                if (page <= 1)
                {
                    listCamera = _context.IC_Camera.FromSqlRaw(PublicFunctions.CreateQueryFilterAllColumn(cameraModel, user.CompanyIndex, filter))
                        .OrderBy(t => t.CreatedDate).Take(limit).ToList();
                }
                else
                {
                    int fromRow = GlobalParams.ROWS_NUMBER_IN_PAGE * (page - 1);
                    listCamera = _context.IC_Camera.FromSqlRaw(PublicFunctions.CreateQueryFilterAllColumn(cameraModel, user.CompanyIndex, filter))
                       .OrderBy(t => t.CreatedDate).Take(limit).Skip(fromRow).Take(limit).ToList();
                }
                    
            }
            DataGridClass dataGrid = new DataGridClass(countPage, listCamera);
            result = Ok(dataGrid);
            return result;
        }

        [Authorize]
        [ActionName("AddCamera")]
        [HttpPost]
        public IActionResult AddCamera([FromBody] CameraParam param)
        {
            UserInfo user = UserInfo.GetFromCache(_cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            param = StringHelper.RemoveWhiteSpace(param);
            if (Enum.IsDefined(typeof(CameraType), param.Type) == false)
            {
                return BadRequest("Camera type is invalid");
            }


            IC_Camera camera = new IC_Camera();
            camera.Name = param.Name;
            camera.IpAddress = param.IpAddress;
            camera.Port = int.Parse(param.Port);
            camera.Serial = param.Serial;
            camera.UserName = param.UserName;
            camera.Password = param.Password;
            camera.Description = param.Description;
            camera.Type = param.Type;
            camera = PublicFunctions.FillGeneralFields(camera, user,false);

            _context.IC_Camera.Add(camera);
            _context.SaveChanges();

            result = Ok(camera);
            return result;
        }
        [Authorize]
        [ActionName("UpdateCamera")]
        [HttpPost]
        public IActionResult UpdateCamera([FromBody] CameraParam param)
        {
            UserInfo user = UserInfo.GetFromCache(_cache, User.Identity.Name);
            IActionResult result = Unauthorized();

            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            param = StringHelper.RemoveWhiteSpace(param);
            if (Enum.IsDefined(typeof(CameraType), param.Type) == false)
            {
                return BadRequest("Camera type is invalid");
            }

            IC_Camera camera = _context.IC_Camera.Where(t => t.Index == param.Index).FirstOrDefault();
            if (camera == null)
            {
                return NotFound("CameraNotExist");
            }

            camera.Name = param.Name;
            camera.IpAddress = param.IpAddress;
            camera.Port = int.Parse(param.Port);
            camera.Serial = param.Serial;
            camera.UserName = param.UserName;
            camera.Password = param.Password;
            camera.Description = param.Description;
            camera.Type = param.Type;
            camera = PublicFunctions.FillGeneralFields(camera, user, true);

            _context.SaveChanges();

            result = Ok(camera);
            return result;
        }
        [ActionName("GetCameraPictureByCameraIndex")]
        [HttpGet]
        public IActionResult GetCameraPictureByCameraIndex(int cameraIndex,string channel)
        {

            UserInfo user = UserInfo.GetFromCache(_cache, User.Identity.Name);
            IActionResult result = Unauthorized();

            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            IC_Camera cameraInfo = _context.IC_Camera.Where(t => t.Index == cameraIndex).FirstOrDefault();
            if (cameraInfo == null)
            {
                return Ok(new CameraPictureResult { Success = false, Error = "Camera index invalid", Link = "" });
            }
            string error = "";
            string picturePath = PublicFunctions.GetImageFromCamera(cameraInfo.IpAddress, cameraInfo.Port.ToString(),
                cameraInfo.UserName, cameraInfo.Password, channel, cameraInfo.Index, ref error,4,user);
            if (error != "")
            {
                return Ok(new CameraPictureResult { Success = false, Error = error, Link = "" });
            }
            string link = this.Request.Scheme + "://" + this.Request.Host.Value + "/" + picturePath;

            return Ok(new CameraPictureResult { Success = true, Error = "Camera index invalid", Link = "" });
        }
        [Authorize]
        [ActionName("DeleteCamera")]
        [HttpPost]
        public IActionResult DeleteCamera([FromBody] List<int> listParam)
        {
            UserInfo user = UserInfo.GetFromCache(_cache, User.Identity.Name);
            IActionResult result = Unauthorized();

            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            _context.IC_Camera.RemoveRange(_context.IC_Camera.Where(t => listParam.Contains(t.Index)).ToList());
            _context.SaveChanges();

            result = Ok();
            return result;
        }
        public class CameraParam
        {
            public int Index { get; set; }
            public string Name { get; set; }
            public string Serial { get; set; }
            public string IpAddress { get; set; }
            public string Port { get; set; }
            public string UserName { get; set; }
            public string Password { get; set; }
            public string Description { get; set; }
            public string Type { get; set; }
        }
    }
}
