using System;
using System.Linq;
using doob.PgSql.ExtensionMethods;

namespace doob.PgSql.Expressions
{
    public abstract class ExpressionBase
    {
        private string _columnName;
        public string ColumnName
        {
            get
            {
                return _columnName;
            }
            set { _columnName = value?.ClearString(); }
        }

        protected readonly PgSqlCommand _pgSqlCommand;


        protected ExpressionBase(string columnName)
        {
            ColumnName = columnName;
            _pgSqlCommand = new PgSqlCommand();
        }


        protected void SetValue(string parameterName, object value)
        {
            _pgSqlCommand.SetValue(ColumnName, parameterName, value);
        }

        protected void OverrideValueType(string parameterName, string overrideType)
        {
            _pgSqlCommand.OverrideValueType(ColumnName, parameterName, overrideType);
        }
        protected void SetCommand(string command)
        {
            _pgSqlCommand.Command = command;
        }


        protected PgSqlParameter this[string key]
        {
            get { return _pgSqlCommand.Parameters.FirstOrDefault(p => p.ParameterName.Equals(key, StringComparison.OrdinalIgnoreCase)); }
        }

        protected string GetId(string name)
        {
            return this[name].UniqueId;
        }

        internal PgSqlCommand GetSqlCommand(Column column)
        {
            _getSqlCommand(column);
            _pgSqlCommand.Parameters.ForEach(p =>
            {
                p.Column = column;
            });
            return _pgSqlCommand;
        }

        protected abstract void _getSqlCommand(Column column);
    }
}