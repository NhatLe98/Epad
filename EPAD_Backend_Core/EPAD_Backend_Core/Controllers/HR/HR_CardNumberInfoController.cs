using EPAD_Backend_Core.Base;
using EPAD_Common.Types;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using EPAD_Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EPAD_Backend_Core.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class HR_CardNumberInfoController : ApiControllerBase
    {
        private readonly IHR_CardNumberInfoService _HR_CardNumberInfoService;
        private readonly IHR_UserService _HR_UserService;
        public HR_CardNumberInfoController(IServiceProvider pProvider) : base(pProvider)
        {
            _HR_CardNumberInfoService = TryResolve<IHR_CardNumberInfoService>();
            _HR_UserService = TryResolve<IHR_UserService>();
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [Authorize]
        [ActionName("GetCustomerAtPage")]
        [HttpGet]
        public async Task<ActionResult<List<HR_CardNumberInfoResult>>> Get([FromQuery] int page, [FromQuery] int pageSize)
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            var allCustomer = await _HR_CardNumberInfoService.GetDataGrid(user.CompanyIndex, page, pageSize);
            return ApiOk(allCustomer);
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [Authorize]
        [ActionName("GetHRCardInfoAtPage")]
        [HttpGet]
        public async Task<ActionResult<List<HR_CardNumberInfoResult>>> GetHRCardInfoAtPage( [FromQuery] int page,[FromQuery] string filter,[FromQuery] int pageSize)
        {
            var user = GetUserInfo();
           if (user == null) return ApiUnauthorized();

            List<AddedParam> addedParams = new List<AddedParam>();
            addedParams.Add(new AddedParam { Key = "Filter", Value = filter });
            addedParams.Add(new AddedParam { Key = "PageIndex", Value = page });
            addedParams.Add(new AddedParam { Key = "PageSize", Value = pageSize });

            var cardInfos = await _HR_CardNumberInfoService.GetPage(addedParams,user.CompanyIndex);
            return ApiOk(cardInfos);
        }

        [Authorize]
        [ActionName("Get_HR_CardNumberInfos")]
        [HttpGet]
        public async Task<ActionResult<List<HR_CardNumberInfoResult>>> Get_HR_CardNumberInfos()
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            var allEmployee = await _HR_CardNumberInfoService.GetAllCardNumberInfo(new string[0], user.CompanyIndex);
            return ApiOk(allEmployee);
        }

        [Authorize]
        [ActionName("Get_HR_CardNumberInfo")]
        [HttpGet("{employeeATID}")]
        public async Task<ActionResult<HR_EmployeeInfoResult>> Get_HR_CardNumberInfo(string employeeATID)
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            var employee = await _HR_CardNumberInfoService.GetAllCardNumberInfoByEmployee(employeeATID, user.CompanyIndex);
            return ApiOk(employee);
        }

        [Authorize]
        [ActionName("Get_HR_CardNumberInfoByCardNumber")]
        [HttpGet("{employeeATID}")]
        public async Task<ActionResult<HR_EmployeeInfoResult>> Get_HR_CardNumberInfoByCardNumber(string employeeATID, [FromQuery] string cardNumber)
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            var employee = await _HR_CardNumberInfoService.GetCardNumberInfo(employeeATID, cardNumber, user.CompanyIndex);
            return ApiOk(employee);
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [Authorize]
        [ActionName("Post_HR_CardNumberInfo")]
        [HttpPost]
        public async Task<ActionResult<HR_EmployeeInfoResult>> Post_HR_CardNumberInfo([FromBody] HR_CardNumberInfoResult value)
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            value.EmployeeATID = value.EmployeeATID.PadLeft(_Config.MaxLenghtEmployeeATID, '0');

            var cardActive = await _HR_CardNumberInfoService.FirstOrDefaultAsync(x => x.CardNumber == value.CardNumber && x.IsActive == true && x.CompanyIndex == user.CompanyIndex);
            if (cardActive != null)
            {
                return ApiError("CardNumberExist");
            }
            var empInfo = await _HR_UserService.GetHR_UserByIDAsync(value.EmployeeATID, user.CompanyIndex);
            if (empInfo == null) {
                return ApiError("EmployeeATIDNotExist");
            }
            value.IsActive = true;
            HR_CardNumberInfo c = _Mapper.Map<HR_CardNumberInfoResult, HR_CardNumberInfo>(value);
           
            BeginTransaction();
            try
            {
                await _HR_CardNumberInfoService.CheckCardActivedOrCreate(c, user.CompanyIndex);
                await SaveChangeAsync();
                CommitTransaction();

                var employee = await _HR_CardNumberInfoService.GetCardNumberInfo(c.EmployeeATID, c.CardNumber, user.CompanyIndex);
                return ApiOk(employee);
            }
            catch (Exception ex)
            {
                RollbackTransaction();
                return ApiError(ex.Message);
            }

        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [Authorize]
        [ActionName("Put_HR_CardNumberInfo")]
        [HttpPut("{employeeATID}")]
        public async Task<ActionResult<HR_EmployeeInfoResult>> Put_HR_CardNumberInfo(string employeeATID, [FromBody] HR_CardNumberInfoResult value)
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            var cardActive = await _HR_CardNumberInfoService
                .CheckCardActiveOtherEmployee(new HR_CardNumberInfo { CardNumber = value.CardNumber, CompanyIndex = user.CompanyIndex, EmployeeATID = value.EmployeeATID }, user.CompanyIndex);
            if (cardActive)
            {
                return ApiError("CardNumberExist");
            }
            var card = await _HR_CardNumberInfoService
                .FirstOrDefaultAsync(x => x.CompanyIndex == user.CompanyIndex && x.EmployeeATID == employeeATID && x.CardNumber == value.CardNumber);

            BeginTransaction();
            try
            {
                card = _Mapper.Map<HR_CardNumberInfoResult, HR_CardNumberInfo>(value);
                await _HR_CardNumberInfoService.CheckCardActivedOrCreate(card, user.CompanyIndex);
                await SaveChangeAsync();
                CommitTransaction();

                var employee = await _HR_CardNumberInfoService.GetCardNumberInfo(card.EmployeeATID, card.CardNumber, user.CompanyIndex);
                return ApiOk(employee);
            }
            catch (Exception ex)
            {
                RollbackTransaction();
                return ApiError(ex.Message);
            }
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [Authorize]
        [ActionName("Put_HR_CardInfo")]
        [HttpPut("{cardIndex}")]
        public async Task<ActionResult<HR_EmployeeInfoResult>> Put_HR_CardInfo(long cardIndex, [FromBody] HR_CardNumberInfoResult value)
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            var empInfo = await _HR_UserService.GetHR_UserByIDAsync(value.EmployeeATID, user.CompanyIndex);
            if (empInfo == null)
            {
                return ApiError("EmployeeInfoExist");
            }

            var cardActive = await _HR_CardNumberInfoService.CheckCardActiveOtherEmployee(new HR_CardNumberInfo { CardNumber = value.CardNumber, CompanyIndex = user.CompanyIndex, EmployeeATID = value.EmployeeATID }, user.CompanyIndex);
            if (cardActive)
            {
                return ApiError("CardNumberExist");
            }

            var card = await _HR_CardNumberInfoService.FirstOrDefaultAsync(x =>  x.CompanyIndex == user.CompanyIndex  && x.Index == value.Index);
            
            try
            {
                card = _Mapper.Map<HR_CardNumberInfoResult, HR_CardNumberInfo>(value);
                card = await _HR_CardNumberInfoService.CheckCardActived(card, user.CompanyIndex);
                await SaveChangeAsync();
                var result = await Task.FromResult(_Mapper.Map<HR_CardNumberInfoResult>(card));
                
                return ApiOk(result);
            }
            catch (Exception ex)
            {
                return ApiError(ex.Message);
            }
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [Authorize]
        [ActionName("DeleteCardMultiByCardIndex")]
        [HttpDelete]
        public async Task<IActionResult> DeleteCardMulti([FromBody] long[] listCardIndex)
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();
            var cardIndex = listCardIndex.ToHashSet();

            BeginTransaction();
            try
            {
                await _HR_CardNumberInfoService.DeleteAsync(x => cardIndex.Contains(x.Index) && x.CompanyIndex == user.CompanyIndex);
                await SaveChangeAsync();
                CommitTransaction();

                return ApiOk();
            }
            catch (Exception ex)
            {
                RollbackTransaction();
                return ApiError(ex.Message);
            }
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [Authorize]
        [ActionName("Delete_HR_CardNumberInfo")]
        [HttpDelete("{employeeATID}")]
        public async Task<IActionResult> Delete_HR_CardNumberInfo(string employeeATID)
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            BeginTransaction();
            try
            {
                await _HR_CardNumberInfoService.DeleteAsync(x => x.EmployeeATID == employeeATID && x.CompanyIndex == user.CompanyIndex);
                //await _HR_UserService.DeleteAsync(x => x.EmployeeATID == employeeATID && x.CompanyIndex == user.CompanyIndex);
                await SaveChangeAsync();
                CommitTransaction();

                return ApiOk();
            }
            catch (Exception ex)
            {
                RollbackTransaction();
                return ApiError(ex.Message);
            }
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [Authorize]
        [ActionName("Delete_HR_CardInfo")]
        [HttpDelete("{cardIndex}")]
        public async Task<IActionResult> Delete_HR_CardInfo(long cardIndex)
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            BeginTransaction();
            try
            {
                await _HR_CardNumberInfoService.DeleteAsync(x => x.Index == cardIndex && x.CompanyIndex == user.CompanyIndex);
                await SaveChangeAsync();
                CommitTransaction();

                return ApiOk();
            }
            catch (Exception ex)
            {
                RollbackTransaction();
                return ApiError(ex.Message);
            }
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [Authorize]
        [ActionName("DeleteCardNumberMultiByEmployee")]
        [HttpDelete]
        public async Task<IActionResult> DeleteCardNumberMulti([FromBody] string[] listEmployee)
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();
            var empLookup = listEmployee.ToHashSet();

            BeginTransaction();
            try
            {
                await _HR_CardNumberInfoService.DeleteAsync(x => empLookup.Contains(x.EmployeeATID) && x.CompanyIndex == user.CompanyIndex);
                await SaveChangeAsync();
                CommitTransaction();

                return ApiOk();
            }
            catch (Exception ex)
            {
                RollbackTransaction();
                return ApiError(ex.Message);
            }
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [Authorize]
        [ActionName("DeleteCardNumberMultiByCardNumber")]
        [HttpDelete]
        public async Task<IActionResult> DeleteCardNumberMultiByCardNumber([FromBody] string[] listCardNumber)
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            BeginTransaction();
            try
            {
                await _HR_CardNumberInfoService.DeleteAsync(x => listCardNumber.Contains(x.CardNumber) && x.CompanyIndex == user.CompanyIndex);
                await SaveChangeAsync();
                CommitTransaction();

                return ApiOk();
            }
            catch (Exception ex)
            {
                RollbackTransaction();
                return ApiError(ex.Message);
            }
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [Authorize]
        [ActionName("DeleteCardNumberMulti")]
        [HttpDelete("{employeeATID}")]
        public async Task<IActionResult> DeleteCardNumberMultiBy(string employeeATID, [FromBody] string[] listCardNumber)
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            BeginTransaction();
            try
            {
                await _HR_CardNumberInfoService.DeleteAsync(x => x.EmployeeATID == employeeATID && listCardNumber.Contains(x.CardNumber) && x.CompanyIndex == user.CompanyIndex);
                await SaveChangeAsync();
                CommitTransaction();

                return ApiOk();
            }
            catch (Exception ex)
            {
                RollbackTransaction();
                return ApiError(ex.Message);
            }
        }
        
    }
}
