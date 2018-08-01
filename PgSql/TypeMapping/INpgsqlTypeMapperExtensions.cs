﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text;
using doob.PgSql.CustomTypes;
using doob.PgSql.ExtensionMethods;
using doob.Reflectensions;
using Newtonsoft.Json.Linq;
using Npgsql;
using Npgsql.PostgresTypes;
using Npgsql.TypeMapping;
using NpgsqlTypes;

namespace doob.PgSql.TypeMapping
{
    public static class INpgsqlTypeMapperExtensions
    {

        public static NpgsqlDbType GetNpgsqlDbType(this INpgsqlTypeMapper npgsqlTypeMapper, Type clrType)
        {
            if (clrType == null)
                return NpgsqlDbType.Unknown;

            if (clrType.IsNullableType())
                clrType = clrType.GetInnerTypeFromNullable();

            if (clrType == typeof(PgSqlLTree))
                return NpgsqlDbType.Text;

            if (clrType.GetTypeInfo().IsEnum)
                return NpgsqlDbType.Text;

            if (clrType == typeof(JValue))
                return NpgsqlDbType.Jsonb;

            if (clrType == typeof(JToken))
                return NpgsqlDbType.Jsonb;

            if (clrType == typeof(JObject))
                return NpgsqlDbType.Jsonb;

            if (clrType.IsDictionaryType())
                return NpgsqlDbType.Jsonb;

            var foundType = npgsqlTypeMapper.Mappings.FirstOrDefault(map => map.ClrTypes.Contains(clrType))
                ?.NpgsqlDbType;

            if (foundType.HasValue)
                return foundType.Value;

            if (clrType.IsArray)
            {
                if (clrType == typeof(byte[]))
                    return NpgsqlDbType.Bytea;

                return NpgsqlDbType.Array | npgsqlTypeMapper.GetNpgsqlDbType(clrType.GetElementType());
            }

            if (clrType.IsListType())
            {
                if (clrType == typeof(byte[]))
                    return NpgsqlDbType.Bytea;

                Type t = clrType;
                if (clrType.GetTypeInfo().BaseType != null && clrType.GetTypeInfo().BaseType.IsListType())
                    t = clrType.GetTypeInfo().BaseType;

                var genericType = t.GetGenericArguments().FirstOrDefault();

                return NpgsqlDbType.Array | npgsqlTypeMapper.GetNpgsqlDbType(genericType);
            }

            if (clrType.GetTypeInfo().IsGenericType && clrType.GetGenericTypeDefinition() == typeof(NpgsqlRange<>))
                return NpgsqlDbType.Range | npgsqlTypeMapper.GetNpgsqlDbType(clrType.GetGenericArguments()[0]);

            if (clrType == typeof(DBNull))
                return NpgsqlDbType.Text;

            return NpgsqlDbType.Jsonb;
        }
        public static NpgsqlDbType GetNpgsqlDbType(this INpgsqlTypeMapper npgsqlTypeMapper, string pgTypeName)
        {
            var isArray = pgTypeName.EndsWith("[]");
            if (isArray) {
                pgTypeName = pgTypeName.Substring(0, pgTypeName.Length - 2);
            }

           
            NpgsqlDbType npgsqlDbType;

            switch (pgTypeName)
            {
                case "json":
                {
                    npgsqlDbType = NpgsqlDbType.Json;
                    break;
                }
                case "jsonb":
                {
                    npgsqlDbType = NpgsqlDbType.Jsonb;
                    break;
                }
                default:
                {
                    npgsqlDbType = npgsqlTypeMapper.Mappings.FirstOrDefault(map => map.PgTypeName.Equals(pgTypeName, StringComparison.CurrentCultureIgnoreCase))?.NpgsqlDbType ?? NpgsqlDbType.Unknown;
                    break;
                }
            }

            if (isArray)
                npgsqlDbType = npgsqlDbType | NpgsqlDbType.Array;

            return npgsqlDbType;
        }


        public static string GetPostgresName(this INpgsqlTypeMapper npgsqlTypeMapper, NpgsqlDbType npgsqlDbType)
        {
            bool isArray = npgsqlDbType.HasFlag(NpgsqlDbType.Array);

            if (isArray) {
                npgsqlDbType = npgsqlDbType & ~NpgsqlDbType.Array;
            }

            var pgName = npgsqlTypeMapper.Mappings.FirstOrDefault(map => map.NpgsqlDbType == npgsqlDbType)?.PgTypeName;
            if (isArray)
                pgName = $"{pgName}[]";

            return pgName;
        }

        
        public static Type GetDotNetType(this INpgsqlTypeMapper npgsqlTypeMapper, string pgTypeName)
        {
            var npgsqlDbType = npgsqlTypeMapper.GetNpgsqlDbType(pgTypeName);
            return npgsqlTypeMapper.GetDotNetType(npgsqlDbType);
        }

        public static string GetPostgresName(this INpgsqlTypeMapper npgsqlTypeMapper, Type clrType)
        {
            if (clrType == null)
                return null;

            var npgsqlDbType = npgsqlTypeMapper.GetNpgsqlDbType(clrType);
            return npgsqlTypeMapper.GetPostgresName(npgsqlDbType);

        }


        public static Type GetDotNetType(this INpgsqlTypeMapper npgsqlTypeMapper, NpgsqlDbType npgsqlDbType)
        {

            Type type = null;

            var isArray = npgsqlDbType.HasFlag(NpgsqlDbType.Array);
            var isRange = npgsqlDbType.HasFlag(NpgsqlDbType.Range);

            npgsqlDbType = npgsqlDbType & ~NpgsqlDbType.Array;
            npgsqlDbType = npgsqlDbType & ~NpgsqlDbType.Range;


            var possibleTypes = npgsqlTypeMapper.Mappings.Where(map =>
                map.NpgsqlDbType == npgsqlDbType && map.ClrTypes != null && map.ClrTypes.Any()).ToList();

            if (possibleTypes.Any()) {
                type = possibleTypes.OrderBy(map => map.ClrTypes.Length).First().ClrTypes.First();
            }


            if (type == null) {
                switch (npgsqlDbType)
                {
                    case NpgsqlDbType.Bigint:
                        type = typeof(long);
                        break;
                    case NpgsqlDbType.Double:
                        type = typeof(double);
                        break;
                    case NpgsqlDbType.Integer:
                        type = typeof(int);
                        break;
                    case NpgsqlDbType.Numeric:
                        type = typeof(decimal);
                        break;
                    case NpgsqlDbType.Real:
                        type = typeof(float);
                        break;
                    case NpgsqlDbType.Smallint:
                        type = typeof(short);
                        break;
                    case NpgsqlDbType.Money:
                        type = typeof(decimal);
                        break;
                    case NpgsqlDbType.Boolean:
                        type = typeof(bool);
                        break;
                    case NpgsqlDbType.Box:
                        type = typeof(NpgsqlBox);
                        break;
                    case NpgsqlDbType.Circle:
                        type = typeof(NpgsqlCircle);
                        break;
                    case NpgsqlDbType.Line:
                        type = typeof(NpgsqlLine);
                        break;
                    case NpgsqlDbType.LSeg:
                        type = typeof(NpgsqlLSeg);
                        break;
                    case NpgsqlDbType.Path:
                        type = typeof(NpgsqlPath);
                        break;
                    case NpgsqlDbType.Point:
                        type = typeof(NpgsqlPoint);
                        break;
                    case NpgsqlDbType.Polygon:
                        type = typeof(NpgsqlPolygon);
                        break;
                    case NpgsqlDbType.Char:
                        type = typeof(string);
                        break;
                    case NpgsqlDbType.Text:
                        type = typeof(string);
                        break;
                    case NpgsqlDbType.Varchar:
                        type = typeof(string);
                        break;
                    case NpgsqlDbType.Name:
                        type = typeof(string);
                        break;
                    case NpgsqlDbType.Citext:
                        type = typeof(string);
                        break;
                    case NpgsqlDbType.InternalChar:
                        type = typeof(byte);
                        break;
                    case NpgsqlDbType.Bytea:
                        type = typeof(byte[]);
                        break;
                    case NpgsqlDbType.Date:
                        type = typeof(DateTime);
                        break;
                    case NpgsqlDbType.Time:
                        type = typeof(TimeSpan);
                        break;
                    case NpgsqlDbType.Timestamp:
                        type = typeof(DateTime);
                        break;
                    case NpgsqlDbType.TimestampTz:
                        type = typeof(DateTime);
                        break;
                    case NpgsqlDbType.Interval:
                        type = typeof(TimeSpan);
                        break;
                    case NpgsqlDbType.TimeTz:
                        type = typeof(DateTimeOffset);
                        break;
                    case NpgsqlDbType.Inet:
                        type = typeof(ValueTuple<IPAddress, int>);
                        break;
                    case NpgsqlDbType.Cidr:
                        type = typeof(ValueTuple<IPAddress, int>);
                        break;
                    case NpgsqlDbType.MacAddr:
                        type = typeof(PhysicalAddress);
                        break;
                    case NpgsqlDbType.MacAddr8:
                        type = typeof(PhysicalAddress);
                        break;
                    case NpgsqlDbType.Bit:
                        type = typeof(BitArray);
                        break;
                    case NpgsqlDbType.Varbit:
                        type = typeof(BitArray);
                        break;
                    case NpgsqlDbType.TsVector:
                        type = typeof(NpgsqlTsVector);
                        break;
                    case NpgsqlDbType.TsQuery:
                        type = typeof(NpgsqlTsQuery);
                        break;
                    case NpgsqlDbType.Uuid:
                        type = typeof(Guid);
                        break;
                    case NpgsqlDbType.Xml:
                        type = typeof(string);
                        break;
                    case NpgsqlDbType.Json:
                        type = typeof(IDictionary<string, object>);
                        break;
                    case NpgsqlDbType.Jsonb:
                        type = typeof(IDictionary<string, object>);
                        break;
                    case NpgsqlDbType.Hstore:
                        type = typeof(IDictionary<string, object>);
                        break;
                    case NpgsqlDbType.Array:
                        break;
                    case NpgsqlDbType.Range:
                        break;
                    case NpgsqlDbType.Refcursor:
                        break;
                    case NpgsqlDbType.Oidvector:
                        type = typeof(uint[]);
                        break;
                    case NpgsqlDbType.Int2Vector:
                        break;
                    case NpgsqlDbType.Oid:
                        type = typeof(uint);
                        break;
                    case NpgsqlDbType.Xid:
                        type = typeof(uint);
                        break;
                    case NpgsqlDbType.Cid:
                        type = typeof(uint);
                        break;
                    case NpgsqlDbType.Regtype:
                        break;
                    case NpgsqlDbType.Tid:
                        break;
                    case NpgsqlDbType.Unknown:
                        break;
                    case NpgsqlDbType.Geometry:
                        break;
                    case NpgsqlDbType.Geography:
                        break;
                }
            }

            if (type == null)
                type = typeof(object);

            if(isArray)
                return typeof(List<>).MakeGenericType(type);

            if (isRange)
                return typeof(NpgsqlRange<>).MakeGenericType(type);

            return type;
        }

    }
}