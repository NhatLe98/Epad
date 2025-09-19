using EPAD_Common.Services;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EPAD_Services.Interface
{
    public interface IGC_CustomerService : IBaseServices<GC_Customer, EPAD_Context>
    {
        Task<GC_Customer> GetDataByIndex(int index);
        Task<List<GC_Customer>> GetVipCustomers();
        Task<GC_Customer> GetDataByEmployeeATIDAndTime(string empATID, DateTime now, int companyIndex);
        Task<GC_Customer> GetDataByEmployeeATID(string empATID, int companyIndex);
        Task<int> GetLastedEmployeeATID(int companyIndex);
        void SaveChanges();

    }
}
