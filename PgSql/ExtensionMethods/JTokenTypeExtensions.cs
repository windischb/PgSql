using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;
using NpgsqlTypes;

namespace doob.PgSql.ExtensionMethods
{
    public static class JTokenTypeExtensions
    {
        public static  NpgsqlDbType ToNpgsqlDbType(this JTokenType jTokenType)
        {

            switch (jTokenType)
            {
                case JTokenType.None:
                    break;
                case JTokenType.Object:
                    return NpgsqlDbType.Jsonb;
                case JTokenType.Array:
                    break;
                case JTokenType.Constructor:
                    break;
                case JTokenType.Property:
                    break;
                case JTokenType.Comment:
                    break;
                case JTokenType.Integer:
                    break;
                case JTokenType.Float:
                    break;
                case JTokenType.String:
                    break;
                case JTokenType.Boolean:
                    break;
                case JTokenType.Null:
                    break;
                case JTokenType.Undefined:
                    break;
                case JTokenType.Date:
                    break;
                case JTokenType.Raw:
                    break;
                case JTokenType.Bytes:
                    break;
                case JTokenType.Guid:
                    break;
                case JTokenType.Uri:
                    break;
                case JTokenType.TimeSpan:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(jTokenType), jTokenType, null);
            }

            return NpgsqlDbType.Unknown;
        }
    }
}
