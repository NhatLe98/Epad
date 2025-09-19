using EPAD_Backend_Core.Base;
using EPAD_Backend_Core.WebUtilitys;
using EPAD_Common.Types;
using EPAD_Common.Utility;
using EPAD_Data.Entities.HR;
using EPAD_Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Generic;
using System.Net;
using System;
using EPAD_Data.Models;
using System.Linq;
using EPAD_Data.Entities;
using Chilkat;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace EPAD_Backend_Core.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("api/IC_LocationOperator/[action]")]
    [ApiController]
    public class IC_LocationOperatorController : ApiControllerBase
    {
        private EPAD_Context context;
        private IMemoryCache cache;

        public IC_LocationOperatorController(IServiceProvider provider) : base(provider)
        {
            context = TryResolve<EPAD_Context>();
            cache = TryResolve<IMemoryCache>();
        }

        [Authorize]
        [ActionName("GetLocationOperatorAtPage")]
        [HttpGet]
        public IActionResult GetLocationOperatorAtPage(int page, string filter, int limit)
        {
            var user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            List<IC_LocationOperator> listUserType = null;
            int countPage = 0;
            if (user == null)
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));

            if (string.IsNullOrEmpty(filter))
            {
                if (page <= 1)
                {
                    listUserType = context.IC_LocationOperator
                        .OrderBy(t => t.Name).Take(limit).ToList();
                }
                else
                {
                    int fromRow = limit * (page - 1);
                    listUserType = context.IC_LocationOperator
                        .OrderBy(t => t.Name)
                        .Skip(fromRow).Take(limit).ToList();
                }
            }
            else
            {
                if (page <= 1)
                {
                    var userTypes = context.IC_LocationOperator
                        .Where(t =>  (t.Name.Contains(filter) || t.Department.Contains(filter)));
                    countPage = userTypes.Count();
                    listUserType = userTypes.OrderBy(x => x.Name).Take(GlobalParams.ROWS_NUMBER_IN_PAGE).ToList();
                }
                else
                {
                    int fromRow = GlobalParams.ROWS_NUMBER_IN_PAGE * (page - 1);
                    var userTypes = context.IC_LocationOperator
                        .Where(t => (t.Name.Contains(filter) || t.Department.Contains(filter)))
                        .OrderBy(t => t.Name);

                    countPage = userTypes.Count();
                    listUserType = userTypes.Skip(fromRow).Take(GlobalParams.ROWS_NUMBER_IN_PAGE).ToList();
                }
            }

            int record = countPage;
            if (string.IsNullOrEmpty(filter))
            {
                record = context.IC_LocationOperator.ToList().Count;
            }
            //double totalPage = ConfigObject.CheckDoubleNumber((record / GlobalParams.ROWS_NUMBER_IN_PAGE).ToString());
            var dataGrid = new DataGridClass(record, listUserType);
            return Ok(dataGrid);
        }

        [Authorize]
        [ActionName("AddLocationOperator")]
        [HttpPost]
        public async Task<IActionResult> AddLocationOperator([FromBody] LocationOperatorParam param)
        {
            var user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));

            param = (LocationOperatorParam)StringHelper.RemoveWhiteSpace(param);

            if (param.Name == "")
                return BadRequest("PleaseFillAllRequiredFields");

            var checkData = await context.IC_LocationOperator.FirstOrDefaultAsync(t => t.Name == param.Name);
            if (checkData != null)
                return Conflict("LocationNameIsExist");

            var data = new IC_LocationOperator();
            data.Name = param.Name;
            data.Department= param.Department;

            await context.IC_LocationOperator.AddAsync(data);
            await context.SaveChangesAsync();

            return Ok();
        }

        [Authorize]
        [ActionName("UpdateLocationOperator")]
        [HttpPost]
        public async Task<IActionResult> UpdateLocationOperator([FromBody] LocationOperatorParam param)
        {
            var user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));

            param = (LocationOperatorParam)StringHelper.RemoveWhiteSpace(param);

            try
            {
                var updateData = await context.IC_LocationOperator.FirstOrDefaultAsync(t => t.Index == param.Index);

                if (updateData == null)
                    return NotFound("LocationOperatorNotExist");

                var checkData = context.IC_LocationOperator.FirstOrDefault(t =>  t.Name == param.Name);
                if (checkData != null && checkData.Index != updateData.Index)
                    return Conflict("LocationNameIsExist");

                if (!string.IsNullOrEmpty(param.Name))
                    updateData.Name = param.Name;

                if (!string.IsNullOrEmpty(param.Department))
                    updateData.Department = param.Department;

                await context.SaveChangesAsync();

                result = Ok();
            }
            catch (Exception ex)
            {
                result = StatusCode((int)HttpStatusCode.InternalServerError, ex.ToString());
            }
            return result;
        }

        [Authorize]
        [ActionName("DeleteLocationOperator")]
        [HttpPost]
        public IActionResult DeleteLocationOperator([FromBody] List<LocationOperatorParam> lsparam)
        {
            var user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));

            foreach (var param in lsparam)
            {
                var userType = context.IC_LocationOperator.FirstOrDefault(t =>  t.Index == param.Index);
                if (userType == null)
                {
                    return NotFound("LocationOperatorNotExist");
                }
                else
                {

                    context.IC_LocationOperator.Remove(userType);
                }
            }
            context.SaveChanges();

            return Ok();
        }

        public class LocationOperatorParam
        {
            public int Index { get; set; }
            public string Name { get; set; }
            public string Department { get; set; }
        }
    }
}
