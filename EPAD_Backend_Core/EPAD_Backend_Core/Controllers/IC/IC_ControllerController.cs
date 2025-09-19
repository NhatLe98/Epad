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
using Microsoft.Extensions.Caching.Memory;

namespace EPAD_Backend_Core.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("api/Controller/[action]")]
    [ApiController]
    public class IC_ControllerController : ApiControllerBase
    {
        private readonly IIC_ControllerService _IC_ControllerService;
        public IC_ControllerController(IServiceProvider provider) : base(provider)
        {
            _IC_ControllerService = TryResolve<IIC_ControllerService>();
        }

        [Authorize]
        [ActionName("GetController")]
        [HttpGet]
        public async Task<IActionResult> GetControllerAsync(int page, string filter, int limit)
        {
            UserInfo user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            var result = await _IC_ControllerService.GetDataGrid(filter, user.CompanyIndex, page, limit);

            return ApiOk(result);
        }

        [Authorize]
        [ActionName("AddController")]
        [HttpPost]
        public async Task<IActionResult> AddControllerAsync(IC_Controller param)
        {
            UserInfo user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            param.CreateDate = DateTime.Now;
            await _IC_ControllerService.InsertAsync(param);
            await SaveChangeAsync();

            return ApiOk();
        }

        [Authorize]
        [ActionName("UpdateController")]
        [HttpPut]
        public async Task<IActionResult> UpdateControllerAsync(IC_Controller param)
        {
            UserInfo user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            IC_Controller controller = await _IC_ControllerService.GetByIndex(param.Index);
            controller = _Mapper.Map<IC_Controller>(param);
            _IC_ControllerService.Update(controller);
            await SaveChangeAsync();

            return ApiOk();
        }


        [Authorize]
        [ActionName("DeleteController")]
        [HttpDelete]
        public async Task<IActionResult> DeleteControllerAsync(string Index)
        {
            UserInfo user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            List<IC_Controller> accountGroups = new List<IC_Controller>();
            var values = Index.Split(',').Where(x => x != "").Select(x => int.Parse(x)).ToHashSet();
            await _IC_ControllerService.DeleteAsync(x => x.CompanyIndex == user.CompanyIndex && values.Contains(x.Index));
            await SaveChangeAsync();

            return ApiOk();
        }
    }
}
