using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;


namespace EPAD_Common.Extensions
{
    public static class GenericExtensions
    {
        public static To PopulateWith<To, From>(this To to, From from)
        {
            Func<PropertyInfo, From, bool> predicate = (p, s) => typeof(From).HasProperty(p.Name);

            foreach (var propertyInfo in typeof(To).GetProperties().Where(prop => prop.CanRead && prop.CanWrite))
            {
                if (predicate(propertyInfo, from))
                {
                    try
                    {
                        var dummy = TryGetProperty(from, propertyInfo.Name);
                        var val = dummy.GetValue(from);
                        propertyInfo.SetValue(to, val);
                    }
                    catch
                    {
                    }
                    // var val = from.TryGetValue<>(propertyInfo.Name);
                }
            }

            return to;
        }

        public static To PopulateIgnoreEmptyWith<To, From>(this To to, From from)
        {
            Func<PropertyInfo, From, bool> predicate = (p, s) => typeof(From).HasProperty(p.Name);

            foreach (var propertyInfo in typeof(To).GetProperties().Where(prop => prop.CanRead && prop.CanWrite))
            {
                if (predicate(propertyInfo, from))
                {
                    try
                    {
                        var dummy = TryGetProperty(from, propertyInfo.Name);
                        var valDest = dummy.GetValue(from);
                        if (valDest == null) continue;
                        if (propertyInfo.PropertyType == typeof(string) && string.IsNullOrEmpty(valDest.ToString())) continue;

                        propertyInfo.SetValue(to, valDest);
                    }
                    catch
                    {
                    }

                }
            }

            return to;
        }

        public static To PopulateWithoutNullValue<To, From>(this To to, From from)
        {
            Func<PropertyInfo, From, bool> predicate = (p, s) => typeof(From).HasProperty(p.Name);

            foreach (var propertyInfo in typeof(To).GetProperties().Where(prop => prop.CanRead && prop.CanWrite))
            {
                if (predicate(propertyInfo, from))
                {
                    try
                    {
                        var dummy = TryGetProperty(from, propertyInfo.Name);
                        var valDest = dummy.GetValue(from);
                        propertyInfo.SetValue(to, valDest);
                    }
                    catch
                    {
                    }
                }
            }

            return to;
        }
        public static bool HasProperty(this Type type, string propName)
        {
            return GetProperty(type, propName) != null;
        }
        public static PropertyInfo TryGetProperty<T>(this T obj, string propName)
        {
            return GetProperty(obj.GetType(), propName);
        }
        static PropertyInfo GetProperty(Type type, string propName)
        {
            var dummy = type is Type ? type : type.GetType();
            return dummy.GetProperties().FirstOrDefault(n => n.Name.Equals(propName));
        }
        public static U TryGetValue<U>(this object obj, string propName)
        {
            try
            {
                var dummy = TryGetProperty(obj, propName);
                var val = dummy.GetValue(obj);
                return (U)val;
            }
            catch
            {
                return default(U);
            }
        }

        public static bool TrySetValue<T>(this T obj, string propName, object value)
        {
            try
            {
                var dummy = obj.GetType().GetProperties().FirstOrDefault(e => e.Name.Equals(propName));
                if (dummy != null)
                {
                    dummy.SetValue(obj, value);
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }


        public static T SetDefaultString<T>(this T obj)
        {
            var props = obj.GetType().GetProperties().Where(n => n.CanWrite && n.PropertyType == typeof(string));
            if (props.Count() > 0)
            {
                props.ToList().ForEach(p =>
                {
                    var value = obj.TryGetValue<string>(p.Name);
                    if (string.IsNullOrEmpty(value))
                        value = "";

                    obj.TrySetValue(p.Name, value.Trim());
                });
            }
            return obj;
        }
    }
}
