using EPAD_Common.Types;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EPAD_Logic
{
    public class IC_EmployeeTransferLogic : IIC_EmployeeTransferLogic
    {
        private EPAD_Context _dbContext;
        public IC_EmployeeTransferLogic(EPAD_Context dbContext)
        {
            this._dbContext = dbContext;
        }
        public IC_EmployeeTransferDTO Get(int companyID, string employeeID)
        {

            var query = _dbContext.IC_EmployeeTransfer.AsQueryable();
            query = query.Where(u => u.CompanyIndex == companyID && u.EmployeeATID == employeeID);
            var data = query.FirstOrDefault();

            return (IC_EmployeeTransferDTO)CommonUtils.ConvertObject(data, new IC_EmployeeTransferDTO());
        }

        public ListDTOModel<IC_EmployeeTransferDTO> GetPage(List<AddedParam> addedParams)
        {

            var query = (from empt in _dbContext.IC_EmployeeTransfer
                         join emp in _dbContext.HR_User
                          on empt.EmployeeATID equals emp.EmployeeATID 

                         join dpOld in _dbContext.IC_Department
                            on empt.OldDepartment equals dpOld.Index into odTransfer
                         from odTransferResult in odTransfer.DefaultIfEmpty()

                         join dpNew in _dbContext.IC_Department
                            on empt.NewDepartment equals dpNew.Index into ndTransfer
                         from ndTransferResult in ndTransfer.DefaultIfEmpty()

                         select new IC_EmployeeTransferDTO
                         {
                             CompanyIndex = empt.CompanyIndex,
                             EmployeeATID = empt.EmployeeATID,
                             FullName = emp.FullName,
                             IsFromTime = empt.FromTime.ToString("dd/MM/yyyy"),
                             IsToTime = empt.ToTime.ToString("dd/MM/yyyy"),
                             FromTime = empt.FromTime,
                             ToTime = empt.ToTime,
                             Description = empt.Description,
                             NewDepartment = empt.NewDepartment,
                             NewDepartmentName = ndTransferResult.Name,
                             OldDepartment = empt.OldDepartment,
                             OldDepartmentName = odTransferResult.Name,
                             RemoveFromOldDepartmentName = empt.RemoveFromOldDepartment == true ? "Có" : "Không",
                             RemoveFromOldDepartment = empt.RemoveFromOldDepartment,
                             AddOnNewDepartment = empt.AddOnNewDepartment,
                             AddOnNewDepartmentName = empt.AddOnNewDepartment == true ? "Có" : "Không",
                             TypeTemporaryTransfer = "Tạm thời",
                             TransferApprovedDate = empt.ApprovedDate.HasValue ? empt.ApprovedDate.Value.ToString("dd/MM/yyyy") : "",
                             TransferApprovedUser = empt.ApprovedUser,
                             Status = empt.Status,
                             TransferApproveStatus = empt.Status == 0 ? "Chờ duyệt" : empt.Status == 1 ? "Đã duyệt" : "Từ chối",
                             TemporaryTransfer = true
                         }).AsQueryable();

            int pageIndex = 1;
            int pageSize = GlobalParams.ROWS_NUMBER_IN_PAGE;
            var pageIndexParam = addedParams.FirstOrDefault(u => u.Key == "PageIndex");
            var pageSizeParam = addedParams.FirstOrDefault(u => u.Key == "PageSize");
            if (pageIndexParam != null && pageIndexParam.Value != null)
            {
                pageIndex = Convert.ToInt32(pageIndexParam.Value);
            }
            if (pageSizeParam != null && pageSizeParam.Value != null)
            {
                pageSize = Convert.ToInt32(pageSizeParam.Value);
            }
            if (addedParams != null)
            {
                foreach (AddedParam p in addedParams)
                {
                    switch (p.Key)
                    {
                        case "Filter":
                            if (p.Value != null)
                            {
                                string searchcode = p.Value.ToString();
                                query = query.Where(u => u.Description.Contains(searchcode)
                                || u.EmployeeATID.Contains(searchcode)
                                || u.FullName.Contains(searchcode)
                                || u.NewDepartmentName.Contains(searchcode)
                                || (u.AddOnNewDepartment == true ? "Có" : "Không").Contains(searchcode)
                                || (u.RemoveFromOldDepartment == true ? "Có" : "Không").Contains(searchcode));
                            }
                            break;
                        case "CompanyIndex":
                            if (p.Value != null)
                            {
                                int companyIndex = Convert.ToInt32(p.Value);
                                query = query.Where(u => u.CompanyIndex == companyIndex);
                            }
                            break;
                        case "FromDate":
                            if (p.Value != null)
                            {
                                DateTime fromDate = Convert.ToDateTime(p.Value);
                                query = query.Where(u => fromDate.Date >= u.FromTime.Value.Date);
                            }
                            break; 
                        case "ToDate":
                            if (p.Value != null)
                            {
                                DateTime toDate = Convert.ToDateTime(p.Value);
                                query = query.Where(u => toDate.Date <= u.ToTime.Value.Date);
                            }
                            break;
                        case "IsPenddingApprove":
                            if (p.Value != null)
                            {
                                bool isPenddingApprove = Convert.ToBoolean(p.Value);
                                query = query.Where(u => isPenddingApprove == true ? u.Status == (short)TransferStatus.Pendding : u.Status != (short)TransferStatus.Pendding);
                            }
                            break;
                        case "ListDepartment":
                            if (p.Value != null)
                            {
                                IList<long> departments = (IList<long>)p.Value;
                                query = query.Where(u => departments.Contains(u.NewDepartment));
                            }
                            break;
                    }
                }
            }
            query = query.OrderBy(u => u.EmployeeATID);

            ListDTOModel<IC_EmployeeTransferDTO> mv = new ListDTOModel<IC_EmployeeTransferDTO>();
            mv.TotalCount = query.Count();
            query = query.Skip((pageIndex - 1) * pageSize).Take(pageSize);
            var data = query.ToList();

            mv.PageIndex = pageIndex;
            mv.Data = data;
            return mv;
        }

        public List<IC_EmployeeTransferDTO> GetMany(List<AddedParam> addedParams)
        {
            var departmentLst = _dbContext.IC_Department;
            var query = (from empt in _dbContext.IC_EmployeeTransfer
                         join emp in _dbContext.HR_User
                          on empt.EmployeeATID equals emp.EmployeeATID

                          join card in _dbContext.HR_CardNumberInfo.Where(x=>x.IsActive == true)
                          on empt.EmployeeATID equals card.EmployeeATID

                         join dpOld in departmentLst
                            on empt.OldDepartment equals dpOld.Index

                         join dpNew in departmentLst
                            on empt.NewDepartment equals dpNew.Index
                         join w in _dbContext.IC_WorkingInfo 

                         on empt.EmployeeATID equals w.EmployeeATID into wWork
                         from wWorkResult in  wWork.DefaultIfEmpty()

                         join m in _dbContext.IC_UserMaster
                        on emp.EmployeeATID equals m.EmployeeATID into dMaster
                         from dMasterResult in dMaster.DefaultIfEmpty()

                         select new IC_EmployeeTransferDTO
                         {
                             CompanyIndex = empt.CompanyIndex,
                             EmployeeATID = empt.EmployeeATID,
                             FullName = emp.FullName,
                             EmployeeCode = emp.EmployeeCode,
                             CardNumber = card.CardNumber,
                             NameOnMachine = dMasterResult.NameOnMachine,
                             Gender= emp.Gender,
                             UpdatedDate = emp.UpdatedDate,
                             StoppedDate = wWorkResult.ToDate,
                             ImageUpload = dMasterResult.FaceV2_Content,
                             IsFromTime = empt.FromTime.ToString("dd/MM/yyyy"),
                             IsToTime = empt.ToTime.ToString("dd/MM/yyyy"),
                             FromTime = empt.FromTime,
                             ToTime = empt.ToTime,
                             Description = empt.Description,
                             NewDepartment = empt.NewDepartment,
                             NewDepartmentName = dpNew.Name,
                             NewDepartmentCode = dpNew.Code,
                             OldDepartment = empt.OldDepartment,
                             OldDepartmentName = dpOld.Name,
                             OldDepartmentCode = dpOld.Code,
                             RemoveFromOldDepartmentName = empt.RemoveFromOldDepartment == true ? "Có" : "Không",
                             RemoveFromOldDepartment = empt.RemoveFromOldDepartment,
                             AddOnNewDepartment = empt.AddOnNewDepartment,
                             AddOnNewDepartmentName = empt.AddOnNewDepartment == true ? "Có" : "Không",
                             TypeTemporaryTransfer = "Tạm thời",
                             TransferApprovedDate = empt.ApprovedDate.HasValue ? empt.ApprovedDate.Value.ToString("dd/MM/yyyy") : "",
                             TransferApprovedUser = empt.ApprovedUser,
                             Status = empt.Status,
                             TransferApproveStatus = empt.Status == 0 ? "Chờ duyệt" : empt.Status == 1 ? "Đã duyệt" : "Từ chối",
                             TemporaryTransfer = true,
                             EmployeeTypeId = emp.EmployeeType
                         });

            if (addedParams != null)
            {
                foreach (AddedParam p in addedParams)
                {
                    switch (p.Key)
                    {
                        case "Filter":
                            if (p.Value != null)
                            {
                               
                                string searchcode = p.Value.ToString();    
                                query = query.Where(u => u.Description.Contains(searchcode)
                                || u.FullName.Contains(searchcode)
                                || u.NewDepartmentName.Contains(searchcode)
                                || (u.AddOnNewDepartment == true ? "Có" : "Không").Contains(searchcode)
                                || (u.RemoveFromOldDepartment == true ? "Có" : "Không").Contains(searchcode));
                            }
                            break;
                        case "CompanyIndex":
                            if (p.Value != null)
                            {
                                int companyIndex = Convert.ToInt32(p.Value);
                                query = query.Where(u => u.CompanyIndex == companyIndex);
                            }
                            break;
                        case "FromDate":
                            if (p.Value != null)
                            {
                                DateTime fromDate = Convert.ToDateTime(p.Value);
                                query = query.Where(u => fromDate.Date >= u.FromTime.Value.Date);
                            }
                            break;
                        case "ToDate":
                            if (p.Value != null)
                            {
                                DateTime toDate = Convert.ToDateTime(p.Value);
                                query = query.Where(u => toDate.Date <= u.ToTime.Value.Date);
                            }
                            break;
                        case "IsPenddingApprove":
                            if (p.Value != null)
                            {
                                bool isPenddingApprove = Convert.ToBoolean(p.Value);
                                query = query.Where(u => isPenddingApprove == true ? u.Status == (short)TransferStatus.Pendding : u.Status != (short)TransferStatus.Pendding);
                            }
                            break;
                        case "IsApprovedOrPending":
                            if (p.Value != null)
                            {
                                var isApproveOrPending = Convert.ToBoolean(p.Value);
                                if (isApproveOrPending)
                                {
                                    query = query.Where(u => u.Status == (short)TransferStatus.Approve || u.Status == (short)TransferStatus.Pendding);
                                }
                            }
                            break;
                        case "Status":
                            if (p.Value != null)
                            {
                                int status = Convert.ToInt32(p.Value);
                                query = query.Where(u => u.Status == status);
                            }
                            break;
                        case "ListEmployeeID":
                            if (p.Value != null)
                            {
                                IList<string> listEmployee = (IList<string>)p.Value;
                                query = query.Where(u => listEmployee.Contains(u.EmployeeATID));
                            }
                            break;
                        case "ListDepartment":
                            if (p.Value != null)
                            {
                                IList<long> departments = (IList<long>)p.Value;
                                query = query.Where(u => departments.Contains(u.NewDepartment));
                            }
                            break;
                        case "IsCurrentTransfer":
                            if (p.Value != null)
                            {
                                bool currentTransfer = Convert.ToBoolean(p.Value);
                                if (currentTransfer)
                                {
                                    query = query.Where(u => DateTime.Now.Date >= u.FromTime.Value.Date && DateTime.Now.Date <= u.ToTime.Value.Date);
                                }
                            }
                            break;
                        case "UserType":
                            if (p.Value != null)
                            {
                                query = query.Where(u => u.EmployeeTypeId == (int)p.Value || (u.EmployeeTypeId == null && (int)p.Value == 1));
                            }
                            break;
                    }
                }
            }
            return query.ToList();
        }

        public void CreateUnsaved(IC_EmployeeTransfer employeeTransfer)
        {
            _dbContext.IC_EmployeeTransfer.Add(employeeTransfer);
        }

        public IC_EmployeeTransfer CreateSave(IC_EmployeeTransfer employeeTransfer)
        {
            _dbContext.IC_EmployeeTransfer.Add(employeeTransfer);
            _dbContext.SaveChanges();
            return employeeTransfer;
        }
        public void CreateSave(List<IC_EmployeeTransfer> listEmployeeTransfer)
        {
            var listEmployeeTransferEmployeeATID = listEmployeeTransfer.Select(x => x.EmployeeATID).ToHashSet();
            var listExistedEmployeeTransfer = _dbContext.IC_EmployeeTransfer.Where(x 
                => listEmployeeTransferEmployeeATID.Contains(x.EmployeeATID)).ToList();
            foreach (var employeeTransfer in listEmployeeTransfer)
            {
                var existedEmployeeTransfer = listExistedEmployeeTransfer.FirstOrDefault(x
                    => x.EmployeeATID == employeeTransfer.EmployeeATID && x.FromTime == employeeTransfer.FromTime
                    && x.NewDepartment == employeeTransfer.NewDepartment && x.CompanyIndex == employeeTransfer.CompanyIndex);
                if (existedEmployeeTransfer != null)
                {
                    _dbContext.IC_EmployeeTransfer.Remove(existedEmployeeTransfer);
                }
                _dbContext.IC_EmployeeTransfer.Add(employeeTransfer);
            }
            _dbContext.SaveChanges();
        }

    }
    public interface IIC_EmployeeTransferLogic
    {
        List<IC_EmployeeTransferDTO> GetMany(List<AddedParam> addedParams);
        IC_EmployeeTransferDTO Get(int companyID, string employeeID);
        ListDTOModel<IC_EmployeeTransferDTO> GetPage(List<AddedParam> addedParams);
        void CreateUnsaved(IC_EmployeeTransfer employeeTransfer);
        IC_EmployeeTransfer CreateSave(IC_EmployeeTransfer employeeTransfer);
        void CreateSave(List<IC_EmployeeTransfer> listEmployeeTransfer);
    }
}