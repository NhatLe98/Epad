using EPAD_Common.Types;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EPAD_Logic
{
    public class IC_DepartmentAndDeviceLogic : IIC_DepartmentAndDeviceLogic
    {
        private EPAD_Context _dbContext;
        public IC_DepartmentAndDeviceLogic(EPAD_Context dbcontext)
        {
            _dbContext = dbcontext;
        }

        public List<IC_DepartmentAndDeviceDTO> GetMany(List<AddedParam> addedParams)
        {
            if (addedParams == null)
                return null;
            var query = _dbContext.IC_DepartmentAndDevice.AsQueryable();
            foreach (var p in addedParams)
            {
                switch (p.Key)
                {
                    case "DepartmentIndex":
                        if (p.Value != null)
                        {
                            int departmentIndex = Convert.ToInt32(p.Value);
                            query = query.Where(e => e.DepartmentIndex == departmentIndex);
                        }
                        break;
                    case "CompanyIndex":
                        if (p.Value != null)
                        {
                            int companyIndex = Convert.ToInt32(p.Value);
                            query = query.Where(e => e.CompanyIndex == companyIndex);
                        }
                        break;
                    case "SerialNumber":
                        if (p.Value != null)
                        {
                            string serialNumber = p.ToString();
                            query = query.Where(e => e.SerialNumber == serialNumber);
                        }
                        break;

                }
            }

            var data = query.Select(e => new IC_DepartmentAndDeviceDTO
            {
                CompanyIndex = e.CompanyIndex,
                SerialNumber = e.SerialNumber,
                DepartmentIndex = e.DepartmentIndex,
                UpdatedDate = e.UpdatedDate,
                UpdatedUser = e.UpdatedUser,
            }).ToList();
            return data;
        }

        public List<IC_DepartmentAndDeviceDTO> GetAll()
        {
            var query = _dbContext.IC_DepartmentAndDevice.AsQueryable();
            var data = query.Select(e => new IC_DepartmentAndDeviceDTO
            {
                CompanyIndex = e.CompanyIndex,
                SerialNumber = e.SerialNumber,
                DepartmentIndex = e.DepartmentIndex,
                UpdatedDate = e.UpdatedDate,
                UpdatedUser = e.UpdatedUser,
            }).ToList();
            return data;
        }

        private void ConvertDTOToData(IC_DepartmentAndDeviceDTO dto, IC_DepartmentAndDevice data)
        {
            data.CompanyIndex = dto.CompanyIndex;
            data.DepartmentIndex = dto.DepartmentIndex;
            data.SerialNumber = dto.SerialNumber;
            data.UpdatedDate = dto.UpdatedDate;
            data.UpdatedUser = dto.UpdatedUser;
        }
        private IC_DepartmentAndDeviceDTO ConvertToDTO(IC_DepartmentAndDevice data)
        {
            IC_DepartmentAndDeviceDTO dto = new IC_DepartmentAndDeviceDTO();
            dto.CompanyIndex = data.CompanyIndex;
            dto.DepartmentIndex = data.DepartmentIndex;
            dto.SerialNumber = data.SerialNumber;
            dto.UpdatedDate = data.UpdatedDate;
            dto.UpdatedUser = data.UpdatedUser;
            return dto;
        }
    }
    public interface IIC_DepartmentAndDeviceLogic
    {
        List<IC_DepartmentAndDeviceDTO> GetMany(List<AddedParam> addedParams);
    }
}
