using EPAD_Backend_Core.Base;
using EPAD_Common.Extensions;
using EPAD_Common.Types;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using EPAD_Data.Models.GC;
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
    public class GC_GatesController : ApiControllerBase
    {
        private IMemoryCache cache;
        IGC_GatesService _GC_GatesService;
        IGC_Gates_LinesService _GC_Gates_LinesService;
        IGC_LinesService _GC_LinesService;
        IGC_Lines_CheckInDeviceService _GC_Lines_CheckInDeviceService;
        IGC_Lines_CheckOutDeviceService _GC_Lines_CheckOutDeviceService;
        public GC_GatesController(IServiceProvider pProvider) : base(pProvider)
        {
            cache = TryResolve<IMemoryCache>();
            _GC_GatesService = TryResolve<IGC_GatesService>();
            _GC_Gates_LinesService = TryResolve<IGC_Gates_LinesService>();
            _GC_LinesService = TryResolve<IGC_LinesService>();
            _GC_Lines_CheckInDeviceService = TryResolve<IGC_Lines_CheckInDeviceService>();
            _GC_Lines_CheckOutDeviceService = TryResolve<IGC_Lines_CheckOutDeviceService>();
        }

        [Authorize]
        [ActionName("GetGatesAll")]
        [HttpGet]
        public IActionResult GetGatesAll()
        {
            UserInfo user = GetUserInfo();

            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }
            var grid = _GC_GatesService.GetDataByCompanyIndex(user.CompanyIndex).Result;
            DataGridClass dataGridClass = new DataGridClass(grid.Count, grid);
            return ApiOk(dataGridClass);
        }

        [Authorize]
        [ActionName("AddGates")]
        [HttpPost]
        public async Task<IActionResult> AddGates(GatesModel param)
        {
            UserInfo user = GetUserInfo();

            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }

            var gates = await _GC_GatesService.GetDataByNameAndCompanyIndex(param.Name, user.CompanyIndex);
            if (gates != null)
            {
                return ApiConflict("GatesExists");
            }

            var isSuccess = await _GC_GatesService.AddGates(param, user);
            return ApiOk(isSuccess);
        }

        [Authorize]
        [ActionName("UpdateGates")]
        [HttpPost]
        public async Task<IActionResult> UpdateGates(GatesModel param)
        {
            UserInfo user = GetUserInfo();

            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }

            var existedGates = await _GC_GatesService.GetDataByNameAndCompanyIndex(param.Name, user.CompanyIndex);
            if (existedGates != null && existedGates.Index != param.Index)
            {
                return ApiConflict("GatesExists");
            }

            var gates = await _GC_GatesService.GetDataByIndex(param.Index);
            if (gates == null)
            {
                return ApiConflict("GatesNotExist");
            }

            var isSuccess = await _GC_GatesService.UpdateGates(param, user);
            return ApiOk(isSuccess);
        }


        [Authorize]
        [ActionName("DeleteGates")]
        [HttpDelete]
        public async Task<IActionResult> DeleteGates([FromBody] List<int> listIndex)
        {
            UserInfo user = GetUserInfo();
            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }

            var isGateUsing = await _GC_GatesService.CheckGateUsing(listIndex);
            if (isGateUsing)
            {
                return ApiConflict("GatesIsUsing");
            }

            var isSuccess = await _GC_GatesService.DeleteGates(listIndex);
            return ApiOk(isSuccess);
        }

        [Authorize]
        [ActionName("UpdateGateLineDevice")]
        [HttpPost]
        public async Task<IActionResult> UpdateGateLineDevice(GatesModel param)
        {
            UserInfo user = GetUserInfo();

            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }

            var isSuccess = await _GC_GatesService.UpdateGateLineDevice(param, user);
            return ApiOk(isSuccess);
        }

        [Authorize]
        [ActionName("GetGateByDevice")]
        [HttpGet]
        public async Task<IActionResult> GetGateByDevice()
        {
            UserInfo user = GetUserInfo();

            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }


            var listLineCheckInDevices = await _GC_Lines_CheckInDeviceService.GetDataByCompanyIndex(user.CompanyIndex);
            var listLineCheckOutDevices = await _GC_Lines_CheckOutDeviceService.GetDataByCompanyIndex(user.CompanyIndex);
            var listGateLine = await _GC_Gates_LinesService.GetAllGatesLines(user.CompanyIndex);
            var listGates = await _GC_GatesService.GetDataByCompanyIndex(user.CompanyIndex);

            var gateSerialData = new List<GateSerialMachine>();

            foreach (var gate in listGates)
            {
                var listSerialDevices = new List<string>();
                var listLineIndex = listGateLine.Where(x => x.GateIndex == gate.Index).Select(x => x.LineIndex).ToList();


                foreach (var line in listLineIndex)
                {
                    var listInSerials = listLineCheckInDevices.Where(e => e.LineIndex == line).Select(e => e.CheckInDeviceSerial);
                    var listOutSerials = listLineCheckOutDevices.Where(e => e.LineIndex == line).Select(e => e.CheckOutDeviceSerial);
                    listInSerials = listInSerials.Concat(listOutSerials);
                    listSerialDevices.AddRange(listInSerials);
                }
                listSerialDevices = listSerialDevices.Where(e => !string.IsNullOrEmpty(e)).Distinct().ToList();
                var item = new GateSerialMachine().PopulateWith(gate);
                item.MachineList = listSerialDevices;
                gateSerialData.Add(item);

            }

            return ApiOk(gateSerialData);
        }

        [Authorize]
        [ActionName("GetGateLinesAsTree")]
        [HttpGet]
        public IActionResult GetGateLinesAsTree()
        {

            UserInfo user = GetUserInfo();

            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }

            var listGate = _GC_GatesService.Where(x => x.CompanyIndex == user.CompanyIndex).ToList();
            var listGateLine = _GC_Gates_LinesService.Where(x => listGate.Select(z => z.Index).Contains(x.GateIndex)).ToList();
            var listLineIndex = listGateLine.Select(t => t.LineIndex).ToList();
            var listLines = _GC_LinesService.Where(t => t.CompanyIndex == user.CompanyIndex && listLineIndex.Contains(t.Index)).ToList();

            List<GateTree> listData = new List<GateTree>();
            for (int i = 0; i < listGate.Count; i++)
            {
                GateTree data = new GateTree();
                data.ID = listGate[i].Index.ToString();
                data.Name = listGate[i].Name;
                data.Type = "Gate";
                data.ParentIndex = 0;
                data.ListChildrent = new List<GateTree>();
                List<GC_Gates_Lines> listGateLineByGateIndex = listGateLine.Where(t => t.GateIndex == listGate[i].Index).ToList();
                foreach (GC_Gates_Lines item in listGateLineByGateIndex)
                {
                    GC_Lines line = listLines.Where(t => t.Index == item.LineIndex).FirstOrDefault();
                    if (line != null)
                    {
                        data.ListChildrent.Add(new GateTree()
                        {
                            ID = $"{listGate[i].Index}-{line.Index}",
                            Name = line.Name,
                            Type = "Line",
                            ParentIndex = listGate[i].Index,
                            ListChildrent = new List<GateTree>()
                        });
                    }
                }
                listData.Add(data);
            }

            return ApiOk(listData);
        }
    }
}
