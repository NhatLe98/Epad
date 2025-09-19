using EPAD_Backend_Core.Models.DTOs;
using EPAD_Common.Services;
using EPAD_Common.Types;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EPAD_Services.Interface
{
    public interface IHR_CustomerCardService : IBaseServices<HR_CustomerCard, EPAD_Context>
    {
        Task<List<CustomerCardModel>> GetAllCustomerCard(UserInfo user);
        Task<CustomerCardModel> GetCustomerCardById(string id, UserInfo user);
        Task<CustomerCardModel> GetCardNumberByNumber(string number, UserInfo user);
        Task<DataGridClass> GetPage(List<AddedParam> addedParams, UserInfo user);
        Task<bool> IsCardNumberExisted(string cardNumber, UserInfo user);
        Task<bool> IsCardNumberUsing(string cardNumber, UserInfo user);
        Task<bool> IsCardNumberUsing(List<string> cardNumber, UserInfo user);
        Task<bool> IsCardNumberUsing(int index, UserInfo user);
        Task<bool> IsCardNumberUsing(List<int> indexes, UserInfo user);
        Task<bool> AddCustomerCard(string cardNumber, bool isSyncToDevice, UserInfo user);
        Task<bool> DeleteCustomerCard(int index);
        Task<bool> DeleteCustomerCard(List<int> index, UserInfo user);
        Task<List<string>> SyncCustomerCardToDevice(List<CustomerCardModel> listCustomerCard, UserInfo user);
        Task<List<string>> UnsyncCustomerCardToDevice(List<CustomerCardModel> listCustomerCard, UserInfo user);
        List<string> GenerateUniqueNumberStrings(int listLength, int lengthString, string prefix, List<string> existingList);
        Task<List<CustomerCardModel>> ValidationImportCustomerCard(List<CustomerCardModel> param, UserInfo user);
        Task<EmployeeUsingCard> GetEmployeeATIDUsingCard(string employeeATID, int companyIndex, DateTime time);
    }
}
