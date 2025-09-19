using System;
using System.Collections.Generic;

namespace EPAD_Common.Extensions
{
    public static class DictionaryExtensions
    {
        public static Dictionary<TKey, TValue> ToDictionarySafe<TSource, TKey, TValue>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            Func<TSource, TValue> valueSelector,
            IEqualityComparer<TKey> comparer = null)
        {
            var result = new Dictionary<TKey, TValue>();
            if (source == null) return result;

            foreach (TSource item in source)
            {
                result[keySelector(item)] = valueSelector(item);
            }
            return result;
        }

        public static Dictionary<TKey, TSource> ToDictionarySafe<TSource, TKey>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector)
        {
            var result = new Dictionary<TKey, TSource>();
            if (source == null) return result;

            foreach (TSource item in source)
            {
                result[keySelector(item)] = item;
            }
            return result;
        }
    }
}
