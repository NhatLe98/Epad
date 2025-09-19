using EPAD_Common.Types;
using EPAD_Data;
using EPAD_Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EPAD_Logic
{
    public class IC_ServiceAndDeviceLogic : IIC_ServiceAndDeviceLogic
    {
        private EPAD_Context _dbContext;
        public IC_ServiceAndDeviceLogic(EPAD_Context dbContext)
        {
            _dbContext = dbContext;
        }

        public IEnumerable<IC_ServiceAndDeviceDTO> GetMany(List<AddedParam> addedParams)
        {
            var query = (from sad in _dbContext.IC_ServiceAndDevices
                         join s in _dbContext.IC_Service
                          on sad.ServiceIndex equals s.Index
                         join d in _dbContext.IC_Device
                         on sad.SerialNumber equals d.SerialNumber
                         select new IC_ServiceAndDeviceDTO
                         {
                             CompanyIndex = sad.CompanyIndex,
                             ServiceType = s.ServiceType,
                             SerialNumber = sad.SerialNumber,
                             ServiceIndex = sad.ServiceIndex,
                             AliasName = d.AliasName,
                             IPAddress = d.IPAddress

                         }).AsQueryable();
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
                        case "ServiceIndex":
                            if (p.Value != null)
                            {
                                int serviceIndex = Convert.ToInt32(p.Value);
                                query = query.Where(u => u.ServiceIndex == serviceIndex);
                            }
                            break;
                        case "ListServiceType":
                            if (p.Value != null)
                            {
                                IList<string> serviceTypes = (IList<string>)p.Value;
                                query = query.Where(u => serviceTypes.Contains(u.ServiceType));
                            }
                            break;
                        case "ServiceType":
                            if (p.Value != null)
                            {
                                string serviceType = p.Value.ToString();
                                query = query.Where(u => u.ServiceType == serviceType);
                            }
                            break;
                        case "ListSerialNumber":
                            if (p.Value != null)
                            {
                                IList<string> serialNumbers = (IList<string>)p.Value;
                                query = query.Where(u => serialNumbers.Contains(u.SerialNumber));
                            }
                            break;
                        case "CompanyIndex":
                            if (p.Value != null)
                            {
                                int companyIndex = Convert.ToInt32(p.Value);
                                query = query.Where(u => u.CompanyIndex == companyIndex);
                            }
                            break;
                        case "ListServiceIndex":
                            if (p.Value != null)
                            {
                                IList<int> listIndex = (IList<int>)p.Value;
                                query = query.Where(u => listIndex.Contains(u.ServiceIndex));
                            }
                            break;
                            
                    }
                }
            }
            query = query.OrderBy(u => u.ServiceIndex);
            //var count = query.Count();
            var data = query.Select(u => new IC_ServiceAndDeviceDTO
            {
                ServiceIndex = u.ServiceIndex,
                CompanyIndex = u.CompanyIndex,
                SerialNumber = u.SerialNumber,
                ServiceType = u.ServiceType,
                AliasName = u.AliasName,
                IPAddress = u.IPAddress
            }).ToList();
            return data;
        }
    }

    public interface IIC_ServiceAndDeviceLogic
    {
        IEnumerable<IC_ServiceAndDeviceDTO> GetMany(List<AddedParam> addedParams);
    }
}
