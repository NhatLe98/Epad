using System;
using System.Collections.Generic;
using System.Reflection;

namespace EPAD_Data.Models
{
    public class CommonUtils
    {
        public static Object ConvertObject(Object source, Object destination)
        {
            PropertyInfo[] propsSource = source.GetType().GetProperties();
            PropertyInfo[] propsDestination = destination.GetType().GetProperties();
            foreach (var propSou in propsSource)
            {
                foreach (var propDes in propsDestination)
                {
                    if (propSou.Name == propDes.Name)
                    {
                        propDes.SetValue(destination, propSou.GetValue(source));
                    }
                }
            }

            return destination;
        }

        public static DateTime GetMaxToday(){
            return DateTime.Today.AddDays(1).AddTicks(-1);
        }

        public static IEnumerable<List<T>> SplitList<T>(List<T> locations, int nSize = 200)
        {
            for (int i = 0; i < locations.Count; i += nSize)
            {
                yield return locations.GetRange(i, Math.Min(nSize, locations.Count - i));
            }
        }

    }
}
