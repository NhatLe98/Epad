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
using EPAD_Logic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace EPAD_Backend_Core.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("api/WorkingInfo/[action]")]
    [ApiController]
    public class IC_WorkingInfoController : ApiControllerBase
    {
        private EPAD_Context context;
        private IMemoryCache cache;
        private IIC_WorkingInfoLogic _iIC_WorkingInfoLogic;
        public IC_WorkingInfoController(IServiceProvider provider):base(provider)
        {
            context = TryResolve<EPAD_Context>();
            cache = TryResolve<IMemoryCache>();
            _iIC_WorkingInfoLogic = TryResolve<IIC_WorkingInfoLogic>();
        }
        [Authorize]
        [ActionName("GetWorkingInforAtPage")]
        [HttpGet]
        public IActionResult GetWorkingInforAtPage(int page, string filter, string fromDate, string toDate)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }
            DataGridClass dataGrid = null;
            DateTime _fromTime = DateTime.ParseExact(fromDate, "yyyy-MM-dd", null);
            DateTime _toTime = DateTime.ParseExact(toDate, "yyyy-MM-dd", null);
            int countPage = 0;
            string[] lsEmp = null;
            string filters = "";
            if (!string.IsNullOrEmpty(filter))
            {
                if (filter.Split(',').Length > 0)
                {
                    filters = filter.Split(',')[0];
                    lsEmp = filter.Split(',').Skip(1).ToArray();
                }
            }

            var obj = from wk in context.IC_WorkingInfo
                      join emp in context.HR_User
                       on wk.EmployeeATID equals emp.EmployeeATID
                      join dp in context.IC_Department
                         on wk.DepartmentIndex equals dp.Index
                      where 
                      (
                        wk.CompanyIndex == user.CompanyIndex
                        && wk.FromDate <= _toTime
                        && 
                        (
                            wk.ToDate == null || wk.ToDate >= _fromTime
                        )
                      )
                       select new
                      {
                          Index = wk.Index,
                          EmployeeATID = wk.EmployeeATID,
                          FullName = emp.FullName,
                          DepartmentIndex = dp.Index,
                          DepartmentName = dp.Name,
                          FromDate = wk.FromDate,
                          ToDate = wk.ToDate
                      };

            if (page <= 1)
            {
                countPage = obj.Count();
                var lsEmployeeTransfer = obj.OrderBy(t => t.EmployeeATID)
                    .Take(GlobalParams.ROWS_NUMBER_IN_PAGE).ToList()
                    .Where(t => string.IsNullOrEmpty(filters) ? true : (t.FullName.Contains(filters) || t.DepartmentName.Contains(filters)) && lsEmp == null ? false : t.EmployeeATID.Contains(string.Join(',',lsEmp)));
                dataGrid = new DataGridClass(countPage, lsEmployeeTransfer);
            }
            else
            {
                int fromRow = GlobalParams.ROWS_NUMBER_IN_PAGE * (page - 1);
                countPage = obj.Count();
                var lsEmployeeTransfer = obj.OrderBy(t => t.EmployeeATID)
                    .Skip(fromRow)
                    .Take(GlobalParams.ROWS_NUMBER_IN_PAGE).ToList()
                    .Where(t => string.IsNullOrEmpty(filters) ? true : (t.FullName.Contains(filters) || t.DepartmentName.Contains(filters)));
                dataGrid = new DataGridClass(countPage, lsEmployeeTransfer);
            }

            result = Ok(dataGrid);
            return result;
        }

        [Authorize]
        [ActionName("PostPage")]
        [HttpPost]
        public IActionResult PostPage([FromBody]  List<AddedParam> addedParams)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = user.CompanyIndex });

            ListDTOModel<IC_WorkingInfoDTO> listData = _iIC_WorkingInfoLogic.GetPage(addedParams);
            DataGridClass dataGrid = new DataGridClass(listData.TotalCount, listData.Data); ;

            return Ok(dataGrid);
        }

        [Authorize]
        [ActionName("GetTransferLast7Days")]
        [HttpGet]
        public async Task<IActionResult> GetTransferLast7Days()
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            var dataGrid = await _iIC_WorkingInfoLogic.GetTransferLast7Days(user);

            return Ok(dataGrid);
        }

        [Authorize]
        [ActionName("AddWorkingInfo")]
        [HttpPost]
        public IActionResult AddWorkingInfo([FromBody] WorkingInfoParam param)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }
            var checkData = context.IC_WorkingInfo.Where(t => t.CompanyIndex == user.CompanyIndex
            && t.EmployeeATID == param.EmployeeATID && param.ToDate != null ? ((t.ToDate != null ? param.FromDate <= t.ToDate : param.FromDate <= t.FromDate)
            && param.ToDate >= t.FromDate) : (param.FromDate <= t.FromDate || param.FromDate <= t.ToDate)).FirstOrDefault();
            if (checkData != null)
            {
                return BadRequest("WorkingInfoIsExists");
            }
            IC_WorkingInfo workingInfo = new IC_WorkingInfo()
            {
                CompanyIndex = user.CompanyIndex,
                EmployeeATID = param.EmployeeATID,
                DepartmentIndex = param.DepartmentIndex,
                FromDate = param.FromDate,
                ToDate = param.ToDate,
                UpdatedDate = DateTime.Now,
                UpdatedUser = user.UserName,
            };
            context.IC_WorkingInfo.Add(workingInfo);
            context.SaveChanges();
            return Ok();
        }

        [Authorize]
        [ActionName("AddListWorkingInfo")]
        [HttpPost]
        public IActionResult AddListWorkingInfo([FromBody] WorkingInfoParam lsparam)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }
            for (int i = 0; i < lsparam.ArrEmployeeATID.Count; i++)
            {
                var checkData = context.IC_WorkingInfo.Where(t => t.CompanyIndex == user.CompanyIndex
           && t.EmployeeATID == lsparam.EmployeeATID && lsparam.ToDate != null ? ((t.ToDate != null ? lsparam.FromDate <= t.ToDate : lsparam.FromDate <= t.FromDate)
           && lsparam.ToDate >= t.FromDate) : (lsparam.FromDate <= t.FromDate || lsparam.FromDate <= t.ToDate)).FirstOrDefault();
                if (checkData != null)
                {
                    return BadRequest("WorkingInfoIsExists");
                }
                IC_WorkingInfo workingInfo = new IC_WorkingInfo()
                {
                    CompanyIndex = user.CompanyIndex,
                    EmployeeATID = lsparam.ArrEmployeeATID[i],
                    DepartmentIndex = lsparam.DepartmentIndex,
                    FromDate = lsparam.FromDate,
                    ToDate = lsparam.ToDate,
                    UpdatedDate = DateTime.Now,
                    UpdatedUser = user.UserName,
                };
                context.IC_WorkingInfo.Add(workingInfo);
            }

            context.SaveChanges();
            return Ok();
        }

        [Authorize]
        [ActionName("AddWorkingInfos")]
        [HttpPost]
        public IActionResult AddWorkingInfos([FromBody] List<WorkingInfoParam> lsparam)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }
            foreach (var param in lsparam)
            {
                var checkData = context.IC_WorkingInfo.Where(t => t.CompanyIndex == user.CompanyIndex
           && t.EmployeeATID == param.EmployeeATID && param.ToDate != null ? ((t.ToDate != null ? param.FromDate <= t.ToDate : param.FromDate <= t.FromDate)
           && param.ToDate >= t.FromDate) : (param.FromDate <= t.FromDate || param.FromDate <= t.ToDate)).FirstOrDefault();
                if (checkData != null)
                {
                    return BadRequest("WorkingInfoIsExists");
                }
                IC_WorkingInfo workingInfo = new IC_WorkingInfo()
                {
                    CompanyIndex = user.CompanyIndex,
                    EmployeeATID = param.EmployeeATID,
                    DepartmentIndex = param.DepartmentIndex,
                    FromDate = param.FromDate,
                    ToDate = param.ToDate,
                    UpdatedDate = DateTime.Now,
                    UpdatedUser = user.UserName,
                };
                context.IC_WorkingInfo.Add(workingInfo);
            }

            context.SaveChanges();
            return Ok();
        }

        [Authorize]
        [ActionName("UpdateWorkingInfo")]
        [HttpPut]
        public IActionResult UpdateWorkingInfo([FromBody] WorkingInfoParam param)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            return Ok();
        }


        [Authorize]
        [ActionName("DeleteWorkingInfo")]
        [HttpDelete]
        public IActionResult DeleteWorkingInfo(int index)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }
            var checkData = context.IC_WorkingInfo.Where(t => t.CompanyIndex == user.CompanyIndex && t.Index.Equals(index)).FirstOrDefault();
            if (checkData != null)
            {
                context.IC_WorkingInfo.Remove(checkData);
                context.SaveChanges();
            }
            return Ok();
        }


        [Authorize]
        [ActionName("DeleteWorkingInfos")]
        [HttpDelete]
        public IActionResult DeleteWorkingInfos(string index)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }
            string[] values = index.Split(',');
            foreach (var value in values)
            {
                var checkData = context.IC_WorkingInfo.Where(t => t.CompanyIndex == user.CompanyIndex && t.Index.Equals(int.Parse(value))).FirstOrDefault();
                if (checkData != null)
                {
                    context.IC_WorkingInfo.Remove(checkData);
                }
            }
            context.SaveChanges();
            return Ok();
        }
        public class WorkingInfoParam
        {
            public List<string> ArrEmployeeATID { get; set; }
            public int Index { get; set; }
            public string EmployeeATID { get; set; }
            public int DepartmentIndex { get; set; }
            public DateTime FromDate { get; set; }
            public DateTime? ToDate { get; set; }
        }
    }
}
