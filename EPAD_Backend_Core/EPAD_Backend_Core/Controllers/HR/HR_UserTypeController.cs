using EPAD_Backend_Core.Base;
using EPAD_Backend_Core.WebUtilitys;
using EPAD_Common.Types;
using EPAD_Common.Utility;
using EPAD_Data;
using EPAD_Data.Entities.HR;
using EPAD_Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace EPAD_Backend_Core.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("api/UserType/[action]")]
    [ApiController]
    public class HR_UserTypeController : ApiControllerBase
    {
        private EPAD_Context context;
        private IMemoryCache cache;

        public HR_UserTypeController(IServiceProvider provider):base(provider)
        {
            context = TryResolve<EPAD_Context>();
            cache = TryResolve<IMemoryCache>();
        }

        [Authorize]
        [ActionName("GetUserTypeAtPage")]
        [HttpGet]
        public IActionResult GetUserTypeAtPage(int page, string filter, int limit)
        {
            var user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            List<HR_UserType> listUserType = null;
            int countPage = 0;
            if (user == null)
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));

            if (string.IsNullOrEmpty(filter))
            {
                if (page <= 1)
                {
                    listUserType = context.HR_UserType.Where(t => t.CompanyIndex == user.CompanyIndex)
                        .OrderBy(t => t.Order).Take(limit).ToList();
                }
                else
                {
                    int fromRow = limit * (page - 1);
                    listUserType = context.HR_UserType.Where(t => t.CompanyIndex == user.CompanyIndex)
                        .OrderBy(t => t.Order)
                        .Skip(fromRow).Take(limit).ToList();
                }
            }
            else
            {
                if (page <= 1)
                {
                    var userTypes = context.HR_UserType
                        .Where(t => t.CompanyIndex == user.CompanyIndex && (t.Name.Contains(filter) || t.Description.Contains(filter)));
                    countPage = userTypes.Count();
                    listUserType = userTypes.OrderBy(x => x.Order).Take(GlobalParams.ROWS_NUMBER_IN_PAGE).ToList();
                }
                else
                {
                    int fromRow = GlobalParams.ROWS_NUMBER_IN_PAGE * (page - 1);
                    var userTypes = context.HR_UserType
                        .Where(t => t.CompanyIndex == user.CompanyIndex && (t.Name.Contains(filter) || t.Description.Contains(filter)))
                        .OrderBy(t => t.Order);

                    countPage = userTypes.Count();
                    listUserType = userTypes.Skip(fromRow).Take(GlobalParams.ROWS_NUMBER_IN_PAGE).ToList();
                }
            }

            var listData = new List<UserTypeResult>();

            for (int i = 0; i < listUserType.Count; i++)
            {
                var data = new UserTypeResult();
                data.Index = listUserType[i].Index;
                data.Code = listUserType[i].Code;
                data.Name = listUserType[i].Name;
                data.EnglishName = listUserType[i].EnglishName;
                data.Description = listUserType[i].Description;
                data.IsEmployee = listUserType[i].IsEmployee ?? false;
                data.Status = ((RowStatus)listUserType[i].StatusId).ToString();
                data.UserTypeId = listUserType[i].UserTypeId;

                listData.Add(data);
            }

            int record = countPage;
            if (string.IsNullOrEmpty(filter))
            {
                record = context.HR_UserType.Where(t => t.CompanyIndex == user.CompanyIndex).ToList().Count;
            }
            //double totalPage = ConfigObject.CheckDoubleNumber((record / GlobalParams.ROWS_NUMBER_IN_PAGE).ToString());
            var dataGrid = new DataGridClass(record, listData);
            return Ok(dataGrid);
        }
        
        [Authorize]
        [ActionName("GetAllUserType")]
        [HttpGet]
        public IActionResult GetAllUserType()
        {
            var user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));

            var listUserType = context.HR_UserType
                .Where(t => t.CompanyIndex == user.CompanyIndex )
                .Select(t => new UserTypeResult
                {
                    Index = t.Index,
                    Code = t.Code,
                    Name = t.Name,
                    Order = t.Order,
                    EnglishName = t.EnglishName,
                    Description = t.Description,
                    IsEmployee = t.IsEmployee ?? false,
                    Status = ((RowStatus)t.StatusId).ToString(),
                    UserTypeId = t.UserTypeId,
                })
                .OrderBy(t => t.Order).ToList();

            return Ok(listUserType);
        }

        [Authorize]
        [ActionName("GetUserTypeTitle")]
        [HttpGet]
        public IActionResult GetUserTypeTitle(int userType)
        {
            var user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));

            var userTypeInfo = context.HR_UserType.FirstOrDefault(t => t.CompanyIndex == user.CompanyIndex && t.Index == userType);
            return Ok(userTypeInfo.Name);
        }

        [Authorize]
        [ActionName("AddUserType")]
        [HttpPost]
        public IActionResult AddUserType([FromBody]UserTypeParam param)
        {
            var user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));

            param = (UserTypeParam)StringHelper.RemoveWhiteSpace(param);

            if (param.Name == "")
                return BadRequest("PleaseFillAllRequiredFields");

            var checkData = context.HR_UserType.FirstOrDefault(t => t.CompanyIndex == user.CompanyIndex && t.Name == param.Name);
            if (checkData != null)
                return Conflict("UserTypeNameIsExist");
            
            var data = new HR_UserType();
            data.Code = param.Code;
            data.Name = param.Name;
            data.EnglishName = param.EnglishName;
            data.Description = param.Description;
            data.CompanyIndex = user.CompanyIndex;
            data.UpdatedDate = DateTime.Now;
            data.UpdatedUser = user.UserName;
            data.StatusId = (short)RowStatus.Active;
            data.UserTypeId = param.UserTypeId;

            context.HR_UserType.Add(data);
            context.SaveChanges();

            return Ok();
        }

        [Authorize]
        [ActionName("UpdateUserType")]
        [HttpPost]
        public IActionResult UpdateUserType([FromBody]UserTypeParam param)
        {
            var user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            
            param = (UserTypeParam)StringHelper.RemoveWhiteSpace(param);

            try
            {
                var updateData = context.HR_UserType.FirstOrDefault(t => t.Index == param.Index);

                if (updateData == null)
                    return NotFound("UserTypeNotExist");

                var checkData = context.HR_UserType.FirstOrDefault(t => t.CompanyIndex == user.CompanyIndex && t.Name == param.Name);
                if (checkData != null && checkData.Index != updateData.Index)
                    return Conflict("UserTypeNameIsExist");

                if (param.UserTypeId > 0)
                    updateData.UserTypeId = param.UserTypeId;

                if (!string.IsNullOrEmpty(param.Code))
                    updateData.Code = param.Code;
                
                if (!string.IsNullOrEmpty(param.Name))
                    updateData.Name = param.Name;
                
                if (!string.IsNullOrEmpty(param.EnglishName))
                    updateData.EnglishName = param.EnglishName;
                
                if (param.Description != null)
                    updateData.Description = param.Description;

                updateData.UpdatedDate = DateTime.Now;
                updateData.UpdatedUser = user.UserName;

                context.SaveChanges();

                result = Ok();
            }
            catch (Exception ex)
            {
                result = StatusCode((int)HttpStatusCode.InternalServerError, ex.ToString());
            }
            return result;
        }

        [Authorize]
        [ActionName("DeleteUserType")]
        [HttpPost]
        public IActionResult DeleteUserType([FromBody]List<UserTypeParam> lsparam)
        {
            var user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));

            foreach (var param in lsparam)
            {
                var userType = context.HR_UserType.FirstOrDefault(t => t.CompanyIndex == user.CompanyIndex && t.Index == param.Index);
                if (userType == null)
                {
                    return NotFound("UserTypeNotExist");
                }
                else
                {
                    userType.StatusId = (short)RowStatus.Inactive;
                    userType.UpdatedDate = DateTime.Now;
                    userType.UpdatedUser = user.UserName;

                    context.HR_UserType.Update(userType);
                }
            }
            context.SaveChanges();

            return Ok();
        }

        public class UserTypeParam
        {
            public int Index { get; set; }
            public short UserTypeId { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
            public string EnglishName { get; set; }
            public string Description { get; set; }
        }

        public class UserTypeResult
        {
            public int Index { get; set; }
            public string Code { get; set; }
            public short Order { get; set; }
            public string Name { get; set; }
            public string EnglishName { get; set; }
            public string Description { get; set; }
            public bool IsEmployee { get; set; }
            public short UserTypeId { get; set; }
            public string Status { get; set; }
        }

    }
}
