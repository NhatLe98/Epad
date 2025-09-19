using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Entities.HR;
using EPAD_Data.Models;
using EPAD_Data.Models.HR;
using EPAD_Data.Models.IC;
using EPAD_Data.Sync_Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EPAD_Logic
{
    public class IC_DepartmentLogic : IIC_DepartmentLogic
    {
        private EPAD_Context _dbContext;
        private readonly ILogger _logger;

        public IC_DepartmentLogic(EPAD_Context dbContext, ILogger<IC_ScheduleAutoHostedLogic> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<int> OldDepartmentUpdate(int companyIndex, List<int> IDs, List<IC_Employee_Integrate> employeeIntegrateList)
        {
            try
            {
                int row = 0;
                var result = await _dbContext.IC_Department
                    .Where(u => u.CompanyIndex == companyIndex && u.OrgUnitID != null && !IDs.Contains(u.OrgUnitID.Value))
                    .ToListAsync();

                if (result.Any())
                {
                    foreach (var item in result)
                    {
                        var employeeIntegrate = employeeIntegrateList.FirstOrDefault(x => x.OrgUnitID == item.OrgUnitID);
                        if (employeeIntegrate != null && item.IsInactive == true)
                        {
                            item.IsInactive = false;
                            item.UpdatedDate = DateTime.Now;
                        }
                        else
                        {
                            item.IsInactive = true;
                            item.UpdatedDate = DateTime.Now;
                        }
                        row++;
                    }
                    _dbContext.IC_Department.UpdateRange(result);
                    await _dbContext.SaveChangesAsync();
                }
                return row;
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex}");
            }
            return 0;
        }


        public async Task<int> OldDepartmentUpdateAVN(int companyIndex, List<Cat_OrgStructure> employeeIntegrateList)
        {
            try
            {
                var ids = employeeIntegrateList.Select(x => x.code).Distinct().ToList();
                int row = 0;
                var result = await _dbContext.IC_Department
                    .Where(u => u.CompanyIndex == companyIndex && !ids.Contains(u.Code) && u.Code != null && u.UpdatedUser == UpdatedUser.AutoIntegrateEmployee.ToString())
                    .ToListAsync();

                if (result.Any())
                {
                    foreach (var item in result)
                    {
                        var employeeIntegrate = employeeIntegrateList.FirstOrDefault(x => x.code == item.Code);
                        if (employeeIntegrate != null && item.IsInactive == true)
                        {
                            item.IsInactive = false;
                            item.UpdatedDate = DateTime.Now;
                        }
                        else
                        {
                            item.IsInactive = true;
                            item.UpdatedDate = DateTime.Now;
                        }
                        row++;
                    }
                    _dbContext.IC_Department.UpdateRange(result);
                    await _dbContext.SaveChangesAsync();
                }
                return row;
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex}");
            }
            return 0;
        }

        public async Task<int> OldDepartmentUpdateStandard(int companyIndex, List<IC_DepartmentIntegrate> employeeIntegrateList)
        {
            try
            {
                var ids = employeeIntegrateList.Select(x => x.Code).Distinct().ToList();
                int row = 0;
                var result = await _dbContext.IC_Department
                    .Where(u => u.CompanyIndex == companyIndex & !ids.Contains(u.Code) && u.Code != null && u.UpdatedUser == UpdatedUser.AutoIntegrateEmployee.ToString())
                    .ToListAsync();

                if (result.Any())
                {
                    foreach (var item in result)
                    {
                        var employeeIntegrate = employeeIntegrateList.FirstOrDefault(x => x.Code == item.Code);
                        if (employeeIntegrate != null && item.IsInactive == true)
                        {
                            item.IsInactive = false;
                            item.UpdatedDate = DateTime.Now;
                        }
                        else
                        {
                            item.IsInactive = true;
                            item.UpdatedDate = DateTime.Now;
                        }
                        row++;
                    }
                    _dbContext.IC_Department.UpdateRange(result);
                    await _dbContext.SaveChangesAsync();
                }
                return row;
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex}");
            }
            return 0;
        }

        public List<IC_DepartmentDTO> GetAll(int companyIndex)
        {
            var query = _dbContext.IC_Department.Where(u => u.CompanyIndex == companyIndex && u.IsInactive != true)
                .Select(u => new IC_DepartmentDTO
                {
                    Index = u.Index,
                    ParentIndex = u.ParentIndex,
                    Code = u.Code,
                    Name = u.Name
                }).OrderBy(u => u.Name).ToList();

            return query;
        }

        public async Task<IC_DepartmentDTO> CheckExistedOrCreateOVN(IC_DepartmentDTO dto, List<IC_Department_Integrate_OVN> departmentIntegrates, int row)
        {
            try
            {
                //Get existing in DB (ePAD)
                var entity = await _dbContext.IC_Department.FirstOrDefaultAsync(e => e.OrgUnitID == dto.OrgUnitID);
                bool isInsert = false;
                bool isUpdate = false;
                if (entity != null)
                {
                    dto.Index = entity.Index;
                    if (entity.Code != dto.Code || entity.Name != dto.Name || entity.Location != dto.Location || entity.Description != dto.Description || entity.OVNID != dto.OVNID
                        || entity.ParentIndex != dto.ParentIndex || entity.OrgUnitID != dto.OrgUnitID || entity.OrgUnitParentNode != dto.OrgUnitParentNode)
                    {
                        entity = ConvertDTOToUpdateData(dto, entity);
                        _dbContext.IC_Department.Update(entity);
                        isUpdate = true;
                        _logger.LogError($"{row}. Update: OVNID {entity.OVNID} - OrgUnitID {entity.OrgUnitID} - OrgUnitParentNode {entity.OrgUnitParentNode}");
                    }
                }
                else
                {
                    entity = new IC_Department();
                    entity = ConvertDTOToInsertData(dto, entity);
                    _dbContext.IC_Department.Add(entity);
                    isInsert = true;
                    _logger.LogError($"{row}. Insert: OVNID {entity.OVNID} - OrgUnitID {entity.OrgUnitID} - OrgUnitParentNode {entity.OrgUnitParentNode}");
                }

                await _dbContext.SaveChangesAsync();
                return ConvertEntitiesToDTOAndActive(entity, isInsert, isUpdate);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex}");
            }
            return null;
        }

        public async Task<IC_DepartmentDTO> CheckExistedOrCreateAVN(IC_DepartmentDTO dto, int row)
        {
            try
            {
                //Get existing in DB (ePAD)
                var entity = await _dbContext.IC_Department.FirstOrDefaultAsync(e => e.Code == dto.Code);
                bool isInsert = false;
                bool isUpdate = false;
                if (entity != null)
                {
                    dto.Index = entity.Index;
                    entity = ConvertDTOToUpdateData(dto, entity);
                    _dbContext.IC_Department.Update(entity);
                    isUpdate = true;
                }
                else
                {
                    entity = new IC_Department();
                    entity = ConvertDTOToInsertData(dto, entity);
                    _dbContext.IC_Department.Add(entity);
                    isInsert = true;
                }

                await _dbContext.SaveChangesAsync();
                return ConvertEntitiesToDTOAndActive(entity, isInsert, isUpdate);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex}");
            }
            return null;
        }

        public IC_DepartmentDTO CheckExistedOrCreate(IC_DepartmentDTO dto, string clientName)
        {

            try
            {
                var entity = new IC_Department();
                if (clientName == ClientName.OVN.ToString())
                {
                    entity = _dbContext.IC_Department
                        .FirstOrDefault(e => e.CompanyIndex == dto.CompanyIndex && e.OrgUnitID == dto.OrgUnitID);

                }
                else
                {
                    entity = _dbContext.IC_Department.FirstOrDefault(e => e.CompanyIndex == dto.CompanyIndex
                        && (e.Code.ToLower() == dto.Code.ToLower() || e.Name.ToLower() == dto.Name.ToLower()) && e.ParentIndex == dto.ParentIndex);
                }

                if (entity != null)
                {
                    if (entity.Code != dto.Code || entity.Name != dto.Name || entity.Location != dto.Location || entity.Description != dto.Description
                         || entity.OrgUnitID != dto.OrgUnitID || entity.OrgUnitParentNode != dto.OrgUnitParentNode)
                    {

                        entity = ConvertDTOToUpdateDataOVN(dto, entity);
                        _dbContext.IC_Department.Update(entity);
                    }
                }
                else
                {
                    entity = new IC_Department();
                    entity = ConvertDTOToInsertData(dto, entity);
                    _dbContext.IC_Department.Add(entity);
                }



                _dbContext.SaveChanges();
                return ConvertToDTO(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex}");
                return null;
            }
        }

        public List<IC_DepartmentDTO> CheckExistedOrCreateList(List<IC_DepartmentDTO> listDepartment)
        {
            if (listDepartment == null || (listDepartment != null && listDepartment.Count() == 0))
                return null;

            var returnList = new List<IC_DepartmentDTO>();
            var listCode = listDepartment.Select(e => e.Name).ToList();
            var dataItem = _dbContext.IC_Department.Where(e => listCode.Contains(e.Name) && e.CompanyIndex == listDepartment.First().CompanyIndex);

            foreach (var item in listDepartment)
            {
                if (dataItem != null)
                {
                    var existed = dataItem.FirstOrDefault(e => e.Name == item.Name);
                    if (existed == null)
                    {
                        existed = new IC_Department();
                        existed.CreatedDate = DateTime.Now;
                        existed = ConvertDTOToInsertData(item, existed);
                        _dbContext.IC_Department.Add(existed);
                        _dbContext.SaveChanges();
                    }
                    returnList.Add(ConvertToDTO(existed));
                }
                else
                {
                    var existed = new IC_Department();
                    existed.CreatedDate = DateTime.Now;
                    existed = ConvertDTOToInsertData(item, existed);
                    _dbContext.IC_Department.Add(existed);
                    _dbContext.SaveChanges();
                    returnList.Add(ConvertToDTO(existed));
                }
            }
            return returnList;
        }

        public List<IC_DepartmentDTO> CheckExistedOrCreateListDepartmentFromImportMAY(List<IC_DepartmentDTO> listDepartment, UserInfo user)
        {
            if (listDepartment == null || (listDepartment != null && listDepartment.Count() == 0))
                return null;

            var returnList = new List<IC_DepartmentDTO>();
            var listCode = listDepartment.Select(e => e.Name).ToList();
            var dataItem = _dbContext.IC_Department.Where(e => listCode.Contains(e.Name) && e.CompanyIndex == listDepartment.First().CompanyIndex).ToList();
            var existed = new IC_Department();
            foreach (var item in listDepartment)
            {
                if (dataItem != null)
                {
                    existed = dataItem.FirstOrDefault(e => e.Name == item.Name);
                    if (existed == null)
                    {
                        existed = new IC_Department();
                        existed.CreatedDate = DateTime.Now;
                        existed = ConvertDTOToInsertData(item, existed);
                        _dbContext.IC_Department.Add(existed);
                    }
                }
                else
                {
                    existed = new IC_Department();
                    existed.CreatedDate = DateTime.Now;
                    existed = ConvertDTOToInsertData(item, existed);
                    _dbContext.IC_Department.Add(existed);
                }
            }
            _dbContext.SaveChanges();

            var departmentDBList = _dbContext.IC_Department.Where(x => listCode.Contains(x.Name)).ToList();
            if (departmentDBList != null)
            {
                foreach (var item in listDepartment)
                {
                    var parentDepartment = departmentDBList.FirstOrDefault(x => x.Name == item.ParentName);
                    existed = departmentDBList.FirstOrDefault(e => e.Name == item.Name);
                    if (existed != null)
                    {
                        existed.ParentIndex = parentDepartment?.Index ?? 0;
                        _dbContext.IC_Department.Update(existed);
                    }
                }
            }
            _dbContext.SaveChanges();

            departmentDBList.ForEach(x =>
            {
                returnList.Add(ConvertToDTO(x));
            });

            return returnList;
        }



        public List<IC_DepartmentDTO> CheckExistedOrCreateListDepartmentFromImport(List<IC_DepartmentDTO> listDepartment, UserInfo user)
        {
            if (listDepartment == null || (listDepartment != null && listDepartment.Count() == 0))
                return null;

            var returnList = new List<IC_DepartmentDTO>();
            var listName = listDepartment.Select(e => e.Name).ToList();
            var listCode = listDepartment.Select(e => e.Code).ToList();
            var listIndex = listDepartment.Select(e => e.Index).ToList();
            var dataItem = _dbContext.IC_Department.Where(e => (listCode.Contains(e.Code) || listIndex.Contains(e.Index))
                && e.CompanyIndex == user.CompanyIndex).ToList();
            var existed = new IC_Department();
            foreach (var item in listDepartment)
            {
                if (dataItem != null)
                {
                    existed = dataItem.FirstOrDefault(x => x.Code == item.Code || x.Index == item.Index);
                    if (existed != null)
                    {
                        existed.ParentIndex = (int)item.ParentIndex;
                        existed.ParentCode = item.ParentCode;
                        existed.Code = item.Code;
                        existed.UpdatedDate = DateTime.Now;
                        _dbContext.IC_Department.Update(existed);
                    }
                    else
                    {
                        existed = new IC_Department();
                        existed.CreatedDate = DateTime.Now;
                        existed = ConvertDTOToInsertData(item, existed);
                        _dbContext.IC_Department.Add(existed);
                    }
                }
                else
                {
                    existed = new IC_Department();
                    existed.CreatedDate = DateTime.Now;
                    existed = ConvertDTOToInsertData(item, existed);
                    _dbContext.IC_Department.Add(existed);
                }

            }
            _dbContext.SaveChanges();

            var departmentDBList = _dbContext.IC_Department.Where(x => listName.Contains(x.Name)).ToList();
            if (departmentDBList != null)
            {
                foreach (var item in listDepartment)
                {
                    if (!string.IsNullOrWhiteSpace(item.ParentCode))
                    {
                        var parentDepartment = departmentDBList.FirstOrDefault(x => x.Code == item.ParentCode);
                        if (parentDepartment != null)
                        {
                            existed = departmentDBList.FirstOrDefault(e => e.Code == item.Code);
                            if (existed != null)
                            {
                                existed.ParentIndex = parentDepartment?.Index ?? null;
                                _dbContext.IC_Department.Update(existed);
                            }
                        }
                    }
                }
            }
            _dbContext.SaveChanges();

            departmentDBList.ForEach(x =>
            {
                returnList.Add(ConvertToDTO(x));
            });

            return returnList;
        }

        public List<IC_DepartmentDTO> CheckExistedListDepartmentFromImport(List<IC_DepartmentDTO> listDepartment, UserInfo user)
        {
            if (listDepartment == null || (listDepartment != null && listDepartment.Count() == 0))
                return null;

            var returnList = new List<IC_DepartmentDTO>();
            var listName = listDepartment.Select(e => e.Name).ToList();
            var listCode = listDepartment.Select(e => e.Code).ToList();
            var listIndex = listDepartment.Select(e => e.Index).ToList();
            var existed = new IC_Department();
            var departmentDBList = _dbContext.IC_Department.Where(x => listName.Contains(x.Name)).ToList();
            departmentDBList.ForEach(x =>
            {
                returnList.Add(ConvertToDTO(x));
            });

            return returnList;
        }

        public List<HR_TeamInfo> CheckExistedOrCreateList(List<HR_TeamInfo> listTeam)
        {
            if (listTeam == null || (listTeam != null && listTeam.Count() == 0))
                return null;

            var returnList = new List<HR_TeamInfo>();
            var listCode = listTeam.Select(e => e.Name).ToList();
            var dataItem = _dbContext.HR_TeamInfo.Where(e => listCode.Contains(e.Name) && e.CompanyIndex == listTeam.First().CompanyIndex);

            foreach (var item in listTeam)
            {
                if (dataItem != null)
                {
                    var existed = dataItem.FirstOrDefault(e => e.Name == item.Name);
                    if (existed == null)
                    {
                        existed = new HR_TeamInfo();
                        existed.Name = item.Name;
                        existed.CompanyIndex = item.CompanyIndex;
                        _dbContext.HR_TeamInfo.Add(existed);
                        _dbContext.SaveChanges();
                    }
                    returnList.Add(existed);
                }
                else
                {
                    var existed = new HR_TeamInfo();
                    existed.Name = item.Name;
                    existed.CompanyIndex = item.CompanyIndex;
                    _dbContext.HR_TeamInfo.Add(existed);
                    _dbContext.SaveChanges();
                    returnList.Add(existed);
                }
            }
            return returnList;
        }

        public List<HR_GradeInfo> CheckExistedOrCreateList(List<HR_GradeInfo> listTeam)
        {
            if (listTeam == null || (listTeam != null && listTeam.Count() == 0))
                return null;

            var returnList = new List<HR_GradeInfo>();
            var listCode = listTeam.Select(e => e.Name).ToList();
            var dataItem = _dbContext.HR_GradeInfo.Where(e => listCode.Contains(e.Name) && e.CompanyIndex == listTeam.First().CompanyIndex);

            foreach (var item in listTeam)
            {
                if (dataItem != null)
                {
                    var existed = dataItem.FirstOrDefault(e => e.Name == item.Name);
                    if (existed == null)
                    {
                        existed = new HR_GradeInfo();
                        existed.Name = item.Name;
                        existed.CompanyIndex = item.CompanyIndex;
                        _dbContext.HR_GradeInfo.Add(existed);
                        _dbContext.SaveChanges();
                    }
                    returnList.Add(existed);
                }
                else
                {
                    var existed = new HR_GradeInfo();
                    existed.Name = item.Name;
                    existed.CompanyIndex = item.CompanyIndex;
                    _dbContext.HR_GradeInfo.Add(existed);
                    _dbContext.SaveChanges();
                    returnList.Add(existed);
                }
            }
            return returnList;
        }

        public List<IC_DepartmentDTO> GetByNames(List<string> names)
        {
            var result = _dbContext.IC_Department.Where(x => names.Contains(x.Name)).ToList()
                .Select(x => ConvertToDTO(x)).ToList();
            return result;
        }

        private IC_DepartmentDTO ConvertToDTO(IC_Department data)
        {
            var dto = new IC_DepartmentDTO
            {
                Index = data.Index,
                Name = data.Name,
                Location = data.Location,
                Description = data.Description,
                Code = data.Code,
                CompanyIndex = data.CompanyIndex,
                ParentIndex = data.ParentIndex,
                CreatedDate = data.CreatedDate,
                UpdatedDate = data.UpdatedDate,
                UpdatedUser = data.UpdatedUser,
                OrgUnitID = data.OrgUnitID ?? 0,
                OrgUnitParentNode = data.OrgUnitParentNode ?? 0,
                OVNID = data.OVNID,
                IsDriverDepartment = data?.IsDriverDepartment ?? false,
                IsContractorDepartment = data?.IsContractorDepartment ?? false,
            };
            return dto;
        }

        private IC_DepartmentDTO ConvertEntitiesToDTOAndActive(IC_Department data, bool isInsert, bool isUpdate)
        {
            var dto = new IC_DepartmentDTO
            {
                Index = data.Index,
                Name = data.Name,
                Location = data.Location,
                Description = data.Description,
                Code = data.Code,
                CompanyIndex = 2,
                ParentIndex = data.ParentIndex,
                CreatedDate = data.CreatedDate,
                UpdatedDate = data.UpdatedDate,
                UpdatedUser = data.UpdatedUser,
                OrgUnitID = data.OrgUnitID ?? 0,
                OrgUnitParentNode = data.OrgUnitParentNode ?? 0,
                OVNID = data.OVNID,
                IsUpdate = isInsert,
                IsInsert = isUpdate,
            };

            return dto;
        }

        private IC_Department ConvertDTOToInsertData(IC_DepartmentDTO dto, IC_Department data)
        {
            data.CompanyIndex = dto.CompanyIndex;
            data.Name = dto.Name;
            data.Location = dto.Location;
            data.Description = dto.Description;
            data.Code = dto.Code;
            data.OVNID = dto.OVNID;
            data.CreatedDate = dto.CreatedDate;
            data.ParentCode = dto.ParentCode;
            data.IsContractorDepartment = dto.IsContractorDepartment;
            data.IsDriverDepartment = dto.IsDriverDepartment;

            if (dto.ParentIndex.HasValue)
                data.ParentIndex = (int)dto.ParentIndex.Value;

            if (dto.OrgUnitID != null)
                data.OrgUnitID = dto.OrgUnitID.Value;

            if (dto.OrgUnitParentNode != null)
                data.OrgUnitParentNode = dto.OrgUnitParentNode.Value;

            return data;
        }

        public IC_Department ConvertDTOToUpdateData(IC_DepartmentDTO dto, IC_Department data)
        {
            data.Code = dto.Code;
            data.Name = dto.Name;
            data.Location = dto.Location;
            data.Description = dto.Description;
            data.OVNID = dto.OVNID;
            data.IsInactive = false;
            data.UpdatedDate = DateTime.Now;
            data.UpdatedUser = dto.UpdatedUser;
            data.ParentCode = dto.ParentCode;

            if (dto.ParentIndex.HasValue)
                data.ParentIndex = (int)dto.ParentIndex.Value;

            if (dto.OrgUnitID != null)
                data.OrgUnitID = dto.OrgUnitID.Value;

            if (dto.OrgUnitParentNode != null)
                data.OrgUnitParentNode = dto.OrgUnitParentNode.Value;

            return data;
        }

        public IC_Department ConvertDTOToUpdateDataOVN(IC_DepartmentDTO dto, IC_Department data)
        {
            data.Code = dto.Code;
            data.Name = dto.Name;
            data.Location = dto.Location;
            data.Description = dto.Description;
            data.IsInactive = false;
            data.UpdatedDate = DateTime.Now;
            data.UpdatedUser = dto.UpdatedUser;

            if (dto.ParentIndex.HasValue)
                data.ParentIndex = (int)dto.ParentIndex.Value;

            if (dto.OrgUnitID != null)
                data.OrgUnitID = dto.OrgUnitID.Value;

            if (dto.OrgUnitParentNode != null)
                data.OrgUnitParentNode = dto.OrgUnitParentNode.Value;

            return data;
        }

        public List<IC_DepartmentImportDTO> ValidationImportDepartment(List<IC_DepartmentImportDTO> param)
        {
            var listDepartmentNameCodeImport = _dbContext.IC_Department.Select(e => new
            {
                Name = e.Name.Trim(),
                Code = e.Code.ToUpper().Trim(),
                ParentIndex = e.ParentIndex,
                ParentName = string.Empty
            }).ToHashSet();

            var listParentIndex = listDepartmentNameCodeImport.Where(x => x.ParentIndex.HasValue && x.ParentIndex.Value > 0)
                .Select(x => x.ParentIndex.Value).ToList();
            var listParentDep = _dbContext.IC_Department.Where(x => listParentIndex.Contains(x.Index)).ToList();
            if (listParentDep.Count > 0)
            {
                listDepartmentNameCodeImport = listDepartmentNameCodeImport.Select(x => new
                {
                    Name = x.Name.Trim(),
                    Code = x.Code,
                    ParentIndex = x.ParentIndex,
                    ParentName = listParentDep.FirstOrDefault(y => y.Index == x.ParentIndex)?.Name?.Trim() ?? string.Empty,
                }).ToHashSet();
            }

            var errorList = new List<IC_DepartmentImportDTO>();
            var checkDuplicateCode = param.GroupBy(x => x.Code.ToUpper().Trim()).Where(g => g.Count() > 1).Select(y => y.Key).ToList();
            var checkDuplicateName = param.GroupBy(x => new { name = x.Name.Trim(), parentName = x.ParentName.Trim() }).Where(g 
                => g.Count() > 1).Select(y => new { y.Key.name, y.Key.parentName }).ToList();
            var checkMaxLength = param.Where(e => e.Code.Length > 50
            || e.Name.Length > 200
            || e.Location.Length > 200
            || e.ParentName.Length > 200
            || e.Description.Length > 2000
            ).ToList();
            var checkIsNull = param.Where(e => string.IsNullOrWhiteSpace(e.Code)
            || string.IsNullOrWhiteSpace(e.Name)).ToList();

            var checkExistedCode = param.Where(e => listDepartmentNameCodeImport.Where(x => x.Code != null)
                .Select(x => x.Code.ToUpper().Trim()).Contains(e.Code.ToUpper().Trim())).ToList();

            //var checkExistedName = param.Where(e 
            //    => listDepartmentNameCodeImport.Select(x => x.Name.Trim()).Contains(e.Name.Trim())).ToList();

            var checkExistedName = param.Where(e
                => listDepartmentNameCodeImport.Any(x => x.Name.Trim() == e.Name.Trim() && x.ParentName.Trim() == e.ParentName.Trim())).ToList();

            var checkDuplicateNameAndParent = param.Where(e => e.ParentName.Trim() == e.Name.Trim()).ToList();

            if (checkDuplicateCode != null && checkDuplicateCode.Count() > 0)
            {
                var duplicate = param.Where(e => checkDuplicateCode.Contains(e.Code.ToUpper().Trim())).ToList();
                foreach (var item in duplicate)
                {
                    item.ErrorMessage = "Trùng mã phòng ban\r\n";
                }
            }

            if (checkDuplicateName != null && checkDuplicateName.Count() > 0)
            {
                //var duplicate = param.Where(e => checkDuplicateName.Contains(e.Name.Trim())).ToList();
                var duplicate = param.Where(e => checkDuplicateName.Any(x => x.name.Trim() == e.Name.Trim() && x.parentName.Trim() == e.ParentName.Trim())).ToList();
                foreach (var item in duplicate)
                {
                    item.ErrorMessage = "Trùng tên phòng ban\r\n";
                }
            }
            if (checkMaxLength != null && checkMaxLength.Count() > 0)
            {
                foreach (var item in checkMaxLength)
                {
                    if (item.Code.Length > 50) item.ErrorMessage += "Mã phòng ban lớn hơn 50 ký tự" + "\r\n";
                    if (item.Name.Length > 200) item.ErrorMessage += "Tên phòng ban lớn hơn 200 ký tự" + "\r\n";
                    if (item.Location.Length > 200) item.ErrorMessage += "Vị trí lớn hơn 200 ký tự" + "\r\n";
                    if (item.ParentName.Length > 200) item.ErrorMessage += "Phòng ban cha lớn hơn 200 ký tự" + "\r\n";
                    if (item.Description.Length > 2000) item.ErrorMessage += "Diễn giải lớn hơn 2000 ký tự" + "\r\n";
                }
            }
            if (checkIsNull != null && checkIsNull.Count() > 0)
            {
                foreach (var item in checkIsNull)
                {
                    if (string.IsNullOrEmpty(item.Code)) item.ErrorMessage += "Mã phòng ban không được để trống\r\n";
                    if (string.IsNullOrEmpty(item.Name)) item.ErrorMessage += "Tên phòng ban không được để trống\r\n";
                }
            }

            if (checkExistedCode != null && checkExistedCode.Count > 0)
            {
                foreach (var item in checkExistedCode)
                {
                    item.ErrorMessage += "Mã phòng ban đã tồn tại\r\n";
                }
            }

            if (checkExistedName != null && checkExistedName.Count > 0)
            {
                foreach (var item in checkExistedName)
                {
                    item.ErrorMessage += "Tên phòng ban đã tồn tại\r\n";
                }
            }

            if (checkDuplicateNameAndParent != null && checkDuplicateNameAndParent.Count > 0)
            {
                foreach (var item in checkDuplicateNameAndParent)
                {
                    item.ErrorMessage += "Phòng ban trùng với phòng ban cha\r\n";
                }
            }
            errorList = param.Where(x => !string.IsNullOrEmpty(x.ErrorMessage)).ToList();
            return errorList;
        }
    }

    public interface IIC_DepartmentLogic
    {
        Task<int> OldDepartmentUpdate(int companyIndex, List<int> IDs, List<IC_Employee_Integrate> employeeIntegrateList);
        List<IC_DepartmentDTO> GetAll(int companyIndex);
        IC_DepartmentDTO CheckExistedOrCreate(IC_DepartmentDTO item, string clientName);
        Task<IC_DepartmentDTO> CheckExistedOrCreateOVN(IC_DepartmentDTO item, List<IC_Department_Integrate_OVN> departmentIntegrates, int row);
        List<IC_DepartmentDTO> CheckExistedOrCreateList(List<IC_DepartmentDTO> listDepartment);
        List<IC_DepartmentDTO> CheckExistedOrCreateListDepartmentFromImport(List<IC_DepartmentDTO> listDepartment, UserInfo user);
        List<IC_DepartmentDTO> CheckExistedListDepartmentFromImport(List<IC_DepartmentDTO> listDepartment, UserInfo user);
        List<HR_TeamInfo> CheckExistedOrCreateList(List<HR_TeamInfo> listDepartment);
        List<HR_GradeInfo> CheckExistedOrCreateList(List<HR_GradeInfo> listDepartment);
        List<IC_DepartmentDTO> GetByNames(List<string> names);
        List<IC_DepartmentImportDTO> ValidationImportDepartment(List<IC_DepartmentImportDTO> param);
        Task<int> OldDepartmentUpdateAVN(int companyIndex, List<Cat_OrgStructure> employeeIntegrateList);
        Task<IC_DepartmentDTO> CheckExistedOrCreateAVN(IC_DepartmentDTO dto, int row);
        List<IC_DepartmentDTO> CheckExistedOrCreateListDepartmentFromImportMAY(List<IC_DepartmentDTO> listDepartment, UserInfo user);
        Task<int> OldDepartmentUpdateStandard(int companyIndex, List<IC_DepartmentIntegrate> employeeIntegrateList);
    }
}
