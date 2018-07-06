using System;
using Newtonsoft.Json.Linq;
using NpgsqlTypes;

namespace doob.PgSql.Clauses
{
    internal static class ValuesClauseHelper
    {
        internal static string GetValuePlaceholder(PgSqlParameter parameter)
        {
            if (parameter.Value == null)
                return "DEFAULT";

            var npgsqlDbType = parameter.Column.GetNpgSqlDbType();

            if (npgsqlDbType == NpgsqlDbType.Text)
            {
                var str = parameter.Value as string;
                if (str != null)
                {
                    if (str.Equals("DEFAULT", StringComparison.OrdinalIgnoreCase))
                    {
                        return "'DEFAULT'";
                    }
                }
            }

            if (parameter.Value is string _str && _str == "DEFAULT")
                return "DEFAULT";

            if (npgsqlDbType == NpgsqlDbType.Uuid)
            {
                if (!String.IsNullOrWhiteSpace(parameter.Column.DefaultValue))
                    if (parameter.Value.ToString() == Guid.Empty.ToString())
                        return "DEFAULT";
            }

            if (npgsqlDbType == NpgsqlDbType.Timestamp)
            {
                if (!String.IsNullOrWhiteSpace(parameter.Column.DefaultValue))
                    if ((DateTime)parameter.Value == default(DateTime))
                        return "DEFAULT";
            }




            if (!String.IsNullOrWhiteSpace(parameter.Column?.DefaultValue) && parameter.Value == null)
                return "DEFAULT";


            return $"@{parameter.UniqueId}";
        }

        internal static bool IsNotDefault(PgSqlParameter parameter)
        {

            var str = parameter.Value as string;
            if (str != null)
            {
                return !str.Equals("DEFAULT", StringComparison.OrdinalIgnoreCase);
            }

            return true;
        }
    }
}