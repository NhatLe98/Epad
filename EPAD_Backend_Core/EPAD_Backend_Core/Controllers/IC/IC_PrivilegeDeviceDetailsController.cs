using EPAD_Backend_Core.Base;
using EPAD_Backend_Core.MainProcess;
using EPAD_Backend_Core.WebUtilitys;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using MongoDB.Bson.IO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;


namespace EPAD_Backend_Core.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("api/PrivilegeDeviceDetails/[action]")]
    [ApiController]
    public class IC_PrivilegeDeviceDetailsController : ApiControllerBase
    {
        private EPAD_Context context;
        private IMemoryCache cache;
        public IC_PrivilegeDeviceDetailsController(IServiceProvider provider):base(provider)
        {
            context = TryResolve<EPAD_Context>();
            cache = TryResolve<IMemoryCache>();
        }

        [Authorize]
        [ActionName("GetPrivilegeDeviceDetail")]
        [HttpGet]
        public IActionResult GetPrivilegeDeviceDetail()
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }
            List<IC_PrivilegeDeviceDetails> listPriDetail = context.IC_PrivilegeDeviceDetails.Where(t => t.CompanyIndex == user.CompanyIndex).ToList();

            List<IC_Device> listDevice = context.IC_Device.ToList();

            var data = new List<IC_PrivilegeDeviceDetails>();
            foreach(var item in listDevice) {
                var existed = listPriDetail.Where(e=>e.SerialNumber == item.SerialNumber);
                if (existed != null)
                {
                    foreach(var itemExisted in existed)
                    {
                        data.Add(new IC_PrivilegeDeviceDetails { PrivilegeIndex = itemExisted.PrivilegeIndex, SerialNumber = itemExisted.SerialNumber, Role = itemExisted.Role });
                    }
                    
                }
                else {
                    data.Add(new IC_PrivilegeDeviceDetails {  SerialNumber = item.SerialNumber});
                }
            }

            result = Ok(data);
            return result;
        }

        [Authorize]
        [ActionName("InsertOrUpdatePrivilegeDeviceDetail")]
        [HttpPost]
        public IActionResult InsertOrUpdatePrivilegeDeviceDetail([FromBody]List<PrivilegeDeviceDetailParam> lsparam)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }
            List<IC_PrivilegeDeviceDetails> listPriDetail = context.IC_PrivilegeDeviceDetails.Where(x => x.CompanyIndex == user.CompanyIndex).ToList();
            List<IC_UserPrivilege> listPri = context.IC_UserPrivilege.Where(x => x.CompanyIndex == user.CompanyIndex).ToList();
            context.IC_PrivilegeDeviceDetails.RemoveRange(listPriDetail);

            foreach (var p in lsparam)
            {
                foreach (var role in p.Roles)
                {
                    IC_UserPrivilege userPri = listPri.FirstOrDefault(x => x.Index == role.PrivilegeId);
                    if (userPri is null) continue;
                    IC_PrivilegeDeviceDetails dt = new IC_PrivilegeDeviceDetails();
                    dt.CompanyIndex = user.CompanyIndex;
                    dt.UpdatedDate = DateTime.Now;
                    dt.UpdatedUser = user.UserName;

                    dt.PrivilegeIndex = role.PrivilegeId;
                    dt.SerialNumber = p.SerialNumber;
                    dt.Role = GetRole(role.State);

                    context.IC_PrivilegeDeviceDetails.Add(dt);
                }
            }
            context.SaveChanges();
            result = Ok();
            return result;
        }

        public class PrivilegeDeviceDetailParam
        {
            public string SerialNumber { get; set; }
            public List<Role> Roles { get; set; }

            public PrivilegeDeviceDetailParam()
            {
                Roles = new List<Role>();
            }
        }
        private string GetRole(string role)
        {
            Privilege pri;
            bool parseResult = Enum.TryParse(role, out pri);
            return parseResult ? pri.ToString() : Privilege.None.ToString();
        }
    }
}