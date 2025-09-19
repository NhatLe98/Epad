using ClosedXML.Excel;
using EPAD_Backend_Core.Base;
using EPAD_Backend_Core.Models.DTOs;
using EPAD_Backend_Core.WebUtilitys;
using EPAD_Common.Types;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using EPAD_Logic;
using EPAD_Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using EPAD_Data.Models.HR;
using EPAD_Data.Entities.HR;

namespace EPAD_Backend_Core.Controllers
{
    
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class HR_StudentInfoController : ApiControllerBase
    {
        private EPAD_Context context;
        private IMemoryCache cache;
        private ConfigObject _config;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IHR_StudentInfoService _HR_StudentInfoService;
        private readonly IHR_UserService _HR_UserService;
        private readonly IIC_UserMasterService _iC_UserMasterService;
        private readonly IHR_CardNumberInfoService _HR_CardNumberInfoService;
        private readonly IHR_StudentClassInfoService _HR_StudentClassInfoService;
        private readonly IHR_ClassInfoService _HR_ClassInfoService;
        private readonly IIC_UserMasterLogic _IIC_UserMasterLogic;
        private readonly IIC_DepartmentLogic _iC_DepartmentLogic;
        private readonly IIC_AuditLogic _IIC_AuditLogic;
        private readonly IIC_ConfigLogic _IIC_ConfigLogic;
        private readonly IIC_CommandLogic _IIC_CommandLogic;
        private readonly string _configClientName;
        public HR_StudentInfoController(IServiceProvider pProvider) : base(pProvider)
        {
            context = TryResolve<EPAD_Context>();
            cache = TryResolve<IMemoryCache>();
            _config = ConfigObject.GetConfig(cache);
            _hostingEnvironment = TryResolve<IHostingEnvironment>();
            _HR_StudentInfoService = TryResolve<IHR_StudentInfoService>();
            _HR_UserService = TryResolve<IHR_UserService>();
            _HR_CardNumberInfoService = TryResolve<IHR_CardNumberInfoService>();
            _HR_StudentClassInfoService = TryResolve<IHR_StudentClassInfoService>();
            _HR_ClassInfoService = TryResolve<IHR_ClassInfoService>();
            _iC_UserMasterService = TryResolve<IIC_UserMasterService>();
            _iC_DepartmentLogic = TryResolve<IIC_DepartmentLogic>();
            _IIC_UserMasterLogic = TryResolve<IIC_UserMasterLogic>();
            _IIC_AuditLogic = TryResolve<IIC_AuditLogic>();
            _IIC_ConfigLogic = TryResolve<IIC_ConfigLogic>();
            _IIC_CommandLogic = TryResolve<IIC_CommandLogic>();
            _configClientName = _Configuration.GetValue<string>("ClientName").ToUpper();
        }

        [Authorize]
        [ActionName("Get_HR_StudentInfoListType")]
        [HttpGet]
        public async Task<ActionResult<List<VStarStudentInfoResult>>> Get_HR_StudentInfoListType(long type)
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            var allEmployee = new List<VStarStudentInfoResult>();
            if (_configClientName == ClientName.VSTAR.ToString())
            {
                allEmployee = await _HR_StudentInfoService.GetAllStudentInfoVStarType(new string[0], user.CompanyIndex, type);
            }
            return ApiOk(allEmployee);
        }

        [Authorize]
        [ActionName("GetStudentAtPage")]
        [HttpGet]
        public async Task<ActionResult<List<HR_EmployeeInfoResult>>> Get([FromQuery] string[] c, [FromQuery] int page, [FromQuery] int pageSize, [FromQuery] string filter)
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            var allStudent = await _HR_StudentInfoService.GetDataGrid(filter, c, user.CompanyIndex, page, pageSize);
            return ApiOk(allStudent);
        }

        //[Authorize]
        [ActionName("Get_HR_Student_ByClassIDs")]
        [HttpGet]
        public async Task<ActionResult<List<HR_StudentInfoResult>>> Get([FromQuery] string classIDs)
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            var allEmployee = await _HR_StudentInfoService.GetAllStudentInfoByClassIDs(classIDs, user.CompanyIndex);
            return ApiOk(allEmployee);
        }

        [Authorize]
        [ActionName("Get_AllHR_ClassInfo")]
        [HttpGet]
        public async Task<ActionResult<List<object>>> Get()
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            var allEmployee = await _HR_StudentInfoService.GetAllStudentClassInfo(user.CompanyIndex);
            return ApiOk(allEmployee);
        }

        [Authorize]
        [ActionName("Get_HR_StudentInfos")]
        [HttpGet]
        public async Task<ActionResult<List<HR_StudentInfoResult>>> Get_HR_StudentInfos()
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            var allEmployee = await _HR_StudentInfoService.GetAllStudentInfo(new string[0], user.CompanyIndex);
            return ApiOk(allEmployee);
        }

        // This API is use only for VStar
        [Authorize]
        [ActionName("Get_HR_StudentInfoList")]
        [HttpGet]
        public async Task<ActionResult<List<VStarStudentInfoResult>>> Get_HR_StudentInfoList()
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            var allEmployee = new List<VStarStudentInfoResult>();
            if (_configClientName == ClientName.VSTAR.ToString())
            {
                allEmployee = await _HR_StudentInfoService.GetAllStudentInfoVStar(new string[0], user.CompanyIndex);
            }
            return ApiOk(allEmployee);
        }

        [Authorize]
        [ActionName("Get_HR_StudentInfo")]
        [HttpGet("{employeeATID}")]
        public async Task<ActionResult<HR_EmployeeInfoResult>> Get_HR_StudentInfo(string employeeATID)
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            var employee = await _HR_StudentInfoService.GetStudentInfo(employeeATID, user.CompanyIndex);
            return ApiOk(employee);
        }

     
        [Authorize]
        [ActionName("Post_HR_StudentInfo")]
        [HttpPost]
        public async Task<ActionResult<HR_EmployeeInfoResult>> Post_HR_StudentInfo([FromBody] HR_StudentInfoResult value)
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();



            value.EmployeeATID = value.EmployeeATID.PadLeft(_Config.MaxLenghtEmployeeATID, '0');
            value.CompanyIndex = user.CompanyIndex;

            HR_User u = _Mapper.Map<HR_StudentInfoResult, HR_User>(value);
            u.EmployeeType = (int)EmployeeType.Student;
            // Student class mapping
            HR_StudentClassInfo sim = new HR_StudentClassInfo
            {
                EmployeeATID = value.EmployeeATID,
                ClassInfoIndex = value.ClassID
            };
            // card info

            var cardActive = await _HR_CardNumberInfoService.FirstOrDefaultAsync(x => x.CardNumber == value.CardNumber &&  x.IsActive == true && x.CompanyIndex == user.CompanyIndex);
            if (cardActive != null)
            {
                return ApiError("CardNumberExist");
            }

            var check = await _HR_UserService.FirstOrDefaultAsync(x => x.CompanyIndex == user.CompanyIndex && x.EmployeeATID == value.EmployeeATID);
            if (check != null)
                return ApiError("EmployeeATIDExist");

            HR_CardNumberInfo c = _Mapper.Map<HR_StudentInfoResult, HR_CardNumberInfo>(value);
            c.IsActive = true;
            c.CardNumber = value.CardNumber;
            // user master
            IC_UserMasterDTO us = new IC_UserMasterDTO
            {
                EmployeeATID = value.EmployeeATID,
                NameOnMachine = value.NameOnMachine,
                CompanyIndex = user.CompanyIndex,
                Password = value.Password,
                CreatedDate = DateTime.Now,
                UpdatedUser = user.UserName
            };

            HR_StudentInfo e = _Mapper.Map<HR_StudentInfoResult, HR_StudentInfo>(value);

            BeginTransaction();
            try
            {
                await _HR_UserService.InsertAsync(u);
                await _HR_StudentInfoService.InsertAsync(e);
                await _HR_CardNumberInfoService.CheckCardActivedOrCreate(c, user.CompanyIndex);
                _IIC_UserMasterLogic.CheckExistedOrCreate(us);
                await _HR_StudentClassInfoService.InsertAsync(sim);
                await SaveChangeAsync();
                CommitTransaction();

                var employee = await _HR_StudentInfoService.GetStudentInfo(u.EmployeeATID, user.CompanyIndex);
                return ApiOk(employee);
            }
            catch (Exception ex)
            {
                RollbackTransaction();
                return ApiError(ex.Message);
            }

        }

        [Authorize]
        [ActionName("Put_HR_StudentInfo")]
        [HttpPut("{employeeATID}")]
        public async Task<ActionResult<HR_EmployeeInfoResult>> Put_HR_StudentInfo(string employeeATID, [FromBody] HR_StudentInfoResult value)
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();
            var cardActive = await _HR_CardNumberInfoService.CheckCardActiveOtherEmployee(new HR_CardNumberInfo { CardNumber = value.CardNumber, CompanyIndex = user.CompanyIndex, EmployeeATID = value.EmployeeATID }, user.CompanyIndex);
            if (cardActive)
            {
                return ApiError("CardNumberExist");
            }
            var u = await _HR_UserService.FirstOrDefaultAsync(x => x.EmployeeATID == employeeATID && x.CompanyIndex == user.CompanyIndex, true);
            var e = await _HR_StudentInfoService.FirstOrDefaultAsync(x => x.EmployeeATID == employeeATID && x.CompanyIndex == user.CompanyIndex, true);
            var us = new IC_UserMasterDTO();
            var sim = await _HR_StudentClassInfoService.FirstOrDefaultAsync(x => x.EmployeeATID == employeeATID && x.ClassInfoIndex == value.ClassID,true);
            var c = await _HR_CardNumberInfoService.FirstOrDefaultAsync(x => x.EmployeeATID == employeeATID && x.CompanyIndex == user.CompanyIndex && x.IsActive == true,true);
            value.CompanyIndex = user.CompanyIndex;

            BeginTransaction();
            try
            {
                u = _Mapper.Map(value, u);
                e = _Mapper.Map(value, e);
                us = _Mapper.Map(value, us);
                c = _Mapper.Map(value, c);

                _HR_UserService.Update(u);
                _HR_StudentInfoService.Update(e);
                if(c != null)
                {
                    await _HR_CardNumberInfoService.CheckCardActivedOrCreate(c, u.CompanyIndex);
                }

                await _IIC_UserMasterLogic.SaveAndOverwriteList(new List<IC_UserMasterDTO>() { us });
                
                await SaveChangeAsync();
                CommitTransaction();

                var employee = await _HR_StudentInfoService.GetStudentInfo(u.EmployeeATID, user.CompanyIndex);
                return ApiOk(employee);
            }
            catch (Exception ex)
            {
                RollbackTransaction();
                return ApiError(ex.Message);
            }
        }

        [Authorize]
        [ActionName("Delete_HR_StudentInfo")]
        [HttpDelete("{employeeATID}")]
        public async Task<IActionResult> Delete_HR_StudentInfo(string employeeATID)
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            BeginTransaction();
            try
            {
                await _HR_StudentInfoService.DeleteAsync(x => x.EmployeeATID == employeeATID && x.CompanyIndex == user.CompanyIndex);
                await _HR_UserService.DeleteAsync(x => x.EmployeeATID == employeeATID && x.CompanyIndex == user.CompanyIndex);
                await _iC_UserMasterService.DeleteAsync(x => x.EmployeeATID == employeeATID && x.CompanyIndex == user.CompanyIndex);
                await _HR_CardNumberInfoService.DeleteAsync(x => x.EmployeeATID == employeeATID && x.CompanyIndex == user.CompanyIndex);
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

        [Authorize]
        [ActionName("DeleteStudentMulti")]
        [HttpDelete]
        public async Task<IActionResult> DeleteStudentMulti([FromBody] string[] listEmployee)
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();
            var empLookup = listEmployee.ToHashSet();

            BeginTransaction();
            try
            {
                await _HR_StudentInfoService.DeleteAsync(x => empLookup.Contains(x.EmployeeATID) && x.CompanyIndex == user.CompanyIndex);
                await _HR_UserService.DeleteAsync(x => empLookup.Contains(x.EmployeeATID) && x.CompanyIndex == user.CompanyIndex);
                await _iC_UserMasterService.DeleteAsync(x => empLookup.Contains(x.EmployeeATID) && x.CompanyIndex == user.CompanyIndex);
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

        [Authorize]
        [ActionName("AddStudentFromExcel")]
        [HttpPost]
        public async Task<IActionResult> AddStudentFromExcel([FromBody] List<IC_StudentImportDTO> param)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            ConfigObject config = ConfigObject.GetConfig(cache);
            IActionResult result = Unauthorized();
            int departmentIndex = 0;
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }
            // validation data
            List<IC_StudentImportDTO> listError = new List<IC_StudentImportDTO>();

            listError = await _HR_StudentInfoService.ValidationImportStudent(param);
            var message = "";
            if (_configClientName == ClientName.VSTAR.ToString())
            {
                string sWebRootFolder = _hostingEnvironment.ContentRootPath;
                string URL = string.Format("{0}://{1}/{2}", Request.Scheme, Request.Host, @"Files/StudentImportError.xlsx");
                FileInfo file = new FileInfo(Path.Combine(sWebRootFolder, @"Files/StudentImportError.xlsx"));
                if (listError != null && listError.Count() > 0)
                {
                    var listEmployeeIDError = listError.Select(e => e.EmployeeATID).ToList();
                    param = param.Where(e => !listEmployeeIDError.Contains(e.EmployeeATID)).ToList();
                    message = listError.Count().ToString();

                    using (var workbook = new XLWorkbook())
                    {
                        var worksheet = workbook.Worksheets.Add("StudentError");
                        var currentRow = 1;
                        worksheet.Cell(currentRow, 1).Value = "Mã chấm công (*)";
                        worksheet.Cell(currentRow, 2).Value = "Mã nhân viên";
                        worksheet.Cell(currentRow, 3).Value = "Họ tên";
                        worksheet.Cell(currentRow, 4).Value = "Mã thẻ";
                        worksheet.Cell(currentRow, 5).Value = "Mật khẩu";
                        worksheet.Cell(currentRow, 6).Value = "Tên trên máy";
                        worksheet.Cell(currentRow, 7).Value = "Giới tính (Nam)";
                        worksheet.Cell(currentRow, 8).Value = "Tên Lớp";
                        worksheet.Cell(currentRow, 9).Value = "Tên Khối";
                        worksheet.Cell(currentRow, 10).Value = "Lỗi";

                        for (int i = 1; i < 11; i++)
                        {
                            worksheet.Cell(1, i).Style.Fill.BackgroundColor = XLColor.Yellow;
                            worksheet.Cell(1, i).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                            worksheet.Column(i).Width = 20;
                        }

                        foreach (var users in listError)
                        {
                            currentRow++;
                            worksheet.Cell(currentRow, 1).Value = users.EmployeeATID;
                            worksheet.Cell(currentRow, 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                            worksheet.Cell(currentRow, 1).Style.NumberFormat.Format = "0".PadLeft(users.EmployeeATID.Length, '0');

                            worksheet.Cell(currentRow, 2).Value = users.EmployeeCode;
                            worksheet.Cell(currentRow, 2).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                            if (!string.IsNullOrWhiteSpace(users.EmployeeCode))
                                worksheet.Cell(currentRow, 2).Style.NumberFormat.Format = "0".PadLeft(users.EmployeeCode.Length, '0');

                            worksheet.Cell(currentRow, 3).Value = users.FullName;
                            worksheet.Cell(currentRow, 3).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 4).Value = users.CardNumber;
                            worksheet.Cell(currentRow, 4).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                            if (!string.IsNullOrWhiteSpace(users.CardNumber))
                                worksheet.Cell(currentRow, 4).Style.NumberFormat.Format = "0".PadLeft(users.CardNumber.Length, '0');

                            worksheet.Cell(currentRow, 5).Value = users.Password;
                            worksheet.Cell(currentRow, 5).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                            if (!string.IsNullOrWhiteSpace(users.Password))
                                worksheet.Cell(currentRow, 5).Style.NumberFormat.Format = "0".PadLeft(users.Password.Length, '0');

                            worksheet.Cell(currentRow, 6).Value = users.NameOnMachine;
                            worksheet.Cell(currentRow, 6).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 7).Value = users.Gender;
                            worksheet.Cell(currentRow, 7).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 8).Value = users.ClassName;
                            worksheet.Cell(currentRow, 8).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 9).Value = users.GradeName;
                            worksheet.Cell(currentRow, 9).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 10).Value = users.ErrorMessage;
                            worksheet.Cell(currentRow, 10).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        }

                        workbook.SaveAs(file.FullName);
                    }

                }


                var listClassInfo = param.GroupBy(e => e.ClassName).Select(e => e.First()).Select(e => e.ClassName).ToList();
                var listClassInfoDB = _HR_ClassInfoService.GetAll(e => listClassInfo.Contains(e.Name)).ToList();
                //List<HR_ClassInfo> listCreate = listClassInfo.Where(e => !string.IsNullOrEmpty(e.ClassCode)).Select(e => new HR_ClassInfo
                //{
                //    Index = Guid.NewGuid().ToString(),
                //    Code = e.ClassCode.Trim(),
                //    CompanyIndex = user.CompanyIndex,
                //    UpdatedDate = DateTime.Now,
                //    UpdatedUser = user.FullName,
                //    CreatedDate = DateTime.Now
                //}).ToList();

                //listCreate = await _HR_ClassInfoService.CheckExistedOrCreate(listCreate, user.CompanyIndex);

                var listEmployeeID = param.Select(e => e.EmployeeATID.PadLeft(config.MaxLenghtEmployeeATID, '0')).ToList();

                var listEmployeeDB = new List<HR_User>();
                var listStudentInfoDB = new List<HR_StudentInfo>();
                var listUserMasterDB = new List<IC_UserMaster>();
                var listCardNumberDB = new List<HR_CardNumberInfo>();
                var listStudentClassDB = new List<HR_StudentClassInfo>();


                if (listEmployeeID.Count > 5000)
                {
                    var listSplitEmployeeID = CommonUtils.SplitList(listEmployeeID, 5000);
                    foreach (var listEmployeeSplit in listSplitEmployeeID)
                    {
                        var resultEmployee = context.HR_User.Where(e => e.CompanyIndex == user.CompanyIndex && listEmployeeSplit.Contains(e.EmployeeATID) && e.EmployeeType == (int)EmployeeType.Student).ToList();
                        var resultStudentInfo = context.HR_StudentInfo.Where(e => e.CompanyIndex == user.CompanyIndex && listEmployeeSplit.Contains(e.EmployeeATID)).ToList();
                        var resultUserMaster = context.IC_UserMaster.Where(e => e.CompanyIndex == user.CompanyIndex && listEmployeeSplit.Contains(e.EmployeeATID)).ToList();
                        var resultCardNumber = context.HR_CardNumberInfo.Where(e => e.CompanyIndex == user.CompanyIndex && e.IsActive == true).ToList();
                        var resultStudentClassInfo = context.HR_StudentClassInfo.Where(e => e.CompanyIndex == user.CompanyIndex && listEmployeeSplit.Contains(e.EmployeeATID)).ToList();
                        listEmployeeDB.AddRange(resultEmployee);
                        listStudentInfoDB.AddRange(resultStudentInfo);
                        listUserMasterDB.AddRange(resultUserMaster);
                        listCardNumberDB.AddRange(resultCardNumber);
                        listStudentClassDB.AddRange(resultStudentClassInfo);
                    }
                }
                else
                {
                    listEmployeeDB = context.HR_User.Where(e => e.CompanyIndex == user.CompanyIndex && listEmployeeID.Contains(e.EmployeeATID) && e.EmployeeType == (int)EmployeeType.Student).ToList();
                    listStudentInfoDB = context.HR_StudentInfo.Where(e => e.CompanyIndex == user.CompanyIndex && listEmployeeID.Contains(e.EmployeeATID)).ToList();
                    listUserMasterDB = context.IC_UserMaster.Where(e => e.CompanyIndex == user.CompanyIndex && listEmployeeID.Contains(e.EmployeeATID)).ToList();
                    listCardNumberDB = context.HR_CardNumberInfo.Where(e => e.CompanyIndex == user.CompanyIndex && e.IsActive == true).ToList();
                    listStudentClassDB = context.HR_StudentClassInfo.Where(e => e.CompanyIndex == user.CompanyIndex && listEmployeeID.Contains(e.EmployeeATID)).ToList();
                }

                List<HR_User> listEmployee = new List<HR_User>();
                List<IC_UserMasterDTO> listUserMaster = new List<IC_UserMasterDTO>();


                foreach (IC_StudentImportDTO item in param)
                {
                    try
                    {
                        //var classInfo = new HR_ClassInfo();
                        //classInfo.Index = Guid.NewGuid().ToString();
                        //if (listCreate != null)
                        //{
                        //    classInfo = listCreate.FirstOrDefault(e => !string.IsNullOrWhiteSpace(item.ClassCode) && e.Code.ToLower() == item.ClassCode.ToLower());
                        //    if (classInfo == null)
                        //    {
                        //        classInfo.Index = Guid.NewGuid().ToString();
                        //    }
                        //}

                        var listGrade = param.GroupBy(e => e.GradeName).Select(e => e.First()).ToList();
                        List<HR_GradeInfo> listGradeCreate = listGrade.Where(e => !string.IsNullOrEmpty(e.GradeName)).Select(e => new HR_GradeInfo
                        {
                            Name = e.GradeName.Trim(),
                            CompanyIndex = user.CompanyIndex
                        }).ToList();
                        listGradeCreate = _iC_DepartmentLogic.CheckExistedOrCreateList(listGradeCreate);

                        item.EmployeeATID = item.EmployeeATID.PadLeft(config.MaxLenghtEmployeeATID, '0');

                        var existedEmployee = listEmployeeDB.FirstOrDefault(e => e.EmployeeATID == item.EmployeeATID);
                        if (existedEmployee != null)
                        {
                            existedEmployee.EmployeeCode = item.EmployeeCode;
                            existedEmployee.FullName = item.FullName;
                            existedEmployee.Gender = (short)item.Gender;
                            existedEmployee.UpdatedDate = DateTime.Now;
                            existedEmployee.UpdatedUser = user.UserName;
                            context.HR_User.Update(existedEmployee);
                        }
                        else
                        {
                            existedEmployee = new HR_User();
                            existedEmployee.CompanyIndex = user.CompanyIndex;
                            existedEmployee.EmployeeATID = item.EmployeeATID;
                            existedEmployee.EmployeeCode = item.EmployeeCode;
                            existedEmployee.FullName = item.FullName;
                            existedEmployee.Gender = (short)item.Gender;
                            existedEmployee.EmployeeType = (int)EmployeeType.Student;
                            existedEmployee.UpdatedDate = DateTime.Now;
                            existedEmployee.UpdatedUser = user.UserName;
                            existedEmployee.CreatedDate = DateTime.Now;
                            context.HR_User.Add(existedEmployee);
                        }

                        var student = listStudentInfoDB.FirstOrDefault(e => e.EmployeeATID == item.EmployeeATID);
                        if (student != null)
                        {
                        }
                        else
                        {
                            student = new HR_StudentInfo();
                            student.EmployeeATID = item.EmployeeATID;
                            student.CompanyIndex = user.CompanyIndex;
                            student.UpdatedDate = DateTime.Now;
                            student.UpdatedUser = user.UserName;
                            context.HR_StudentInfo.Add(student);
                        }

                        var classInfo = listClassInfoDB.FirstOrDefault(e => e.Name == item.ClassName);
                        if (classInfo != null)
                        {
                            var studentClass = listStudentClassDB.FirstOrDefault(e => e.EmployeeATID == item.EmployeeATID);
                            if (studentClass == null)
                            {
                                studentClass = new HR_StudentClassInfo();
                                studentClass.ClassInfoIndex = classInfo.Index;
                                studentClass.EmployeeATID = item.EmployeeATID;
                                studentClass.TeacherID = 0.ToString();
                                studentClass.NannyID = 0.ToString();
                                studentClass.CompanyIndex = user.CompanyIndex;
                                studentClass.CreatedDate = DateTime.Now;
                                studentClass.UpdatedUser = user.UserName;
                                context.HR_StudentClassInfo.Add(studentClass);
                            }
                            else
                            {
                                studentClass.ClassInfoIndex = classInfo.Index;
                                studentClass.CreatedDate = DateTime.Now;
                                studentClass.UpdatedUser = user.UserName;
                                context.HR_StudentClassInfo.Update(studentClass);
                            }
                        }

                        var existedCardNumber = listCardNumberDB.FirstOrDefault(e => e.CardNumber == item.CardNumber);
                        if (existedCardNumber != null)
                        {
                            if (existedEmployee.EmployeeATID == item.EmployeeATID) { }
                            else
                            {

                            }

                        }
                        else
                        {
                            existedCardNumber = new HR_CardNumberInfo();
                            existedCardNumber.EmployeeATID = existedEmployee.EmployeeATID;
                            existedCardNumber.CompanyIndex = existedEmployee.CompanyIndex;
                            existedCardNumber.CardNumber = item.CardNumber;
                            existedCardNumber.IsActive = true;
                            existedCardNumber.CreatedDate = DateTime.Now;
                            existedCardNumber.UpdatedDate = existedEmployee.UpdatedDate;
                            existedCardNumber.UpdatedUser = existedEmployee.UpdatedUser;
                            context.HR_CardNumberInfo.Add(existedCardNumber);
                        }


                        var existedUserMaster = listUserMasterDB.FirstOrDefault(e => e.EmployeeATID == item.EmployeeATID);
                        if (existedUserMaster == null)
                        {
                            existedUserMaster = new IC_UserMaster();
                            existedUserMaster.EmployeeATID = existedEmployee.EmployeeATID;
                            existedUserMaster.CompanyIndex = existedEmployee.CompanyIndex;
                            existedUserMaster.NameOnMachine = item.NameOnMachine;
                            existedUserMaster.CardNumber = item.CardNumber;
                            existedUserMaster.Password = item.Password;
                            existedUserMaster.AuthenMode = AuthenMode.FullAccessRight.ToString();
                            existedUserMaster.Privilege = GlobalParams.DevicePrivilege.SDKStandardRole;
                            existedUserMaster.UpdatedDate = DateTime.Now;
                            existedUserMaster.CreatedDate = DateTime.Now;
                            existedUserMaster.UpdatedUser = user.UserName;
                            context.IC_UserMaster.Add(existedUserMaster);
                        }
                        else
                        {
                            existedUserMaster.NameOnMachine = item.NameOnMachine;
                            existedUserMaster.CardNumber = item.CardNumber;
                            existedUserMaster.Password = item.Password;
                            existedUserMaster.UpdatedDate = DateTime.Now;
                            existedUserMaster.UpdatedUser = user.UserName;
                            context.IC_UserMaster.Update(existedUserMaster);
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                }
                try
                {
                    context.SaveChanges();
                }
                catch (Exception ex)
                {
                }
                // Add audit log
                IC_AuditEntryDTO audit = new IC_AuditEntryDTO(null);
                audit.TableName = "HR_Student";
                audit.UserName = user.UserName;
                audit.CompanyIndex = user.CompanyIndex;
                audit.State = AuditType.Added;
                //audit.Description = AuditType.Added.ToString() + " Import From Excel " + listEmployeeID.Count().ToString() + " Student";
                audit.Description = AuditType.Added.ToString() + "StudentFromExcel:/:" + listEmployeeID.Count().ToString();
                audit.DateTime = DateTime.Now;
                _IIC_AuditLogic.Create(audit);

                //List<AddedParam> addedParams = new List<AddedParam>();
                //addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = user.CompanyIndex });
                //addedParams.Add(new AddedParam { Key = "EventType", Value = ConfigAuto.ADD_OR_DELETE_USER });
                //var systemconfigs = _IIC_ConfigLogic.GetMany(addedParams);
                //if (systemconfigs != null)
                //{
                //    var sysconfig = systemconfigs.FirstOrDefault();
                //    if (sysconfig != null)
                //    {
                //        if (sysconfig.IntegrateLogParam.AutoIntegrate)
                //        {
                //            _IIC_CommandLogic.SyncWithEmployee(listEmployeeID, user.CompanyIndex);
                //        }
                //    }
                //}
            }
            else
            {
                string sWebRootFolder = _hostingEnvironment.ContentRootPath;
                string URL = string.Format("{0}://{1}/{2}", Request.Scheme, Request.Host, @"Files/StudentImportError.xlsx");
                FileInfo file = new FileInfo(Path.Combine(sWebRootFolder, @"Files/StudentImportError.xlsx"));
                if (listError != null && listError.Count() > 0)
                {
                    var listEmployeeIDError = listError.Select(e => e.EmployeeATID).ToList();
                    param = param.Where(e => !listEmployeeIDError.Contains(e.EmployeeATID)).ToList();
                    message = listError.Count().ToString();

                    using (var workbook = new XLWorkbook())
                    {
                        var worksheet = workbook.Worksheets.Add("StudentError");
                        var currentRow = 1;
                        worksheet.Cell(currentRow, 1).Value = "Mã chấm công (*)";
                        worksheet.Cell(currentRow, 2).Value = "Mã nhân viên";
                        worksheet.Cell(currentRow, 3).Value = "Họ tên";
                        worksheet.Cell(currentRow, 4).Value = "Mã thẻ";
                        worksheet.Cell(currentRow, 5).Value = "Mật khẩu";
                        worksheet.Cell(currentRow, 6).Value = "Tên trên máy";
                        worksheet.Cell(currentRow, 7).Value = "Giới tính (Nam)";
                        worksheet.Cell(currentRow, 8).Value = "Tên Lớp";
                        worksheet.Cell(currentRow, 9).Value = "Lỗi";

                        for (int i = 1; i < 10; i++)
                        {
                            worksheet.Cell(1, i).Style.Fill.BackgroundColor = XLColor.Yellow;
                            worksheet.Cell(1, i).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                            worksheet.Column(i).Width = 20;
                        }

                        foreach (var users in listError)
                        {
                            currentRow++;
                            worksheet.Cell(currentRow, 1).Value = users.EmployeeATID;
                            worksheet.Cell(currentRow, 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                            worksheet.Cell(currentRow, 1).Style.NumberFormat.Format = "0".PadLeft(users.EmployeeATID.Length, '0');

                            worksheet.Cell(currentRow, 2).Value = users.EmployeeCode;
                            worksheet.Cell(currentRow, 2).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                            if (!string.IsNullOrWhiteSpace(users.EmployeeCode))
                                worksheet.Cell(currentRow, 2).Style.NumberFormat.Format = "0".PadLeft(users.EmployeeCode.Length, '0');

                            worksheet.Cell(currentRow, 3).Value = users.FullName;
                            worksheet.Cell(currentRow, 3).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 4).Value = users.CardNumber;
                            worksheet.Cell(currentRow, 4).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                            if (!string.IsNullOrWhiteSpace(users.CardNumber))
                                worksheet.Cell(currentRow, 4).Style.NumberFormat.Format = "0".PadLeft(users.CardNumber.Length, '0');

                            worksheet.Cell(currentRow, 5).Value = users.Password;
                            worksheet.Cell(currentRow, 5).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                            if (!string.IsNullOrWhiteSpace(users.Password))
                                worksheet.Cell(currentRow, 5).Style.NumberFormat.Format = "0".PadLeft(users.Password.Length, '0');

                            worksheet.Cell(currentRow, 6).Value = users.NameOnMachine;
                            worksheet.Cell(currentRow, 6).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 7).Value = users.Gender;
                            worksheet.Cell(currentRow, 7).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 8).Value = users.ClassName;
                            worksheet.Cell(currentRow, 8).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 9).Value = users.ErrorMessage;
                            worksheet.Cell(currentRow, 9).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        }

                        workbook.SaveAs(file.FullName);
                    }

                }


                var listClassInfo = param.GroupBy(e => e.ClassName).Select(e => e.First()).Select(e => e.ClassName).ToList();
                var listClassInfoDB = _HR_ClassInfoService.GetAll(e => listClassInfo.Contains(e.Name)).ToList();
                //List<HR_ClassInfo> listCreate = listClassInfo.Where(e => !string.IsNullOrEmpty(e.ClassCode)).Select(e => new HR_ClassInfo
                //{
                //    Index = Guid.NewGuid().ToString(),
                //    Code = e.ClassCode.Trim(),
                //    CompanyIndex = user.CompanyIndex,
                //    UpdatedDate = DateTime.Now,
                //    UpdatedUser = user.FullName,
                //    CreatedDate = DateTime.Now
                //}).ToList();

                //listCreate = await _HR_ClassInfoService.CheckExistedOrCreate(listCreate, user.CompanyIndex);

                var listEmployeeID = param.Select(e => e.EmployeeATID.PadLeft(config.MaxLenghtEmployeeATID, '0')).ToList();

                var listEmployeeDB = new List<HR_User>();
                var listStudentInfoDB = new List<HR_StudentInfo>();
                var listUserMasterDB = new List<IC_UserMaster>();
                var listCardNumberDB = new List<HR_CardNumberInfo>();
                var listStudentClassDB = new List<HR_StudentClassInfo>();


                if (listEmployeeID.Count > 5000)
                {
                    var listSplitEmployeeID = CommonUtils.SplitList(listEmployeeID, 5000);
                    foreach (var listEmployeeSplit in listSplitEmployeeID)
                    {
                        var resultEmployee = context.HR_User.Where(e => e.CompanyIndex == user.CompanyIndex && listEmployeeSplit.Contains(e.EmployeeATID) && e.EmployeeType == (int)EmployeeType.Student).ToList();
                        var resultStudentInfo = context.HR_StudentInfo.Where(e => e.CompanyIndex == user.CompanyIndex && listEmployeeSplit.Contains(e.EmployeeATID)).ToList();
                        var resultUserMaster = context.IC_UserMaster.Where(e => e.CompanyIndex == user.CompanyIndex && listEmployeeSplit.Contains(e.EmployeeATID)).ToList();
                        var resultCardNumber = context.HR_CardNumberInfo.Where(e => e.CompanyIndex == user.CompanyIndex && e.IsActive == true).ToList();
                        var resultStudentClassInfo = context.HR_StudentClassInfo.Where(e => e.CompanyIndex == user.CompanyIndex && listEmployeeSplit.Contains(e.EmployeeATID)).ToList();
                        listEmployeeDB.AddRange(resultEmployee);
                        listStudentInfoDB.AddRange(resultStudentInfo);
                        listUserMasterDB.AddRange(resultUserMaster);
                        listCardNumberDB.AddRange(resultCardNumber);
                        listStudentClassDB.AddRange(resultStudentClassInfo);
                    }
                }
                else
                {
                    listEmployeeDB = context.HR_User.Where(e => e.CompanyIndex == user.CompanyIndex && listEmployeeID.Contains(e.EmployeeATID) && e.EmployeeType == (int)EmployeeType.Student).ToList();
                    listStudentInfoDB = context.HR_StudentInfo.Where(e => e.CompanyIndex == user.CompanyIndex && listEmployeeID.Contains(e.EmployeeATID)).ToList();
                    listUserMasterDB = context.IC_UserMaster.Where(e => e.CompanyIndex == user.CompanyIndex && listEmployeeID.Contains(e.EmployeeATID)).ToList();
                    listCardNumberDB = context.HR_CardNumberInfo.Where(e => e.CompanyIndex == user.CompanyIndex && e.IsActive == true).ToList();
                    listStudentClassDB = context.HR_StudentClassInfo.Where(e => e.CompanyIndex == user.CompanyIndex && listEmployeeID.Contains(e.EmployeeATID)).ToList();
                }

                List<HR_User> listEmployee = new List<HR_User>();
                List<IC_UserMasterDTO> listUserMaster = new List<IC_UserMasterDTO>();


                foreach (IC_StudentImportDTO item in param)
                {
                    try
                    {
                        //var classInfo = new HR_ClassInfo();
                        //classInfo.Index = Guid.NewGuid().ToString();
                        //if (listCreate != null)
                        //{
                        //    classInfo = listCreate.FirstOrDefault(e => !string.IsNullOrWhiteSpace(item.ClassCode) && e.Code.ToLower() == item.ClassCode.ToLower());
                        //    if (classInfo == null)
                        //    {
                        //        classInfo.Index = Guid.NewGuid().ToString();
                        //    }
                        //}

                        item.EmployeeATID = item.EmployeeATID.PadLeft(config.MaxLenghtEmployeeATID, '0');

                        var existedEmployee = listEmployeeDB.FirstOrDefault(e => e.EmployeeATID == item.EmployeeATID);
                        if (existedEmployee != null)
                        {
                            existedEmployee.EmployeeCode = item.EmployeeCode;
                            existedEmployee.FullName = item.FullName;
                            existedEmployee.Gender = (short)item.Gender;
                            existedEmployee.UpdatedDate = DateTime.Now;
                            existedEmployee.UpdatedUser = user.UserName;
                            context.HR_User.Update(existedEmployee);
                        }
                        else
                        {
                            existedEmployee = new HR_User();
                            existedEmployee.CompanyIndex = user.CompanyIndex;
                            existedEmployee.EmployeeATID = item.EmployeeATID;
                            existedEmployee.EmployeeCode = item.EmployeeCode;
                            existedEmployee.FullName = item.FullName;
                            existedEmployee.Gender = (short)item.Gender;
                            existedEmployee.EmployeeType = (int)EmployeeType.Student;
                            existedEmployee.UpdatedDate = DateTime.Now;
                            existedEmployee.UpdatedUser = user.UserName;
                            existedEmployee.CreatedDate = DateTime.Now;
                            context.HR_User.Add(existedEmployee);
                        }

                        var student = listStudentInfoDB.FirstOrDefault(e => e.EmployeeATID == item.EmployeeATID);
                        if (student != null)
                        {
                        }
                        else
                        {
                            student = new HR_StudentInfo();
                            student.EmployeeATID = item.EmployeeATID;
                            student.CompanyIndex = user.CompanyIndex;
                            student.UpdatedDate = DateTime.Now;
                            student.UpdatedUser = user.UserName;
                            context.HR_StudentInfo.Add(student);
                        }

                        var classInfo = listClassInfoDB.FirstOrDefault(e => e.Name == item.ClassName);
                        if (classInfo != null)
                        {
                            var studentClass = listStudentClassDB.FirstOrDefault(e => e.EmployeeATID == item.EmployeeATID);
                            if (studentClass == null)
                            {
                                studentClass = new HR_StudentClassInfo();
                                studentClass.ClassInfoIndex = classInfo.Index;
                                studentClass.EmployeeATID = item.EmployeeATID;
                                studentClass.TeacherID = 0.ToString();
                                studentClass.NannyID = 0.ToString();
                                studentClass.CompanyIndex = user.CompanyIndex;
                                studentClass.CreatedDate = DateTime.Now;
                                studentClass.UpdatedUser = user.UserName;
                                context.HR_StudentClassInfo.Add(studentClass);
                            }
                            else
                            {
                                studentClass.ClassInfoIndex = classInfo.Index;
                                studentClass.CreatedDate = DateTime.Now;
                                studentClass.UpdatedUser = user.UserName;
                                context.HR_StudentClassInfo.Update(studentClass);
                            }
                        }

                        var existedCardNumber = listCardNumberDB.FirstOrDefault(e => e.CardNumber == item.CardNumber);
                        if (existedCardNumber != null)
                        {
                            if (existedEmployee.EmployeeATID == item.EmployeeATID) { }
                            else
                            {

                            }

                        }
                        else
                        {
                            existedCardNumber = new HR_CardNumberInfo();
                            existedCardNumber.EmployeeATID = existedEmployee.EmployeeATID;
                            existedCardNumber.CompanyIndex = existedEmployee.CompanyIndex;
                            existedCardNumber.CardNumber = item.CardNumber;
                            existedCardNumber.IsActive = true;
                            existedCardNumber.CreatedDate = DateTime.Now;
                            existedCardNumber.UpdatedDate = existedEmployee.UpdatedDate;
                            existedCardNumber.UpdatedUser = existedEmployee.UpdatedUser;
                            context.HR_CardNumberInfo.Add(existedCardNumber);
                        }


                        var existedUserMaster = listUserMasterDB.FirstOrDefault(e => e.EmployeeATID == item.EmployeeATID);
                        if (existedUserMaster == null)
                        {
                            existedUserMaster = new IC_UserMaster();
                            existedUserMaster.EmployeeATID = existedEmployee.EmployeeATID;
                            existedUserMaster.CompanyIndex = existedEmployee.CompanyIndex;
                            existedUserMaster.NameOnMachine = item.NameOnMachine;
                            existedUserMaster.CardNumber = item.CardNumber;
                            existedUserMaster.Password = item.Password;
                            existedUserMaster.AuthenMode = AuthenMode.FullAccessRight.ToString();
                            existedUserMaster.Privilege = GlobalParams.DevicePrivilege.SDKStandardRole;
                            existedUserMaster.UpdatedDate = DateTime.Now;
                            existedUserMaster.CreatedDate = DateTime.Now;
                            existedUserMaster.UpdatedUser = user.UserName;
                            context.IC_UserMaster.Add(existedUserMaster);
                        }
                        else
                        {
                            existedUserMaster.NameOnMachine = item.NameOnMachine;
                            existedUserMaster.CardNumber = item.CardNumber;
                            existedUserMaster.Password = item.Password;
                            existedUserMaster.UpdatedDate = DateTime.Now;
                            existedUserMaster.UpdatedUser = user.UserName;
                            context.IC_UserMaster.Update(existedUserMaster);
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                }
                try
                {
                    context.SaveChanges();
                }
                catch (Exception ex)
                {
                }
                // Add audit log
                IC_AuditEntryDTO audit = new IC_AuditEntryDTO(null);
                audit.TableName = "HR_Student";
                audit.UserName = user.UserName;
                audit.CompanyIndex = user.CompanyIndex;
                audit.State = AuditType.Added;
                //audit.Description = AuditType.Added.ToString() + " Import From Excel " + listEmployeeID.Count().ToString() + " Student";
                audit.Description = AuditType.Added.ToString() + "StudentFromExcel:/:" + listEmployeeID.Count().ToString();
                audit.DateTime = DateTime.Now;
                _IIC_AuditLogic.Create(audit);

                //List<AddedParam> addedParams = new List<AddedParam>();
                //addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = user.CompanyIndex });
                //addedParams.Add(new AddedParam { Key = "EventType", Value = ConfigAuto.ADD_OR_DELETE_USER });
                //var systemconfigs = _IIC_ConfigLogic.GetMany(addedParams);
                //if (systemconfigs != null)
                //{
                //    var sysconfig = systemconfigs.FirstOrDefault();
                //    if (sysconfig != null)
                //    {
                //        if (sysconfig.IntegrateLogParam.AutoIntegrate)
                //        {
                //            _IIC_CommandLogic.SyncWithEmployee(listEmployeeID, user.CompanyIndex);
                //        }
                //    }
                //}
            }

            result = Ok(message);
            return result;
        }

        [Authorize]
        [ActionName("DeleteStudentFromExcel")]
        [HttpPost]
        public IActionResult DeleteEmployeeFromExcel([FromBody] List<AddedParam> addedParams)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }
            var paramEmployee = addedParams.FirstOrDefault(e => e.Key == "ListEmployeeATID");
            var paramDeleteOndevice = addedParams.FirstOrDefault(e => e.Key == "IsDeleteOnDevice");
            var lsemployeeATID = new List<string>();
            var isDeleteOnDevice = false;
            if (paramEmployee != null)
            {
                lsemployeeATID = JsonConvert.DeserializeObject<List<string>>(paramEmployee.Value.ToString());
            }
            else
            {
                return BadRequest("");
            }
            var listPadleft = new List<string>();
            foreach (var item in lsemployeeATID)
            {
                listPadleft.Add(item.PadLeft(_config.MaxLenghtEmployeeATID, '0'));
            }
            var existedEmployee = context.HR_User.Where(e => e.CompanyIndex == user.CompanyIndex && listPadleft.Contains(e.EmployeeATID)).ToList();

            if (paramDeleteOndevice != null)
            {
                isDeleteOnDevice = Convert.ToBoolean(paramDeleteOndevice.Value);
                if (isDeleteOnDevice)
                {
                    var lsSerialHw = context.IC_Device.Where(e => e.CompanyIndex == user.CompanyIndex).Select(e => e.SerialNumber).ToList();

                    if (existedEmployee != null)
                    {
                        listPadleft = existedEmployee.Select(e => e.EmployeeATID).ToList();

                        IC_UserinfoOnMachineParam paramUserOnMachine = new IC_UserinfoOnMachineParam();
                        paramUserOnMachine.ListEmployeeaATID = listPadleft;
                        paramUserOnMachine.CompanyIndex = user.CompanyIndex;
                        paramUserOnMachine.AuthenMode = "";
                        paramUserOnMachine.FullInfo = false;
                        List<UserInfoOnMachine> lstUser = _IIC_CommandLogic.GetListUserInfoOnMachine(paramUserOnMachine);

                        IC_CommandParamDTO commandParam = new IC_CommandParamDTO();
                        commandParam.IsOverwriteData = false;
                        commandParam.Action = CommandAction.DeleteUserById;
                        commandParam.AuthenMode = "";
                        commandParam.FromTime = DateTime.Now;
                        commandParam.ToTime = DateTime.Now;
                        commandParam.ListEmployee = lstUser;
                        commandParam.ListSerialNumber = lsSerialHw;
                        commandParam.Privilege = GlobalParams.DevicePrivilege.SDKStandardRole;
                        commandParam.ExternalData = "";
                        List<CommandResult> lstCmd = _IIC_CommandLogic.CreateListCommands(commandParam);
                        if (lstCmd != null && lstCmd.Count() > 0)
                        {
                            IC_GroupCommandParamDTO groupCommand = new IC_GroupCommandParamDTO();
                            groupCommand.CompanyIndex = user.CompanyIndex;
                            groupCommand.UserName = user.UserName;
                            groupCommand.ListCommand = lstCmd;
                            groupCommand.GroupName = GroupName.DeleteUserById.ToString();
                            groupCommand.EventType = "";
                            _IIC_CommandLogic.CreateGroupCommands(groupCommand);
                        }
                    }
                }
            }

            var listEmp = context.HR_User.Where(t => t.CompanyIndex == user.CompanyIndex && listPadleft.Contains(t.EmployeeATID)).ToArray();
            var listStudentInfo = context.HR_StudentInfo.Where(x => x.CompanyIndex == user.CompanyIndex && listPadleft.Contains(x.EmployeeATID)).ToArray();
            var listStudentClassInfo = context.HR_StudentClassInfo.Where(e => e.CompanyIndex == user.CompanyIndex && listPadleft.Contains(e.EmployeeATID)).ToArray();
            var listUserMaster = context.IC_UserMaster.Where(x => x.CompanyIndex == user.CompanyIndex && listPadleft.Contains(x.EmployeeATID)).ToArray();
            try
            {
                context.HR_User.RemoveRange(listEmp);
                context.HR_StudentInfo.RemoveRange(listStudentInfo);
                context.HR_StudentClassInfo.RemoveRange(listStudentClassInfo);
                context.IC_UserMaster.RemoveRange(listUserMaster);
                context.SaveChanges();
                // Add audit log
                IC_AuditEntryDTO audit = new IC_AuditEntryDTO(null);
                audit.TableName = "IC_StudentInfo";
                audit.UserName = user.UserName;
                audit.CompanyIndex = user.CompanyIndex;
                audit.State = AuditType.Deleted;
                //audit.Description = AuditType.Deleted.ToString() + " From Excel " + listEmp.Count().ToString() + " Employee";
                audit.Description = AuditType.Deleted.ToString() + "StudentFromExcel:/:" + listEmp.Count().ToString();
                audit.DateTime = DateTime.Now;
                _IIC_AuditLogic.Create(audit);
                result = Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            result = Ok();
            return result;
        }

        [Authorize]
        [ActionName("ExportToExcel")]
        [HttpPost]
        public async Task<IActionResult> ExportToExcel([FromBody] List<AddedParam> addedParams)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                //return new byte[0];
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }
            ConfigObject config = ConfigObject.GetConfig(cache);

            var addedParamTrans = new List<AddedParam>();

            addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = user.CompanyIndex });
            addedParams.Add(new AddedParam { Key = "ListDepartment", Value = user.ListDepartmentAssigned });
            addedParams.Add(new AddedParam { Key = "TransferStatus", Value = TransferStatus.Approve });
            addedParams.Add(new AddedParam { Key = "IsCurrentWorkingAndNoDepartment", Value = true });

            if (config.IntegrateDBOther == false)
            {
                List<IC_EmployeeDTO> employees = null;
                //employees = context.IC_Employee.Where(t => t.CompanyIndex == user.CompanyIndex).OrderBy(t => t.EmployeeATID).ToList();
                //employees = _IIC_EmployeeLogic.GetManyExport(addedParams);

                var obj = employees.Select(e => new
                {
                    EmployeeATID = e.EmployeeATID.PadLeft(_config.MaxLenghtEmployeeATID, '0'),
                    EmployeeCode = e.EmployeeCode,
                    FullName = e.FullName,
                    CardNumber = e.CardNumber,
                    NameOnMachine = e.NameOnMachine,
                    _Gender = e.Gender.HasValue ? e.Gender.Value == 1 ? "x" : "" : "",
                    _DepartmentName = e.DepartmentName
                }).ToList();

                string sWebRootFolder = _hostingEnvironment.ContentRootPath;
                string URL = string.Format("{0}://{1}/{2}", Request.Scheme, Request.Host, @"Files/Student.xlsx");
                FileInfo file = new FileInfo(Path.Combine(sWebRootFolder, @"Files/Student.xlsx"));

                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Employees");
                    var currentRow = 1;
                    worksheet.Cell(currentRow, 1).Value = "Mã chấm công (*)";
                    worksheet.Cell(currentRow, 2).Value = "Mã học sinh";
                    worksheet.Cell(currentRow, 3).Value = "Họ tên";
                    worksheet.Cell(currentRow, 4).Value = "Mã thẻ";
                    worksheet.Cell(currentRow, 5).Value = "Tên trên máy";
                    worksheet.Cell(currentRow, 6).Value = "Giới tính (Nam)";
                    worksheet.Cell(currentRow, 7).Value = "Lớp";

                    for (int i = 1; i < 8; i++)
                    {
                        worksheet.Cell(1, i).Style.Fill.BackgroundColor = XLColor.Yellow;
                        worksheet.Cell(1, i).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        worksheet.Column(i).Width = 20;
                    }

                    foreach (var users in obj)
                    {
                        currentRow++;
                        worksheet.Cell(currentRow, 1).Value = users.EmployeeATID;
                        worksheet.Cell(currentRow, 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        worksheet.Cell(currentRow, 1).Style.NumberFormat.Format = "0".PadLeft(users.EmployeeATID.Length, '0');

                        worksheet.Cell(currentRow, 2).Value = users.EmployeeCode;
                        worksheet.Cell(currentRow, 2).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        if (!string.IsNullOrWhiteSpace(users.EmployeeCode))
                            worksheet.Cell(currentRow, 2).Style.NumberFormat.Format = "0".PadLeft(users.EmployeeCode.Length, '0');

                        worksheet.Cell(currentRow, 3).Value = users.FullName;
                        worksheet.Cell(currentRow, 3).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                        worksheet.Cell(currentRow, 4).Value = users.CardNumber;
                        worksheet.Cell(currentRow, 4).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        if (!string.IsNullOrWhiteSpace(users.CardNumber))
                            worksheet.Cell(currentRow, 4).Style.NumberFormat.Format = "0".PadLeft(users.CardNumber.Length, '0');

                        worksheet.Cell(currentRow, 5).Value = users.NameOnMachine;
                        worksheet.Cell(currentRow, 5).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                        worksheet.Cell(currentRow, 6).Value = users._Gender;
                        worksheet.Cell(currentRow, 6).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                        worksheet.Cell(currentRow, 7).Value = users._DepartmentName;
                        worksheet.Cell(currentRow, 7).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    }

                    //var workbookBytes = new byte[0];
                    //using (var ms = new MemoryStream())
                    //{
                    //    workbook.SaveAs(ms);
                    //    return workbookBytes = ms.ToArray();
                    //}
                    workbook.SaveAs(file.FullName);
                    return Ok(URL);
                }
            }
            //return new byte[0];
            return NotFound("TemplateError");
        }

    }
}
