using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EPAD_Common.Extensions
{
    public static class EnumExtension
    {
        public static List<object> GetListEnum<T>() where T : Enum
        {
            var result = new List<object>();
            foreach (var enumValue in Enum.GetValues(typeof(T)))
            {
                var index = (int)enumValue;
                var value = Enum.GetName(typeof(T), enumValue);
                result.Add(new { Index = index, Value = value });
            }
            return result;
        }
    }
}
