using System;
using System.Collections.Generic;
using System.Linq;
using EPAD_Backend_Core.Base;
using EPAD_Backend_Core.WebUtilitys;
using EPAD_Common.Types;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace EPAD_Backend_Core.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("api/PrivilegeDepartment/[action]")]
    [ApiController]
    public class IC_PrivilegeDepartmentController : ApiControllerBase
    {
        private EPAD_Context context;
        private ezHR_Context otherContext;
        private IMemoryCache cache;
        public IC_PrivilegeDepartmentController(IServiceProvider provider):base(provider)
        {
            context = TryResolve<EPAD_Context>();
            cache = TryResolve<IMemoryCache>();
            otherContext = TryResolve<ezHR_Context>();
        }

        [Authorize]
        [ActionName("GetDepartmentsByPrivilegeIndex")]
        [HttpGet]
        public IActionResult GetDepartmentsByPrivilegeIndex(int privilegeIndex)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }
            IC_UserPrivilege privilegeCheck = context.IC_UserPrivilege.Where(t => t.Index == privilegeIndex).FirstOrDefault();
            if (privilegeCheck == null)
            {
                return BadRequest("UserPrivilegeNotExist");
            }
            List<IC_PrivilegeDepartment> listPrivilegeDepartment = context.IC_PrivilegeDepartment.Where(t => t.CompanyIndex == user.CompanyIndex && t.PrivilegeIndex == privilegeIndex).ToList();

            List<long> listData = listPrivilegeDepartment.Select(x => x.DepartmentIndex).ToList();
            return Ok(listData);
        }
        [Authorize]
        [ActionName("UpdatePrivilegeAndDepartments")]
        [HttpPost]
        public IActionResult UpdatePrivilegeAndDepartments([FromBody] PrivilegeAndDepartmentsParam param)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            IC_UserPrivilege privilegeCheck = context.IC_UserPrivilege.Where(t => t.Index == param.PrivilegeIndex).FirstOrDefault();
            if (privilegeCheck == null)
            {
                return BadRequest("UserPrivilegeNotExist");
            }
            ConfigObject config = ConfigObject.GetConfig(cache);
            List<IC_DepartmentDTO> lstDept = new List<IC_DepartmentDTO>();
            if (config.IntegrateDBOther)
            {
                lstDept = otherContext.HR_Department.Where(x => x.CompanyIndex == config.CompanyIndex)
                   .Select(x => new IC_DepartmentDTO() { Index = x.Index, Name = x.Name, Code = x.Code, ParentIndex = x.ParentIndex }).ToList();
            }
            else
            {
                lstDept = context.IC_Department.Where(x => x.CompanyIndex == user.CompanyIndex)
                   .Select(x => new IC_DepartmentDTO() { Index = x.Index, Name = x.Name, Code = x.Code, ParentIndex = x.ParentIndex }).ToList();
            }

            context.IC_PrivilegeDepartment.RemoveRange(context.IC_PrivilegeDepartment.Where(t => t.CompanyIndex == user.CompanyIndex
                && t.PrivilegeIndex == param.PrivilegeIndex));
            DateTime now = DateTime.Now;
            List<long> listDepartmentIndex = new List<long>();
            for (int i = 0; i < param.ListDepartmentIndexs.Count; i++)
            {
                IC_DepartmentDTO depInfo = lstDept.Where(t => t.Index == param.ListDepartmentIndexs[i]).FirstOrDefault();
                if (depInfo != null) 
                {
                    if (listDepartmentIndex.Contains(param.ListDepartmentIndexs[i]) == false)
                    {
                        listDepartmentIndex.Add(param.ListDepartmentIndexs[i]);
                    }
                    //RecursiveGetListParentDepartmentIndex(lstDept, depInfo.ParentIndex == null ? long.Parse("0") : depInfo.ParentIndex.Value, ref listDepartmentIndex);
                }
            }


            for (int i = 0; i < listDepartmentIndex.Count; i++)
            {
                IC_PrivilegeDepartment dataInsert = new IC_PrivilegeDepartment();
                dataInsert.PrivilegeIndex = param.PrivilegeIndex;
                dataInsert.DepartmentIndex = listDepartmentIndex[i];
                dataInsert.CompanyIndex = user.CompanyIndex;
                dataInsert.Role = "Full";
                dataInsert.UpdatedDate = now;
                dataInsert.UpdatedUser = user.UserName;

                context.IC_PrivilegeDepartment.Add(dataInsert);
            }
            context.SaveChanges();

            return Ok();
        }
        private void RecursiveGetListParentDepartmentIndex(List<IC_DepartmentDTO> pListDep,long pParentDepIndex, ref List<long> listDepartmentIndex)
        {
            IC_DepartmentDTO parentDep = pListDep.Where(t => t.Index == pParentDepIndex).FirstOrDefault();
            if(parentDep != null)
            {
                if (listDepartmentIndex.Contains(parentDep.Index) == false)
                {
                    listDepartmentIndex.Add(parentDep.Index);
                }
                RecursiveGetListParentDepartmentIndex(pListDep, parentDep.ParentIndex == null ? long.Parse("0") : parentDep.ParentIndex.Value, ref listDepartmentIndex);
            }
        }
    }

    public class PrivilegeAndDepartmentsParam
    {
        public int PrivilegeIndex { get; set; }
        public List<int> ListDepartmentIndexs { get; set; }
    }
}
