﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using doob.PgSql.TypeMapping;
using Newtonsoft.Json.Linq;
using Npgsql;
using NpgsqlTypes;

namespace doob.PgSql.ExtensionMethods
{
    public static class PgSqlParameterExtensions
    {
        public static NpgsqlParameter ToNpgsqlParameter(this PgSqlParameter param)
        {
            try
            {
                if (param.Value == null)
                    return new NpgsqlParameter(param.UniqueId, DBNull.Value);

                //if (Value.GetType().GetTypeInfo().IsEnum)
                //    Value = Json.ToJson(Value);


                NpgsqlDbType type = NpgsqlDbType.Text;
                bool findType = true;


                if (param.Column?.DotNetType?.IsEnum == true)
                {
                    switch (param.Value)
                    {
                        case int i:
                        {
                            var val = (Enum) Enum.ToObject(param.Column.DotNetType, i);
                            return new NpgsqlParameter(param.UniqueId, type) {Value = val.GetName()};
                        }
                    }
                }

                //var typeInfo = param.Value.GetType().GetTypeInfo();
                //if (typeInfo.IsEnum)
                //{
                //    type = NpgsqlDbType.Text;
                //    findType = false;
                //}
                    

                if(findType)
                    if(!Enum.TryParse(param.OverrideType, true, out type))
                    {
                        type = param.Column.GetNpgSqlDbType();// PgSqlTypeManager.GetNpgsqlDbType(param.Column.DotNetType);
                    }

                if (type == NpgsqlDbType.Json || type == NpgsqlDbType.Jsonb)
                {
                    var valueType = PgSqlTypeManager.GetNpgsqlDbType(param.Value.GetType());
                    if(valueType != NpgsqlDbType.Json && valueType != NpgsqlDbType.Jsonb)
                        type = NpgsqlDbType.Text; //PgSqlTypeManager.GetNpgsqlDbType(param.Value.GetType());
                }
                

                object _value = null;
                switch (type)
                {
                    case NpgsqlDbType.Enum:
                    {
                        //TODO: Error: When specifying NpgsqlDbType.Enum, EnumType must be specified as well
                        type = NpgsqlDbType.Text;
                        _value = JSON.ToJson(param.Value);
                        break;
                    }
                    case NpgsqlDbType.Jsonb:
                    {
                        _value = JSON.ToJson(param.Value);
                        break;
                    }
                    case NpgsqlDbType.Array | NpgsqlDbType.Jsonb:
                    {
                        var objAr = new List<object>();
                        if (param.Value is string)
                        {
                            objAr.Add(param.Value.ToString());
                        }
                        else
                        {
                            var arr = param.Value as IEnumerable;
                            if (arr != null)
                                foreach (var o in arr)
                                {
                                    objAr.Add(JSON.ToJson(o));
                                }
                        }

                        _value = objAr;
                        break;
                    }

                    case NpgsqlDbType.Array | NpgsqlDbType.Uuid:
                    {
                        var json = JSON.ToJson(param.Value);
                        _value = JSON.ToObject<List<Guid>>(json);

                        break;
                    }
                    case NpgsqlDbType.Uuid:
                    {
                        _value = new Guid(param.Value.ToString());
                        break;
                    }
                    default:
                    {
                        _value = param.Value;
                        break;
                    }
                }


                return new NpgsqlParameter(param.UniqueId, type) { Value = _value };
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}
