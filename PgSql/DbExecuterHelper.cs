using System;
using System.Collections;
using System.Collections.Generic;
using doob.PgSql.ExtensionMethods;
using doob.PgSql.TypeMapping;
using Newtonsoft.Json.Linq;
using Npgsql.Schema;
using NpgsqlTypes;

namespace doob.PgSql
{
    internal static class DbExecuterHelper
    {
        public static object ConvertFromDB(object data, NpgsqlDbColumn column)
        {

            if (data == null)
                return null;

            if (data == DBNull.Value)
                return null;

            var postgresType = column.DataTypeName;
            if (column.DataType == typeof(Array))
                postgresType = $"{postgresType}[]";

            var type = PgSqlTypeManager.GetDotNetType(postgresType);
            var npgsqlType = PgSqlTypeManager.GetNpgsqlDbType(postgresType);


            object newObject;
            switch (npgsqlType)
            {
                case NpgsqlDbType.Json:
                case NpgsqlDbType.Jsonb:
                    {
                        if (type.IsListType())
                        {
                            var list = new List<object>();
                            var enumerable = data as IEnumerable;
                            foreach (var o in enumerable)
                            {
                                var jt = JToken.Parse(o.ToString());
                                var jo = jt.ToBasicDotNetObject();
                                list.Add(jo);
                            }
                            newObject = list;
                        }
                        else
                        {
                            var jt = JToken.Parse(data.ToString());
                            newObject = jt.ToBasicDotNetObject();
                        }
                        break;
                    }
                case NpgsqlDbType.Json | NpgsqlDbType.Array:
                case NpgsqlDbType.Jsonb | NpgsqlDbType.Array:
                    {
                        var list = new List<object>();
                        var enumerable = data as IEnumerable;
                        foreach (var o in enumerable)
                        {
                            var jt = JToken.Parse(o.ToString());
                            var jo = jt.ToBasicDotNetObject();
                            list.Add(jo);
                        }
                        newObject = list;
                        break;
                    }
                default:
                    newObject = data;
                    break;
            }

            return newObject;
        }
    }
}
