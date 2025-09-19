using EPAD_Backend_Core.Models.DTOs;
using EPAD_Common.Services;
using EPAD_Common.Types;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using EPAD_Data.Models.HR;
using EPAD_Services.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EPAD_Services.Impl
{
    public class HR_StudentInfoService : BaseServices<HR_StudentInfo, EPAD_Context>, IHR_StudentInfoService
    {
        private ConfigObject _config;
        ezHR_Context ezHR_Context;
        public HR_StudentInfoService(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _config = ConfigObject.GetConfig(_Cache);
            ezHR_Context = serviceProvider.GetService<ezHR_Context>();
        }

        public async Task<List<HR_StudentInfoResult>> GetAllStudentInfo(string[] pStudentATIDs, int pCompanyIndex)
        {
            var empLookup = pStudentATIDs.ToHashSet();
            var dummy = from u in DbContext.HR_User.Where(x => x.CompanyIndex == pCompanyIndex && (empLookup.Count > 0 ? empLookup.Contains(x.EmployeeATID) : true))
                        join e in DbContext.HR_StudentInfo.Where(x => x.CompanyIndex == pCompanyIndex)
                        on u.EmployeeATID equals e.EmployeeATID

                        join sci in DbContext.HR_StudentClassInfo.Where(e => e.CompanyIndex == pCompanyIndex)
                        on e.EmployeeATID equals sci.EmployeeATID into studentclassinfo
                        from studentclassinfoResult in studentclassinfo.DefaultIfEmpty()

                        join classinfo in DbContext.HR_ClassInfo.Where(e => e.CompanyIndex == pCompanyIndex)
                        on studentclassinfoResult.ClassInfoIndex equals classinfo.Index into classinfo
                        from classinfoResult in classinfo.DefaultIfEmpty()

                        join c in DbContext.HR_CardNumberInfo.Where(x => x.CompanyIndex == pCompanyIndex && (empLookup.Count > 0 ? empLookup.Contains(x.EmployeeATID) : true) && x.IsActive == true)
                        on u.EmployeeATID equals c.EmployeeATID into card
                        from ci in card.DefaultIfEmpty()



                        select new { User = u, Employee = e, CardInfo = ci, ClassInfo = classinfoResult };

            var result = dummy.AsEnumerable().Select(x =>
            {
                var rs = _Mapper.Map<HR_StudentInfoResult>(x.User);
                rs = _Mapper.Map(x.Employee, rs);
                if (x.ClassInfo != null)
                {
                    rs.ClassID = x.ClassInfo?.Index;
                }
                rs.CardNumber = x.CardInfo?.CardNumber;
                return rs;
            }).ToList();
            return await Task.FromResult(result);
        }

        public async Task<List<VStarStudentInfoResult>> GetAllStudentInfoVStar(string[] pStudentATIDs, int pCompanyIndex)
        {
            var empLookup = pStudentATIDs.ToHashSet();
            var dummy = from u in DbContext.HR_User.Where(x => x.CompanyIndex == pCompanyIndex && (empLookup.Count > 0 ? empLookup.Contains(x.EmployeeATID) : true)).ToList()
                        join e in DbContext.HR_StudentInfo.Where(x => x.CompanyIndex == pCompanyIndex)
                        on u.EmployeeATID equals e.EmployeeATID

                        join sci in DbContext.HR_StudentClassInfo.Where(e => e.CompanyIndex == pCompanyIndex).ToList()
                        on e.EmployeeATID equals sci.EmployeeATID into studentclassinfo
                        from studentclassinfoResult in studentclassinfo.DefaultIfEmpty()

                        join classinfo in DbContext.HR_ClassInfo.Where(e => e.CompanyIndex == pCompanyIndex)
                        on studentclassinfoResult?.ClassInfoIndex equals classinfo.Index into classinfo
                        from classinfoResult in classinfo.DefaultIfEmpty()

                        //join gradeinfo in DbContext.HR_GradeInfo
                        //on classinfoResult.GradeIndex equals gradeinfo.Index

                        join c in DbContext.HR_CardNumberInfo.Where(x => x.CompanyIndex == pCompanyIndex && (empLookup.Count > 0 ? empLookup.Contains(x.EmployeeATID) : true) && x.IsActive == true)
                        on u.EmployeeATID equals c.EmployeeATID into card
                        from ci in card.DefaultIfEmpty()



                        select new { User = u, Employee = e, CardInfo = ci, ClassInfo = classinfoResult };

            var result = dummy.ToList().Select(x =>
            {
                var rs = new VStarStudentInfoResult();
                rs.EmployeeATID = x.User?.EmployeeATID ?? x.Employee?.EmployeeATID ?? "";
                rs.EmployeeCode = x.User?.EmployeeCode ?? "";
                rs.FullName = x.User?.FullName ?? "";
                rs.CardNumber = x.CardInfo?.CardNumber ?? "";
                rs.ClassIndex = (x.ClassInfo != null) ? x.ClassInfo?.Index ?? "" : "";
                rs.GradeIndex = (x.ClassInfo != null) ? x.ClassInfo?.GradeIndex ?? 0 : 0;
                return rs;
            }).ToList();
            return await Task.FromResult(result);
        }



        public async Task<List<HR_StudentInfoResult>> GetAllStudentInfoByClassIDs(string pClassIDs, int pCompanyIndex)
        {
            var classIDs = JsonConvert.DeserializeObject<List<string>>(pClassIDs);
            var dummy = from u in DbContext.HR_User.Where(x => x.CompanyIndex == pCompanyIndex)
                        join e in DbContext.HR_StudentInfo.Where(x => x.CompanyIndex == pCompanyIndex && (classIDs.Count > 0 ? classIDs.Contains(x.ClassID) : true))
                        on u.EmployeeATID equals e.EmployeeATID
                        join c in DbContext.HR_CardNumberInfo.Where(x => x.CompanyIndex == pCompanyIndex && x.IsActive == true)
                        on u.EmployeeATID equals c.EmployeeATID into card
                        from ci in card.DefaultIfEmpty()

                        select new { User = u, Employee = e, CardInfo = ci };

            var result = dummy.AsEnumerable().Select(x =>
            {
                var rs = _Mapper.Map<HR_StudentInfoResult>(x.User);
                rs = _Mapper.Map(x.Employee, rs);
                rs.CardNumber = x.CardInfo?.CardNumber;
                return rs;
            }).ToList();
            return await Task.FromResult(result);
        }

        public async Task<List<object>> GetAllStudentClassInfo(int pCompanyIndex)
        {
            var result = new List<object>();
            result.Add(new { ClassID = "CLS-10A1", Code = "CLS-10A1", Name = "10A1" });
            result.Add(new { ClassID = "CLS-10A2", Code = "CLS-10A2", Name = "10A2" });
            result.Add(new { ClassID = "CLS-10A3", Code = "CLS-10A3", Name = "10A3" });
            result.Add(new { ClassID = "CLS-10A4", Code = "CLS-10A4", Name = "10A4" });
            result.Add(new { ClassID = "CLS-10A5", Code = "CLS-10A5", Name = "10A5" });

            return await Task.FromResult(result);
        }

        public List<HR_ParentInfoResult> GetParentInfoByStudent(int pCompanyIndex, string studentATID)
        {
            var dummy = from u in DbContext.HR_User.Where(x => x.CompanyIndex == pCompanyIndex)
                        join e in DbContext.HR_ParentInfo.Where(x => x.CompanyIndex == pCompanyIndex && x.Students.Contains(studentATID))
                        on u.EmployeeATID equals e.EmployeeATID

                        select new { User = u, Employee = e };

            var result = dummy.AsEnumerable().Select(x =>
            {
                var rs = _Mapper.Map<HR_ParentInfoResult>(x.User);
                rs = _Mapper.Map(x.Employee, rs);
                return rs;
            }).ToList();

            return result;
        }

        public async Task<DataGridClass> GetDataGrid(string pFilter, string[] pClassID, int pCompanyIndex, int pPage, int pLimit)
        {
            var classLookup = pClassID.ToHashSet();
            var dummy = from u in DbContext.HR_User.Where(x => x.CompanyIndex == pCompanyIndex && x.EmployeeType == (int)EmployeeType.Student)
                        join e in DbContext.HR_StudentInfo.Where(x => x.CompanyIndex == pCompanyIndex)
                        on u.EmployeeATID equals e.EmployeeATID

                        join c in DbContext.HR_CardNumberInfo.Where(x => x.CompanyIndex == pCompanyIndex && x.IsActive == true)
                        on u.EmployeeATID equals c.EmployeeATID into card
                        from ci in card.DefaultIfEmpty()

                        join sci in DbContext.HR_StudentClassInfo.Where(e => e.CompanyIndex == pCompanyIndex)
                        on e.EmployeeATID equals sci.EmployeeATID into studentclassinfo
                        from studentclassinfoResult in studentclassinfo.DefaultIfEmpty()

                        join classinfo in DbContext.HR_ClassInfo.Where(e => e.CompanyIndex == pCompanyIndex)
                        on studentclassinfoResult.ClassInfoIndex equals classinfo.Index into classinfo
                        from classinfoResult in classinfo.DefaultIfEmpty()

                        join p in DbContext.HR_ParentInfo.Where(x => x.CompanyIndex == pCompanyIndex)
                        on u.EmployeeATID equals p.EmployeeATID into parent
                        from pi in parent.DefaultIfEmpty()

                        join us in DbContext.IC_UserMaster.Where(e => e.CompanyIndex == pCompanyIndex)
                        on u.EmployeeATID equals us.EmployeeATID into usWork
                        from usResult in usWork.DefaultIfEmpty()

                        select new { EmployeeATID = u.EmployeeATID, User = u, Employee = e, CardInfo = ci, UserMaster = usResult, ClassInfo = classinfoResult };

            if (classLookup.Count > 0)
            {
                dummy = dummy.Where(x =>
                    classLookup.Contains(x.Employee.ClassID) || classLookup.Contains(x.ClassInfo.Index));
            }
            if (!string.IsNullOrWhiteSpace(pFilter))
            {
                dummy = dummy.Where(x => x.User.FullName.Contains(pFilter) || x.EmployeeATID.Contains(pFilter));
            }

            if (pPage < 1) pPage = 1;

            var result = dummy.OrderBy(x => x.EmployeeATID).Skip((pPage - 1) * pLimit).Take(pLimit).AsEnumerable().Select(x =>
            {

                var rs = _Mapper.Map<HR_StudentInfoResult>(x.User);
                if (x.Employee != null)
                    rs = _Mapper.Map(x.Employee, rs);
                if (x.ClassInfo != null)
                {
                    rs.ClassID = x.ClassInfo?.Index;
                }
                if (x.UserMaster != null)
                    rs = _Mapper.Map(x.UserMaster, rs);
                if (x.CardInfo != null)
                    rs = _Mapper.Map(x.CardInfo, rs);


                rs.ParentsInfo = GetParentInfoByStudent(pCompanyIndex, x.EmployeeATID).Select(x => x.EmployeeATID).ToArray();

                return rs;
            }).ToList();

            var gridClass = new DataGridClass(dummy.Count(), result);

            return await Task.FromResult(gridClass);
        }

        public async Task<HR_StudentInfoResult> GetStudentInfo(string pEmployeeATID, int pCompanyIndex)
        {
            var dummy = from u in DbContext.HR_User.Where(x => x.CompanyIndex == pCompanyIndex && x.EmployeeATID == pEmployeeATID)
                        join e in DbContext.HR_StudentInfo.Where(x => x.CompanyIndex == pCompanyIndex)
                        on u.EmployeeATID equals e.EmployeeATID


                        join sci in DbContext.HR_StudentClassInfo.Where(e => e.CompanyIndex == pCompanyIndex)
                        on e.EmployeeATID equals sci.EmployeeATID into studentclassinfo
                        from studentclassinfoResult in studentclassinfo.DefaultIfEmpty()

                        join classinfo in DbContext.HR_ClassInfo.Where(e => e.CompanyIndex == pCompanyIndex)
                        on studentclassinfoResult.ClassInfoIndex equals classinfo.Index into classinfo
                        from classinfoResult in classinfo.DefaultIfEmpty()

                        join c in DbContext.HR_CardNumberInfo.Where(x => x.CompanyIndex == pCompanyIndex && x.EmployeeATID == pEmployeeATID && x.IsActive == true)
                        on u.EmployeeATID equals c.EmployeeATID into card
                        from ci in card.DefaultIfEmpty()

                        select new { User = u, Employee = e, CardInfo = ci, ClassInfo = classinfoResult };

            var result = dummy.AsEnumerable().Select(x =>
            {
                var rs = _Mapper.Map<HR_StudentInfoResult>(x.User);
                rs = _Mapper.Map(x.Employee, rs);
                if (x.ClassInfo != null)
                {
                    rs.ClassID = x.ClassInfo?.Index;
                }
                rs.CardNumber = x.CardInfo?.CardNumber;
                return rs;
            }).FirstOrDefault();

            return await Task.FromResult(result);
        }

        public async Task<List<IC_StudentImportDTO>> ValidationImportStudent(List<IC_StudentImportDTO> param)
        {
            var listEmployeATIDDB = DbContext.HR_User.Select(e => e.EmployeeATID).ToHashSet();

            List<IC_StudentImportDTO> errorList = new List<IC_StudentImportDTO>();
            var checkDuplicate = param.GroupBy(x => x.EmployeeATID).Where(g => g.Count() > 1).Select(y => y.Key).ToList();
            var checkMaxLength = param.Where(e => e.EmployeeATID.Length > 100
            || e.ClassName.Length > 20
            || e.EmployeeCode.Length > 50
            || e.FullName.Length > 200
            || e.CardNumber.Length > 30
            || e.NameOnMachine.Length > 20).ToList();
            var checkIsNull = param.Where(e => string.IsNullOrWhiteSpace(e.EmployeeATID)
            || string.IsNullOrWhiteSpace(e.ClassName)).ToList();


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
                    if (item.ClassName.Length > 20) item.ErrorMessage += "Tên lớp lớn hơn 20 ký tự" + "\r\n";
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
                    //if (string.IsNullOrEmpty(item.ClassName)) item.ErrorMessage += "Tên lớp không được để trống";
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

        public async Task<List<VStarStudentInfoResult>> GetAllStudentInfoVStarType(string[] pStudentATIDs, int pCompanyIndex, long type)
        {

            var empLookup = pStudentATIDs.ToHashSet();
            var result = new List<VStarStudentInfoResult>();

            if (!_config.IntegrateDBOther)
            {
                var dummy = from u in DbContext.HR_User.Where(x => x.CompanyIndex == pCompanyIndex && (empLookup.Count > 0 ? empLookup.Contains(x.EmployeeATID) : true)).ToList()
                            join e in DbContext.HR_StudentInfo.Where(x => x.CompanyIndex == pCompanyIndex)
                            on u.EmployeeATID equals e.EmployeeATID

                            join sci in DbContext.HR_StudentClassInfo.Where(e => e.CompanyIndex == pCompanyIndex).ToList()
                            on e.EmployeeATID equals sci.EmployeeATID into studentclassinfo
                            from studentclassinfoResult in studentclassinfo.DefaultIfEmpty()

                            join classinfo in DbContext.HR_ClassInfo.Where(e => e.CompanyIndex == pCompanyIndex)
                            on studentclassinfoResult?.ClassInfoIndex equals classinfo.Index into classinfo
                            from classinfoResult in classinfo.DefaultIfEmpty()

                                //join gradeinfo in DbContext.HR_GradeInfo
                                //on classinfoResult.GradeIndex equals gradeinfo.Index

                            join c in DbContext.HR_CardNumberInfo.Where(x => x.CompanyIndex == pCompanyIndex && (empLookup.Count > 0 ? empLookup.Contains(x.EmployeeATID) : true) && x.IsActive == true)
                            on u.EmployeeATID equals c.EmployeeATID into card
                            from ci in card.DefaultIfEmpty()



                            select new { User = u, Employee = e, CardInfo = ci, ClassInfo = classinfoResult };

                 result = dummy.ToList().Select(x =>
                {
                    var rs = new VStarStudentInfoResult();
                    rs.EmployeeATID = x.User?.EmployeeATID ?? x.Employee?.EmployeeATID ?? "";
                    rs.EmployeeCode = x.User?.EmployeeCode ?? "";
                    rs.FullName = x.User?.FullName ?? "";
                    rs.CardNumber = x.CardInfo?.CardNumber ?? "";
                    rs.ClassIndex = (x.ClassInfo != null) ? x.ClassInfo?.Index ?? "" : "";
                    rs.GradeIndex = (x.ClassInfo != null) ? x.ClassInfo?.GradeIndex ?? 0 : 0;
                    return rs;
                }).ToList();
            }
            else
            {
                
                var departmentList = ezHR_Context.HR_Department.Where(x => x.CompanyIndex == _config.CompanyIndex).ToList();
                var typegroups = GetChildren(departmentList, type);
                var allTypes = typegroups.Select(x => x.Index).ToList();
                var dummy = (from e in ezHR_Context.HR_Employee.Where(x => x.CompanyIndex == _config.CompanyIndex)
                join wi in ezHR_Context.HR_WorkingInfo.Where(x => x.CompanyIndex == _config.CompanyIndex)
                on e.EmployeeATID equals wi.EmployeeATID into eWork
                from eWorkResult in eWork.DefaultIfEmpty()

                join d in ezHR_Context.HR_Department.Where(x => x.CompanyIndex == _config.CompanyIndex)
                on eWorkResult.DepartmentIndex equals d.Index into dWork
                from dWorkResult in dWork.DefaultIfEmpty()
        
                where (e.MarkForDelete == null || e.MarkForDelete == false) // loc nhan vien chua nghi viec
                 && dWorkResult != null 

                select new { EmployeeATID = e.EmployeeATID, Employee = e, WorkingInfo = eWorkResult, Department = dWorkResult, FullName = e.LastName + " " + e.MidName + " " + e.FirstName });
              
                result = dummy.ToList().Select(x =>
                {
                    var gradeIndex = (x.Department != null) ? x.Department?.ParentIndex != null ? (int)x.Department?.ParentIndex : 0 : 0;
                    var rs = new VStarStudentInfoResult();
                    rs.EmployeeATID = x.EmployeeATID;
                    rs.EmployeeCode = x.Employee.EmployeeCode;
                    rs.FullName = x.FullName;
                    rs.CardNumber = x.Employee.CardNumber;
                    rs.ClassIndex = (x.Department != null) ? x.Department?.Index != null ? x.Department?.Index.ToString() : "" : "";
                    rs.GradeIndex = (x.Department != null) ? x.Department?.ParentIndex != null ? (int)x.Department?.ParentIndex : 0 : 0;
                    rs.ClassName = x.Department?.Name;
                    rs.GradeName = gradeIndex == 0 ? "" : departmentList.FirstOrDefault(x => x.Index == long.Parse(gradeIndex.ToString())).Name;
                    return rs;
                }).ToList();

                result = result.Where(x => !string.IsNullOrEmpty(x.ClassIndex) && allTypes.Contains(Convert.ToInt64(x.ClassIndex))).ToList();
            }
            return await Task.FromResult(result);
        }


        private List<HR_Department> GetChildren(List<HR_Department> foos, long id)
        {
            return foos
                .Where(x => x.ParentIndex == id)
                .Union(foos.Where(x => x.ParentIndex == id)
                    .SelectMany(y => GetChildren(foos, y.Index))
                ).ToList();
        }

        private List<IC_Department> GetChildren(List<IC_Department> foos, long id)
        {
            return foos
                .Where(x => x.ParentIndex == id)
                .Union(foos.Where(x => x.ParentIndex == id)
                    .SelectMany(y => GetChildren(foos, y.Index))
                ).ToList();
        }

       

    }
}
