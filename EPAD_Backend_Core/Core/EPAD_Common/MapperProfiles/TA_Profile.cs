using AutoMapper;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Common.MapperProfiles
{
    public class TA_Profile : Profile
    {
        public TA_Profile()
        {
            // Student result
            CreateMap<TA_AjustAttendanceLogDTO, TA_AjustAttendanceLogInsertDTO>();
        }
    }
}
