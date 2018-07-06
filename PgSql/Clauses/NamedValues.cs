using System;
using System.Collections.Generic;
using System.Linq;
using doob.PgSql.Interfaces;

namespace doob.PgSql.Clauses
{
    public class NamedValues : IValues
    {

        public readonly List<IValueItem> _values = new List<IValueItem>();

        private NamedValues() { }
       
        public static NamedValues Create()
        {
            return new NamedValues();
        }
      
        public static NamedValues Create(string key, object value)
        {
            return new NamedValues().AddValue(key, value);
        }

        public NamedValues AddValue(string key, object value)
        {
            var val = new ValueItem(key, value);
            _values.Add(val);
            return this;
        }


        public PgSqlCommand GetSqlCommand(TableDefinition tableDefinition)
        {
            var sqlCommand = new PgSqlCommand();

            var values = _values.Select(val =>
            {
                var col = tableDefinition.GetColumnByClrName(val._key) ??
                          tableDefinition.GetColumnByDbName(val._key);

                var expr = new PgSqlParameter(col.GetNameForDb(), val._value);
                return expr;
            }).ToList();

            var valuesIds = values.Select(v => v.UniqueId).ToList();


            var sortedValues = new List<PgSqlParameter>();

            if (tableDefinition != null)
            {
                foreach (var column in tableDefinition.Columns())
                {
                    
                    var exists = values.FirstOrDefault(v => v.ColumnName.Equals(column.GetNameForDb(), StringComparison.OrdinalIgnoreCase));
                    if (exists != null)
                    {
                        exists.Column = column;
                        sortedValues.Add(exists);
                    }
                    else
                    {
                        var defaultExpr = new PgSqlParameter(column.GetNameForDb(), "DEFAULT");
                        defaultExpr.Column = column;
                        sortedValues.Add(defaultExpr);
                    }
                }
            }
            else
            {
                sortedValues = values;
            }
            var sortedValuesIds = sortedValues.Select(v => v.UniqueId).ToList();

            if (_values.Any())
            {
                var parms = sortedValues.Where(ValuesClauseHelper.IsNotDefault);
                var parmsIds = parms.Select(p => p.UniqueId);
                var placeHolders = sortedValues.Select(ValuesClauseHelper.GetValuePlaceholder);
                sqlCommand.AppendCommand($"{String.Join(", ", placeHolders)}", parms);
            }
                

            return sqlCommand;
        }
    }
}
