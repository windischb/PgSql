using System;
using doob.PgSql.ExtensionMethods;
using doob.PgSql.Interfaces;

namespace doob.PgSql.Statements
{
    public class IntoDestinationTable : IIntoDestination
    {
        public string DestinationType { get; } = "Table";
        private string _table;
        private string _column;
        private string _alias;


        public static IntoDestinationTable Parse(string sqlStatement)
        {
            var field = new IntoDestinationTable();

            if (sqlStatement == null) throw new ArgumentNullException(nameof(sqlStatement));

            sqlStatement = sqlStatement.ClearString();

            if (sqlStatement.EndsWith(" AS", StringComparison.OrdinalIgnoreCase))
            {
                sqlStatement = sqlStatement.Substring(0, sqlStatement.Length - 3);
            }
            var indexOfAs = sqlStatement.ToUpper().IndexOf(" AS ", StringComparison.OrdinalIgnoreCase);
            if (indexOfAs > 0)
            {
                field._alias = sqlStatement.Substring(indexOfAs + 4).ClearString();
                sqlStatement = sqlStatement.Substring(0, indexOfAs).ClearString();
            }

            if (sqlStatement.Contains("."))
            {
                var arr = sqlStatement.Split('.');
                field._table = arr[0].ClearString();
                sqlStatement = arr[1].ClearString();
            }

            if (sqlStatement == "*")
                field._alias = null;

            field._column = sqlStatement;

            return field;
        }

        private IntoDestinationTable()
        {
            
        }
        public IntoDestinationTable(string name)
        {
            var f = IntoDestinationTable.Parse(name);
            _table = f._table;
            _column = f._column;
            _alias = f._alias;
        }
        public IntoDestinationTable(string name, string alias) : this($"{name} AS {alias}")
        {
        }


        public PgSqlCommand GetSqlCommand(TableDefinition tableDefinition)
        {
            var sqlCommand = new PgSqlCommand();

            if (!String.IsNullOrWhiteSpace(_table))
                sqlCommand.AppendCommand($"\"{_table}\".");

            sqlCommand.AppendCommand($"\"{_column}\"");

            if (!String.IsNullOrWhiteSpace(_alias))
                sqlCommand.AppendCommand($" AS \"{_alias}\"");

            return sqlCommand;
        }
    }
}
