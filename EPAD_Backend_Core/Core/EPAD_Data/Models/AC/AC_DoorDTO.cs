using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models
{
    public class AC_DoorDTO
    {
        public int Index { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string DoorSettingDescription { get; set; }
        public int AreaIndex { get; set; }
        public List<int> AreaIndexes { get; set; }
        public List<int> DoorIndexes { get; set; }
        public List<string> SerialNumberLst { get; set; }
        public List<string> NameDeviceLst { get; set; }
        public int DoorOpenTimezoneUID { get; set; }
        public int Timezone { get; set; }
        public string TimezoneName { get; set; }
        public string AreaName { get; set; }
        public string DeviceListName { get; set; }
        public string DoorOpenTimezoneUIDString { get; set; }
    }
}
