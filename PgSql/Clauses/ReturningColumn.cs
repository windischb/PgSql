using System;
using doob.PgSql.ExtensionMethods;
using doob.PgSql.Interfaces;

namespace doob.PgSql.Clauses
{
    public class ReturningColumn : IReturningValue
    {
        public string ReturningType { get; } = "Column";
        private string _column;
        private string _alias;


        public static ReturningColumn Parse(string sqlStatement)
        {
            var field = new ReturningColumn();

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

            if (sqlStatement == "*")
                field._alias = null;

            field._column = sqlStatement;

            return field;
        }

        private ReturningColumn()
        {
            
        }
        public ReturningColumn(string name)
        {
            var f = ReturningColumn.Parse(name);
            _column = f._column;
            _alias = f._alias;
        }
        public ReturningColumn(string name, string alias) : this($"{name} AS {alias}")
        {
        }

        public PgSqlCommand GetSqlCommand(TableDefinition tableDefinition)
        {
            var sqlCommand = new PgSqlCommand();

            sqlCommand.AppendCommand($"\"{_column}\"");

            if (!String.IsNullOrWhiteSpace(_alias))
                sqlCommand.AppendCommand($" AS \"{_alias}\"");

            return sqlCommand;
        }
    }

    public interface IReturningValue : ISQLCommand
    {
        string ReturningType { get; }
    }
}
