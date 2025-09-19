using AutoMapper;
using EPAD_Backend_Core.Protos;
using EPAD_Data.Models;
using Google.Protobuf;
using System;

namespace EPAD_Backend_Core.Grpc.MapperProfile
{
    public class GrpcProfile : Profile
    {
        public GrpcProfile()
        {
            CreateMap<DateTime, Google.Protobuf.WellKnownTypes.Timestamp>().ConvertUsing(x => Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.SpecifyKind(x, DateTimeKind.Utc)));
            CreateMap<Google.Protobuf.WellKnownTypes.Timestamp, DateTime>().ConvertUsing(x => x.ToDateTime());
            CreateMap<byte[], ByteString>().ConstructUsing(x => ByteArrayToByteString(x));
            CreateMap<HR_EmployeeInfoResult, EmployeeInfoResult>().ReverseMap();
        }

        private ByteString ByteArrayToByteString(byte[] arg)
        {
            return arg != null ? ByteString.CopyFrom(arg) : null;
        }
    }
}
