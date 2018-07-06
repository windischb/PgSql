using System;
using Newtonsoft.Json.Linq;

namespace doob.PgSql
{
    public class PgSqlParameter
    {
        public string UniqueId { get; set; }
        public string ColumnName { get; set; }
        public string ParameterName { get; set; }
        public object Value { get; set; }
        public Type ClrType { get; set; }
        public string OverrideType { get; set; }
        public Column Column { get; set; }

        public PgSqlParameter(string column, object value)
        {
            UniqueId = Guid.NewGuid().ToString("N");
            ColumnName = column;
            ClrType = value?.GetType();
            Value = value;

        }
        public PgSqlParameter(string column, object value, string parameterName) : this(column, value)
        {
            ParameterName = parameterName;
        }

        public PgSqlParameter RebuildWithNewId()
        {
            return new PgSqlParameter(ColumnName, Value, ColumnName);
        }

        internal PgSqlParameter SetColum(TableDefinition tableDefinition)
        {
            if (tableDefinition != null)
            {
                var col = tableDefinition.GetColumnBuilderByClrName(ColumnName);
                if (col != null)
                    Column = col;
            }
            return this;
        }

        internal PgSqlParameter SetColum(Column column)
        {

            Column = column;
            return this;
        }

    }
}
