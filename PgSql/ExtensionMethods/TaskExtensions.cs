using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using doob.Reflectensions;

namespace doob.PgSql.ExtensionMethods
{
    public static class TaskExtensions
    {
        public static async Task<TResult> CastToTaskOf<TResult>(this Task<object> task)
        {
            var obj = await task;
            if (obj == null)
                return (default);
            return obj.CastTo<TResult>();
        }

        public static async Task<IEnumerable<TResult>> SelectAsync<TSource, TResult>(this Task<IEnumerable<TSource>> source, Func<TSource, TResult> selector) {
            var ie = await source;
            return ie.Select(selector);
        }

        public static async Task<T> FirstOrDefaultAsync<T>(this Task<IEnumerable<T>> source)
        {
            var ie = await source;
            return ie.FirstOrDefault();
        }

        public static async Task<List<T>> ToListAsync<T>(this Task<IEnumerable<T>> source)
        {
            var ie = await source;
            return ie.ToList();
        }
    }
}
