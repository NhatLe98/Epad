using EPAD_Backend_Core.Models.DTOs;
using EPAD_Common.Services;
using EPAD_Common.Types;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace EPAD_Services.Interface
{
    public interface IHR_CustomerInfoService : IBaseServices<HR_CustomerInfo, EPAD_Context>
    {
        Task<DataGridClass> GetDataGrid(int pCompanyIndex, int pPage, int pLimit);
        Task<List<HR_CustomerInfoResult>> GetCustomerInfoExcludeExpired(string[] pEmployeeATIDs, int pCompanyIndex);
        Task<List<HR_CustomerInfoResult>> GetAllCustomerInfo(string[] pCustomerATIDs, int pCompanyIndex);
        Task<List<HR_CustomerInfoResult>> GetNewestActiveCustomerInfo(string[] pEmployeeATIDs, int pCompanyIndex);
        Task<List<HR_CustomerInfoResult>> GetNewestCustomerInfo(string[] pEmployeeATIDs, int pCompanyIndex);
        Task<List<HR_CustomerInfoResult>> GetAllCustomerAndContractorInfo(string[] pEmployeeATIDs, int pCompanyIndex);
        Task<HR_CustomerInfoResult> GetCustomerInfo(string pEmployeeATID, int pCompanyIndex);
        Task<DataGridClass> GetPage(List<AddedParam> addedParams, int pCompanyIndex);
        Task<DataGridClass> GetPageAdvance(List<AddedParam> addedParams, int pCompanyIndex, List<string> studentOfParent);
        Task CheckCustomerInfoActivedOrCreate(HR_CustomerInfo hrCustomerInfo, int pCompanyIndex);
        Task<List<IC_CustomerImportDTO>> ValidationImportCustomer(List<IC_CustomerImportDTO> param, int userType, UserInfo user);
        Task<List<IC_CustomerImportDTO>> ValidationImportCustomer_KinhDo(List<IC_CustomerImportDTO> param, int userType, UserInfo user);
        List<string> GenerateUniqueNumberStrings(int listLength, int lengthString, string prefix, List<string> existingList);
        Task<bool> ImportDataFromGoogleSheet(int companyIndex);
    }
}
