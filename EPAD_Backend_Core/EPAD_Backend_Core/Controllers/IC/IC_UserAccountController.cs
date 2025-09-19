using EPAD_Backend_Core.Base;
using EPAD_Backend_Core.WebUtilitys;
using EPAD_Common.Types;
using EPAD_Common.Utility;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using EPAD_Logic;
using EPAD_Logic.SendMail;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EPAD_Common.Extensions;
using EPAD_Services.Interface;

namespace EPAD_Backend_Core.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("api/UserAccount/[action]")]
    [ApiController]
    public class IC_UserAccountController : ApiControllerBase
    {
        private EPAD_Context context;
        private IMemoryCache cache;
        private IIC_AuditLogic _iIC_AuditLogic;
        private IIC_UserAccountService _iIC_UserAccountService;
        public IC_UserAccountController(IServiceProvider provider):base(provider)
        {
            context = TryResolve<EPAD_Context>();
            cache = TryResolve<IMemoryCache>();
            _iIC_AuditLogic = TryResolve<IIC_AuditLogic>();
            _iIC_UserAccountService = TryResolve<IIC_UserAccountService>();
        }
        [Authorize]
        [ActionName("CheckValidPassword")]
        [HttpGet]
        public IActionResult CheckValidPassword([FromQuery] string username, [FromQuery] string password)
        {
            var result = _iIC_UserAccountService.CheckAccountExisted(username, password);

            return ApiOk(result);
        }
        [Authorize]
        [ActionName("GetAllUserAccount")]
        [HttpGet]
        public IActionResult GetAllUserAccount()
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            List<IC_UserAccount> accountList = context.IC_UserAccount.Where(t => t.CompanyIndex == user.CompanyIndex).ToList();

            result = Ok(accountList);
            return result;
        }
        [Authorize]
        [ActionName("GetUserAccountAtPage")]
        [HttpGet]
        public IActionResult GetUserAccountAtPage(int page, string filter, int limit)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            List<IC_UserAccount> accounts = null;
            int countPage = 0;
            if (string.IsNullOrEmpty(filter))
            {
                if (page <= 1)
                {
                    accounts = context.IC_UserAccount.Where(t => t.CompanyIndex == user.CompanyIndex).Take(limit).ToList();
                }
                else
                {
                    int fromRow = limit * (page - 1);
                    accounts = context.IC_UserAccount.Where(t => t.CompanyIndex == user.CompanyIndex)
                        .Skip(fromRow).Take(limit).ToList();
                }
            }
            else
            {
                if (page <= 1)
                {
                   var lsAccounts = context.IC_UserAccount.Join(context.IC_UserPrivilege, ac => ac.AccountPrivilege, pr => pr.Index, (ac, pr) => new { ac, pr })
                        .AsEnumerable()
                        .Where(t => t.ac.CompanyIndex == user.CompanyIndex && (
                                   t.ac.UserName.Contains(filter)
                                || t.ac.Name.Contains(filter)
                                || t.ac.UpdatedUser.Contains(filter)
                                || t.pr.Name.Contains(filter)
                                || (t.ac.UpdatedDate.GetValueOrDefault().ToString("d/M/yyyy HH:mm:ss tt").Contains(filter))
                                )).Select(t => t.ac);
                    accounts = lsAccounts.Take(limit).ToList();
                    countPage = lsAccounts.Count();
                }
                else
                {
                    int fromRow = limit * (page - 1);
                    var lsAccounts = context.IC_UserAccount.Join(context.IC_UserPrivilege, ac => ac.AccountPrivilege, pr => pr.Index, (ac, pr) => new { ac, pr })
                        .AsEnumerable()
                        .Where(t => t.ac.CompanyIndex == user.CompanyIndex && (
                               t.ac.UserName.Contains(filter)
                            || t.ac.Name.Contains(filter)
                            || t.ac.UpdatedUser.Contains(filter)
                            || t.pr.Name.Contains(filter))
                            || (t.ac.UpdatedDate.GetValueOrDefault().ToString("d/M/yyyy HH:mm:ss tt").Contains(filter))
                            ).OrderBy(t => t.ac.Name).Select(t => t.ac);
                    accounts = lsAccounts.Skip(fromRow).Take(limit).ToList();
                    countPage = lsAccounts.Count();
                }
            }

            var obj = from uc in accounts
                      join up in context.IC_UserPrivilege on uc.AccountPrivilege equals up.Index into temp
                      from dummy in temp.DefaultIfEmpty()
                      select new
                      {
                          Name = uc.Name,
                          UserName = uc.UserName,
                          AccountPrivilege = uc.AccountPrivilege,
                          IsAccountPrivilege = dummy == null ? "" : dummy.Name,
                          IsLockTo = uc.LockTo.HasValue ? uc.LockTo.Value.ToddMMyyyyHHmmss() : "",
                          UpdatedUser = uc.UpdatedUser,
                          IsUpdatedDate = uc.UpdatedDate.HasValue ? uc.UpdatedDate.Value.ToddMMyyyyHHmmss() : ""
                      };


            int record = 0;
            if (string.IsNullOrEmpty(filter))
            {
                record = context.IC_UserAccount.Where(t => t.CompanyIndex == user.CompanyIndex).ToList().Count;
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
        [ActionName("GetUserAccountInfo")]
        [HttpGet]
        public IActionResult GetUserAccountInfo()
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized("TokenExpired");
            }
            IC_UserAccount accountList = context.IC_UserAccount.Where(t => t.CompanyIndex == user.CompanyIndex && t.UserName == user.UserName).FirstOrDefault();

            result = Ok(accountList);
            return result;
        }
        [Authorize]
        [ActionName("GetAllUserAccountInfo")]
        [HttpGet]
        public IActionResult GetAllUserAccountInfo()
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            IC_UserAccount accountList = context.IC_UserAccount.Where(t => t.CompanyIndex == user.CompanyIndex && t.UserName == user.UserName).FirstOrDefault();

            result = Ok(accountList);
            return result;
        }

        [Authorize]
        [ActionName("AddUserAccount")]
        [HttpPost]
        public IActionResult AddUserAccount([FromBody]UserAccountParam param)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            param = (UserAccountParam)StringHelper.RemoveWhiteSpace(param);
            if (param.UserName == "" || param.Name == "")
            {
                return BadRequest("PleaseFillAllRequiredFields");
            }
            if (StringHelper.IsValidEmail(param.UserName) == false)
            {
                return BadRequest("EmailInvalidFormat");
            }

            IC_UserAccount checkData = context.IC_UserAccount.Where(t => t.CompanyIndex == user.CompanyIndex && t.UserName == param.UserName).FirstOrDefault();
            if (checkData != null)
            {
                return Conflict("UserNameIsExists");
            }

            IC_UserAccount userAccountInsert = new IC_UserAccount();
            userAccountInsert.UserName = param.UserName;
            userAccountInsert.CompanyIndex = user.CompanyIndex;
            userAccountInsert.Name = param.Name;
            userAccountInsert.Password = StringHelper.SHA1(param.Password);

            userAccountInsert.Disabled = param.Disabled;
            userAccountInsert.LockTo = param.LockTo;
            userAccountInsert.AccountPrivilege = param.AccountPrivilege;
            userAccountInsert.CreatedDate = DateTime.Now;

            userAccountInsert.UpdatedDate = DateTime.Now;
            userAccountInsert.UpdatedUser = user.UserName;

            context.IC_UserAccount.Add(userAccountInsert);
            context.SaveChanges();
            // Add audit log
            IC_AuditEntryDTO audit = new IC_AuditEntryDTO(null);
            audit.TableName = "IC_UserAccount";
            audit.UserName = user.UserName;
            audit.CompanyIndex = user.CompanyIndex;
            audit.State = AuditType.Added;
            //audit.Description = AuditType.Added.ToString() + "User: " + param.UserName.ToString();
            audit.Description = AuditType.Added.ToString() + "User:/:" + param.UserName.ToString();
            _iIC_AuditLogic.Create(audit);

            result = Ok();
            return result;
        }

        [Authorize]
        [ActionName("UpdateUserAccount")]
        [HttpPost]
        public IActionResult UpdateUserAccount([FromBody]UserAccountParam param)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            param = (UserAccountParam)StringHelper.RemoveWhiteSpace(param);

            IC_UserAccount company = context.IC_UserAccount.Where(t => t.CompanyIndex == user.CompanyIndex && t.UserName == param.UserName).FirstOrDefault();
            if (company == null)
            {
                return NotFound("UserAccountNotExists");
            }

            company.Name = param.Name;
            //company.Password = Misc.SHA1(param.Password);

            company.Disabled = param.Disabled;
            company.LockTo = param.LockTo;
            company.AccountPrivilege = param.AccountPrivilege;

            company.UpdatedDate = DateTime.Now;
            company.UpdatedUser = user.UserName;

            context.SaveChanges();

            // Add audit log
            IC_AuditEntryDTO audit = new IC_AuditEntryDTO(null);
            audit.TableName = "IC_UserAccount";
            audit.UserName = user.UserName;
            audit.CompanyIndex = user.CompanyIndex;
            audit.State = AuditType.Modified;
            //audit.Description = AuditType.Modified.ToString() + "User: " + param.UserName.ToString();
            audit.Description = AuditType.Modified.ToString() + "User:/:" + param.UserName.ToString();
            _iIC_AuditLogic.Create(audit);

            result = Ok();
            return result;
        }

        [Authorize]
        [ActionName("ChangePassword")]
        [HttpPost]
        public IActionResult ChangePassword([FromBody]ChangePasswordParam param)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            IC_UserAccount account = context.IC_UserAccount.Where(t => t.CompanyIndex == user.CompanyIndex && t.UserName == param.UserName).FirstOrDefault();
            if (account == null)
            {
                return NotFound("UserAccountNotExists");
            }

            if (!StringHelper.SHA1(param.Password).Equals(account.Password))
            {
                return NotFound("OldPasswordNotAsSameRecentPassword");
            }

            if (StringHelper.SHA1(param.Password) != account.Password)
            {
                return BadRequest("OldPasswordInvalid");
            }

            account.Password = StringHelper.SHA1(param.NewPassword);
            context.SaveChanges();

            result = Ok();
            return result;

        }

        [ActionName("SendResetPasswordCode")]
        [HttpPost]
        public IActionResult SendResetPasswordCode([FromBody] ChangePasswordParam param)
        {
            IActionResult result = null;
            IC_UserAccount account = context.IC_UserAccount.Where(t => t.UserName == param.UserName).FirstOrDefault();
            if (account == null)
            {
                return NotFound("UserAccountNotExists");
            }

            Random rd = new Random();
            int code = rd.Next(1000, 9999);
            account.ResetPasswordCode = code.ToString();

            if (SendMail(param.UserName, code,account.Name) == true)
            {
                context.SaveChanges();
            }
            result = Ok();
            return result;
        }

        private bool SendMail(string pUserName, int pCode, string pName)
        {
            string body = System.IO.File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + @"\Files" + @"\TemplateResetPassword.txt", Encoding.UTF8);
            string link = this.Request.Scheme + "://" + this.Request.Host.Value + "/resetpassword?email=" + pUserName + "&code=" + pCode;
            body = body.Replace("<code>", " " + pCode);
            body = body.Replace("<empName>", " " + pName);
            body = body.Replace("<link>", " " + link);
            Dictionary<string, string> dicAttachment = new Dictionary<string, string>();
            dicAttachment.Add("imgLogo", "logo.png");
            dicAttachment.Add("imgBackground", "backgroud.png");
            return EmailUltility.SendEmailWithHtmlBody("", "Thông tin lấy lại mật khẩu EPAD", body, pUserName, "", cache, dicAttachment);

        }

        [ActionName("ResetPassword")]
        [HttpPost]
        public IActionResult ResetPassword([FromBody]ResetPassParam param)
        {
            //UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            //IActionResult result = Unauthorized();
            //if (user == null)
            //{
            //    return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            //}

            IC_UserAccount account = context.IC_UserAccount.Where(t => t.UserName == param.UserName).FirstOrDefault();
            if (account == null)
            {
                return NotFound("UserAccountNotExists");
            }

            if (account.ResetPasswordCode != param.Code)
            {
                return NotFound("ResetCodeNotExists");
            }

            account.Password = StringHelper.SHA1(param.NewPassword);
            account.ResetPasswordCode = null;
            context.SaveChanges();
            return Ok();
        }


        [Authorize]
        [ActionName("DeleteUserAccount")]
        [HttpPost]
        public IActionResult DeleteUserAccount([FromBody]List<UserAccountParam> lsparam)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            foreach (var param in lsparam)
            {
                IC_UserAccount deleteData = context.IC_UserAccount.Where(t => t.CompanyIndex == user.CompanyIndex && t.UserName == param.UserName).FirstOrDefault();
                IC_UserAccount checkIsMyself = context.IC_UserAccount.Where(t => t.CompanyIndex == user.CompanyIndex && t.UserName == user.UserName && t.UserName == param.UserName).FirstOrDefault();
                if (deleteData == null)
                {
                    return NotFound("DepartmentNotExist");
                }
                else if (checkIsMyself != null)
                {
                    return NotFound("CannotDeleteYourSelf");
                }
                else
                {
                    context.IC_UserAccount.Remove(deleteData);
                }
                // Add audit log
                IC_AuditEntryDTO audit = new IC_AuditEntryDTO(null);
                audit.TableName = "IC_UserAccount";
                audit.UserName = user.UserName;
                audit.CompanyIndex = user.CompanyIndex;
                audit.State = AuditType.Deleted;
                //audit.Description = AuditType.Deleted.ToString() + "User: " + param.UserName.ToString();
                audit.Description = AuditType.Deleted.ToString() + "User:/:" + param.UserName.ToString();
                _iIC_AuditLogic.Create(audit);
            }
            context.SaveChanges();
            result = Ok();
            return result;
        }
    }

    public class UserAccountParam
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Name { get; set; }

        public bool Disabled { get; set; }
        public DateTime? LockTo { get; set; }

        public short AccountPrivilege { get; set; }
    }
    public class ChangePasswordParam
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }
    }
    public class ResetPassParam
    {
        public string UserName { get; set; }
        public string Code { get; set; }
        public string NewPassword { get; set; }
    }
}
