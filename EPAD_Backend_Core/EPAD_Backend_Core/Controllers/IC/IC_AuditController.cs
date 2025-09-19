using System;
using System.Collections.Generic;
using System.Linq;
using EPAD_Backend_Core.Base;
using EPAD_Backend_Core.WebUtilitys;
using EPAD_Common.Types;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using EPAD_Logic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;

namespace EPAD_Backend_Core.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("api/Audit/[action]")]
    [ApiController]
    public class IC_AuditController : ApiControllerBase
    {
        static private EPAD_Context context;
        private IMemoryCache cache;
        private IIC_AuditLogic _iC_Audit;
        public IC_AuditController(IServiceProvider provider) : base(provider)
        {
            context = TryResolve<EPAD_Context>();
            cache = TryResolve<IMemoryCache>();
            _iC_Audit = TryResolve<IIC_AuditLogic>();
        }
        [Authorize]
        [ActionName("GetMany")]
        [HttpGet]
        public IActionResult GetMany(string filter)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            DataGridClass dataGrid = null;
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }
            List<AddedParam> addedParams = new List<AddedParam>();
            addedParams.Add(new AddedParam { Key = "Filter", Value = filter });
            addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = user.CompanyIndex });
            var data  =_iC_Audit.GetMany(addedParams);
            dataGrid = new DataGridClass(data.Count(), data);
            return Ok(dataGrid);
        }
        [Authorize]
        [ActionName("GetPage")]
        [HttpGet]
        public IActionResult GetPage(string filter, int limit)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }
            DataGridClass dataGrid = null;
            List<AddedParam> addedParams = JsonConvert.DeserializeObject<List<AddedParam>>(filter);
            addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = user.CompanyIndex });
            addedParams.Add(new AddedParam { Key = "PageSize", Value = limit });
            ListDTOModel<IC_AuditEntryDTO> listData = _iC_Audit.GetPage(addedParams);
            dataGrid = new DataGridClass(listData.TotalCount, listData.Data);
            result = Ok(dataGrid);
            return result;
        }

        [Authorize]
        [ActionName("PostPage")]
        [HttpPost]
        public IActionResult PostPage([FromBody] List<AddedParam> addedParams)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }
            addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = user.CompanyIndex });

            ListDTOModel<IC_AuditEntryDTO> listData = _iC_Audit.GetPage(addedParams);
            DataGridClass dataGrid = new DataGridClass(listData.TotalCount, listData.Data); ;

            return Ok(dataGrid);
        }

        [Authorize]
        [ActionName("Delete")]
        [HttpDelete]
        public IActionResult Delete(int index)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            IC_AuditEntryDTO audit = new IC_AuditEntryDTO(null);
            audit.Index = index;
            audit.CompanyIndex = user.CompanyIndex;
            _iC_Audit.Delete(audit);
            return Ok();
        }

        [Authorize]
        [ActionName("DeleteList")]
        [HttpDelete]
        public IActionResult DeleteList(string listIndex)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }
            List<IC_Controller> accountGroups = new List<IC_Controller>();
            List<string> values = listIndex.Split(',').ToList();
            List<AddedParam> addedParams = new List<AddedParam>();
            addedParams.Add(new AddedParam { Key = "ListIndex", Value = values });
            _iC_Audit.DeleteList(addedParams, user);
            return Ok();
        }

        [Authorize]
        [ActionName("DeleteList")]
        [HttpPost]
        public IActionResult DeleteList([FromBody] List<IC_SystemCommandDTO> listSystemCommand)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Ok();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            try
            {
                List<AddedParam> addedParams = new List<AddedParam>();
                addedParams.Add(new AddedParam { Key = "ListIndex", Value = listSystemCommand.Select(e=>e.Index).ToList() });
                _iC_Audit.DeleteList(addedParams,user);
                result = Ok();
            }
            catch (Exception ex)
            {
                result = StatusCode(500, ex.ToString());
            }
            return result;
        }
    }
}
