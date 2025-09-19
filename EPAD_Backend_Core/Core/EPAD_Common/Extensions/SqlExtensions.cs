using EPAD_Common.Types;
using Microsoft.Data.SqlClient;
using System;
using System.Linq;

namespace EPAD_Common.Extensions
{
    public static class SqlExtensions
    {
        public static SqlCommand CreateSqlCommand(this SqlConnection conn, StoreProcedureInfo spInfo)
        {
            SqlCommand cmd = new SqlCommand(spInfo.Name, conn);

            foreach (var paramInfo in spInfo.Params)
            {
                SqlParameter param = new SqlParameter();
                param.ParameterName = paramInfo.Name;
                param.SqlDbType = paramInfo.SqlDbType;
                param.Value = paramInfo.Value;

                cmd.Parameters.Add(param);
            }
            return cmd;
        }

        public static IQueryable<T> If<T>(
        this IQueryable<T> source,
        bool condition,
        Func<IQueryable<T>, IQueryable<T>> transform)
        {
            return condition ? transform(source) : source;
        }
    }
}
