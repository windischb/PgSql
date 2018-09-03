using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using doob.PgSql.CustomTypes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Reflectensions.ExtensionMethods;

namespace doob.PgSql.ExtensionMethods
{
    
    public static class PublicObjectExtensions {

        public static bool IsBasicDotNetObject(this object value)
        {

            if (value == null)
                return false;

            var type = value.GetType();
            return type.IsBasicDotNetType();

        }

    }

    internal static class ObjectExtensions
    {
        public static T CloneTo<T>(this object @object)
        {
            if (@object == null)
                return default(T);

            var json = Converter.Json.ToJson(@object);
            return Converter.Json.ToObject<T>(json);
        }

        public static T Clone<T>(this T @object)
        {
            var json = Converter.Json.ToJson(@object);
            return Converter.Json.ToObject<T>(json);
        }


        public static bool CanConvertToDictionary(this object value)
        {
            if (value == null)
                return false;

            if (value.IsBasicDotNetObject())
                return false;

            if (value is PgSqlLTree)
                return false;

            if (value.GetType().IsDictionaryType())
                return true;

            if (value.GetType().IsEnumerableType())
                return false;

            return true;
        }

      
        public static Dictionary<string, object> ToColumsDictionary(this object @object)
        {

            switch (@object)
            {
                case ExpandoObject exp:
                {
                    return new Dictionary<string, object>(exp);
                }
                case JObject jObject:
                {
                    return jObject.Properties().ToDictionary(jp => jp.Name, jp => (object)jp.Value);
                }
                case IDictionary idict:
                {
                    var tempDict = new Dictionary<string, object>();
                    foreach (var idictKey in idict.Keys)
                    {
                        tempDict.Add(idictKey.ToString(), idict[idictKey]);
                    }

                    return tempDict;
                }
            }


            return @object.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .ToDictionary(pInfo => pInfo.Name, pInfo => pInfo.GetValue(@object));

        }

       
    }

}
