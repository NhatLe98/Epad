using EPAD_Backend_Core.Base;
using EPAD_Common.Types;
using EPAD_Common.Utility;
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
    [Route("api/UserPrivilege/[action]")]
    [ApiController]
    public class IC_UserPrivilegeController : ApiControllerBase
    {

        private EPAD_Context context;
        private IMemoryCache cache;
        public IC_UserPrivilegeController(IServiceProvider provider):base(provider)
        {
            context = TryResolve<EPAD_Context>();
            cache = TryResolve<IMemoryCache>();
        }

        [Authorize]
        [ActionName("GetUserPrivilege")]
        [HttpGet]
        public IActionResult GetUserPrivilege()
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized("TokenExpired");
            }

            List<IC_UserPrivilege> userGroup = null;


            userGroup = context.IC_UserPrivilege.Where(t => t.CompanyIndex == user.CompanyIndex).ToList();
            var dep = from _user in userGroup
                      select new
                      {
                          value = _user.Index.ToString(),
                          label = _user.Name
                      };

            result = Ok(dep);
            return result;
        }
        [Authorize]
        [ActionName("GetUserPrivilegeAtPage")]
        [HttpGet]
        public IActionResult GetUserAccountAtPage(int page, string filter,int limit)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized("TokenExpired");
            }

            List<IC_UserPrivilege> accountList = null;
            int countPage = 0;
            if (string.IsNullOrEmpty(filter))
            {
                if (page <= 1)
                {
                    accountList = context.IC_UserPrivilege.Where(t => t.CompanyIndex == user.CompanyIndex).Take(limit).ToList();

                }
                else
                {
                    int fromRow = limit * (page - 1);
                    accountList = context.IC_UserPrivilege.Where(t => t.CompanyIndex == user.CompanyIndex)
                        .Skip(fromRow).Take(limit).ToList();
                }
            }
            else
            {
                if (page <= 1)
                {
                   var lsAccountList = context.IC_UserPrivilege.Where(t => t.CompanyIndex == user.CompanyIndex && (
                      t.Name.Contains(filter)
                    || t.Note.Contains(filter)
                    || t.UpdatedUser.Contains(filter)
                    || (t.IsAdmin ? "Có" : "Không").Contains(filter)
                    || (t.UpdatedDate.Value.ToString().Contains(filter))));

                    accountList = lsAccountList.Take(limit).ToList();
                    countPage = lsAccountList.Count();
                }
                else
                {
                    int fromRow = limit * (page - 1);
                    var lsAccountList = context.IC_UserPrivilege.Where(t => t.CompanyIndex == user.CompanyIndex && (
                      t.Name.Contains(filter)
                    || t.Note.Contains(filter)
                    || t.UpdatedUser.Contains(filter)
                    || (t.IsAdmin ? "Có" : "Không").Contains(filter)
                    || (t.UpdatedDate.Value.ToString().Contains(filter))
                    )).OrderBy(t => t.Name);

                    accountList = lsAccountList.Skip(fromRow).Take(limit).ToList();
                    countPage = lsAccountList.Count();
                }
            }
            var obj = from t in accountList
                      select new
                      {
                          Index = t.Index,
                          Name = t.Name,
                          UseForDefault = t.UseForDefault,
                          IsUseForDefault = t.UseForDefault == true ? "Có" : "Không",
                          IsAdminName = t.IsAdmin == true ? "Có" : "Không",
                          IsAdmin = t.IsAdmin,
                          Note = t.Note,
                          UpdatedDate = t.UpdatedDate.HasValue ? t.UpdatedDate.Value.ToString("dd/MM/yyyy") : "",
                          UpdatedUser = t.UpdatedUser
                      };

            int record = 0;
            if (string.IsNullOrEmpty(filter))
            {
                 record = context.IC_UserPrivilege.Where(t => t.CompanyIndex == user.CompanyIndex).ToList().Count;
            }
            else
            {
                record = countPage;
            }
            //double totalPage = ConfigObject.CheckDoubleNumber((record / GlobalParams.ROWS_NUMBER_IN_PAGE).ToString());
            DataGridClass dataGrid = new DataGridClass(record, obj);

            result = Ok(dataGrid);
            return result;
        }

        [Authorize]
        [ActionName("GetAllUserPrivilege")]
        [HttpGet]
        public IActionResult GetAllUserPrivilege()
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized("TokenExpired");
            }


            List<IC_UserPrivilege> accountList = context.IC_UserPrivilege.Where(t => t.CompanyIndex == user.CompanyIndex).ToList();

            var datajson = from t in accountList
                           select new
                           {
                               Index = t.Index,
                               Name = t.Name
                           };

            result = Ok(datajson);
            return result;
        }



        [Authorize]
        [ActionName("AddUserPrivilege")]
        [HttpPost]
        public IActionResult AddUserPrivilege([FromBody]UserPrivilegeParam param)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized("TokenExpired");
            }

            param = (UserPrivilegeParam)StringHelper.RemoveWhiteSpace(param);
            if (param.Name == "")
            {
                return BadRequest("PleaseFillAllRequiredFields");
            }
            IC_UserPrivilege checkData = context.IC_UserPrivilege
                .Where(t => t.CompanyIndex == user.CompanyIndex && t.Name == param.Name).FirstOrDefault();
            if (checkData != null)
            {
                return BadRequest("UserPrivilegeIsExist");
            }
            IC_UserPrivilege UserPrivilege = new IC_UserPrivilege();
            UserPrivilege.Name = param.Name;
            UserPrivilege.UseForDefault = param.UseForDefault;
            UserPrivilege.IsAdmin = param.IsAdmin;
            UserPrivilege.Note = param.Note;
            UserPrivilege.CompanyIndex = user.CompanyIndex;
            UserPrivilege.CreatedDate = DateTime.Now;
            UserPrivilege.UpdatedDate = DateTime.Now;
            UserPrivilege.UpdatedUser = user.FullName;
            context.IC_UserPrivilege.Add(UserPrivilege);
            context.SaveChanges();

            result = Ok();
            return result;
        }
        [ActionName("UpdateUserPrivilege")]
        [HttpPost]
        public IActionResult UpdateUserPrivilege([FromBody]UserPrivilegeParam param)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized("TokenExpired");
            }

            param = (UserPrivilegeParam)StringHelper.RemoveWhiteSpace(param);
            if (param.Name == "")
            {
                return BadRequest("PleaseFillAllRequiredFields");
            }

            IC_UserPrivilege updateData = context.IC_UserPrivilege.Where(t => t.CompanyIndex == user.CompanyIndex && t.Index == param.Index).FirstOrDefault();


            updateData.Name = param.Name;
            updateData.UseForDefault = param.UseForDefault;
            updateData.IsAdmin = param.IsAdmin;
            updateData.Note = param.Note;

            updateData.UpdatedDate = DateTime.Now;
            updateData.UpdatedUser = user.FullName;

            context.SaveChanges();

            result = Ok();
            return result;
        }
        [Authorize]
        [ActionName("DeleteUserPrivilege")]
        [HttpPost]
        public IActionResult DeleteDepartment([FromBody]List<UserPrivilegeParam> lsparam)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized("TokenExpired");
            }

            foreach (var param in lsparam)
            {
                IC_UserPrivilege deleteData = context.IC_UserPrivilege.Where(t => t.CompanyIndex == user.CompanyIndex && t.Index == param.Index).FirstOrDefault();
                IC_UserAccount checkDataExistUser = context.IC_UserAccount.Where(t => t.CompanyIndex == user.CompanyIndex && t.AccountPrivilege == param.Index).FirstOrDefault();
                if (deleteData == null)
                {
                    return NotFound("DepartmentNotExist");
                }
                else if (checkDataExistUser != null)
                {
                    return NotFound("UserAccountExist");
                }
                else
                {
                    context.IC_UserPrivilege.Remove(deleteData);
                }
            }
            context.SaveChanges();

            result = Ok();
            return result;
        }

    }
    public class UserPrivilegeParam
    {
        public int Index { get; set; }
        public string Name { get; set; }

        public bool UseForDefault { get; set; }

        public bool IsAdmin { get; set; }

        public string Note { get; set; }


    }
}