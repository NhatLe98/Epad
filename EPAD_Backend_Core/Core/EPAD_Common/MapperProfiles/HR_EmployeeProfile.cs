using AutoMapper;
using EPAD_Data.Entities;
using EPAD_Data.Entities.HR;
using EPAD_Data.Models;
using EPAD_Data.Models.HR;
using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Common.MapperProfiles
{
    public class HR_EmployeeProfile : Profile
    {
        public HR_EmployeeProfile()
        {
            CreateMap<HR_EmployeeInfo, HR_EmployeeInfoResult>().ReverseMap();
            CreateMap<HR_CardNumberInfo, HR_EmployeeInfoResult>().ReverseMap();
            CreateMap<HR_ClassInfo, HR_ClassInfoResult>().ReverseMap();
            CreateMap<HR_PositionInfo, HR_PositionInfoResult>().ReverseMap();
            CreateMap<HR_CardNumberInfo, HR_ClassInfoResult>().ReverseMap();
            CreateMap<HR_CardNumberInfo, HR_ParentInfoResult>().ReverseMap();
            CreateMap<HR_CardNumberInfo, HR_CardNumberInfoResult>()
            .ForMember(d => d.Status, o => o.MapFrom(s=>s.IsActive == true ? "Hoạt động":"Không hoạt động"))
            .ReverseMap();
        }
    }
}
