using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EPAD_Data
{
    public enum FormRole
    {
        Full, ReadOnly, None, Edit
    }

    public enum CommandStatus
    {
        UnExecute, Executing, Success, Failed, DeleteManual
    }

    public enum CommandAction
    {
        DownloadAllLog, DownloadLogFromToTime, UploadUsers, GetDeviceInfo,
        DownloadAllUser, DownloadAllUserMaster, DownloadUserById, DownloadUserMasterById, SetTimeDevice,
        DeleteAllUser, RestartDevice, DeleteAllFingerPrint, DeleteUserById, DeleteAllLog, DeleteLogFromToTime, RESTART_SERVICE, CallIntegratedLog,
        UploadTimeZone, UploadAccGroup, UploadAccHoliday, UploadACUsers, UnlockDoor, DeleteACUser, UploadACUsersFromExcel, DeleteAllHoliday, SetDoorSetting, DeleteTimezoneById, DownloadStateLog
    }

    public enum RelayType
    {
        ModbusTCP, ClientTCP
    }

    public enum CommandName
    {
        UploadUsers, UploadCustomers, UploadParents, UploadStudents
    }

    public enum RemoveStoppedWorkingEmployeesType
    {
        NotUse, Day, Week, Month
    }

    public enum ShowStoppedWorkingEmployeesType
    {
        NotUse, Day, Week, Month
    }

    public enum ConfigAuto
    {
        DOWNLOAD_LOG,
        DELETE_LOG,
        START_MACHINE,
        DOWNLOAD_USER,
        ADD_OR_DELETE_USER,
        EMPLOYEE_INTEGRATE,
        EMPLOYEE_SHIFT_INTEGRATE,
        INTEGRATE_LOG_REALTIME,
        INTEGRATE_LOG,
        FULL_CAPACITY,
        TIME_SYNC,
        DELETE_SYSTEM_COMMAND,
        GENERAL_SYSTEM_CONFIG,
        ECMS_DEFAULT_MEAL_CARD_DEPARTMENT,
        MANAGE_STOPPED_WORKING_EMPLOYEES_DATA,
        INTEGRATE_EMPLOYEE_BUSINESS_TRAVEL,
        SEND_MAIL_WHEN_DEVICE_OFFLINE,
        EMPLOYEE_INTEGRATE_TO_DATABASE,
        LOG_INTEGRATE_TO_DATABASE,
        DOWNLOAD_STATE_LOG,
        RE_PROCESSING_REGISTERCARD,
        DOWNLOAD_PARKING_LOG,
        CREATE_DEPARTMENT_IMPORT_EMPLOYEE,
        INTEGRATE_INFO_TO_OFFLINE,
        AUTO_DELETE_BLACKLIST,
        CONFIG_EMAIL_ALLOW_IMPORT_GGSHEET
    }

    public enum ConfigAutoGroupDevice
    {
        DOWNLOAD_LOG,
        DELETE_LOG,
        START_MACHINE,
        DOWNLOAD_USER
    }

    public enum Privilege
    {
        None, ReadOnly, Full, Edit
    }

    public enum DescriptionType
    {
        Security
    }

    public enum AuditType
    {
        None,
        Added,
        Modified,
        Deleted,
        Login,
        Logout,
        RunIntegrate, // Chạy tích hợp nhân sự
        ChangePassword,
        AssignPrivilegeUserGroup,
        ChangeDepartment,
        ApproveChangeDepartment,
        IntegrateLogRealTime,
        UploadUsers,
        UpdateUserPrivilege,
        DownloadAllUser,
        DownloadAllUserMaster,
        DownloadUserById,
        DownloadUserMasterById,
        DeleteAllUser,
        DeleteAllFingerPrint,
        GetDeviceInfo,
        RestartDevice,
        RestartService,
        SetDeviceTime,
        DownloadLogFromToTime,
        DownloadAllLog,
        UploadTimeZone,
        UploadAccHoliday,
        UploadAccGroup,
        UnlockDoor,
        DeleteACUser,
        UploadACUsersFromExcel,
        DeleteAllHoliday,
        UploadACUsers,
        SetDoorSetting,
        DeleteTimezoneById
    }

    public enum AuditStatus
    {
        Processing,
        Completed,
        Error,
        Unexecuted,
    }

    public enum AuthenMode
    {
        FullAccessRight, Password, CardNumber, Finger, Face, Veins, Finger_Face, CardNumber_Finger
    }

    public enum UserSyncAuthMode
    {
        Finger,
        CardNumber,
        Password,
        Face,
    }

    public enum TargetDownloadUser
    {
        AllUser,
        NewUsers,
    }

    public enum TransferStatus
    {
        Pendding, Approve, Reject
    }

    public enum SendMailAction
    {
        None, Always, WhenError
    }

    public enum CameraType
    {
        Picture, ANPR
    }

    public enum EmployeeType
    {
        Employee = 1,
        Guest = 2,
        Student = 3,
        Parents = 4,
        Nanny = 5,
        Contractor = 6,
        Teacher = 7,
        Driver = 8,
    }

    public enum DepartmentType
    {
        EmployeeAndContractor = 10
    }

    public enum GenderEnum
    {
        Female = 0,
        Male = 1,
        Other = 2
    }

    public enum WorkingStatus
    {
        Pending = 1,
        Approved = 2,
        Rejected = 3
    }

    public enum RowStatus
    {
        Pending,
        Active,
        Inactive
    }

    public enum InOutMode
    {
        Input,
        Output
    }

    public enum SignalStatus
    {
        Output = 0,
        Input = 1,
        InOutput = 2
    }

    public enum ClientName
    {
        VSTAR,
        AEON,
        AVN,
        MAY,
        OVN,
        TOYOTA,
        PSV,
        MONDELEZ
    }

    public enum ProducerEnum
    {
        Czkem = 1,
        FkAttend = 2,
        FR05 = 3,
        HIK = 4
    }

    public class ValueResult
    {
        public static string Success = CommandStatusValue.Success.ToString();
        public static string Failure = CommandStatusValue.Failed.ToString();
    }

    public enum CommandStatusValue
    {
        UnExecute, Executing, Success, Failed
    }

    public enum UpdatedUser
    {
        SYSTEM_AUTO = 1,
        AutoIntegrateEmployee = 2,
        IntegrateEmployee = 3,
        UserSystem = 4
    }

    public enum GroupName
    {
        TransferEmployee = 1,
        DeleteEmployeeStopped = 2,
        DeleteCustomer = 3,
        UploadUsers = 4,
        DownloadAllLog = 5,
        DownloadAllUserMaster = 6,
        RestartDevice = 7,
        SetTimeDevice = 8,
        DeleteUserById = 9,
        DeleteAllLog = 10,
        DeleteLogFromToTime = 11,
        DownloadLogFromToTime = 12,
        UploadACUsers = 13,
        DeleteACUser = 14,
        UploadACUsersFromExcel = 15,
        DeleteAllHoliday = 16,
        DOWNLOAD_STATE_LOG = 17
    }

    public enum DeviceStatus
    {
        Undefined,
        Input,
        Output,
        MiddleOut,
        MiddleInt
    }

    public enum AVNEmployeeStatus
    {
        D, // Xóa
        N, // Tạo mới
        U // Cập nhật
    }

    public enum DurationTypeValue
    {
        E_FIRSTHALFSHIFT,
        E_LASTHALFSHIFT,
        E_FULLSHIFT
    }

    public enum AeonDepartmentValue
    {
        User = 1,
        Department = 2
    }

    public class DepartmentKeyVstar
    {
        public const string RegularDepartment = "Van Phong";
        public const string OfficeDepartment = "Chuyen Mon";
        public const string AllClassInfo = "HS TH";
        public const string Parents = "Parents";
        public const string HSTH = "HS TH";
        public const string HSTHCS = "HS THCS";
        public const string HSTHPT = "HS THPT";
        public const string Visitors = "Visitors";

    }
    public enum EmployeeTypeVstar
    {
        T, //Teacher
        E, //Employee
        P, //Parents
        S, //Student
        V  //Visitors
    }

    public class NationalityName
    {
        public const string VN = "Viet Nam";
        public const string PH = "Philippin";
    }

    public enum NationalityKeyVstar
    {
        VN, //Viet Name
        PH, //Philippin
        NA
    }

    public class GradeName
    {
        public const string TH = "Tieu hoc";
        public const string THCS = "THCS";
        public const string THPT = "THPT";
    }

    public enum DeviceOnOffStatus
    {
        Online,
        Offline
    }

    public enum EMonitoringError
    {
        NoError = 1000,
        CardNotRegister = 1,
        EmployeeInBlackList = 2,
        EmployeeNotRegisterAccessGroup,
        CustomerInfoNotExistOrExpired,
        NotFoundCustomerRule,
        NotFoundRule,
        AreaNotAllowed,
        CheckInLogNotExist,
        EmployeeInLeaveTime,
        EmployeeOutLeaveTime,
        EmployeeInMissionTime,
        EmployeeOutMissionTime,
        EmployeeInStoppedWorkingTime,
        NotFoundBikeInParking,
        CheckInGateLogNotExist,
        CheckInLobbyLogNotExist,
        CheckOutLobbyLogNotExist,
        EmployeeRegisteredFirstHaftLeave,
        EmployeeRegisteredLastHaftLeave,
        NotInBreakOutTime,
        NotAllowBreakInOut,
        VerifyPlateInAndVerifyPlateOut,
        VerifyPlateAndRegisterPlateDifferent,
        NotFoundLogAreaGroupParent,
        ExceedNumberOfAccess,
        EmployeeNotInAccessGroup,
        NotRegisteredUseParkingYet,
        VehicleInfoOfEmployeeNotExistOrExpired,
        ExceedParkingDayLimit,
        ExceedMaxMinuteAllowOutsideInWorkingTime,
        CannotLoadScheduleData,
        EmployeeNotAssignSchedule,
        ShiftNotExist,
        ExceedCheckOutTime,
        OutOfWorkingTime,
        CheckInOutLobbySuccess,
        WorkingOverTime,
        CheckInNotYet,
        CheckOutNotYet,
        ExceedCheckInTime
    }

    public enum LogType
    {
        Walker, Customer, Parking
    }

    public enum ObjectAccessType
    {
        Employee,
        Customer, Student
    }

    public enum ApproveStatus
    {
        Waiting, Approved
    }
    public class GCSEnum
    {
        public static string GetErrorByStatus(EMonitoringError monitoringError)
        {
            if (monitoringError == EMonitoringError.NoError)
            {
                return "";
            }
            string error = monitoringError.ToString();
            return error;

        }
    }

    public enum SoftwareType
    {
        Standard,
        Office,
        File
    }

    public enum LeaveType
    {
        Mission = 1,
        Leaveday = 2
    }

    public enum ACOperation
    {
        Sync,
        Delete
    }

    public enum ViewMode
    {
        All,
        LatestData
    }

    public enum AjustAttendanceLogOperator
    {
        NoProcess,
        Add,
        Update,
        Delete,
        Import
    }

    public enum TimeAttendanceLog
    {
        X = 1,
        P = 2,
        P2 = 3,
        CT = 4,
        V = 5,
        KL = 6,
        L = 7
    }

    public enum ParkingType
    {
        Standard,
        Vinparking,
        Lovad
    }

    public enum OptionFilterLog
    {
        TotalHour = 1,
        TotalHourNormal = 2,
        OverTime = 3,
        TotalHourNormalAndOverTime = 4,
        CheckInLate = 5,
        CheckOutEarly = 6,
        HourByLogInOut = 7
    }
}
