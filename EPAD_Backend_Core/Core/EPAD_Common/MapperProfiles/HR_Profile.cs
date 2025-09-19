using AutoMapper;
using EPAD_Common.Extensions;
using EPAD_Common.Utility;
using EPAD_Data.Entities;
using EPAD_Data.Entities.HR;
using EPAD_Data.Models;
using EPAD_Data.Models.HR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EPAD_Common.MapperProfiles
{
    public class HR_Profile : Profile
    {
        public HR_Profile()
        {
            CreateMap<HR_EmployeeInfoResult, HR_UserLookupInfo>().ReverseMap();
            CreateMap<HR_EmployeeInfoResult, HR_EmployeeInfo>().ReverseMap();
            CreateMap<HR_EmployeeInfoResult, HR_CardNumberInfo>().ReverseMap();
            CreateMap<HR_EmployeeInfoResult, IC_WorkingInfo>().ReverseMap();
            CreateMap<HR_EmployeeInfoResult, IC_UserMaster>().ReverseMap();
            CreateMap<HR_EmployeeInfoResult, IC_UserMasterDTO>().ReverseMap();
            CreateMap<HR_EmployeeInfoResult, HR_User>()
                .ForMember(d => d.Avatar, o => o.MapFrom(s => !string.IsNullOrWhiteSpace(s.Avatar) ? Convert.FromBase64String(s.Avatar) : null))
                .AfterMap((src, dest) => dest.Avatar = dest.Avatar != null && dest.Avatar.Length > 0 ? dest.Avatar : null)
                .ForMember(d => d.YearOfBirth, o => o.MapFrom(s => StringHelper.GetDateOfBirth("Year", s.BirthDay)))
                .ForMember(d => d.MonthOfBirth, o => o.MapFrom(s => StringHelper.GetDateOfBirth("Month", s.BirthDay)))
                .ForMember(d => d.DayOfBirth, o => o.MapFrom(s => StringHelper.GetDateOfBirth("Day", s.BirthDay)))
                .ReverseMap()
                .ForMember(d => d.Avatar, o => o.MapFrom(s => (s.Avatar != null && s.Avatar.Length > 0) ? Convert.ToBase64String(s.Avatar) : null))
                .ForMember(d => d.BirthDay, o => o.MapFrom(s => new DateTime(
                    s.YearOfBirth != null ? s.YearOfBirth.Value != 0 ? s.YearOfBirth.Value : 1900 : 1900,
                    s.MonthOfBirth != null ? s.MonthOfBirth.Value != 0 ? s.MonthOfBirth.Value : 1 : 1,
                    s.DayOfBirth != null ? s.DayOfBirth.Value != 0 ? s.DayOfBirth.Value : 1 : 1).ToString("yyyy-MM-dd")));

            CreateMap<HR_EmployeeInfoResult, HR_Employee>()
                .ForMember(d => d.Avatar, o => o.MapFrom(s => Convert.FromBase64String(s.Avatar)))
                .ForMember(d => d.YearOfBirth, o => o.MapFrom(s => StringHelper.GetDateOfBirth("Year", s.BirthDay)))
                .ForMember(d => d.MonthOfBirth, o => o.MapFrom(s => StringHelper.GetDateOfBirth("Month", s.BirthDay)))
                .ForMember(d => d.DayOfBirth, o => o.MapFrom(s => StringHelper.GetDateOfBirth("Day", s.BirthDay)))
                .ReverseMap()
                .ForMember(d => d.Avatar, o => o.MapFrom(s => s.Avatar))
                .ForMember(d => d.BirthDay, o => o.MapFrom(s => new DateTime(
                    s.YearOfBirth != null ? s.YearOfBirth.Value != 0 ? s.YearOfBirth.Value : 1900 : 1900,
                    s.MonthOfBirth != null ? s.MonthOfBirth.Value != 0 ? s.MonthOfBirth.Value : 1 : 1,
                    s.DayOfBirth != null ? s.DayOfBirth.Value != 0 ? s.DayOfBirth.Value : 1 : 1).ToString("yyyy-MM-dd")));
            // Customer Info
            CreateMap<HR_CustomerInfoResult, HR_CustomerInfo>()
                .ForMember(d => d.StudentOfParent, o => o.MapFrom(s => (s.StudentOfParent != null && s.StudentOfParent.Count > 0)
                    ? string.Join(",", s.StudentOfParent)
                    : string.Empty))
                .ReverseMap()
                .ForMember(d => d.IsVIPString, o => o.MapFrom(s => s.IsVIP == true ? "Khách VIP" : ""))
                .ForMember(d => d.AllowPhone, o => o.MapFrom(s => s.IsAllowPhone == true ? "Yes" : "No"))
                .ForMember(d => d.StartTime, o => o.MapFrom(s => s.FromTime))
                .ForMember(d => d.EndTime, o => o.MapFrom(s => s.ToTime))
                .ForMember(d => d.StudentOfParent, o => o.MapFrom(s => (!string.IsNullOrWhiteSpace(s.StudentOfParent)
                    && !s.StudentOfParent.All(x => x == ',')) ? s.StudentOfParent.SplitByComma() : new List<string>()));
            CreateMap<HR_CustomerInfoResult, IC_UserMaster>().ReverseMap();
            CreateMap<HR_CustomerInfoResult, HR_ContractorInfo>().ReverseMap();
            CreateMap<HR_CustomerInfoResult, HR_CardNumberInfo>().ReverseMap();
            CreateMap<HR_CustomerInfoResult, IC_UserMasterDTO>().ReverseMap();
            CreateMap<HR_CustomerInfoResult, IC_UserMaster>().ReverseMap();
            CreateMap<HR_CustomerInfoResult, HR_User>()
                .ForMember(d => d.Avatar, o => o.MapFrom(s => !string.IsNullOrWhiteSpace(s.Avatar) ? Convert.FromBase64String(s.Avatar) : null))
                .AfterMap((src, dest) => dest.Avatar = dest.Avatar != null && dest.Avatar.Length > 0 ? dest.Avatar : null)
                .ForMember(d => d.YearOfBirth, o => o.MapFrom(s => StringHelper.GetDateOfBirth("Year", s.BirthDay)))
                .ForMember(d => d.MonthOfBirth, o => o.MapFrom(s => StringHelper.GetDateOfBirth("Month", s.BirthDay)))
                .ForMember(d => d.DayOfBirth, o => o.MapFrom(s => StringHelper.GetDateOfBirth("Day", s.BirthDay)))
                .ReverseMap()
                .ForMember(d => d.Avatar, o => o.MapFrom(s => (s.Avatar != null && s.Avatar.Length > 0) ? Convert.ToBase64String(s.Avatar) : null))
                .ForMember(d => d.BirthDay, o => o.MapFrom(s => new DateTime(
                    s.YearOfBirth != null ? s.YearOfBirth.Value != 0 ? s.YearOfBirth.Value : 1900 : 1900,
                    s.MonthOfBirth != null ? s.MonthOfBirth.Value != 0 ? s.MonthOfBirth.Value : 1 : 1,
                    s.DayOfBirth != null ? s.DayOfBirth.Value != 0 ? s.DayOfBirth.Value : 1 : 1).ToString("yyyy-MM-dd")));
            CreateMap<HR_CustomerInfoResult, IC_WorkingInfo>()
                .ForMember(d => d.FromDate, o => o.MapFrom(s => s.FromTime != null ? s.FromTime : DateTime.Now))
                .ForMember(d => d.ToDate, o => o.MapFrom(s => s.ToTime != null ? s.ToTime : null))
                .ReverseMap();
            CreateMap<HR_CustomerInfoResult, IC_Department>()
                .ForMember(d => d.Name, o => o.MapFrom(x => x.DepartmentName))
                .ReverseMap()
                .ForMember(d => d.DepartmentName, o => o.MapFrom(x => x.Name))
                .ForMember(d => d.DepartmentIndex, o => o.MapFrom(x => x.Index));

            CreateMap<HR_CustomerInfoResult, HR_WorkingInfo>()
                .ReverseMap();
            CreateMap<HR_StudentInfoResult, IC_UserMasterDTO>().ReverseMap();
            CreateMap<HR_StudentInfoResult, HR_StudentInfo>().ReverseMap();
            CreateMap<HR_StudentInfoResult, HR_CardNumberInfo>().ReverseMap();
            CreateMap<HR_StudentInfoResult, HR_User>()
                .ForMember(d => d.Avatar, o => o.MapFrom(s => !string.IsNullOrWhiteSpace(s.Avatar) ? Convert.FromBase64String(s.Avatar) : null))
                .AfterMap((src, dest) => dest.Avatar = dest.Avatar != null && dest.Avatar.Length > 0 ? dest.Avatar : null)
                .ReverseMap()
                .ForMember(d => d.Avatar, o => o.MapFrom(s => (s.Avatar != null && s.Avatar.Length > 0) ? Convert.ToBase64String(s.Avatar) : null));


            CreateMap<HR_ParentInfoResult, IC_UserMasterDTO>().ReverseMap();
            CreateMap<HR_ParentInfoResult, IC_UserMaster>().ReverseMap();
            CreateMap<HR_ParentInfoResult, HR_CardNumberInfo>().ReverseMap();
            CreateMap<HR_ParentInfoResult, HR_ParentInfo>().ReverseMap();
            CreateMap<HR_ParentInfoResult, HR_User>()
                .ForMember(d => d.Avatar, o => o.MapFrom(s => !string.IsNullOrWhiteSpace(s.Avatar) ? Convert.FromBase64String(s.Avatar) : null))
                .AfterMap((src, dest) => dest.Avatar = dest.Avatar != null && dest.Avatar.Length > 0 ? dest.Avatar : null)
                .ReverseMap()
                .ForMember(d => d.Avatar, o => o.MapFrom(s => (s.Avatar != null && s.Avatar.Length > 0) ? Convert.ToBase64String(s.Avatar) : null))
                .ForMember(d => d.BirthDay, o => o.MapFrom(s => new DateTime(
                    s.YearOfBirth != null ? s.YearOfBirth.Value != 0 ? s.YearOfBirth.Value : 1900 : 1900,
                    s.MonthOfBirth != null ? s.MonthOfBirth.Value != 0 ? s.MonthOfBirth.Value : 1 : 1,
                    s.DayOfBirth != null ? s.DayOfBirth.Value != 0 ? s.DayOfBirth.Value : 1 : 1).ToString("yyyy-MM-dd")));

            CreateMap<HR_ContractorInfoResult, HR_User>()
               .ForMember(d => d.Avatar, o => o.MapFrom(s => !string.IsNullOrWhiteSpace(s.Avatar) ? Convert.FromBase64String(s.Avatar) : null))
               .AfterMap((src, dest) => dest.Avatar = dest.Avatar != null && dest.Avatar.Length > 0 ? dest.Avatar : null)
               .ReverseMap()
               .ForMember(d => d.Avatar, o => o.MapFrom(s => (s.Avatar != null && s.Avatar.Length > 0) ? Convert.ToBase64String(s.Avatar) : null));

            CreateMap<HR_TeacherInfoResult, HR_User>()
               .ForMember(d => d.Avatar, o => o.MapFrom(s => !string.IsNullOrWhiteSpace(s.Avatar) ? Convert.FromBase64String(s.Avatar) : null))
               .AfterMap((src, dest) => dest.Avatar = dest.Avatar != null && dest.Avatar.Length > 0 ? dest.Avatar : null)
               .ReverseMap()
               .ForMember(d => d.Avatar, o => o.MapFrom(s => (s.Avatar != null && s.Avatar.Length > 0) ? Convert.ToBase64String(s.Avatar) : null));

            CreateMap<HR_NannyInfoResult, HR_User>()
               .ForMember(d => d.Avatar, o => o.MapFrom(s => !string.IsNullOrWhiteSpace(s.Avatar) ? Convert.FromBase64String(s.Avatar) : null))
               .AfterMap((src, dest) => dest.Avatar = dest.Avatar != null && dest.Avatar.Length > 0 ? dest.Avatar : null)
               .ReverseMap()
               .ForMember(d => d.Avatar, o => o.MapFrom(s => (s.Avatar != null && s.Avatar.Length > 0) ? Convert.ToBase64String(s.Avatar) : null));

            CreateMap<HR_UserResult, HR_User>()
               .ForMember(d => d.Avatar, o => o.MapFrom(s => !string.IsNullOrWhiteSpace(s.Avatar) ? Convert.FromBase64String(s.Avatar) : null))
               .AfterMap((src, dest) => dest.Avatar = dest.Avatar != null && dest.Avatar.Length > 0 ? dest.Avatar : null)
               .ReverseMap()
               .ForMember(d => d.Avatar, o => o.MapFrom(s => (s.Avatar != null && s.Avatar.Length > 0) ? Convert.ToBase64String(s.Avatar) : null));

            CreateMap<HR_ContractorInfoResult, HR_ContractorInfo>().ReverseMap();

            CreateMap<HR_PositionInfoResult, HR_PositionInfo>().ReverseMap();

            CreateMap<HR_CardNumberInfoResult, HR_CardNumberInfo>().ReverseMap();

            CreateMap<HR_Employee, HR_CardNumberInfoResult>().ReverseMap();

            CreateMap<HR_User, HR_UserLookupInfo>()
                .ForMember(d => d.BirthDay, o => o.MapFrom(s => new DateTime(
                    s.YearOfBirth != null ? s.YearOfBirth.Value != 0 ? s.YearOfBirth.Value : 1900 : 1900,
                    s.MonthOfBirth != null ? s.MonthOfBirth.Value != 0 ? s.MonthOfBirth.Value : 1 : 1,
                    s.DayOfBirth != null ? s.DayOfBirth.Value != 0 ? s.DayOfBirth.Value : 1 : 1).ToString("yyyy-MM-dd")))
                .ForMember(d => d.Gender, o => o.MapFrom(s => s.Gender != null ? s.Gender : 2))
                .ReverseMap()
                .ForMember(d => d.YearOfBirth, o => o.MapFrom(s => DateTime.Parse(s.BirthDay == "" ? "1900-01-01" : s.BirthDay).Year))
                .ForMember(d => d.MonthOfBirth, o => o.MapFrom(s => DateTime.Parse(s.BirthDay == "" ? "1900-01-01" : s.BirthDay).Month))
                .ForMember(d => d.DayOfBirth, o => o.MapFrom(s => DateTime.Parse(s.BirthDay == "" ? "1900-01-01" : s.BirthDay).Day));

            CreateMap<HR_StudentInfoResult, IC_UserMaster>().ReverseMap();
            CreateMap<HR_TeacherInfoResult, HR_TeacherInfo>().ReverseMap();
            CreateMap<HR_NannyInfoResult, HR_NannyInfo>().ReverseMap();

            CreateMap<HR_DriverInfoDTO, IC_PlanDock>()
                   .ForMember(d => d.Avatar, o => o.MapFrom(s => !string.IsNullOrWhiteSpace(s.Avatar) ? Convert.FromBase64String(s.Avatar) : null))
                   .AfterMap((src, dest) => dest.Avatar = dest.Avatar != null && dest.Avatar.Length > 0 ? dest.Avatar : null).ReverseMap()
             .ForMember(d => d.BirthDay, o => o.MapFrom(s => s.BirthDay != null ? s.BirthDay : null))
             .ForMember(d => d.Avatar, o => o.MapFrom(s => (s.Avatar != null && s.Avatar.Length > 0) ? Convert.ToBase64String(s.Avatar) : null));
            CreateMap<HR_DriveInfoImportParam, IC_PlanDock>().ReverseMap();
        }
    }
}
