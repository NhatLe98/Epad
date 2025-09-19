using EPAD_Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models
{
    public class AreaGroupFullInfo : GC_AreaGroup
    {
        public List<int> DeviceGroups { get; set; }
    }

    public class AreaGroupModel : GC_AreaGroup
    {
        public List<int> GroupDevice { get; set; }
        public string GroupDeviceString { get; set; }
        public string CreatedDateString { get; set; }
        public string UpdatedDateString { get; set; }
        public string AreaGroupParentName { get; set; }
        public List<string> MachineList { get; set; }
    }

    public class AreaGroupRequestModel
    {
        public GC_AreaGroup AreaGroup { get; set; }
        public List<int> GroupDevice { get; set; }
    }
}
