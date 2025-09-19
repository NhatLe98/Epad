using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EPAD_Common.MapperProfiles
{
    public class EntitiesProfile : Profile
    {
        public EntitiesProfile()
        {
            var assembly = typeof(EPAD_Data.EPAD_Context).Assembly;
            var allClass = assembly.GetTypes()
                .Where(e => !string.IsNullOrEmpty(e.Namespace) && e.Namespace.EndsWith(".Entities"))
                .Where(e => e.IsClass && !e.IsAbstract && e.DeclaringType == null)
                .ToArray();

            foreach (var item in allClass)
            {
                CreateMap(item, item);
            }
        }
    }
}
