using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace doob.PgSql.Json.Converters
{
    internal sealed class DefaultDictionaryConverter : CustomCreationConverter<IDictionary<string, object>>
    {
        private bool _ignoreCase;

        public DefaultDictionaryConverter()
        {
            
        }

        public DefaultDictionaryConverter(bool ignoreCase = false)
        {
            _ignoreCase = ignoreCase;
        }
        public override IDictionary<string, object> Create(Type objectType)
        {
            if (_ignoreCase)
            {
                return new Dictionary<string, object>(StringComparer.CurrentCultureIgnoreCase);
            }
            else
            {
                return new Dictionary<string, object>(StringComparer.CurrentCulture);
            }

        }

        public override bool CanConvert(Type objectType)
        {
            // in addition to handling ExpandoObject
            // we want to handle the deserialization of dict value
            // which is of type object
            return objectType == typeof(object) || base.CanConvert(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.StartObject || reader.TokenType == JsonToken.Null)
                return base.ReadJson(reader, objectType, existingValue, serializer);

            // if the next token is not an object
            // then fall back on standard deserializer (strings, numbers etc.)
            return serializer.Deserialize(reader);
        }
    }
}
