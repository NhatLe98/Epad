using System;
using System.Collections.Generic;
using System.Linq;
using EPAD_Common.Types;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;

namespace EPAD_Logic
{
    public class IC_CommandSystemGroupLogic : IIC_CommandSystemGroupLogic
    {
        private EPAD_Context _dbContext;
        public IC_CommandSystemGroupLogic(EPAD_Context dbContext)
        {
            _dbContext = dbContext;

        }
        public List<IC_CommandSystemGroupDTO> GetMany(List<AddedParam> addedParams)
        {
            if (addedParams == null)
                return null;

            var query = _dbContext.IC_CommandSystemGroup.AsQueryable();

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
                                query = query.Where(u => u.EventType.Contains(filter)
                                        || u.GroupName.Contains(filter));
                            }
                            break;
                        case "SystemCommandStatus":
                            if (p.Value != null)
                            {
                                bool excuted = Convert.ToBoolean(p.Value);
                                query = query.Where(u => u.Excuted == excuted);
                            }
                            break;
                        case "ListSystemCommandGroupIndex":
                            if (p.Value != null)
                            {
                                IList<int> listSystemCommandGroupIndex = (IList<int>)p.Value;
                                query = query.Where(u => listSystemCommandGroupIndex.Contains(u.Index));
                            }
                            break;
                        case "CompanyIndex":
                            if (p.Value != null)
                            {
                                int companyIndex = Convert.ToInt32(p.Value);
                                query = query.Where(u => u.CompanyIndex == companyIndex);
                            }
                            break;
                        case "AfterHours":
                            if (p.Value != null)
                            {
                                int afterHours = Convert.ToInt32(p.Value);
                                query = query.Where(u => DateTime.Now.Subtract(u.CreatedDate.Value.Date).TotalHours >= afterHours);

                            }
                            break;
                        case "FromDate":
                            if (p.Value != null)
                            {
                                DateTime fromDate = Convert.ToDateTime(p.Value);
                                query = query.Where(u => u.CreatedDate.Value.Date >= fromDate.Date);
                            }
                            break;
                        case "ToDate":
                            if (p.Value != null)
                            {
                                DateTime toDate = Convert.ToDateTime(p.Value);
                                query = query.Where(u => u.CreatedDate.Value.Date <= toDate.Date);
                            }
                            break;
                        case "Excuted":
                            if (p.Value != null)
                            {
                                bool Excuted = Convert.ToBoolean(p.Value);
                                query = query.Where(u => u.Excuted == Excuted);
                            }
                            break;
                    }
                }
            }
            var data = query.Select(u => new IC_CommandSystemGroupDTO
            {
                CompanyIndex = u.CompanyIndex,
                CreatedDate = u.CreatedDate,
                EventType = u.EventType,
                Excuted = u.Excuted,
                ExternalData = u.ExternalData,
                GroupName = u.GroupName,
                Index = u.Index,
                UpdatedDate = u.UpdatedDate,
                UpdatedUser = u.UpdatedUser
            }).ToList();
            return data;
        }

        public List<IC_CommandSystemGroupDTO> UpdateList(List<IC_CommandSystemGroupDTO> listCommandSystemGroup)
        {
            var listIndex = listCommandSystemGroup.Select(u => u.Index).ToList();

            var query = _dbContext.IC_CommandSystemGroup.AsQueryable();
            List<IC_CommandSystemGroup> listData = query.Where(u => listIndex.Contains(u.Index)).ToList();
            if (listData.Count > 0)
            {
                foreach (var data in listData)
                {
                    var dto = listCommandSystemGroup.First(u => u.Index == data.Index);
                    ConvertDTOToData(dto, data);
                    _dbContext.IC_CommandSystemGroup.Update(data);
                    dto = ConvertToDTO(data);
                }
            }

            return listCommandSystemGroup;
        }
        private void ConvertDTOToData(IC_CommandSystemGroupDTO dto, IC_CommandSystemGroup data)
        {
            data.Index = dto.Index;
            data.GroupName = dto.GroupName;
            data.Excuted = dto.Excuted;
            data.EventType = dto.EventType;
            data.ExternalData = dto.ExternalData;
            data.UpdatedUser = dto.UpdatedUser;
            data.CompanyIndex = dto.CompanyIndex;

            if (dto.Index > 0)
            {
                data.UpdatedDate = DateTime.Now;
            }
            else
            {
                data.CreatedDate = DateTime.Now;
            }
        }
        private IC_CommandSystemGroupDTO ConvertToDTO(IC_CommandSystemGroup data)
        {
            if (data != null)
            {
                IC_CommandSystemGroupDTO dto = new IC_CommandSystemGroupDTO();
                dto.Index = data.Index;
                dto.GroupName = data.GroupName;
                dto.Excuted = data.Excuted;
                dto.EventType = data.EventType;
                dto.ExternalData = data.ExternalData;
                dto.UpdatedUser = data.UpdatedUser;
                dto.CompanyIndex = data.CompanyIndex;
                dto.UpdatedDate = data.UpdatedDate;
                dto.CreatedDate = data.CreatedDate;
                return dto;
            }
            return null;
        }
    }
    public interface IIC_CommandSystemGroupLogic
    {
        List<IC_CommandSystemGroupDTO> GetMany(List<AddedParam> addedParams);
        List<IC_CommandSystemGroupDTO> UpdateList(List<IC_CommandSystemGroupDTO> listCommandSystemGroup);
    }
}
