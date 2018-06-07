using System;
using System.Collections.Generic;
using System.Text;
using doob.PgSql.Interfaces;

namespace doob.PgSql.Statements
{
    public class AlterTable : ISQLCommand
    {
        public string Name { get; set; }



        private List<IAlterTableAction> _actions = new List<IAlterTableAction>();


        public AlterTable AddAction(IAlterTableAction action)
        {
            _actions.Add(action);
            return this;
        }

        public PgSqlCommand GetSqlCommand(TableDefinition tableDefinition)
        {
            
            var sqlCommand = new PgSqlCommand();

            sqlCommand.AppendCommandLine($"ALTER TABLE {Name}");
            foreach (var alterTableAction in _actions)
            {
                sqlCommand.AppendCommandLine(alterTableAction.GetSqlCommand(tableDefinition).Command);
            }

            return sqlCommand;

        }
    }
}
