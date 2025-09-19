using EPAD_Common.Types;
using EPAD_Data;
using EPAD_Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EPAD_Logic
{
    public class IC_GroupDeviceDetailLogic : IIC_GroupDeviceDetailLogic
    {
        private EPAD_Context _dbContext;
        public IC_GroupDeviceDetailLogic(EPAD_Context dbContext)
        {
            _dbContext = dbContext;
        }
        public List<IC_GroupDeviceDetailDTO> GetMany(List<AddedParam> addeParams)
        {
            if (addeParams == null)
                return new List<IC_GroupDeviceDetailDTO>();

            var query = (from gdd in _dbContext.IC_GroupDeviceDetails
                         join gd in _dbContext.IC_GroupDevice on gdd.GroupDeviceIndex equals gd.Index
                         select new IC_GroupDeviceDetailDTO
                         {
                             CompanyIndex = gdd.CompanyIndex,
                             GroupDeviceIndex = gdd.GroupDeviceIndex,
                             SerialNumber = gdd.SerialNumber,
                             GroupDeviceName = gd.Name,
                             GroupDeviceDescription = gd.Description
                         }).AsQueryable();

            if (addeParams != null)
            {
                foreach (AddedParam param in addeParams)
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
                        case "GroupDeviceIndex":
                            if (param.Value != null)
                            {
                                int groupDeviceIndex = Convert.ToInt32(param.Value);
                                query = query.Where(u => u.GroupDeviceIndex == groupDeviceIndex);
                            }
                            break;
                            
                    }
                }
            }
            
            return query.ToList() ;
        }
    }
    public interface IIC_GroupDeviceDetailLogic {
        List<IC_GroupDeviceDetailDTO> GetMany(List<AddedParam> addeParams);
    }
}
