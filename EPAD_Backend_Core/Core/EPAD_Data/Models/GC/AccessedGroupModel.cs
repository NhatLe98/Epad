using EPAD_Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models
{
    public class AccessedGroupModel
    {
        public int Index { get; set; }

        public int GeneralAccessRuleIndex { get; set; }
        public string GeneralAccessRuleName { get; set; }
        public List<int> GeneralAccessRuleGateIndexList { get; set; }
        public List<string> GeneralAccessRuleGateNameList { get; set; }
        public List<int> GeneralAccessRuleLineIndexList { get; set; }
        public List<string> GeneralAccessRuleLineNameList { get; set; }
        public int ParkingLotRuleIndex { get; set; }
        public string ParkingLotRuleName { get; set; }
        public string Description { get; set; }
        public string Name { get; set; }
        public string NameInEng { get; set; }
        public bool IsGuestDefaultGroup { get; set; }
        public bool IsDriverDefaultGroup { get; set; }
        public string IsGuestDefaultGroupName { get; set; }
        public string IsDriverDefaultGroupName { get; set; }
        public int CompanyIndex { get; set; }
    }


    //public class AreaGroupRequestModel
    //{
    //    public GC_AreaGroup AreaGroup { get; set; }
    //    public List<int> GroupDevice { get; set; }
    //}
}
