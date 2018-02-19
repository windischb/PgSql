using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Linq;

namespace doob.PgSql.ExtensionMethods
{
    public static class TypeExtensions
    {
        public static bool IsBasicDotNetType(this Type type)
        {

            if (type == null)
                return false;

            if (type.IsPrimitive)
                return true;

            if (type == typeof(String))
                return true;

            if (type == typeof(DateTime))
                return true;

            if (type == typeof(Guid))
                return true;

            if (type == typeof(Uri))
                return true;

            if (type == typeof(TimeSpan))
                return true;

            if (type.Assembly == typeof(object).Assembly)
                return true;

            return false;
        }

        public static bool IsNullable(this Type type)
        {
            var typeInfo = type.GetTypeInfo();
            return typeInfo.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        public static Type GetInnerTypeFromNullable(this Type nullableType)
        {
            return nullableType.GetGenericArguments()[0];
        }

        public static bool IsDictionaryType(this Type type)
        {
            var implementedInterfaces = type.GetInterfaces();
            var ret = implementedInterfaces.Contains(typeof(IDictionary));
            return ret;
        }

        public static bool IsListType(this Type type)
        {
            if (type.IsDictionaryType())
                return false;

            var targetType = typeof(IList<>);
            var implementedInterfaces = type.GetInterfaces();
            var ret = implementedInterfaces.Any(i => i.GetTypeInfo().IsGenericType
                                                     && i.GetGenericTypeDefinition() == targetType);
            if (!ret)
                ret = implementedInterfaces.Contains(typeof(IEnumerable));

            return ret;
        }

        private static readonly ConcurrentDictionary<Type, JTokenType> JTokenTypeCache = new ConcurrentDictionary<Type, JTokenType>();
        internal static JTokenType GetJTokenType(this Type type)
        {

            if (JTokenTypeCache.TryGetValue(type, out JTokenType jType))
            {
                return jType;
            };


            try
            {
                var inst = Activator.CreateInstance(type);
                var jtype = JSON.ToJToken(inst).Type;
                JTokenTypeCache.AddOrUpdate(type, jtype, (type1, tokenType) => jtype);
                return jtype;
            }
            catch
            {
                // ignored
            }

            return JTokenType.Object;
        }
    }
}
