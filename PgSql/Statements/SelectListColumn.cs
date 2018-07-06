using System;
using doob.PgSql.CustomTypes;
using doob.PgSql.ExtensionMethods;

namespace doob.PgSql.Statements
{
    public class SelectListColumn : ISelectListItem
    {
        public string Table { get; set; }
        public string Column { get; set; }
        public string Alias { get; set; }
        public Type DotnetType { get; set; }

        public static SelectListColumn Parse(string sqlStatement)
        {
            var field = new SelectListColumn();

            sqlStatement = sqlStatement?.ClearString() ?? throw new ArgumentNullException(nameof(sqlStatement));

            if (sqlStatement.EndsWith(" AS", StringComparison.OrdinalIgnoreCase))
            {
                sqlStatement = sqlStatement.Substring(0, sqlStatement.Length - 3);
            }
            var indexOfAs = sqlStatement.ToUpper().IndexOf(" AS ", StringComparison.OrdinalIgnoreCase);
            if (indexOfAs > 0)
            {
                field.Alias = sqlStatement.Substring(indexOfAs + 4).ClearString();
                sqlStatement = sqlStatement.Substring(0, indexOfAs).ClearString();
            }

            if (sqlStatement.Contains("."))
            {
                var arr = sqlStatement.Split('.');
                field.Table = arr[0].ClearString();
                sqlStatement = arr[1].ClearString();
            }

            if (sqlStatement == "*")
                field.Alias = null;



            if (sqlStatement.Contains("::"))
            {
                var indexOfType = sqlStatement.IndexOf("::", StringComparison.OrdinalIgnoreCase);
                field.Column = sqlStatement.Substring(0, indexOfType).ClearString();
            }
            else
            {
                field.Column = sqlStatement;
            }


            return field;
        }

        private SelectListColumn()
        {

        }
        public SelectListColumn(string column)
        {
            var f = SelectListColumn.Parse(column);
            Table = f.Table;
            Column = f.Column;
            Alias = f.Alias;
        }

        public SelectListColumn(string column, string alias) : this($"{column} AS {alias}")
        {
        }

        public SelectListColumn(string table, string column, string alias) : this($"{table}.{column} AS {alias}")
        {
        }


        public PgSqlCommand GetSqlCommand(TableDefinition tableDefinition)
        {
            var sqlCommand = new PgSqlCommand();

            if (!String.IsNullOrWhiteSpace(Table))
                sqlCommand.AppendCommand($"\"{Table}\".");

            if (Column == "*")
            {
                sqlCommand.AppendCommand($"*");
            }
            else
            {
                sqlCommand.AppendCommand($"\"{Column}\"");

                if (DotnetType == typeof(PgSqlLTree))
                    sqlCommand.AppendCommand("::text");


                //if (!String.IsNullOrWhiteSpace(Alias))
                //    sqlCommand.AppendCommand($" AS \"{Alias}\"");
            }

            return sqlCommand;
        }
    }
}
