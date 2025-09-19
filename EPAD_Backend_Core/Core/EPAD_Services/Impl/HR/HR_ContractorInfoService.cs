using EPAD_Common.Services;
using EPAD_Common.Types;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using EPAD_Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EPAD_Services.Impl
{
    public class HR_ContractorInfoService : BaseServices<HR_ContractorInfo, EPAD_Context>, IHR_ContractorInfoService
    {
        public HR_ContractorInfoService(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public async Task<List<HR_ContractorInfoResult>> GetAllContractorInfo(string[] pContractorATIDs, int pCompanyIndex)
        {
            var empLookup = pContractorATIDs.ToHashSet();
            var dummy = from u in DbContext.HR_User.Where(x => x.CompanyIndex == pCompanyIndex && (empLookup.Count > 0 ? empLookup.Contains(x.EmployeeATID) : true))
                        join e in DbContext.HR_ContractorInfo.Where(x => x.CompanyIndex == pCompanyIndex && (empLookup.Count > 0 ? empLookup.Contains(x.EmployeeATID) : true))
                        on u.EmployeeATID equals e.EmployeeATID
                        join c in DbContext.HR_CardNumberInfo.Where(x => x.CompanyIndex == pCompanyIndex && (empLookup.Count > 0 ? empLookup.Contains(x.EmployeeATID) : true) && x.IsActive == true)
                        on u.EmployeeATID equals c.EmployeeATID into card
                        from ci in card.DefaultIfEmpty()

                        select new { User = u, Employee = e, CardInfo = ci };

            var result = dummy.AsEnumerable().Select(x =>
            {
                var rs = _Mapper.Map<HR_ContractorInfoResult>(x.User);
                rs = _Mapper.Map(x.Employee, rs);
                rs.CardNumber = x.CardInfo?.CardNumber;
                return rs;
            }).ToList();
            return await Task.FromResult(result);
        }

        public async Task<HR_ContractorInfoResult> GetContractorInfo(string pEmployeeATID, int pCompanyIndex)
        {
            var dummy = from u in DbContext.HR_User.Where(x => x.CompanyIndex == pCompanyIndex && x.EmployeeATID == pEmployeeATID)
                        join e in DbContext.HR_ContractorInfo.Where(x => x.CompanyIndex == pCompanyIndex && x.EmployeeATID == pEmployeeATID)
                        on u.EmployeeATID equals e.EmployeeATID
                        join c in DbContext.HR_CardNumberInfo.Where(x => x.CompanyIndex == pCompanyIndex && x.EmployeeATID == pEmployeeATID && x.IsActive == true)
                        on u.EmployeeATID equals c.EmployeeATID into card
                        from ci in card.DefaultIfEmpty()

                        select new { User = u, Employee = e, CardInfo = ci };

            var result = dummy.AsEnumerable().Select(x =>
            {
                var rs = _Mapper.Map<HR_ContractorInfoResult>(x.User);
                rs = _Mapper.Map(x.Employee, rs);
                rs.CardNumber = x.CardInfo?.CardNumber;
                return rs;
            }).FirstOrDefault();

            return await Task.FromResult(result);
        }

        public async Task<DataGridClass> GetDataGrid(int pCompanyIndex, int pPage, int pLimit)
        {
            var dummy = from u in DbContext.HR_User.Where(x => x.CompanyIndex == pCompanyIndex && x.EmployeeType == 6)
                        join e in DbContext.HR_ContractorInfo.Where(x => x.CompanyIndex == pCompanyIndex)
                        on u.EmployeeATID equals e.EmployeeATID
                        join c in DbContext.HR_CardNumberInfo.Where(x => x.CompanyIndex == pCompanyIndex && x.IsActive == true)
                        on u.EmployeeATID equals c.EmployeeATID into card
                        from ci in card.DefaultIfEmpty()

                        select new { EmployeeATID = u.EmployeeATID, User = u, Employee = e, CardInfo = ci };

            if (pPage < 1) pPage = 1;

            var result = dummy.OrderBy(x => x.EmployeeATID).Skip((pPage - 1) * pLimit).Take(pLimit).AsEnumerable().Select(x =>
            {
                var rs = _Mapper.Map<HR_CustomerInfoResult>(x.User);
                rs = _Mapper.Map(x.Employee, rs);
                rs.CardNumber = x.CardInfo?.CardNumber;
                return rs;
            }).ToList();

            var gridClass = new DataGridClass(result.Count, result);

            return await Task.FromResult(gridClass);
        }
    }
}
