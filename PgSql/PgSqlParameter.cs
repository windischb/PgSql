using System;

namespace doob.PgSql
{
    public class PgSqlParameter
    {
        public string UniqueId { get; set; }
        public string ColumnName { get; set; }
        public string ParameterName { get; set; }
        public object Value { get; set; }
        public string OverrideType { get; set; }
        public Column Column { get; set; }

        public PgSqlParameter(string column, object value)
        {
            UniqueId = Guid.NewGuid().ToString("N");
            ColumnName = column;
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
                var col = tableDefinition.GetColumn(ColumnName);
                if (col != null)
                    Column = col;
            }
            return this;
        }

        internal PgSqlParameter SetColum(ColumnBuilder column)
        {

            Column = column;
            return this;
        }

    }
}
