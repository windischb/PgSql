using System;

namespace doob.PgSql.Clauses
{
    internal static class ValuesClauseHelper
    {
        internal static string GetValuePlaceholder(PgSqlParameter parameter)
        {
            if(parameter.Value == null)
                return "DEFAULT";

            if (parameter.Column?.Properties.DotNetType == typeof(String))
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

            if (parameter.Column?.Properties.DotNetType == typeof(Guid))
            {
                if(!String.IsNullOrWhiteSpace(parameter.Column.Properties.DefaultValue))
                    if(parameter.Value.ToString() == Guid.Empty.ToString())
                        return "DEFAULT";
            }

            if (parameter.Column?.Properties.DotNetType == typeof(DateTime))
            {
                if (!String.IsNullOrWhiteSpace(parameter.Column.Properties.DefaultValue))
                    if ((DateTime)parameter.Value == default(DateTime))
                        return "DEFAULT";
            }


            

            if (!String.IsNullOrWhiteSpace(parameter.Column?.Properties.DefaultValue) && parameter.Value == null)
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
