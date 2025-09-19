using EPAD_Common.Extensions;
using EPAD_Common.Services;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using EPAD_Services.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPAD_Services.Impl
{
    public class GC_CustomerService : BaseServices<GC_Customer, EPAD_Context>, IGC_CustomerService
    {
        public GC_CustomerService(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public async Task<GC_Customer> GetDataByIndex(int index)
        {
            return await DbContext.GC_Customer.FirstOrDefaultAsync(x => x.Index == index);
        }
        public async Task<List<GC_Customer>> GetVipCustomers()
        {
            return await DbContext.GC_Customer.Where(x => x.IsVip).ToListAsync();
        }
        public async Task<GC_Customer> GetDataByEmployeeATIDAndTime(string empATID, DateTime now, int companyIndex)
        {
            return await DbContext.GC_Customer.Where(x => x.CompanyIndex == companyIndex && x.EmployeeATID == empATID
                && x.FromTime <= now && (x.ToTime >= now || (x.ExtensionTime != null && x.ExtensionTime >= now))).FirstOrDefaultAsync();
        }
        public async Task<GC_Customer> GetDataByEmployeeATID(string empATID, int companyIndex)
        {
            return await DbContext.GC_Customer.Where(x => x.CompanyIndex == companyIndex && x.EmployeeATID == empATID).FirstOrDefaultAsync();
        }

        public async Task<int> GetLastedEmployeeATID(int companyIndex)
        {
            var customer = await DbContext.GC_Customer.Where(e => e.CompanyIndex == companyIndex).OrderByDescending(e => e.EmployeeATID).FirstOrDefaultAsync();
            if (customer != null)
            {
                int.TryParse(customer.EmployeeATID, out int customerATID);
                return customerATID;
            }
            else
            {
                return 0;
            }
        }

        public void SaveChanges()
        {
            DbContext.SaveChanges();
        }
    }
}
