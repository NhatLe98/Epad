using EPAD_Common.Services;
using EPAD_Common.Types;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EPAD_Services.Interface
{
    public interface IHR_CardNumberInfoService : IBaseServices<HR_CardNumberInfo, EPAD_Context>
    {
        public Task<DataGridClass> GetDataGrid(int pCompanyIndex, int pPage, int pLimit);
        public Task<DataGridClass> GetPage(List<AddedParam> addedParams, int pCompanyIndex);
        public Task<List<HR_CardNumberInfoResult>> GetAllCardNumberInfo(string[] pEmployeeATIDs, int companyIndex);
        public Task<List<HR_CardNumberInfoResult>> GetAllCardNumberInfoByEmployee(string pEmployeeATIDs, int companyIndex);
        public Task<HR_CardNumberInfoResult> GetCardNumberInfo(string pEmployeeATID, string pCardNumber, int pCompanyIndex);
        public Task CheckCardActivedOrCreateList(List<HR_CardNumberInfo> listHRCardInfo, int pCompanyIndex, bool? isOverwrite = null);
        public Task CheckCardActivedOrCreate(HR_CardNumberInfo hrCardInfo, int pCompanyIndex);
        Task<bool> ReturnOrDeleteCard(string cardNumber, UserInfo user);
        Task AddOrUpdateNewCard(HR_CardNumberInfo hrCardInfo, UserInfo user);
        public Task CheckCardNumberInforActivedOrCreate(HR_CardNumberInfo hrCardInfo, int pCompanyIndex);
        public Task<bool> CheckCardActiveCurrentEmployee(HR_CardNumberInfo hrCardInfo, int pCompanyIndex);
        public Task<bool> CheckCardActiveOtherEmployee(HR_CardNumberInfo hrCardInfo, int pCompanyIndex);
        public Task<HR_CardNumberInfo> CheckCardActived(HR_CardNumberInfo hrCardInfo, int pCompanyIndex);


    }
}
