using EPAD_Backend_Core.Base;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;


namespace EPAD_Backend_Core.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("api/PrivilegeDetail/[action]")]
    [ApiController]
    public class IC_PrivilegeDetailController : ApiControllerBase
    {
        private EPAD_Context context;
        private IMemoryCache cache;
        public IC_PrivilegeDetailController(IServiceProvider provider) :base(provider)
        {
            context = TryResolve<EPAD_Context>();
            cache = TryResolve<IMemoryCache>();
        }
        [Authorize]
        [ActionName("GetPrivilegeDetail")]
        [HttpGet]
        public IActionResult GetPrivilegeDetail()
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized("TokenExpired");
            }
            List<IC_PrivilegeDetails> listPriDetail = context.IC_PrivilegeDetails.Where(t => t.CompanyIndex == user.CompanyIndex).ToList();

            var jsonData = from t in listPriDetail
                           select new
                           {
                               PrivilegeIndex = t.PrivilegeIndex,
                               FormName = t.FormName,
                               Role = t.Role
                           };

            result = Ok(jsonData);
            return result;
        }

        [Authorize]
        [ActionName("GetPrivilegeDetailByForm")]
        [HttpGet]
        public IActionResult GetPrivilegeDetailByForm(string formKey)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized("TokenExpired");
            }
            List<IC_PrivilegeDetails> listPriDetail = context.IC_PrivilegeDetails.Where(x => x.CompanyIndex == user.CompanyIndex && x.FormName.Equals(formKey, StringComparison.OrdinalIgnoreCase)).ToList();

            result = Ok(listPriDetail);
            return result;
        }

        [Authorize]
        [ActionName("GetPrivilegeDetailByPrivilegeGroup")]
        [HttpGet]
        public IActionResult GetPrivilegeDetailByPrivilegeGroup(int privilegeGroupIndex)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized("TokenExpired");
            }
            List<IC_PrivilegeDetails> listPriDetail = context.IC_PrivilegeDetails.Where(x => x.CompanyIndex == user.CompanyIndex && x.PrivilegeIndex == privilegeGroupIndex).ToList();

            result = Ok(listPriDetail);
            return result;
        }

        [Authorize]
        [ActionName("UpdatePrivilegeDetail")]
        [HttpPost]
        public IActionResult UpdatePrivilegeDetail([FromBody]List<IC_PrivilegeDetailParamDTO> param)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized("TokenExpired");
            }
            
            List<IC_PrivilegeDetails> listPriDetail = context.IC_PrivilegeDetails.Where(x => x.CompanyIndex == user.CompanyIndex).ToList();
            List<IC_UserPrivilege> listPri = context.IC_UserPrivilege.Where(x => x.CompanyIndex == user.CompanyIndex).ToList();

            context.IC_PrivilegeDetails.RemoveRange(listPriDetail);
            
            foreach(IC_PrivilegeDetailParamDTO p in param)
            {
                foreach(Role role in p.Roles)
                {
                    IC_UserPrivilege userPri = listPri.FirstOrDefault(x => x.Index == role.PrivilegeId);
                    if (userPri is null) continue;
                    IC_PrivilegeDetails dt = new IC_PrivilegeDetails();
                    dt.CompanyIndex = user.CompanyIndex;
                    dt.UpdatedDate = DateTime.Now;
                    dt.UpdatedUser = user.UserName;

                    dt.PrivilegeIndex = role.PrivilegeId;
                    dt.FormName = p.Menu;
                    dt.Role = GetRole(role.State);

                    context.IC_PrivilegeDetails.Add(dt);
                }
            }

            context.SaveChanges();

            result = Ok();
            return result;
        }

        [Authorize]
        [ActionName("GetCurrentPrivilege")]
        [HttpGet]
        public IActionResult GetCurrentPrivilege()
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized("TokenExpired");
            }
            if (user?.ListPrivilege == null)
            {
                return BadRequest("user.ListPrivilege is null");
            }
            var rs = from pri in user.ListPrivilege
                     select new
                     {
                         FormName = pri.FormName,
                         Roles = pri.Roles.Select(x => x.ToString())
                     };
            
            result = Ok(rs);
            return result;
        }

        [Authorize]
        [ActionName("CheckPrivilege")]
        [HttpGet]
        public IActionResult CheckPrivilege(string formName)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized("TokenExpired");
            }

            var rs = user.ListPrivilege?.FirstOrDefault(x => x.FormName == formName);

            bool accept = false;

            if(rs != null && rs.Roles.Count > 0 && rs.Roles.IndexOf(FormRole.None) == -1)
            {
                accept = true;
            }


            result = Ok(accept);
            return result;
        }

        [Authorize]
        [ActionName("CheckPrivilegeFull")]
        [HttpGet]
        public IActionResult CheckPrivilegeFull(string formName)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized("TokenExpired");
            }

            var rs = user.ListPrivilege?.FirstOrDefault(x => x.FormName == formName);

            bool accept = false;

            if (rs != null && rs.Roles.Count > 0 && rs.Roles.IndexOf(FormRole.Full) >= 0)
            {
                accept = true;
            }


            result = Ok(accept);
            return result;
        }


        protected string GetRole(string role)
        {
            Privilege pri;
            bool parseResult = Enum.TryParse(role, out pri);
            return parseResult ? pri.ToString() : Privilege.None.ToString();
        }
    }
}
