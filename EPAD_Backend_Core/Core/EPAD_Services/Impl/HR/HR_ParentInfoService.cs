using EPAD_Backend_Core.Models.DTOs;
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
    public class HR_ParentInfoService : BaseServices<HR_ParentInfo, EPAD_Context>, IHR_ParentInfoService
    {
        private ConfigObject _config;
        public HR_ParentInfoService(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _config = ConfigObject.GetConfig(_Cache);
        }

        public async Task<List<HR_ParentInfoResult>> GetAllParentInfo(string[] pEmployeeATIDs, int pCompanyIndex)
        {
            var empLookup = pEmployeeATIDs.ToHashSet();
            var dummy = from u in DbContext.HR_User.Where(x => x.CompanyIndex == pCompanyIndex && (empLookup.Count > 0 ? empLookup.Contains(x.EmployeeATID) : true))
                        join e in DbContext.HR_ParentInfo.Where(x => x.CompanyIndex == pCompanyIndex)
                        on u.EmployeeATID equals e.EmployeeATID
                        join c in DbContext.HR_CardNumberInfo.Where(x => x.CompanyIndex == pCompanyIndex && (empLookup.Count > 0 ? empLookup.Contains(x.EmployeeATID) : true) && x.IsActive == true)
                        on u.EmployeeATID equals c.EmployeeATID into card
                        from ci in card.DefaultIfEmpty()

                        select new { User = u, Employee = e, CardInfo = ci };

            var result = dummy.AsEnumerable().Select(x =>
            {
                var rs = _Mapper.Map<HR_ParentInfoResult>(x.User);
                rs = _Mapper.Map(x.Employee, rs);
                rs.CardNumber = x.CardInfo?.CardNumber;
                return rs;
            }).ToList();
            return await Task.FromResult(result);
        }

        public async Task<DataGridClass> GetDataGrid(string pFilter, int pCompanyIndex, int pPage, int pLimit)
        {
            var dummy = from u in DbContext.HR_User.Where(x => x.CompanyIndex == pCompanyIndex && x.EmployeeType == (int)EmployeeType.Parents)
                        join e in DbContext.HR_ParentInfo.Where(x => x.CompanyIndex == pCompanyIndex)
                        on u.EmployeeATID equals e.EmployeeATID
                        join c in DbContext.HR_CardNumberInfo.Where(x => x.CompanyIndex == pCompanyIndex && x.IsActive == true)
                        on u.EmployeeATID equals c.EmployeeATID into card
                        from ci in card.DefaultIfEmpty()
                        join us in DbContext.IC_UserMaster.Where(e => e.CompanyIndex == pCompanyIndex)
                        on u.EmployeeATID equals us.EmployeeATID into usWork
                        from usResult in usWork.DefaultIfEmpty()

                        select new { EmployeeATID = u.EmployeeATID, User = u, Employee = e, CardInfo = ci, UserMaster = usResult };

            if (pFilter != "" && pFilter != null)
            {
                dummy = dummy.Where(x => x.User.FullName.Contains(pFilter) || x.EmployeeATID.Contains(pFilter));
            }

            if (pPage < 1) pPage = 1;

            var result = dummy.OrderBy(x => x.EmployeeATID).Skip((pPage - 1) * pLimit).Take(pLimit).AsEnumerable().Select(x =>
            {
                var rs = _Mapper.Map<HR_ParentInfoResult>(x.User);
                if(x.Employee != null)
                    rs = _Mapper.Map(x.Employee, rs);
                if(x.UserMaster!=null)
                    rs = _Mapper.Map(x.UserMaster, rs);
                if(x.CardInfo != null)
                    rs = _Mapper.Map(x.CardInfo, rs);
                //rs.CardNumber = x.CardInfo?.CardNumber;
                //if(rs.DayOfBirth > 0 && rs.MonthOfBirth > 0 && rs.YearOfBirth > 0 )
                //{
                //    rs.BirthDay = new DateTime((int) rs.YearOfBirth, (int) rs.MonthOfBirth, (int) rs.DayOfBirth);
                //}
                return rs;
            }).ToList();

            var gridClass = new DataGridClass(dummy.Count(), result);

            return await Task.FromResult(gridClass);
        }

        public async Task<HR_ParentInfoResult> GetParentInfo(string pEmployeeATID, int pCompanyIndex)
        {
            var dummy = from u in DbContext.HR_User.Where(x => x.CompanyIndex == pCompanyIndex && x.EmployeeATID == pEmployeeATID)
                        join e in DbContext.HR_ParentInfo.Where(x => x.CompanyIndex == pCompanyIndex)
                        on u.EmployeeATID equals e.EmployeeATID
                        join c in DbContext.HR_CardNumberInfo.Where(x => x.CompanyIndex == pCompanyIndex && x.EmployeeATID == pEmployeeATID && x.IsActive == true)
                        on u.EmployeeATID equals c.EmployeeATID into card
                        from ci in card.DefaultIfEmpty()

                        select new { User = u, Employee = e, CardInfo = ci };

            var result = dummy.AsEnumerable().Select(x =>
            {
                var rs = _Mapper.Map<HR_ParentInfoResult>(x.User);
                rs = _Mapper.Map(x.Employee, rs);
                rs.CardNumber = x.CardInfo?.CardNumber;
                return rs;
            }).FirstOrDefault();

            return await Task.FromResult(result);
        }

        public async Task<List<IC_ParentImportDTO>> ValidationImportParent(List<IC_ParentImportDTO> param)
        {
            var listEmployeATIDDB = DbContext.HR_User.Where(e => e.EmployeeType != (int)EmployeeType.Parents).Select(e => e.EmployeeATID).ToHashSet();

            List<IC_ParentImportDTO> errorList = new List<IC_ParentImportDTO>();
            var checkDuplicate = param.GroupBy(x => x.EmployeeATID).Where(g => g.Count() > 1).Select(y => y.Key).ToList();
            var checkMaxLength = param.Where(e => e.EmployeeATID.Length > 100
            || e.EmployeeCode.Length > 50
            || e.FullName.Length > 200
            || e.CardNumber.Length > 30
            || e.NameOnMachine.Length > 20).ToList();
            var checkIsNull = param.Where(e => string.IsNullOrWhiteSpace(e.EmployeeATID)).ToList();

            var checkExisted = param.Where(e => listEmployeATIDDB.Contains(e.EmployeeATID.PadLeft(_config.MaxLenghtEmployeeATID, '0'))).ToList();

            if (checkDuplicate != null && checkDuplicate.Count() > 0)
            {
                var duplicate = param.Where(e => checkDuplicate.Contains(e.EmployeeATID)).ToList();
                foreach (var item in duplicate)
                {
                    item.ErrorMessage = "Trùng mã nhân viên\r\n";
                }
                errorList.AddRange(duplicate);
            }
            if (checkMaxLength != null && checkMaxLength.Count() > 0)
            {
                foreach (var item in checkMaxLength)
                {
                    if (item.EmployeeATID.Length > 100) item.ErrorMessage += "Mã chấm công lớn hơn 50 ký tự" + "\r\n";
                    if (item.EmployeeCode.Length > 50) item.ErrorMessage += "Mã nhân viên lớn hơn 50 ký tự" + "\r\n";
                    if (item.FullName.Length > 200) item.ErrorMessage += "Tên nhân viên lớn hơn 200 ký tự" + "\r\n";
                    if (item.CardNumber.Length > 30) item.ErrorMessage += "Mã thẻ lớn hơn 30 ký tự" + "\r\n";
                    if (item.NameOnMachine.Length > 20) item.ErrorMessage += "Tên trên máy lớn hơn 20 ký tự" + "\r\n";

                }
                errorList.AddRange(checkMaxLength);
            }
            if (checkIsNull != null && checkIsNull.Count() > 0)
            {
                foreach (var item in checkIsNull)
                {
                    if (string.IsNullOrEmpty(item.EmployeeATID)) item.ErrorMessage += "Mã chấm không được để trống\r\n";
                }

                errorList.AddRange(checkIsNull);
            }
            if (checkExisted != null && checkExisted.Count > 0)
            {
                foreach (var item in checkExisted)
                {
                    item.ErrorMessage += "Mã nhân viên đã tồn tại\r\n";
                }
                errorList.AddRange(checkExisted);
            }
            return await Task.FromResult(errorList);
        }
    }
}
