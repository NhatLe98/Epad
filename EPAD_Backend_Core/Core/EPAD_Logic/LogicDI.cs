using EPAD_Logic.SendMail;
using Microsoft.Extensions.DependencyInjection;

namespace EPAD_Logic
{
    public static class LogicDI
    {
        public static IServiceCollection AddLogic(this IServiceCollection services)
        {
            services.AddEmailProvider();

            services.AddScoped<IIC_EmployeeTransferLogic, IC_EmployeeTransferLogic>();
            services.AddScoped<IIC_CommandLogic, IC_CommandLogic>();
            services.AddScoped<IIC_WorkingInfoLogic, IC_WorkingInfoLogic>();
            services.AddScoped<IIC_UserNotificationLogic, IC_UserNotificationLogic>();
            services.AddScoped<IIC_UserInfoLogic, IC_UserInfoLogic>();
            services.AddScoped<IIC_ServiceAndDeviceLogic, IC_ServiceAndDeviceLogic>();
            services.AddScoped<IIC_ServiceLogic, IC_ServiceLogic>();
            services.AddScoped<IIC_DeviceLogic, IC_DeviceLogic>();
            services.AddScoped<IIC_GroupDeviceDetailLogic, IC_GroupDeviceDetailLogic>();
            services.AddScoped<IIC_DepartmentAndDeviceLogic, IC_DepartmentAndDeviceLogic>();
            services.AddScoped<IIC_ServiceAndDeviceLogic, IC_ServiceAndDeviceLogic>();
            services.AddScoped<IIC_EmployeeLogic, IC_EmployeeLogic>();
            services.AddScoped<IIC_SystemCommandLogic, IC_SystemCommandLogic>();
            services.AddScoped<IIC_DepartmentLogic, IC_DepartmentLogic>();
            services.AddScoped<IIC_CommandSystemGroupLogic, IC_CommandSystemGroupLogic>();
            services.AddScoped<IIC_ConfigLogic, IC_ConfigLogic>();
            services.AddScoped<IIC_UserMasterLogic, IC_UserMasterLogic>();
            services.AddScoped<IIC_AuditLogic, IC_AuditLogic>();
            services.AddScoped<IIC_SignalRLogic, IC_SignalRLogic>();
            services.AddScoped<IIC_ScheduleAutoHostedLogic, IC_ScheduleAutoHostedLogic>();
            services.AddScoped<IIC_CachingLogic, IC_CachingLogic>();
            services.AddScoped<IIC_HistoryTrackingIntegrateLogic, IC_HistoryTrackingIntegrateLogic>();
            // HR Serivce here
            services.AddScoped<IHR_EmployeeLogic, HR_EmployeeLogic>();
            services.AddScoped<IHR_EmployeeInfoLogic, HR_EmployeeInfoLogic>();
            services.AddScoped<IHR_DepartmentLogic, HR_DepartmentLogic>();
            services.AddScoped<IHR_WorkingInfoLogic, HR_WorkingInfoLogic>();
            services.AddScoped<IIC_ModbusReplayControllerLogic, IC_ModbusReplayControllerLogic>();
            services.AddScoped<IIC_ClientTCPControllerLogic, IC_ClientTCPControllerLogic>();
            services.AddScoped<IHR_PositionInfoLogic, HR_PositionInfoLogic>();
            // Customer intergrate employee service
            services.AddScoped<IIC_Employee_IntegrateLogic, IC_Employee_IntegrateLogic>();
            services.AddScoped<IEmployee_Shift_IntegrateLogic, Employee_Shift_IntegrateLogic>();
            // Integrate tracking
            services.AddScoped<IIC_IntegrateLogic, IC_IntegrateLogic>();
            return services;
        }
    }
}
