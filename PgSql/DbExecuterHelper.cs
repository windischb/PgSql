//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Data;
//using doob.PgSql.ExtensionMethods;
//using doob.PgSql.TypeMapping;
//using Newtonsoft.Json.Linq;
//using Npgsql.Schema;
//using NpgsqlTypes;

//namespace doob.PgSql
//{
//    internal static class DbExecuterHelper
//    {
//        public static JToken ConvertFromDB(object data, NpgsqlDbColumn column)
//        {

//            if (data == null)
//                return null;

//            if (data == DBNull.Value)
//                return null;

//            var postgresType = column.DataTypeName;
//            if (column.DataType == typeof(Array))
//                postgresType = $"{postgresType}[]";

//            var type = PgSqlTypeManager_old.GetDotNetType(postgresType);
//            var npgsqlType = PgSqlTypeManager_old.GetNpgsqlDbType(postgresType);


//            JToken newObject = null;
//            switch (npgsqlType)
//            {
//                case NpgsqlDbType.Json:
//                case NpgsqlDbType.Jsonb:
//                {
//                    if (type.IsListType())
//                    {
//                        var list = new JArray();
//                        var enumerable = data as IEnumerable;
//                        foreach (var o in enumerable)
//                        {
//                            var jt = JToken.Parse(o.ToString());
//                            //var jo = jt.ToBasicDotNetObject();
//                            list.Add(jt);
//                        }
//                        newObject = list;
//                    }
//                    else
//                    {
//                        newObject = JToken.Parse(data.ToString());
//                    }
//                    break;
//                }
//                case NpgsqlDbType.Json | NpgsqlDbType.Array:
//                case NpgsqlDbType.Jsonb | NpgsqlDbType.Array:
//                {
//                    var list = new JArray();
//                    var enumerable = data as IEnumerable;
//                    foreach (var o in enumerable)
//                    {
//                        var jt = JToken.Parse(o.ToString());

//                        list.Add(jt);
//                    }
//                    newObject = list;
//                    break;
//                }
//                default:
//                    newObject = JToken.FromObject(data);
//                    break;
//            }

//            return newObject;
//        }

//        public static JToken ConvertFromDB(object data, DataColumn column)
//        {
//            if (data == null)
//                return null;

//            if (data == DBNull.Value)
//                return null;

//            var postgresType = column.DataType.Name;
//            if (column.DataType == typeof(Array))
//                postgresType = $"{postgresType}[]";

//            var type = PgSqlTypeManager_old.GetDotNetType(postgresType);
//            var npgsqlType = PgSqlTypeManager_old.GetNpgsqlDbType(postgresType);


//            JToken newObject = null;
//            switch (npgsqlType)
//            {
//                case NpgsqlDbType.Json:
//                case NpgsqlDbType.Jsonb:
//                {
//                    if (type.IsListType())
//                    {
//                        var list = new JArray();
//                        var enumerable = data as IEnumerable;
//                        foreach (var o in enumerable)
//                        {
//                            var jt = JToken.Parse(o.ToString());
//                            //var jo = jt.ToBasicDotNetObject();
//                            list.Add(jt);
//                        }
//                        newObject = list;
//                    }
//                    else
//                    {
//                        newObject = JToken.Parse(data.ToString());
//                    }
//                    break;
//                }
//                case NpgsqlDbType.Json | NpgsqlDbType.Array:
//                case NpgsqlDbType.Jsonb | NpgsqlDbType.Array:
//                {
//                    var list = new JArray();
//                    var enumerable = data as IEnumerable;
//                    foreach (var o in enumerable)
//                    {
//                        var jt = JToken.Parse(o.ToString());

//                        list.Add(jt);
//                    }
//                    newObject = list;
//                    break;
//                }
//                default:
//                    newObject = JToken.FromObject(data);
//                    break;
//            }
//            return newObject;
//        }
//    }
//}
