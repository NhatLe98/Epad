using EPAD_Data.Entities;
using EPAD_Data.Entities.HR;

using Microsoft.EntityFrameworkCore;

namespace EPAD_Data
{
    public partial class EPAD_Context
    {
        public DbSet<HR_User> HR_User { get; set; }
        public DbSet<HR_UserContactInfo> HR_UserContactInfo { get; set; }
        public DbSet<HR_UserType> HR_UserType { get; set; }
        public DbSet<HR_EmployeeInfo> HR_EmployeeInfo { get; set; }
        public DbSet<HR_CustomerInfo> HR_CustomerInfo { get; set; }
        public DbSet<HR_CustomerCard> HR_CustomerCard { get; set; }
        public DbSet<HR_ContractorInfo> HR_ContractorInfo { get; set; }
        public DbSet<HR_StudentInfo> HR_StudentInfo { get; set; }
        public DbSet<HR_ParentInfo> HR_ParentInfo { get; set; }
        public DbSet<HR_NannyInfo> HR_NannyInfo { get; set; }
        public DbSet<HR_TeacherInfo> HR_TeacherInfo { get; set; }
        public DbSet<HR_CardNumberInfo> HR_CardNumberInfo { get; set; }
        public DbSet<HR_ClassInfo> HR_ClassInfo { get; set; }
        public DbSet<HR_PositionInfo> HR_PositionInfo { get; set; }
        public DbSet<HR_StudentClassInfo> HR_StudentClassInfo { get; set; }
        public DbSet<HR_GradeInfo> HR_GradeInfo { get; set; }
        public DbSet<HR_TeamInfo> HR_TeamInfo { get; set; }
        public DbSet<HR_ExcusedAbsent> HR_ExcusedAbsent { get; set; }
        public DbSet<HR_ExcusedAbsentReason> HR_ExcusedAbsentReason { get; set; }
        public DbSet<HR_ExcusedLateEntry> HR_ExcusedLateEntry { get; set; }
        public DbSet<HR_Rules_InOutTime> HR_Rules_InOutTime { get; set; }
        public DbSet<HR_FloorLevel> HR_FloorLevel { get; set; }
        public DbSet<HR_DormActivity> HR_DormActivity { get; set; }
        public DbSet<HR_DormRation> HR_DormRation { get; set; }
        public DbSet<HR_DormLeaveType> HR_DormLeaveType { get; set; }
        public DbSet<HR_DormRoom> HR_DormRoom { get; set; }
        public DbSet<HR_DormRegister> HR_DormRegister { get; set; }
        public DbSet<HR_DormRegister_Activity> HR_DormRegister_Activity { get; set; }
        public DbSet<HR_DormRegister_Ration> HR_DormRegister_Ration { get; set; }
        public DbSet<IC_AttendanceLogClassRoom> IC_AttendanceLogClassRoom { get; set; }
        public DbSet<IC_AttendanceLog> IC_AttendanceLog { get; set; }
        public DbSet<IC_Command> IC_Command { get; set; }
        public DbSet<IC_CommandSystemGroup> IC_CommandSystemGroup { get; set; }
        public DbSet<IC_Company> IC_Company { get; set; }

        public DbSet<IC_Config> IC_Config { get; set; }
        public DbSet<IC_MailHistory> IC_MailHistory { get; set; }
        public DbSet<IC_Department> IC_Department { get; set; }
        public DbSet<IC_DepartmentAndDevice> IC_DepartmentAndDevice { get; set; }
        public DbSet<IC_Device> IC_Device { get; set; }
        public DbSet<IC_EmployeeType> IC_EmployeeType { get; set; }

        //public DbSet<IC_Employee> IC_Employee { get; set; }
        public DbSet<IC_EmployeeTransfer> IC_EmployeeTransfer { get; set; }
        public DbSet<IC_OperationLog> IC_OperationLog { get; set; }


        public DbSet<IC_Service> IC_Service { get; set; }
        public DbSet<IC_ServiceAndDevices> IC_ServiceAndDevices { get; set; }
        public DbSet<IC_SystemCommand> IC_SystemCommand { get; set; }
        public DbSet<IC_UserAccount> IC_UserAccount { get; set; }
        public DbSet<IC_UserMaster> IC_UserMaster { get; set; }
        public DbSet<IC_UserFaceTemplate> IC_UserFaceTemplate { get; set; }
        public DbSet<IC_UserFinger> IC_UserFinger { get; set; }
        public DbSet<IC_UserInfo> IC_UserInfo { get; set; }
        public DbSet<IC_UserPrivilege> IC_UserPrivilege { get; set; }
        public DbSet<IC_PrivilegeDetails> IC_PrivilegeDetails { get; set; }
        public DbSet<IC_PrivilegeDeviceDetails> IC_PrivilegeDeviceDetails { get; set; }
        public DbSet<IC_PrivilegeMachineRealtime> IC_PrivilegeMachineRealtime { get; set; }
        public DbSet<IC_PrivilegeDepartment> IC_PrivilegeDepartment { get; set; }
        public DbSet<IC_GroupDevice> IC_GroupDevice { get; set; }
        public DbSet<IC_GroupDeviceDetails> IC_GroupDeviceDetails { get; set; }
        public DbSet<IC_ConfigByGroupMachine> IC_ConfigByGroupMachine { get; set; }

        public DbSet<IC_UserFaceTemplate_v2> IC_UserFaceTemplate_v2 { get; set; }
        public DbSet<IC_WorkingInfo> IC_WorkingInfo { get; set; }
        public DbSet<IC_AppLicense> IC_AppLicense { get; set; }
        public DbSet<IC_HardwareLicense> IC_HardwareLicense { get; set; }
        public DbSet<IC_AccessToken> IC_AccessToken { get; set; }
        public DbSet<IC_Controller> IC_Controller { get; set; }

        public DbSet<IC_RelayController> IC_RelayController { get; set; }
        public DbSet<IC_RelayControllerChannel> IC_RelayControllerChannel { get; set; }
        public DbSet<IC_Camera> IC_Camera { get; set; }
        public DbSet<IC_Dashboard> IC_Dashboard { get; set; }
        public DbSet<IC_UserNotification> IC_UserNotification { get; set; }

        public virtual DbSet<HR_EmployeeReport> HR_EmployeeReport { get; set; }

        public DbSet<IC_Audit> IC_Audit { get; set; }

        public DbSet<IC_Shift> IC_Shift { get; set; }
        public DbSet<IC_Employee_Shift> IC_Employee_Shift { get; set; }
        public DbSet<IC_Printer> IC_Printer { get; set; }
        public DbSet<IC_HistoryTrackingIntegrate> IC_HistoryTrackingIntegrate { get; set; }
        public DbSet<IC_DepartmemtAEONSync> IC_DepartmemtAEONSync { get; set; }
        public DbSet<IC_AttendancelogIntegrate> IC_AttendancelogIntegrate { get; set; }
        public DbSet<IC_DeviceHistory> IC_DeviceHistory { get; set; }
        public DbSet<IC_EmployeeStopped> IC_EmployeeStopped { get; set; }
        public DbSet<AC_TimeZone> AC_TimeZone { get; set; }
        public DbSet<AC_AccGroup> AC_AccGroup { get; set; }
        public DbSet<AC_AccHoliday> AC_AccHoliday { get; set; }
        public DbSet<AC_Door> AC_Door { get; set; }
        public DbSet<AC_DoorAndDevice> AC_DoorAndDevice { get; set; }
        public DbSet<AC_Area> AC_Area { get; set; }
        public DbSet<AC_AreaLimited> AC_AreaLimited { get; set; }
        public DbSet<AC_AreaLimitedAndDoor> AC_AreaLimitedAndDoor { get; set; }
        public DbSet<AC_AreaAndDoor> AC_AreaAndDoor { get; set; }
        public DbSet<AC_UserMaster> AC_UserMaster { get; set; }

        public DbSet<GC_AreaGroup> GC_AreaGroup { get; set; }
        public DbSet<GC_AreaGroup_GroupDevice> GC_AreaGroup_GroupDevice { get; set; }
        public DbSet<GC_Gates> GC_Gates { get; set; }
        public DbSet<GC_Lines> GC_Lines { get; set; }
        public DbSet<GC_Gates_Lines> GC_Gates_Lines { get; set; }

        public DbSet<GC_AccessedGroup> GC_AccessedGroup { get; set; }
        public DbSet<GC_Rules_General> GC_Rules_General { get; set; }
        public DbSet<GC_Rules_General_Log> GC_Rules_General_Log { get; set; }
        public DbSet<GC_Rules_Warning> GC_Rules_Warning { get; set; }
        public DbSet<GC_Rules_WarningGroup> GC_Rules_WarningGroup { get; set; }
        public DbSet<GC_Rules_Warning_EmailSchedule> GC_Rules_Warning_EmailSchedules { get; set; }
        public DbSet<GC_Rules_Warning_ControllerChannel> GC_Rules_Warning_ControllerChannels { get; set; }
        public DbSet<GC_ParkingLot> GC_ParkingLot { get; set; }
        public DbSet<GC_ParkingLotDetail> GC_ParkingLotDetail { get; set; }
        public DbSet<GC_Rules_General_AreaGroup> GC_Rules_General_AreaGroup { get; set; }
        public DbSet<GC_Rules_ParkingLot> GC_Rules_ParkingLot { get; set; }
        public DbSet<GC_TimeLog> GC_TimeLog { get; set; }
        public DbSet<GC_TimeLog_Image> GC_TimeLog_Image { get; set; }
        public DbSet<GC_TruckDriverLog> GC_TruckDriverLog { get; set; }
        public DbSet<GC_TruckExtraDriverLog> GC_TruckExtraDriverLog { get; set; }
        public DbSet<GC_Rules_GeneralAccess> GC_Rules_GeneralAccess { get; set; }
        public DbSet<GC_Lines_CheckOutDevice> GC_Lines_CheckOutDevice { get; set; }
        public DbSet<GC_Lines_CheckInDevice> GC_Lines_CheckInDevice { get; set; }
        public DbSet<GC_Lines_CheckInRelayController> GC_Lines_CheckInRelayController { get; set; }
        public DbSet<GC_Lines_CheckOutRelayController> GC_Lines_CheckOutRelayController { get; set; }
        public DbSet<GC_Lines_CheckInCamera> GC_Lines_CheckInCamera { get; set; }
        public DbSet<GC_Lines_CheckOutCamera> GC_Lines_CheckOutCamera { get; set; }
        public DbSet<GC_Employee_AccessedGroup> GC_Employee_AccessedGroup { get; set; }
        public DbSet<GC_Department_AccessedGroup> GC_Department_AccessedGroup { get; set; }
        public DbSet<GC_Rules_GeneralAccess_Gates> GC_Rules_GeneralAccess_Gates { get; set; }
        public DbSet<GC_Rules_General_AreaGroup> GC_Rules_General_AreaGroups { get; set; }
        public DbSet<GC_Customer> GC_Customer { get; set; }
        public DbSet<GC_Rules_Customer> GC_Rules_Customer { get; set; }
        public DbSet<GC_Rules_Customer_Gates> GC_Rules_Customer_Gates { get; set; }
        public DbSet<GC_ParkingLotAccessed> GC_ParkingLotAccessed { get; set; }
        public DbSet<GC_EmployeeVehicle> GC_EmployeeVehicle { get; set; }
        public DbSet<GC_CustomerVehicle> GC_CustomerVehicle { get; set; }
        public DbSet<GC_BlackList> GC_BlackList { get; set; }
        public DbSet<AC_AccessedGroup> AC_AccessedGroup { get; set; }
        public DbSet<AC_DepartmentAccessedGroup> AC_DepartmentAccessedGroup { get; set; }
        public DbSet<AC_StateLog> AC_StateLog { get; set; }
        public DbSet<IC_RegisterCard> IC_RegisterCard { get; set; }
        public DbSet<IC_VehicleLog> IC_VehicleLog { get; set; }
        public DbSet<TA_Rules_Global> TA_Rules_Global { get; set; }
        public DbSet<TA_Rules_Shift> TA_Rules_Shift { get; set; }
        public DbSet<TA_Rules_Shift_InOut> TA_Rules_Shift_InOut { get; set; }
        public DbSet<TA_Shift> TA_Shift { get; set; }
        public DbSet<TA_AnnualLeave> TA_AnnualLeave { get; set; }
        public DbSet<TA_AjustAttendanceLog> TA_AjustAttendanceLog { get; set; }
        public DbSet<TA_EmployeeShift> TA_EmployeeShift { get; set; }
        public DbSet<TA_ScheduleFixedByDepartment> TA_ScheduleFixedByDepartment { get; set; }
        public DbSet<TA_ScheduleFixedByEmployee> TA_ScheduleFixedByEmployee { get; set; }
        public DbSet<TA_LeaveRegistration> TA_LeaveRegistration { get; set; }
        public DbSet<TA_LeaveDateType> TA_LeaveDateType { get; set; }
        public DbSet<TA_Holiday> TA_Holiday { get; set; }
        public DbSet<TA_BusinessRegistration> TA_BusinessRegistration { get; set; }
        public DbSet<TA_TimeAttendanceLog> TA_TimeAttendanceLog { get; set; }
        public DbSet<HR_User_Note> HR_User_Note { get; set; }
        public DbSet<TA_AjustAttendanceLogHistory> TA_AjustAttendanceLogHistory { get; set; }
        public DbSet<TA_AjustTimeAttendanceLog> TA_AjustTimeAttendanceLog { get; set; }
        public DbSet<IC_PlanDock> IC_PlanDock { get; set; }
        public DbSet<IC_StatusDock> IC_StatusDock { get; set; }
        public DbSet<IC_LocationOperator> IC_LocationOperator { get; set; }
        public DbSet<HR_StudentsAccordingToTheRegimen> HR_StudentsAccordingToTheRegimen { get; set; }
        public DbSet<HR_Rules_InOutTimeDetail> HR_Rules_InOutTimeDetail { get; set; }
        public DbSet<HR_EmailDeclareGuest> HR_EmailDeclareGuest { get; set; }
        public DbSet<IC_UserAudit> IC_UserAudit { get; set; }
        public DbSet<TA_ListLocation> TA_ListLocation { get; set; }
        public DbSet<TA_LocationByDepartment> TA_LocationByDepartment { get; set; }
        public DbSet<TA_LocationByEmployee> TA_LocationByEmployee { get; set; }
     }
}
