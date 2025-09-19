using AutoMapper;
using EPAD_Common.Types;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EPAD_Logic
{
    public class IC_ConfigLogic : IIC_ConfigLogic
    {
        private EPAD_Context _dbContext;
        private readonly IMapper _mapper;
        private IServiceScopeFactory _scopeFactory;

        public IC_ConfigLogic(IServiceScopeFactory scopeFactory, IMapper mapper)
        {
            _scopeFactory = scopeFactory;
            _mapper = mapper;
        }
        public async Task<List<IC_ConfigDTO>> GetMany(List<AddedParam> addedParams)
        {
            if (addedParams == null)
                return null;

            using var scope = _scopeFactory.CreateScope();
            _dbContext = scope.ServiceProvider.GetRequiredService<EPAD_Context>();
            var query =  _dbContext.IC_Config.AsQueryable();

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
                                query = query.Where(u => u.EventType.Contains(filter));
                            }
                            break;
                        case "EventType":
                            if (p.Value != null)
                            {
                                string eventType = p.Value.ToString();
                                query = query.Where(u => u.EventType == eventType);
                            }
                            break;
                        case "CompanyIndex":
                            if (p.Value != null)
                            {
                                int companyIndex = Convert.ToInt32(p.Value);
                                query = query.Where(u => u.CompanyIndex == companyIndex);
                            }
                            break;

                    }
                }
            }
            var data = await query.ToListAsync();

            return _mapper.Map<List<IC_ConfigDTO>>(data);
        }

    }

    public interface IIC_ConfigLogic
    {
        Task<List<IC_ConfigDTO>> GetMany(List<AddedParam> addedParams);
    }
}
