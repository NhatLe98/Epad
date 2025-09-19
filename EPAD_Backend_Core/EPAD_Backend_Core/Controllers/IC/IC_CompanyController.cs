using EPAD_Backend_Core.Base;
using EPAD_Backend_Core.WebUtilitys;
using EPAD_Common.Utility;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Linq;

namespace EPAD_Backend_Core.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("api/Company/[action]")]
    [ApiController]
    public class IC_CompanyController : ApiControllerBase
    {
        private EPAD_Context context;
        private IMemoryCache cache;
        public IC_CompanyController(IServiceProvider provider) : base(provider)
        {
            context = TryResolve<EPAD_Context>();
            cache = TryResolve<IMemoryCache>();
        }
        [ActionName("AddCompany")]
        [HttpPost]
        public IActionResult AddCompany([FromBody]CompanyParam param)
        {
            param = (CompanyParam)StringHelper.RemoveWhiteSpace(param);

            if (param.TaxCode == "" || param.Name == "")
            {
                return BadRequest();
            }
            IC_Company checkData = context.IC_Company.Where(t => t.TaxCode == param.TaxCode).FirstOrDefault();
            if (checkData != null)
            {
                return Conflict(PublicFunctions.CreateHttpErrorContent("CompanyIsExist"));
            }
            IC_Company companyInsert = new IC_Company();
            companyInsert.TaxCode = param.TaxCode;
            companyInsert.Name = param.Name;
            companyInsert.Address = param.Address;
            companyInsert.Phone = param.Phone;

            companyInsert.Description = param.Description;
            companyInsert.Password = StringHelper.SHA1(param.Password);
            companyInsert.CreatedDate = DateTime.Now;
            companyInsert.UpdatedDate = DateTime.Now;
            companyInsert.UpdatedUser = "admin";

            context.IC_Company.Add(companyInsert);
            context.SaveChanges();

            return Ok(companyInsert);
        }
        
        [Authorize]
        [ActionName("UpdateCompany")]
        [HttpPost]
        public IActionResult UpdateCompany([FromBody]CompanyParam param)
        {
 
            UserInfo user = UserInfo.GetFromCache(cache,User.Identity.Name);
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            
            param = (CompanyParam)StringHelper.RemoveWhiteSpace(param);
            
            IC_Company company = context.IC_Company.Where(t => t.Index == param.Index).FirstOrDefault();
            if (company == null)
            {
                return NotFound(PublicFunctions.CreateHttpErrorContent("CompanyNotExists"));
            }
            company.Name = param.Name;
            company.Address = param.Address;
            company.Phone = param.Phone;

            company.Description = param.Description;
            company.Password = StringHelper.SHA1(param.Password);

            company.UpdatedDate = DateTime.Now;
            company.UpdatedUser = user.UserName;

            context.SaveChanges();

            return Ok();
        }

        public class CompanyParam
        {
            public int Index { get; set; }
            public string TaxCode { get; set; }
            public string Name { get; set; }
            public string Address { get; set; }

            public string Phone { get; set; }
            public string Description { get; set; }
            public string Password { get; set; }
        }
    }
}
