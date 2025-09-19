using AutoMapper;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using EPAD_Data.Models.IC;
using EPAD_Data.Sync_Entities;
using Newtonsoft.Json;
using System;

namespace EPAD_Common.MapperProfiles
{
    public class IC_Profile : Profile
    {
        public IC_Profile()
        {
            // Student result
            CreateMap<IC_UserMaster, HR_StudentInfoResult>();

            // Employee Result
            CreateMap<IC_WorkingInfo, HR_EmployeeInfoResult>();
            CreateMap<IC_UserMaster, HR_EmployeeInfoResult>();
            CreateMap<IC_Department, HR_EmployeeInfoResult>();
            //.ForMember(d => d.DepartmentName, o => o.MapFrom(s => string.IsNullOrEmpty(s.Name) ? "Không có phòng ban" : s.Name));
            
            CreateMap<IC_Config, IC_ConfigDTO>()
                .ForMember(d => d.IntegrateLogParam, s => s.MapFrom(o => o.CustomField != null ? JsonConvert.DeserializeObject<IntegrateLogParam>(o.CustomField, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }) : new IntegrateLogParam()));
            
            CreateMap<IC_Department, IC_DepartmentImportDTO>();
            CreateMap<IC_DepartmentImportDTO, IC_Department>();

            CreateMap<IC_Department_Integrate_AEON, IC_Department_Integrate_AEON_Dto>();

            CreateMap<Employee_Shift_Integrate, IC_Employee_Shift_Integrate>();

            CreateMap<Cat_OrgStructure, IC_Department_Integrate_AVN>()
                .ForMember(d => d.IntegrateDate, o => o.MapFrom(s => DateTime.Now));

            CreateMap<Att_BusinessTravel, IC_BussinessTravel_Integrate_AVN>()
               .ForMember(d => d.IntegrateDate, o => o.MapFrom(s => DateTime.Now));

            CreateMap<Hre_Profile, IC_Employee_Integrate_AVN>()
              .ForMember(d => d.IntegrateDate, o => o.MapFrom(s => DateTime.Now));

            CreateMap<Att_Roster, IC_EmployeeShift_Integrate_AVN>()
             .ForMember(d => d.IntegrateDate, o => o.MapFrom(s => DateTime.Now));

            CreateMap<Att_OverTimePlan, IC_OverTimePlan_Integrate_AVN>()
             .ForMember(d => d.IntegrateDate, o => o.MapFrom(s => DateTime.Now));

            CreateMap<Cat_Position, IC_Position_Integrate_AVN>()
             .ForMember(d => d.IntegrateDate, o => o.MapFrom(s => DateTime.Now));

            CreateMap<Cat_Shift, IC_Shift_Integrate_AVN>()
            .ForMember(d => d.IntegrateDate, o => o.MapFrom(s => DateTime.Now));

            CreateMap<IC_AttendanceLog, IC_AttendancelogIntegrate>().ReverseMap();

            CreateMap<IC_PlanDock, TruckHistoryModel>().ReverseMap();
        }
    }
}
