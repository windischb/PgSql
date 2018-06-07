using System;
using System.Collections.Generic;
using System.Linq;
using doob.PgSql.ExtensionMethods;
using Newtonsoft.Json.Linq;

namespace doob.PgSql.Expressions.Logical
{
    public class ExpressionLike : ExpressionBase
    {
        private readonly bool _ignoreCase;
        private Type valueType;

        public ExpressionLike(string propertyName, string value, bool ignoreCase) : base(propertyName)
        {

            value = value?
                 .Replace("_", "\\_")
                 .Replace("%", "\\%")
                 .ReplaceUnescaped("*", "%")
                 .ReplaceUnescaped("\\*", "*")
                 .ReplaceUnescaped("?", "_")
                 .ReplaceUnescaped("\\?", "?");



            if (value == null)
            {
                SetValue("like", DBNull.Value);
            }
            else
            {
                valueType = value.GetType();
                SetValue("like", value);
            }

            _ignoreCase = ignoreCase;
        }

        protected override void _getSqlCommand(Column column)
        {

            _getSqlCommandForPostgres(column);


        }

        private void _getSqlCommandForPostgres(Column column)
        {

            OverrideValueType("like", "text");
            string like = _ignoreCase ? "ILIKE" : "LIKE";

            var props = ColumnName.Split(".".ToCharArray());
            var collName = $"\"{props[0]}\"";


            var propertiesList = new List<string>();
            string searchstr = string.Empty;
            string comm = String.Empty;

            var jTokenType = column.DotNetType.GetJTokenType();
            switch (jTokenType)
            {
                case JTokenType.Array when jTokenType == JTokenType.Object:
                    for (int i = 1; i < props.Length - 1; i++)
                    {
                        propertiesList.Add(props[i]);
                    }

                    if (propertiesList.Any())
                    {
                        searchstr = $"->{String.Join("->", propertiesList.Select(s => $"'{s}'"))}";
                    }

                    searchstr = $"{searchstr}->>'{props.Last()}'::text";


                    comm = $"exists (Select 1 from unnest({collName}) as objects where objects{searchstr} {like} @{GetId("like")})";

                    SetCommand(comm);
                    return;

                case JTokenType.Object:

                    if (props.Length == 1)
                    {
                        break;
                    }

                    for (int i = 1; i < props.Length - 1; i++)
                    {
                        propertiesList.Add(props[i]);
                    }

                    if (propertiesList.Any())
                    {
                        searchstr = $"->{String.Join("->", propertiesList.Select(s => $"'{s}'"))}";
                    }

                    searchstr = $"{searchstr}->>'{props.Last()}'::text";

                    comm = $"{collName}{searchstr} {like} @{GetId("like")}";


                    SetCommand(comm);
                    return;
            }

            SetCommand($"{collName}::text {like} @{GetId("like")}");
        }

    }
}