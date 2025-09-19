using EPAD_Data.Models;
using EPAD_Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Common.Extensions
{
    public static class AreaGroupExtension
    {
        public static EMonitoringError GetCheckError(this AreaGroupFullInfo areaGroup, bool isCheckOut = false)
        {
            var name = areaGroup.Name?.ToLower();
            if (string.IsNullOrEmpty(name)) return EMonitoringError.CheckInGateLogNotExist;
            if (name.Contains("gate")) return EMonitoringError.CheckInGateLogNotExist;
            if (name.Contains("lobby")) return isCheckOut ? EMonitoringError.CheckOutLobbyLogNotExist : EMonitoringError.CheckInLobbyLogNotExist;
            return EMonitoringError.CheckInGateLogNotExist;
        }
    }
}
