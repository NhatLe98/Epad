using AutoMapper;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Common.MapperProfiles
{
    public class HR_2_ICProfile : Profile
    {
        public HR_2_ICProfile()
        {
            CreateMap<HR_Department, IC_Department>()
                .ForMember(d => d.Index, o => o.MapFrom(s => (int)s.Index))
                .ForMember(d => d.ParentIndex, o => o.MapFrom(s => (int?)s.ParentIndex))
            .ReverseMap()
                .ForMember(d => d.Index, o => o.MapFrom(s => (long)s.Index))
                .ForMember(d => d.ParentIndex, o => o.MapFrom(s => (long?)s.ParentIndex));


            //CreateMap<HR_Employee, IC_Employee>()
            //    .ForMember(d => d.FullName, o => o.MapFrom(s => $"{s.LastName} {s.MidName} {s.FirstName}"))
            //    .ForMember(d => d.Gender, o => o.MapFrom(s => s.Gender == true ? 1 : 2));

            CreateMap<HR_Employee, HR_User>()
                .ForMember(d => d.FullName, o => o.MapFrom(s => $"{s.LastName} {s.MidName} {s.FirstName}"))
                .ForMember(d => d.Gender, o => o.MapFrom(s => s.Gender == true ? (short)GenderEnum.Male : (short)GenderEnum.Female));

            CreateMap<HR_Employee, HR_UserResult>()
                .ForMember(d => d.Avatar, o => o.MapFrom(s => Convert.ToBase64String(s.Image)))
                .ForMember(d => d.FullName, o => o.MapFrom(s => $"{s.LastName} {s.MidName} {s.FirstName}"))
                .ForMember(d => d.Gender, o => o.MapFrom(s => s.Gender == true ? (short)GenderEnum.Male : (short)GenderEnum.Female));

            CreateMap<HR_WorkingInfo, IC_WorkingInfo>()
                .ForMember(d => d.Index, o => o.MapFrom(s => (int)s.Index));
        }
    }
}
