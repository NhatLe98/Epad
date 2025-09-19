using EPAD_Data.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models
{
    public class EmployeeAccessRule
    {
        public string EmployeeATID { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int? AccessedGroupIndex { get; set; }
        public int? ParkingLotIndex { get; set; }
        public int GeneralAccessRuleIndex { get; set; }
        /*  các qui định về thời gian vào ra cho phép  */
        public bool CheckInByShift { get; set; }
        public DateTime? CheckInTime { get; set; }
        public int MaxEarlyCheckInMinute { get; set; }
        public int MaxLateCheckInMinute { get; set; }
        public bool CheckOutByShift { get; set; }
        public DateTime? CheckOutTime { get; set; }
        public int MaxEarlyCheckOutMinute { get; set; }
        public int MaxLateCheckOutMinute { get; set; }
        public bool AllowEarlyOutLateInMission { get; set; }
        public bool AllowFreeInAndOutInTimeRange { get; set; }
        public int MissionMaxEarlyCheckOutMinute { get; set; }
        public int MissionMaxLateCheckInMinute { get; set; }
        public bool AdjustByLateInEarlyOut { get; set; }
        public DateTime? BeginLastHaftTime { get; set; }
        public DateTime? EndFirstHaftTime { get; set; }
        /*  các qui định liên quan đến ra giữa giờ có đăng ký  */
        public bool AllowInLeaveDay { get; set; }
        public bool AllowInMission { get; set; }
        public bool AllowInBreakTime { get; set; }
        /*  các qui định liên quan đến ra giữa giờ không đăng ký */
        public bool AllowCheckOutInWorkingTime { get; set; }
        public string AllowCheckOutInWorkingTimeRange { get; set; }
        public int MaxMinuteAllowOutsideInWorkingTime { get; set; }
        /*  các qui định cấm vào ra  */
        public bool DenyInLeaveWholeDay { get; set; }
        public bool DenyInMissionWholeDay { get; set; }
        public bool DenyInStoppedWorkingInfo { get; set; }
        /*  các quy định nhà xe*/
        public bool UseTimeLimitParking { get; set; }
        public int LimitDayNumber { get; set; }
        public bool UseCardDependent { get; set; }
        public bool UseRequiredParkingLotAccessed { get; set; }
        public bool UseRequiredEmployeeVehicle { get; set; }


        public bool CheckLogByAreaGroup { get; set; }
        public bool CheckLogByShift { get; set; }
        public List<GC_Rules_General_AreaGroup> AreaGroups { get; set; }

    }

    public class AllowCheckOutInWorkingTimeRangeModel 
    {
        [JsonProperty("FromTime")]
        public DateTime FromTime { get; set; }

        [JsonProperty("ToTime")]
        public DateTime ToTime { get; set; }

        [JsonProperty("Error")]
        public string Error { get; set; }
    }

    public class AllowCheckOutInWorkingTimeRangeObject : AllowCheckOutInWorkingTimeRangeModel
    { 
        public int Index { get; set; }
    }
}
