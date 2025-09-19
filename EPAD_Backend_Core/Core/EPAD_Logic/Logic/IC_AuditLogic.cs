using System;
using System.Collections.Generic;
using System.Linq;
using EPAD_Common.Types;
using EPAD_Data;
using EPAD_Data.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace EPAD_Logic
{
    public class IC_AuditLogic : IIC_AuditLogic
    {
        public EPAD_Context _dbContext;
        private readonly ILogger _logger;
        private static IMemoryCache _cache;
        private ConfigObject _config;

        public IC_AuditLogic(EPAD_Context context, IMemoryCache cache)
        {
            _dbContext = context;
            _cache = cache;
            _config = ConfigObject.GetConfig(_cache);
        }
        public List<IC_AuditEntryDTO> GetMany(List<AddedParam> addedParams)
        {
            if (addedParams == null)
                return null;

            var query = _dbContext.IC_Audit.AsQueryable();

            if (addedParams != null)
            {
                foreach (AddedParam p in addedParams)
                {
                    switch (p.Key)
                    {
                        case "Filter":
                            if (p.Value != null)
                            {
                            }
                            break;
                        case "CompanyIndex":
                            if (p.Value != null)
                            {
                                int companyIndex = Convert.ToInt32(p.Value);
                                query = query.Where(u => u.CompanyIndex == companyIndex);
                            }
                            break;
                        case "UserName":
                            if (p.Value != null)
                            {
                                string username = p.Value.ToString();
                                query = query.Where(u => u.UserName == username);
                            }
                            break;
                        case "State":
                            if (p.Value != null)
                            {
                                string state = p.Value.ToString();
                                query = query.Where(u => u.State == state);
                            }
                            break;
                        case "FromDate":
                            if (p.Value != null)
                            {
                                DateTime fromDate = Convert.ToDateTime(p.Value);
                                query = query.Where(u => u.DateTime.Date >= fromDate.Date);
                            }
                            break;
                        case "ToDate":
                            if (p.Value != null)
                            {
                                DateTime toDate = Convert.ToDateTime(p.Value);
                                query = query.Where(u => u.DateTime.Date <= toDate.Date);
                            }
                            break;

                    }
                }
            }
            var data = query.Select(u => new IC_AuditEntryDTO(null)
            {
                Index = u.Index,
                CompanyIndex = u.CompanyIndex,
                UserName = u.UserName,
                DateTime = u.DateTime,
                TableName = u.TableName,
                KeyValuesString = u.KeyValues,
                OldValuesString = u.OldValues,
                NewValuesString = u.NewValues,
                AffectedColumns = u.AffectedColumns,
                StateString = u.State,
                Description =u.Description
            }).ToList();
            return data;
            // throw new NotImplementedException();
        }

        public ListDTOModel<IC_AuditEntryDTO> GetPage(List<AddedParam> addedParams)
        {
            if (addedParams == null)
                return null;
            var query = _dbContext.IC_Audit.AsQueryable();

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
                                string filter = p.Value.ToString();
                                query = query.Where(u => u.UserName.Contains(filter)
                                || u.Description.Contains(filter));
                            }
                            break;
                        case "State":
                            if (p.Value != null)
                            {
                                string state = p.Value.ToString();
                                query = query.Where(u => u.State == state);
                            }
                            break;
                        case "UserName":
                            if (p.Value != null)
                            {
                                string username = p.Value.ToString();
                                query = query.Where(u => u.UserName == username);
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
                                query = query.Where(u => u.DateTime >= fromDate);
                            }
                            break;
                        case "ToDate":
                            if (p.Value != null)
                            {
                                DateTime toDate = Convert.ToDateTime(p.Value);
                                query = query.Where(u => u.DateTime <= toDate);
                            }
                            break;
                        case "Status":
                            if (!string.IsNullOrEmpty(p.Value?.ToString()))
                            {
                                var status = p.Value.ToString();
                                query = query.Where(x => status.Contains(x.Status));
                            }
                            break;
                    }
                }
            }
            query = query.OrderByDescending(u => u.DateTime);
            ListDTOModel<IC_AuditEntryDTO> mv = new ListDTOModel<IC_AuditEntryDTO>();
            mv.TotalCount = query.Count();
            query = query.Skip((pageIndex - 1) * pageSize).Take(pageSize);
            var data = query.Select(u => new IC_AuditEntryDTO(null)
            {
                Index = u.Index,
                CompanyIndex = u.CompanyIndex,
                UserName = u.UserName,
                DateTime = u.DateTime,
                TableName = u.TableName,
                KeyValuesString = u.KeyValues,
                OldValuesString = u.OldValues,
                NewValuesString = u.NewValues,
                AffectedColumns = u.AffectedColumns,
                StateString = u.State,
                Description = u.Description,
                StatusString = u.Status,
                PageName = u.PageName,
                NumFailure = u.NumFailure,
                NumSuccess = u.NumSuccess,
                Name = u.Name,
            }).ToList();

            mv.PageIndex = pageIndex;
            mv.Data = data;
            return mv;
        }

        public IC_AuditEntryDTO Create(IC_AuditEntryDTO audit)
        {
            
            _dbContext.IC_Audit.Add(audit.ToAudit());
            _dbContext.SaveChanges();
            return audit;
        }
        public IC_AuditEntryDTO Update(IC_AuditEntryDTO audit)
        {
            var data = _dbContext.IC_Audit.FirstOrDefault(x => x.Index == audit.Index);
            if (data != null)
            {
                data.Status = audit.Status.ToString();
                data.Description = audit.Description;
                data.DescriptionEn = audit.DescriptionEn;
                data.DateTime = audit.DateTime;
                data.TableName = audit.TableName;
                data.KeyValues = audit.KeyValuesString;
                data.NewValues = audit.NewValuesString;
                data.OldValues = audit.OldValuesString;
                data.NumFailure = audit.NumFailure;
                data.NumSuccess = audit.NumSuccess;
                _dbContext.IC_Audit.Update(data);
                _dbContext.SaveChanges();
            }
            return audit;
        }
        public IC_AuditEntryDTO Delete(IC_AuditEntryDTO audit) {
            var data = _dbContext.IC_Audit.FirstOrDefault(e => e.Index == audit.Index);
            if (data != null)
            {
                _dbContext.IC_Audit.Remove(data);
                _dbContext.SaveChanges();
            }
            return audit;
        }

        public bool DeleteList(List<AddedParam> addedParams, UserInfo user)
        {
            if (addedParams == null)
                return false;

            var query = _dbContext.IC_Audit.AsQueryable();
            if (addedParams != null)
            {
                foreach (AddedParam p in addedParams) 
                {
                    switch (p.Key)
                    {
                        case "ListIndex":
                            if (p.Value != null)
                            {
                                IList<int> listIndex = (IList<int>)p.Value; 
                                query = query.Where(u => listIndex.Contains(u.Index));
                            }
                            break;
                        case "State":
                            if (p.Value != null)
                            {
                                string state = p.Value.ToString();
                                query = query.Where(u => u.State == state);
                            }
                            break;
                        case "UserName":
                            if (p.Value != null)
                            {
                                string username = p.Value.ToString();
                                query = query.Where(u => u.UserName == username);
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
                                query = query.Where(u => u.DateTime.Date >= fromDate.Date);
                            }
                            break;
                        case "ToDate":
                            if (p.Value != null)
                            {
                                DateTime toDate = Convert.ToDateTime(p.Value);
                                query = query.Where(u => u.DateTime.Date <= toDate.Date);
                            }
                            break;
                    }
                }
                var data = query.ToList();
                if (data != null)
                {
                    _dbContext.IC_Audit.RemoveRange(data);
                    _dbContext.SaveChanges();
                }
            }
            return true;
        }

        public IC_AuditEntryDTO CreateDefaultAudit
    (UserInfo user, string pageName, AuditType auditType)
        {
            return Create(new IC_AuditEntryDTO(user, pageName, auditType,
                AuditStatus.Processing, null, null));
        }

        public void UpdateAuditStatusToError(IC_AuditEntryDTO audit, string description, string descriptionEn)
        {
            audit.Status = AuditStatus.Error;
            audit.Description = description;
            audit.DescriptionEn = descriptionEn;
            audit.DateTime = DateTime.Now;
            if (audit.NumFailure == null)
            {
                audit.NumFailure = 1;
            }
            if (audit.NumSuccess == null)
            {
                audit.NumSuccess = 0;
            }
            Update(audit);
        }

        public void UpdateAuditStatusToCompleted(IC_AuditEntryDTO audit,
            int? numSuccess, int? numFailure,
            string description = null, string descriptionEn = null)
        {
            audit.Status = AuditStatus.Completed;
            audit.Description = description;
            audit.DescriptionEn = descriptionEn;
            audit.DateTime = DateTime.Now;
            audit.NumSuccess = (short?)numSuccess;
            audit.NumFailure = (short?)numFailure;
            Update(audit);
        }

    }

    public interface IIC_AuditLogic
    {
        List<IC_AuditEntryDTO> GetMany(List<AddedParam> addedParams);
        ListDTOModel<IC_AuditEntryDTO> GetPage(List<AddedParam> addedParams);
        IC_AuditEntryDTO Create(IC_AuditEntryDTO audit);
        IC_AuditEntryDTO Delete(IC_AuditEntryDTO audit);
        bool DeleteList(List<AddedParam> addedParams, UserInfo user);
        IC_AuditEntryDTO CreateDefaultAudit(UserInfo user, string pageName, AuditType auditType);
        void UpdateAuditStatusToError(IC_AuditEntryDTO audit, string description, string descriptionEn);
        void UpdateAuditStatusToCompleted(IC_AuditEntryDTO audit,
            int? numSuccess, int? numFailure, string description = null, string descriptionEn = null);
    }
}
