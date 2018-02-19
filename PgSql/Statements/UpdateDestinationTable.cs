using System;
using doob.PgSql.ExtensionMethods;
using doob.PgSql.Interfaces;

namespace doob.PgSql.Statements
{
    public class UpdateDestinationTable : IUpdateDestination
    {
        public string DestinationType { get; } = "Table";
        private string _table;
        private string _column;
        private string _alias;
        private bool _includeDescendants;

        public static UpdateDestinationTable Parse(string sqlStatement)
        {
            var field = new UpdateDestinationTable();

            if (sqlStatement == null) throw new ArgumentNullException(nameof(sqlStatement));

            sqlStatement = sqlStatement.ClearString();

            if (sqlStatement.StartsWith("only", StringComparison.OrdinalIgnoreCase))
            {
                field._includeDescendants = false;
                sqlStatement = sqlStatement.Substring(4).ClearString();
            }
            else
            {
                field._includeDescendants = true;
            }

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

        private UpdateDestinationTable()
        {

        }
        public UpdateDestinationTable(string name)
        {
            var f = UpdateDestinationTable.Parse(name);
            _table = f._table;
            _column = f._column;
            _alias = f._alias;
        }
        public UpdateDestinationTable(string column, string alias) : this($"{column} AS {alias}")
        {
        }

        public UpdateDestinationTable IncludeDescendants(bool include)
        {
            _includeDescendants = include;
            return this;
        }

        public UpdateDestinationTable IncludeDescendants()
        {
            return IncludeDescendants(true);
        }

        public PgSqlCommand GetSqlCommand(TableDefinition tableDefinition)
        {
            var sqlCommand = new PgSqlCommand();

                if (!_includeDescendants)
                {
                    sqlCommand.AppendCommand($"ONLY ");
                }

            if (!String.IsNullOrWhiteSpace(_table))
                sqlCommand.AppendCommand($"\"{_table}\".");

            sqlCommand.AppendCommand($"\"{_column}\"");

            if (!String.IsNullOrWhiteSpace(_alias))
                sqlCommand.AppendCommand($" AS \"{_alias}\"");

            return sqlCommand;
        }
    }
}
