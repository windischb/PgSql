using System;
using doob.PgSql.ExtensionMethods;
using doob.PgSql.Interfaces;

namespace doob.PgSql.Clauses
{
    public class FromSourceTableOrView : IFromSource, ISQLCommand
    {
        public string SourceType { get; } = "TableOrView";
        private string _schema;
        private string _name;
        private string _alias;
        private bool _includeChildTables;

        public static FromSourceTableOrView Parse(string sqlStatement)
        {
            var field = new FromSourceTableOrView();

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
                field._schema = arr[0].ClearString();
                sqlStatement = arr[1].ClearString();
            }

            if (sqlStatement == "*")
                field._alias = null;

            field._name = sqlStatement;

            return field;
        }

        private FromSourceTableOrView()
        {

        }
        public FromSourceTableOrView(string name)
        {
            var f = FromSourceTableOrView.Parse(name);
            _schema = f._schema;
            _name = f._name;
            _alias = f._alias;
        }
        public FromSourceTableOrView(string column, string alias) : this($"{column} AS {alias}")
        {
        }
        public FromSourceTableOrView(string table, string column, string alias) : this($"{table}.{column} AS {alias}")
        {
        }

        public FromSourceTableOrView IncludeChildTables(bool include)
        {
            _includeChildTables = include;
            return this;
        }

        //public override string ToString()
        //{

        //    var strBuilder = new StringBuilder();

        //    if (!_includeChildTables)
        //    {
        //        strBuilder.Append("ONLY ");
        //    }

        //    if (!String.IsNullOrWhiteSpace(_schema))
        //        strBuilder.Append($"\"{_schema}\".");

        //    strBuilder.Append($"\"{_name}\"");

        //    if (!String.IsNullOrWhiteSpace(_alias))
        //        strBuilder.Append($" AS \"{_alias}\"");

        //    return strBuilder.ToString();
        //}

        public PgSqlCommand GetSqlCommand(TableDefinition tableDefinition)
        {
            var sqlCommand = new PgSqlCommand();

           
                if (!_includeChildTables)
                {
                    sqlCommand.AppendCommand("ONLY ");
                }

            if (!String.IsNullOrWhiteSpace(_schema))
                sqlCommand.AppendCommand($"\"{_schema}\".");

            sqlCommand.AppendCommand($"\"{_name}\"");

            if (!String.IsNullOrWhiteSpace(_alias))
                sqlCommand.AppendCommand($" AS \"{_alias}\"");

            return sqlCommand;
        }
    }
}
