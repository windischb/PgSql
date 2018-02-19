using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Reflection;
using doob.PgSql.CustomTypes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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

        public static bool IsListType(this object value)
        {
            if (value == null)
                return false;

           return value.GetType().IsListType();
        }

    }

    internal static class ObjectExtensions
    {
        public static T CloneTo<T>(this object @object)
        {
            if (@object == null)
                return default(T);

            var json = JSON.ToJson(@object);
            return JSON.ToObject<T>(json);
        }

        public static T Clone<T>(this T @object)
        {
            var json = JSON.ToJson(@object);
            return JSON.ToObject<T>(json);
        }

        public static bool CastToBoolean(this Object value)
        {
            if (value == null)
                return false;

            if (value is Boolean)
                return (bool)value;

            string str = value.ToString();
            bool ret;
            if (bool.TryParse(str, out ret))
            {
                return ret;
            }

            if (str.ToLower() == "yes")
            {
                return true;
            }

            return ret;
        }

        public static int ToInt(this object value)
        {
            return value?.ToString().ToInt(true) ?? default(int);
        }

        public static bool IsDictionaryType(this object @object) {

            return @object?.GetType().IsDictionaryType() == true;
        }

        public static bool CanConvertToDictionary(this object value)
        {
            if (value == null)
                return false;

            if (value.IsBasicDotNetObject())
                return false;

            if (value is PgSqlLTree)
                return false;

            if (value.IsDictionaryType())
                return true;

            if (value.IsListType())
                return false;

            return true;
        }

        public static Dictionary<string, object> ToDotNetDictionary(this object @object, bool ignoreCase = false)
        {

            StringComparer comp = StringComparer.CurrentCulture;
            if (ignoreCase)
                comp = StringComparer.CurrentCultureIgnoreCase;

            Dictionary<string, object> tempDict = null;

            if (@object is ExpandoObject)
            {
                tempDict = new Dictionary<string, object>(@object as IDictionary<string, object>, comp);
            }


            if (tempDict == null && @object is JObject)
            {
                tempDict = new Dictionary<string, object>(((JObject)@object).ToObject<Dictionary<string, object>>(), comp);
            }


            if (tempDict == null)
            {
                var dict = @object as IDictionary;
                if (dict != null)
                {
                    tempDict = new Dictionary<string, object>(comp);
                    foreach (string o in dict.Keys)
                    {
                        tempDict.Add(o, dict[o]);
                    }
                }
                else
                {
                    tempDict = new Dictionary<string, object>(comp);

                    foreach (var propertyInfo in @object.GetType().GetProperties())
                    {
                        if (tempDict.ContainsKey(propertyInfo.Name))
                            continue;


                        var val = propertyInfo.GetValue(@object);

                        if (val == null)
                        {
                            tempDict.Add(propertyInfo.Name, null);
                            continue;
                        }

                        var type = val.GetType();

                        if (type == typeof(ExpandoObject))
                        {
                            tempDict.Add(propertyInfo.Name, val.ToDotNetDictionary(ignoreCase));
                            continue;
                        }

                        if (type == typeof(PgSqlLTree))
                        {
                            tempDict.Add(propertyInfo.Name, val);
                            continue;
                        }

                        var isJType = val is JToken;
                        if (isJType)
                        {
                            val = ((JToken)val).ToBasicDotNetObject();
                            type = val.GetType();
                        }



                        if (type == typeof(String))
                        {
                            tempDict.Add(propertyInfo.Name, val.ToString());
                            continue;
                        }

                        if (type.IsListType())
                        {
                            tempDict.Add(propertyInfo.Name, JArray.FromObject(val, JsonSerializer.Create(JSON.JsonSerializerSettings)).ToBasicDotNetObject());
                            continue;
                        }

                        if (val.GetType().GetTypeInfo().BaseType == typeof(object))
                        {
                            tempDict.Add(propertyInfo.Name, val.ToDotNetDictionary(ignoreCase));
                            continue;
                        }

                        tempDict.Add(propertyInfo.Name, val);
                    }

                }
            }



            return tempDict;
        }

    }

}
