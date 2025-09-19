using EPAD_Backend_Core.Base;
using EPAD_Backend_Core.WebUtilitys;
using EPAD_Common;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using EPAD_Logic.SendMail;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EPAD_Backend_Core.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("api/Integrate/[action]")]
    [ApiController]
    public class IC_IntegrateController : ApiControllerBase
    {
        private EPAD_Context context;
        private IMemoryCache cache;
        private IEmailProvider emailProvider;

        public IC_IntegrateController(IServiceProvider provider) : base(provider)
        {
            context = TryResolve<EPAD_Context>();
            cache = TryResolve<IMemoryCache>();
            emailProvider = TryResolve<IEmailProvider>();
        }

        [Authorize]
        [ActionName("UpdateEmployeeIntegrate")]
        [HttpPost]
        public IActionResult UpdateEmployeeIntegrate([FromBody]List<EmployeeIntegrate> listParam)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }
            if (user.Index == 0)
            {
                return BadRequest("NotPrivilege");
            }
            List<string> listEmpATID = new List<string>();
            for (int i = 0; i < listParam.Count; i++)
            {
                if (listEmpATID.Contains(listParam[i].EmployeeATID) == false)
                {
                    listEmpATID.Add(listParam[i].EmployeeATID);
                }
            }
            List<HR_User> listEmployee = context.HR_User.Where(t => t.CompanyIndex == user.CompanyIndex && listEmpATID.Contains(t.EmployeeATID)).ToList();
            List<IC_UserInfo> listUserInfo = context.IC_UserInfo.Where(t => t.CompanyIndex == user.CompanyIndex && listEmpATID.Contains(t.EmployeeATID)).ToList();
            List<IC_Department> listDepartment = context.IC_Department.Where(t => t.CompanyIndex == user.CompanyIndex && t.IsInactive != true).ToList();
            DateTime now = DateTime.Now;
            string error = "";
            int errorIndex = 0;

            EmployeeIntegrate_Log log = new EmployeeIntegrate_Log();
            log.ServiceId = user.Index;
            log.IntegrateTime = now;
            log.Success = true;
            log.ListEmployee = listParam;

            using (IDbContextTransaction transaction = context.Database.BeginTransaction())
            {
                try
                {
                    for (int i = 0; i < listParam.Count; i++)
                    {
                        errorIndex = listParam[i].Index;
                        HR_User employee = listEmployee.Find(t => t.EmployeeATID == listParam[i].EmployeeATID);
                        if (employee == null)
                        {
                            employee = new HR_User();

                            employee.EmployeeATID = listParam[i].EmployeeATID;
                            employee.CompanyIndex = user.CompanyIndex;
                            employee.CreatedDate = now;
                            context.HR_User.Add(employee);
                        }
                        employee.EmployeeCode = listParam[i].EmployeeCode;
                        employee.FullName = listParam[i].FullName;

                        //employee.DepartmentIndex = GetDepartmentIndexFromString(listParam[i].Department, listDepartment, user.CompanyIndex, now, context, transaction, ref error);
                        if (error != "")
                        {
                            break;
                        }
                        double dataCheck = 0;
                        if (double.TryParse(listParam[i].EmployeeATID, out dataCheck) == false)
                        {
                            error = "EmployeeATIDInvalid";
                            break;
                        }
                        
                        employee.UpdatedDate = now;
                        employee.UpdatedUser = UpdatedUser.AutoIntegrateEmployee.ToString();
                        if (listParam[i].CardNumber.Trim() != "")
                        {
                            if (double.TryParse(listParam[i].CardNumber, out dataCheck) == false)
                            {
                                error = "CardNumberInvalid";
                                break;
                            }

                           // employee.CardNumber = listParam[i].CardNumber;
                            List<IC_UserInfo> listUserByEmpATID = listUserInfo.Where(t => t.EmployeeATID == listParam[i].EmployeeATID).ToList();
                            if (listUserByEmpATID.Count == 0)
                            {
                                IC_UserInfo userInfo = CreateNewUserInfo(listParam[i].EmployeeATID, user.CompanyIndex, listParam[i].CardNumber, now);
                                context.IC_UserInfo.Add(userInfo);
                                listUserInfo.Add(userInfo);
                            }
                            else
                            {
                                for (int j = 0; j < listUserByEmpATID.Count; j++)
                                {
                                    listUserByEmpATID[j].CardNumber = listParam[i].CardNumber;
                                    listUserByEmpATID[j].UpdatedDate = now;
                                }
                            }
                        }

                        if (listParam[i].Status.ToUpper() == "D")
                        {
                            //employee.StoppedDate = now.Date;
                            //if (listParam[i].StoppedDate != null)
                            //{
                            //    employee.StoppedDate = listParam[i].StoppedDate;
                            //}
                        }
                    }
                }
                catch (Exception ex)
                {
                    error = ex.ToString();
                }
                if (error != "")
                {
                    log.Success = false;
                    log.Error = error;
                    log.RowErrorIndex = errorIndex;

                    transaction.Rollback();

                    result = BadRequest(error);
                    emailProvider.SendMailIntegrateEmployeeDone(0, listParam.Count, DateTime.Now);
                }
                else
                {
                    context.SaveChanges();
                    transaction.Commit();
                    result = Ok();
                    emailProvider.SendMailIntegrateEmployeeDone(listParam.Count, 0, DateTime.Now);
                }
                //ghi log 
                MongoDBHelper<EmployeeIntegrate_Log> mongoObject = new MongoDBHelper<EmployeeIntegrate_Log>("employee_integrate",cache);
                mongoObject.AddDataToCollection(log, true);
            }


            return result;

        }

        private IC_UserInfo CreateNewUserInfo(string pEmpATID,int pCompanyIndex,string pCardNumber, DateTime now)
        {
            IC_UserInfo userInfo = new IC_UserInfo();
            userInfo.EmployeeATID = pEmpATID;
            userInfo.CompanyIndex = pCompanyIndex;
            userInfo.SerialNumber = "";
            userInfo.UserName = "";
            userInfo.CardNumber = pCardNumber;
            userInfo.Privilege = 0;
            userInfo.Password = "";
            userInfo.Reserve1 = "";
            userInfo.Reserve2 = 0;
            userInfo.CreatedDate = now;
            userInfo.UpdatedDate = now;
            userInfo.UpdatedUser = UpdatedUser.IntegrateEmployee.ToString();

            return userInfo;
        }

        private int GetDepartmentIndexFromString(string pDepartment, List<IC_Department> pListDepartment,int pCompanyIndex,DateTime now, 
            EPAD_Context context, IDbContextTransaction transaction,
            ref string error)
        {
            string[] arrDep = pDepartment.Split('/');
            int parentIndex = 0;
            for (int i = 0; i < arrDep.Length; i++)
            {
                if (arrDep[i].Trim() == "")
                {
                    error = "DepartmentError";
                    break;
                }
                IC_Department department = pListDepartment.Where(t => t.Name == arrDep[i]).FirstOrDefault();
                if (department == null)
                {
                    department = new IC_Department();
                    department.Name = arrDep[i];
                    department.Location = "";
                    department.Description = "";

                    department.ParentIndex = parentIndex;
                    department.CompanyIndex = pCompanyIndex;
                    department.CreatedDate = now;

                    department.UpdatedDate = now;
                    department.UpdatedUser = UpdatedUser.IntegrateEmployee.ToString();

                    context.IC_Department.Add(department);
                    context.SaveChanges();
                    
                }
                parentIndex = department.Index;
            }
            return parentIndex;
        }
    }
}
