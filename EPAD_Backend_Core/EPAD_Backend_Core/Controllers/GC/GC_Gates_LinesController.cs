using EPAD_Backend_Core.Base;
using EPAD_Common.Types;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using EPAD_Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EPAD_Backend_Core.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class GC_Gates_LinesController : ApiControllerBase
    {
        private IMemoryCache cache;
        IGC_LinesService _GC_LinesService;
        IGC_Gates_LinesService _GC_Gates_LinesService;
        public GC_Gates_LinesController(IServiceProvider pProvider) : base(pProvider)
        {
            cache = TryResolve<IMemoryCache>();
            _GC_LinesService = TryResolve<IGC_LinesService>();
            _GC_Gates_LinesService = TryResolve<IGC_Gates_LinesService>();
        }

        [Authorize]
        [ActionName("GetAllGates")]
        [HttpGet]
        public async Task<IActionResult> GetAllGates(int gateIndex)
        {
            UserInfo user = GetUserInfo();

            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }

            var dataResult = await _GC_Gates_LinesService.GetAllGates(user.CompanyIndex);
            return ApiOk(dataResult);
        }

        [Authorize]
        [ActionName("GetAllGatesLines")]
        [HttpGet]
        public async Task<IActionResult> GetAllGatesLines(int gateIndex)
        {
            UserInfo user = GetUserInfo();

            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }

            var dataResult = await _GC_Gates_LinesService.GetAllGatesLines(user.CompanyIndex);
            return ApiOk(dataResult);
        }

        [Authorize]
        [ActionName("GetByGateIndex")]
        [HttpGet]
        public async Task<IActionResult> GetByGateIndex(int gateIndex)
        {
            UserInfo user = GetUserInfo();

            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }

            List<GC_Gates_Lines> listGateLine = await _GC_Gates_LinesService.GetDataByGateIndexAndCompanyIndex(gateIndex, 
                user.CompanyIndex);
            List<GC_Lines> listLines = await _GC_LinesService.GetDataByCompanyIndex(user.CompanyIndex);

            List<GateLineData> listData = new List<GateLineData>();
            for (int i = 0; i < listLines.Count; i++)
            {
                GC_Gates_Lines gateLine = listGateLine.Where(t => t.LineIndex == listLines[i].Index).FirstOrDefault();
                GateLineData data = new GateLineData();
                data.GateIndex = gateIndex;
                data.LineIndex = listLines[i].Index;
                data.LineName = listLines[i].Name;
                data.InGroup = false;
                if (gateLine != null)
                {
                    data.InGroup = true;
                }
                listData.Add(data);
            }

            return ApiOk(listData);
        }

        [Authorize]
        [ActionName("UpdateByGateIndex")]
        [HttpPut]
        public async Task<IActionResult> UpdateByGateIndex(GateLineParam param)
        {
            UserInfo user = GetUserInfo();

            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }

            var isUpdateSuccess = await _GC_Gates_LinesService.UpdateGateLine(param, user);

            //Misc.LoadMonitoringDeviceList(_GC_DbContext, _MemoryCache);
            return ApiOk(isUpdateSuccess);
        }
    }
}
