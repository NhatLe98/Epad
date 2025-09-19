using EPAD_Common.Extensions;
using EPAD_Common.Services;
using EPAD_Common.Types;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using EPAD_Services.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace EPAD_Services.Impl
{
    public class GC_AccessedGroupService : BaseServices<GC_AccessedGroup, EPAD_Context>, IGC_AccessedGroupService
    {
        public EPAD_Context _DBContext;
        public GC_AccessedGroupService(IServiceProvider serviceProvider, EPAD_Context context) : base(serviceProvider)
        {
            _DBContext = context;
        }
        public async Task<DataGridClass> GetAccessedGroup(int page, int pageSize, string filter, UserInfo user)
        {
            var data = new List<AccessedGroupModel>();
            var accessedGroups = await GetDataByCompanyIndex(user.CompanyIndex);
            if (!string.IsNullOrEmpty(filter))
            {
                accessedGroups = accessedGroups.Where(x => x.Name.Contains(filter) || x.NameInEng.Contains(filter)).ToList();
            }
            var generalAccessRuleList = await _DBContext.GC_Rules_GeneralAccess.AsNoTracking().Where(x
                => accessedGroups.Select(x => x.GeneralAccessRuleIndex).Contains(x.Index)).ToListAsync();
            var generalAccessRuleIndexes = generalAccessRuleList.Select(x => x.Index).ToList();
            var generalAccessRuleGateList = await _DBContext.GC_Rules_GeneralAccess_Gates.AsNoTracking().Where(x
                => generalAccessRuleIndexes.Contains(x.RulesGeneralIndex)).ToListAsync();
            var generalAccessRuleGateIndexList = generalAccessRuleGateList.Select(x => x.GateIndex).ToList();
            var generalAccessRuleLineIndexList = generalAccessRuleGateList.Select(x => x.LineIndexs).ToList();
            var lineIndexList = new List<int>();
            foreach (var item in generalAccessRuleLineIndexList)
            {
                if (!string.IsNullOrWhiteSpace(item) && !item.All(x => x == ','))
                {
                    lineIndexList.AddRange(item.Split(',').Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => int.Parse(x)));
                }
            }
            var gateList = await _DBContext.GC_Gates.AsNoTracking().Where(x => generalAccessRuleGateIndexList.Contains(x.Index)).ToListAsync();
            var lineList = await _DBContext.GC_Lines.AsNoTracking().Where(x => lineIndexList.Contains(x.Index)).ToListAsync();
            var parkingLotList = await _DBContext.GC_Rules_ParkingLot.AsNoTracking().Where(x
                => accessedGroups.Select(x => x.ParkingLotRuleIndex).Contains(x.Index)).ToListAsync();
            foreach (var accessedGroup in accessedGroups)
            {
                var item = new AccessedGroupModel().PopulateWith(accessedGroup);
                item.Index = accessedGroup.Index;
                item.CompanyIndex = user.CompanyIndex;
                item.Description = accessedGroup.Description;
                item.Name = accessedGroup.Name;
                item.NameInEng = accessedGroup?.NameInEng;
                item.IsDriverDefaultGroup = accessedGroup.IsDriverDefaultGroup;
                item.IsGuestDefaultGroup = accessedGroup.IsGuestDefaultGroup;
                item.IsDriverDefaultGroupName = accessedGroup.IsDriverDefaultGroup ? "Có" : "Không";
                item.IsGuestDefaultGroupName = accessedGroup.IsGuestDefaultGroup ? "Có" : "Không";
                item.ParkingLotRuleName = parkingLotList.FirstOrDefault(x => x.Index == accessedGroup.ParkingLotRuleIndex)?.Name;
                item.ParkingLotRuleIndex = accessedGroup.ParkingLotRuleIndex;
                item.GeneralAccessRuleName = generalAccessRuleList.FirstOrDefault(x => x.Index == accessedGroup.GeneralAccessRuleIndex)?.Name;
                item.GeneralAccessRuleIndex = accessedGroup.GeneralAccessRuleIndex;
                item.GeneralAccessRuleGateIndexList = generalAccessRuleGateList.Where(x
                    => x.RulesGeneralIndex == accessedGroup.GeneralAccessRuleIndex)?.Select(x => x.GateIndex).ToList() ?? new List<int>();
                item.GeneralAccessRuleGateNameList = gateList.Where(x
                    => item.GeneralAccessRuleGateIndexList.Contains(x.Index))?.Select(x => x.Name).ToList() ?? new List<string>();
                var itemLineIndexes = generalAccessRuleGateList.Where(x
                    => x.RulesGeneralIndex == accessedGroup.GeneralAccessRuleIndex
                    && !string.IsNullOrWhiteSpace(x.LineIndexs)
                    && !x.LineIndexs.All(y => y == ','))?.Select(x => x.LineIndexs).ToList() ?? new List<string>();
                if (itemLineIndexes.Count > 0)
                {
                    var lineIndexes = new List<int>();
                    foreach (var stringLineIndex in itemLineIndexes)
                    {
                        lineIndexes.AddRange(stringLineIndex.Split(',').Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => int.Parse(x)));
                    }
                    if (lineIndexes.Count > 0)
                    {
                        item.GeneralAccessRuleLineIndexList = lineList.Where(x
                            => lineIndexes.Contains(x.Index))?.Select(x => x.Index).ToList() ?? new List<int>();
                        item.GeneralAccessRuleLineNameList = lineList.Where(x
                            => lineIndexes.Contains(x.Index))?.Select(x => x.Name).ToList() ?? new List<string>();
                    }
                }
                data.Add(item);
            }

            var skip = (page - 1) * pageSize;
            if (skip < 0)
            {
                skip = 0;
            }
            int countTotal = data.Count;
            var dataResult = data.Skip(skip).Take(pageSize).ToList();

            var grid = new DataGridClass(countTotal, dataResult);
            return grid;
        }

        public async Task<bool> AddAreaGroup(GC_AccessedGroup param, UserInfo user)
        {
            var result = true;
            try
            {
                var accessedGroup = new GC_AccessedGroup();
                accessedGroup.Name = param.Name;
                accessedGroup.NameInEng = string.IsNullOrEmpty(param.NameInEng) ? param.Name : param.NameInEng;
                accessedGroup.Description = param.Description;
                accessedGroup.CompanyIndex = user.CompanyIndex;
                accessedGroup.UpdatedDate = DateTime.Now;
                accessedGroup.UpdatedUser = user.UserName;
                accessedGroup.GeneralAccessRuleIndex = param.GeneralAccessRuleIndex;
                accessedGroup.ParkingLotRuleIndex = param.ParkingLotRuleIndex;
                accessedGroup.IsGuestDefaultGroup = param.IsGuestDefaultGroup;
                accessedGroup.IsDriverDefaultGroup = param.IsDriverDefaultGroup;

                await _DBContext.GC_AccessedGroup.AddAsync(accessedGroup);
                await _DBContext.SaveChangesAsync();
                return result;
            }
            catch (Exception ex)
            {
                result = false;
            }
            return result;
        }

        public async Task<List<GC_AccessedGroup>> GetAccessedGroupNormal(int companyIndex)
        {
            return await _DBContext.GC_AccessedGroup.Where(x => x.CompanyIndex == companyIndex && x.IsDriverDefaultGroup == false && x.IsGuestDefaultGroup == false).ToListAsync();
        }

        public async Task<List<GC_AccessedGroup>> GetDataByCompanyIndex(int companyIndex)
        {
            return await _DBContext.GC_AccessedGroup.Where(x => x.CompanyIndex == companyIndex).ToListAsync();
        }
        public GC_AccessedGroup GetDataByIndex(int index)
        {
            return _DBContext.GC_AccessedGroup.FirstOrDefault(x => x.Index == index);
        }

        public async Task<GC_AccessedGroup> GetDataByNameAndCompanyIndex(string name, int companyIndex)
        {
            return await FirstOrDefaultAsync(e => e.Name.ToUpper() == name.ToUpper()
            && e.CompanyIndex == companyIndex);
        }

        public async Task<List<GC_Rules_ParkingLot>> GetDataRulesParkingLotByCompanyIndex(int companyIndex)
        {
            return await _DBContext.GC_Rules_ParkingLot.Where(x => x.CompanyIndex == companyIndex).ToListAsync();
        }

        public async Task<List<GC_Rules_GeneralAccess>> GetDataRulesGeneralAccessByCompanyIndex(int companyIndex)
        {
            return await _DBContext.GC_Rules_GeneralAccess.Where(x => x.CompanyIndex == companyIndex).ToListAsync();
        }

        public bool CheckExistDriverGroup(int companyIndex, int index)
        {
            return _DBContext.GC_AccessedGroup.Any(x => x.CompanyIndex == companyIndex && x.IsDriverDefaultGroup && x.Index != index);
        }

        public bool CheckExistGuestGroup(int companyIndex, int index)
        {
            return _DBContext.GC_AccessedGroup.Any(x => x.CompanyIndex == companyIndex && x.IsGuestDefaultGroup && x.Index != index);
        }
    }
}
