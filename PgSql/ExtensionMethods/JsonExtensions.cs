using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace doob.PgSql.ExtensionMethods
{
    public static class JsonExtensions
    {

        public static async Task<T> ToObjectAsync<T>(this Task<JObject> jObject)
        {
            var obj = await jObject;
            if (obj == null)
            {
                return default;
            }

            return Converter.Json.ToObject<T>(obj);
        }
    }
}
