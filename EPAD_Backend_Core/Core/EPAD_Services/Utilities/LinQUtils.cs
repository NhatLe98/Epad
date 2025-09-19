using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;

namespace EPAD_Services.Utilities
{

    public static class LinQUtils
    {
        /// <summary>
        ///     just select what you need.
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="source"></param>
        /// <param name="fields">Example: id,name,price (split by comma (,))</param>
        /// <returns></returns>
        public static IQueryable<TResult> GetOnDynamicSelector<TResult>(this IQueryable source, string fields)
        {
            if (string.IsNullOrEmpty(fields))
            {
                fields = string.Join(',', typeof(TResult).GetProperties().Select(s => s.Name).ToArray());
            }
            return source.Select<TResult>(ToDynamicSelector(fields));
        }

        private static string ToDynamicSelector(string selectors)
        {
            return @"new {" + selectors.Replace("-", string.Empty).Replace("_", string.Empty) + "}";
        }
    }
}
