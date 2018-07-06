using System;
using System.Collections.Generic;
using System.Text;
using doob.PgSql.ExtensionMethods;
using doob.PgSql.TypeMapping;

namespace doob.PgSql.Statements
{
    public class AddColumnAction : IAlterTableAction
    {
        public string Name { get; set; }
        public string DataType { get; set; }

        public bool ThrowIfExists { get; set; }

        public AddColumnAction(Column column)
        {
            Name = column.DbName;
            DataType = PgSqlTypeManager.GetPostgresName(column.DotNetType);
        }

        public PgSqlCommand GetSqlCommand(TableDefinition tableDefinition)
        {
            var sqlCommand = new PgSqlCommand();

            sqlCommand.AppendCommand($"ADD COLUMN ");
            if (!ThrowIfExists)
            {
                sqlCommand.AppendCommand("IF NOT EXISTS ");
            }

            sqlCommand.AppendCommand($"{Name.ClearString()} {DataType}");

            sqlCommand.AppendCommand(";");

            return sqlCommand;
        }
    }
}
