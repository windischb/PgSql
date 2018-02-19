using System;
using System.Collections.Generic;
using System.Linq;
using doob.PgSql.Interfaces;

namespace doob.PgSql.Clauses
{
    public class PositionedValues : IValues
    {

        internal readonly List<IValueItem> _values = new List<IValueItem>();

        private PositionedValues() { }

        public static PositionedValues Create()
        {
            return new PositionedValues();
        }
        public static PositionedValues Create(object value)
        {
            return new PositionedValues().AddValue(value);
        }

        public PositionedValues AddValue(object value)
        {
            var val = new ValueItem(value);
            _values.Add(val);
            return this;
        }


        public PgSqlCommand GetSqlCommand(TableDefinition tableDefinition)
        {
            var sqlCommand = new PgSqlCommand();

            var sortedValues = new List<PgSqlParameter>();
            var columns = tableDefinition.Columns().ToList();
            for (int i = 0; i < columns.Count; i++)
            {
                var column = columns[i];
                IValueItem valueItem = null;

                if (_values.Count > i)
                    valueItem = _values[i];

                if (valueItem == null)
                {
                    var defaultExpr = new PgSqlParameter(i.ToString(), "DEFAULT");
                    sortedValues.Add(defaultExpr);
                }
                else
                {
                    var expr = new PgSqlParameter(i.ToString(), valueItem._value);
                    sortedValues.Add(expr);
                }
            }


            if (_values.Any())
                sqlCommand.AppendCommand($"{String.Join(", ", sortedValues.Select(ValuesClauseHelper.GetValuePlaceholder))}", sortedValues.Where(ValuesClauseHelper.IsNotDefault));


            return sqlCommand;
        }
    }
}
