using EPAD_Backend_Core.Base;
using EPAD_Common.Types;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using EPAD_Services.Business;
using EPAD_Services.Business.Parking;
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
    public class GC_ViolationController : ApiControllerBase
    {
        IGC_TimeLogService _GC_TimeLogService;
        CustomerProcess _customerProcess;
        public GC_ViolationController(IServiceProvider pServiceProvider) : base(pServiceProvider)
        {
            _GC_TimeLogService = TryResolve<IGC_TimeLogService>();
            _customerProcess = TryResolve<CustomerProcess>();
        }

        //[Authorize]
        //[ActionName("GetByFilter")]
        //[HttpPost]
        //public IActionResult GetByFilter(ListViolationParam model)
        //{
        //    UserInfo user = GetUserInfo();

        //    IActionResult result = ApiUnauthorized();
        //    if (user == null)
        //    {
        //        return result;
        //    }

        //    var grid = _GC_TimeLogService
        //        .GetPaginationAsync(x => x.CompanyIndex == user.CompanyIndex
        //        && x.ObjectAccessType == model.ObjectAccessType
        //        && !string.IsNullOrEmpty(x.Error)
        //        && x.Time >= model.FromDate && x.Time <= model.ToDate
        //        && model.Status.Contains(x.ApproveStatus), model.Page, model.PageSize).Result;

        //    return ApiOk(grid);
        //}

        [Authorize]
        [ActionName("GetViolationByShift")]
        [HttpPost]
        public IActionResult GetViolationByShift(ViolationShiftModel model)
        {

            return null;
            //var result = await _GCS_MonitoringClient.GetViolationByShift(model);
            //var data = JsonConvert.DeserializeObject<List<ViolationShift>>(result.Data.ToString());
            var results = _customerProcess.CheckShiftByList(model.EmployeeATIDs, DateTime.Now/*.AddHours(3)*/);

            return ApiOk(results);
        }

        public class ListViolationParam
        {
            public string ObjectAccessType { get; set; }
            public DateTime FromDate { get; set; }
            public DateTime ToDate { get; set; }
            public List<int> Status { get; set; }
            public int Page { get; set; }
            public int PageSize { get; set; }
        }
        public class ViolationShiftModel
        {
            public string[] EmployeeATIDs { get; set; }
            public bool IsCustomer { get; set; }
        }
    }
}
