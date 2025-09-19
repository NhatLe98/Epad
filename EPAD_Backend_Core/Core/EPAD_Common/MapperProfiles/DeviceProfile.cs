using AutoMapper;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using System;
using System.Collections.Generic;

namespace EPAD_Common.MapperProfiles
{
    public class DeviceProfile : Profile
    {
        public DeviceProfile()
        {
            CreateMap<IC_Device, ComboboxItem>()
                .ForMember(d => d.label, o => o.MapFrom(s => string.IsNullOrEmpty(s.AliasName) ? s.SerialNumber : s.AliasName))
                .ForMember(d => d.value, o => o.MapFrom(s => s.SerialNumber));

            CreateMap<IC_Device, ComboboxDeviceItem>()
                .ForMember(d => d.label, o => o.MapFrom(s => string.IsNullOrEmpty(s.AliasName) ? s.SerialNumber : s.AliasName))
                .ForMember(d => d.value, o => o.MapFrom(s => s.SerialNumber))
                .ForMember(d => d.status, o => o.MapFrom(x => GetDeviceStatusBoolean(x)));


            CreateMap<DeviceParam, IC_Device>()
                .ForMember(d => d.UpdatedDate, o => { o.MapFrom(s => DateTime.Now); })
                .ForMember(d => d.UseSDK, o => o.MapFrom(s => s.UseSDK == "true"))
                .ForMember(d => d.UsePush, o => o.MapFrom(s => s.UsePush == "true"));

            CreateMap<DeviceParamInfo, IC_Device>()
                .ForMember(d => d.UpdatedDate, o => { o.MapFrom(s => DateTime.Now); })
                .ForMember(d => d.UseSDK, o => o.MapFrom(s => s.UseSDK == "true"))
                .ForMember(d => d.UsePush, o => o.MapFrom(s => s.UsePush == "true"))
                .ForMember(d => d.LastConnection, o => o.MapFrom(s => DateTime.Now));

            CreateMap<DeviceCapacityParam, IC_Device>();


        }

        public bool GetDeviceStatusBoolean(IC_Device pDevice)
        {
            if (CaculateTime(pDevice.LastConnection, DateTime.Now) < 5)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private double CaculateTime(DateTime? time1, DateTime time2)
        {
            DateTime temp = new DateTime();
            if (time1.HasValue)
            {
                temp = time1.Value;
            }
            else
            {
                temp = new DateTime(2000, 1, 1, 0, 0, 0);
            }
            TimeSpan time = new TimeSpan();
            time = time2 - temp;
            return time.TotalMinutes;
        }
    }
}
