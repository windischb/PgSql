using System;
using System.Collections.Generic;
using System.Text;
using doob.PgSql.ExtensionMethods;
using Reflectensions.ExtensionMethods;

namespace doob.PgSql.Tables
{
    public static class Table
    {
        public static TypedTable<T> GetTypedTable<T>(ConnectionString connectionString)
        {
            return new TypedTable<T>(connectionString);
        }
        public static TypedTable<T> GetTypedTable<T>(ConnectionStringBuilder connectionStringBuilder)
        {
            return GetTypedTable<T>(connectionStringBuilder.GetConnection());
        }
        public static TypedTable<T> GetTypedTable<T>(Action<ConnectionStringBuilder> builder)
        {
            return GetTypedTable<T>(builder.InvokeAction());
        }
    }
}
