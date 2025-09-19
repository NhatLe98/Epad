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
    public class GC_ParkingLotController : ApiControllerBase
    {
        private IMemoryCache cache;
        IGC_ParkingLotService _GC_ParkingLotService;
        public GC_ParkingLotController(IServiceProvider pProvider) : base(pProvider)
        {
            cache = TryResolve<IMemoryCache>();
            _GC_ParkingLotService = TryResolve<IGC_ParkingLotService>();
        }

        [Authorize]
        [ActionName("GetParkingLots")]
        [HttpGet]
        public async Task<IActionResult> GetParkingLots(int page, string filter, int pageSize)
        {
            UserInfo user = GetUserInfo();

            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }

            var grid = await _GC_ParkingLotService.GetDataByPage(user.CompanyIndex, page, filter, pageSize);
            return ApiOk(grid);
        }

        [Authorize]
        [ActionName("GetParkingLotsAll")]
        [HttpGet]
        public IActionResult GetParkingLotsAll()
        {
            UserInfo user = GetUserInfo();

            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }
            var grid = _GC_ParkingLotService.GetDataByCompanyIndex(user.CompanyIndex).Result;
            var dataGridClass = new DataGridClass(grid.Count, grid);
            return ApiOk(dataGridClass);
        }

        [Authorize]
        [ActionName("AddParkingLot")]
        [HttpPost]
        public async Task<IActionResult> AddParkingLot(GC_ParkingLot param)
        {
            UserInfo user = GetUserInfo();

            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }

            var parkinglot = await _GC_ParkingLotService.GetDataByCodeAndCompanyIndex(param.Code, user.CompanyIndex);
            if (parkinglot != null)
            {
                return ApiConflict("ParkingLotExists");
            }

            var isSuccess = await _GC_ParkingLotService.AddParkingLot(param, user);
            return ApiOk(isSuccess);
        }

        [Authorize]
        [ActionName("UpdateParkingLot")]
        [HttpPut]
        public async Task<IActionResult> UpdateParkingLot(GC_ParkingLot param)
        {
            UserInfo user = GetUserInfo();

            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }

            var parkingLot = await _GC_ParkingLotService.GetDataByIndex(param.Index);
            if (parkingLot == null)
            {
                return ApiConflict("ParkingLotNotExist");
            }
            var isSuccess = await _GC_ParkingLotService.UpdateParkingLot(param, user);

            return ApiOk(isSuccess);
        }


        [Authorize]
        [ActionName("DeleteParkingLot")]
        [HttpDelete]
        public async Task<IActionResult> DeleteParkingLot([FromBody] List<int> listIndex)
        {
            UserInfo user = GetUserInfo();
            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }

            var deleteMessage = await _GC_ParkingLotService.TryDeleteParkingLot(listIndex, user.CompanyIndex);
            return ApiOk(deleteMessage);
        }
    }
}
