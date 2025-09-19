using System.Net.Http;
using System.Threading.Tasks;
using AutoMapper;
using EPAD_Common.MapperProfiles;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace EPAD_Common.Extensions
{
    public static class Extensions
    {
        public static async Task<T> ReadAsAsync<T>(this HttpContent pContent)
        {
            var json = await pContent.ReadAsStringAsync();
            var rs = JsonConvert.DeserializeObject<T>(json);
            return rs;
        }

        public static string GetCurrentUserName(this Microsoft.AspNetCore.Http.IHttpContextAccessor context)
        {
            return context.HttpContext.Session.GetString("UserName");
        }

        public static int? GetCompanyIndex(this Microsoft.AspNetCore.Http.IHttpContextAccessor context)
        {
            return context.HttpContext.Session.GetInt32("CompanyIndex");
        }

        public static int? GetPrivilegeIndex(this IHttpContextAccessor context)
        {
            return context.HttpContext.Session.GetInt32("PrivilegeIndex");
        }
    }

    public static class IMappingOperationOptionsExtensions
    {
        public static void ExcludeMembers(this AutoMapper.IMappingOperationOptions options, params string[] members)
        {
            options.Items[MappingProfile.MemberExclusionKey] = members;
        }

        public static TDestination Map<TSource, TDestination>(this TDestination destination, TSource source, IMapper _mapper)
        {
            return _mapper.Map(source, destination);
        }
    }

    public static class ObjectExtensions
    {
        public static T CopyToNewObject<T>(T obj)
        {
            var str = JsonConvert.SerializeObject(obj);
            var ret = JsonConvert.DeserializeObject<T>(str);
            return ret;
        }
        static public object GetValObjDy(this object obj, string propertyName)
        {
            return obj.GetType().GetProperty(propertyName).GetValue(obj, null);
        }
    }
}
