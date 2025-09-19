namespace EPAD_Data
{
    public class GlobalParams
    {
        public const string __PASSWORD_SALT = "";
        public static int ROWS_NUMBER_IN_PAGE = 40;
        public static int ROWS_NUMBER_IN_REALTIME_PAGE = 100;
        public static int ROWS_NUMBER_SYSTEM_LOG = 40;
        public static int COMMAND_NUMBER_RETURN_SDK = 5;
        public static int COMMAND_NUMBER_RETURN_PUSH = 1;
        public static int MAX_LENGTH_ATID = 9;
        public static string UpdatedUser = "MonitoringAuto";


        public class ValueFunction
        {
            public const string DownloadAllLog = "DownloadAllLog";
            public const string DownloadLogFromToTime = "DownloadLogFromToTime";
            public const string DeleteAllLog = "DeleteAllLog";
            public const string DeleteLogFromToTime = "DeleteLogFromToTime";
            public const string GetDeviceInfo = "GetDeviceInfo";
            public const string RestartDevice = "RestartDevice";
            public const string RestartService = "RESTART_SERVICE";
            public const string DownloadAllUser = "DownloadAllUser";
            public const string DownloadUserById = "DownloadUserById";
            public const string DeleteAllUser = "DeleteAllUser";
            public const string DeleteAllFingerPrint = "DeleteAllFingerPrint";
            public const string DeleteUserById = "DeleteUserById";
            public const string UploadUsers = "UploadUsers";
            public const string RESTART_SERVICE = "RESTART_SERVICE";
            public const string DownloadAllUserMaster = "DownloadAllUserMaster";
            public const string DownloadUserMasterById = "DownloadUserMasterById";
            public const string SetTimeDevice = "SetTimeDevice";
            public const string UploadTimeZone = "UploadTimeZone";
            public const string UploadAccGroup = "UploadAccGroup";
            public const string UploadAccHoliday = "UploadAccHoliday";
            public const string UploadACUsers = "UploadACUsers";
            public const string UnlockDoor = "UnlockDoor";
            public const string DeleteACUser = "DeleteACUser";
            public const string UploadACUsersFromExcel = "UploadACUsersFromExcel";
            public const string DeleteAllHoliday = "DeleteAllHoliday";
            public const string SetDoorSetting = "SetDoorSetting";
            public const string DeleteTimezoneById = "DeleteTimezoneById";
            
        }

        public class ServiceType {
            public const string PUSHInterfaceService = "PUSHInterfaceService";
            public const string SDKInterfaceService = "SDKInterfaceService";
        }

        public class DevicePrivilege {
            public const int PUSHStandardRole = 0;
            public const int PUSHAdminRole = 14;
            public const int SDKStandardRole = 0;
            public const int SDKAdminRole = 3;
            public const int SDKUserRegisterRole = 4; // Quyền đăng ký
        }
    }
}