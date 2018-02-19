using System;
using System.Collections.Generic;
using System.Linq;
using doob.PgSql.Json.ContractResolvers;
using doob.PgSql.Json.Converters;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using ExpandoObjectConverter = doob.PgSql.Json.Converters.ExpandoObjectConverter;

namespace doob.PgSql
{
    public static class JSON
    {

        private static JsonSerializerSettings _jsonSerializerSettings;
        internal static JsonSerializerSettings JsonSerializerSettings
        {
            get => _jsonSerializerSettings ?? (_jsonSerializerSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore,
                NullValueHandling = NullValueHandling.Include,
                CheckAdditionalContent = false,
                ContractResolver = new PrivateSetterContractResolver()
            });
            set => _jsonSerializerSettings = value;
        }

        private static List<Type> _jsonConversters;
        public static void RegisterJsonConverters<T>() where T : JsonConverter
        {
            if (_jsonConversters == null)
                _jsonConversters = new List<Type>();

            if (!_jsonConversters.Contains(typeof(T)))
                _jsonConversters.Add(typeof(T));

            var l = _jsonConversters.Select(Activator.CreateInstance).Cast<JsonConverter>().ToList();
            JsonSerializerSettings.Converters = l;
        }

        public static void UnRegisterJsonConverters<T>() where T : JsonConverter
        {
            if (_jsonConversters == null)
                return;

            if (_jsonConversters.Contains(typeof(T)))
                _jsonConversters.Remove(typeof(T));

            var l = _jsonConversters.Select(t => Activator.CreateInstance<T>()).Cast<JsonConverter>().ToList();
            JsonSerializerSettings.Converters = l;
        }

        static JSON()
        {
            RegisterJsonConverters<StringEnumConverter>();
            RegisterJsonConverters<PgSqlLTreeConverter>();
            RegisterJsonConverters<IPAddressConverter>();
            RegisterJsonConverters<IPEndPointConverter>();
            RegisterJsonConverters<DefaultDictionaryConverter>();
            RegisterJsonConverters<ExpandoObjectConverter>();
        }

        internal static string ToJson(object @object, bool formatted = false)
        {
            var formating = Formatting.None;
            if(formatted)
                formating = Formatting.Indented;

            return JsonConvert.SerializeObject(@object, formating, JsonSerializerSettings);

        }

        internal static T ToObject<T>(string json)
        {
            try {
                return JsonConvert.DeserializeObject<T>(json, JsonSerializerSettings);

            } catch (Exception e) {
                Console.WriteLine(e);
                throw;
            }
            
        }

        internal static T ToObject<T>(string json, Type type)
        {
            return (T)JsonConvert.DeserializeObject(json, type, JsonSerializerSettings);
        }

        internal static dynamic ToDynamic(string json)
        {
            return JsonConvert.DeserializeObject<dynamic>(json, JsonSerializerSettings);
        }

        internal static string Beautify(string json)
        {
            return JToken.Parse(json).ToString(Formatting.Indented);
        }

        internal static string Minify(string json)
        {
            return JToken.Parse(json).ToString(Formatting.None);
        }

        internal static bool CompareEqual(object object1, object object2)
        {
            var json1 = ToJson(object1);
            var json2 = ToJson(object2);
            return json1 == json2;
        }


        internal static T ToObject<T>(JToken jToken)
        {
            return jToken.ToObject<T>(JsonSerializer.Create(JsonSerializerSettings));
        }

        internal static Dictionary<string, object> ToDictionary(object data, bool ignoreCase = false)
        {
            if (data == null)
                return null;
            var json = ToJson(data);

            return ToDictionary(json, ignoreCase);
        }

        //internal static Dictionary<string, object> ToDictionary(JObject jObject, bool ignoreCase = false)
        //{
        //    if (jObject == null)
        //        return null;

        //    var json = jObject.ToString();

        //    return ToDictionary(json, ignoreCase);

        //}

        internal static Dictionary<string, object> ToDictionary(string json, bool ignoreCase = false) {
            if (String.IsNullOrWhiteSpace(json))
                return null;


            return JsonConvert.DeserializeObject<Dictionary<string, object>>(json, JsonSerializerSettings);
            //return JsonConvert.DeserializeObject<Dictionary<string, object>>(json, new DefaultDictionaryConverter(ignoreCase));
        }

        

        internal static JToken ToJToken(object value)
        {
            return JToken.FromObject(value, JsonSerializer.Create(JsonSerializerSettings));
        }

        internal static JObject ToJObject(object value) {
            return JObject.FromObject(value, JsonSerializer.Create(JsonSerializerSettings));
        }
    }

    
}
