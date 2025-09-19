using EPAD_Common.Types;
using EPAD_Data;
using EPAD_Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EPAD_Logic
{
    public class HR_DepartmentLogic : IHR_DepartmentLogic
    {
        private ezHR_Context _dbContext;
        public HR_DepartmentLogic(ezHR_Context dbContext)
        {
            _dbContext = dbContext;
        }
        public List<IC_DepartmentDTO> GetMany(List<AddedParam> addedParams) {

            if (addedParams == null)
                return new List<IC_DepartmentDTO>();

            var query = _dbContext.HR_Department.AsQueryable();
            if (addedParams != null)
            {
                foreach (AddedParam param in addedParams)
                {
                    switch (param.Key)
                    {
                        case "CompanyIndex":
                            if (param.Value != null)
                            {
                                int companyIndex = Convert.ToInt32(param.Value);
                                query = query.Where(u => u.CompanyIndex == companyIndex);
                            }
                            break;
                        case "ListDepartment":
                            if (param.Value != null)
                            {
                                IList<long> departments = (IList<long>)param.Value;
                                query = query.Where(u => departments.Contains(u.Index));
                            }
                            break;
                    }
                }
            }
            var data = query.Select(u => new IC_DepartmentDTO
            {
                Index = u.Index,
                Name = u.Name
            }).ToList();
            return data;
        }
    }
    public interface IHR_DepartmentLogic {
        List<IC_DepartmentDTO> GetMany(List<AddedParam> addedParams);
    }
}
