using EPAD_Common.Types;
using EPAD_Data;
using EPAD_Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EPAD_Logic
{
    public class IC_ServiceLogic : IIC_ServiceLogic
    {
        private EPAD_Context _dbContext;
        public IC_ServiceLogic(EPAD_Context dbContext)
        {
            _dbContext = dbContext;
        }
        public IEnumerable<IC_ServiveDTO> GetMany(List<AddedParam> addedParams)
        {
            var query = _dbContext.IC_Service.AsNoTracking();
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
                        case "ServiceType":
                            if (p.Value != null)
                            {
                                string servicetype = p.Value.ToString();
                                query = query.Where(u => u.ServiceType == servicetype);
                            }
                            break;
                        

                    }
                }
            }
            query = query.OrderBy(u => u.Index);
            var count = query.Count();
            var data = query.Select(u => new IC_ServiveDTO
            {
                Index = u.Index,
                CompanyIndex = u.CompanyIndex,
                Name = u.Name,
                Description = u.Description,
                ServiceType = u.ServiceType
            }).ToList();
            return data;
        }
    }

    public interface IIC_ServiceLogic
    {
        IEnumerable<IC_ServiveDTO> GetMany(List<AddedParam> addedParams);
    }
}
