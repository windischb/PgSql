using System;
using doob.PgSql.CustomTypes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace doob.PgSql.Json.Converters
{
    public class PgSqlLTreeConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            new JValue(value.ToString()).WriteTo(writer, Array.Empty<JsonConverter>());
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.String)
                return (object)new PgSqlLTree(reader.Value.ToString());
            return (object)null;
        }

        public override bool CanConvert(Type objectType)
        {
            return (object)objectType == (object)typeof(PgSqlLTree);
        }
    }
}
