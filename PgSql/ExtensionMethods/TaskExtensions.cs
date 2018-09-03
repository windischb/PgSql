using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Reflectensions.ExtensionMethods;

namespace doob.PgSql.ExtensionMethods
{
    public static class TaskExtensions
    {
        public static async Task<TResult> CastToTaskOf<TResult>(this Task<object> task)
        {
            var obj = await task;
            if (obj == null)
                return (default);
            return obj.ConvertTo<TResult>();
        }

    }
}
