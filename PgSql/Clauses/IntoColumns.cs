using System;
using System.Collections.Generic;
using System.Linq;
using doob.PgSql.Interfaces;

namespace doob.PgSql.Clauses
{
    public class IntoColumns : IInsertMember
    {

        public readonly List<IntoColumnsItem> _columns = new List<IntoColumnsItem>();

        private IntoColumns() { }


        public static IntoColumns Create()
        {
            return new IntoColumns();
        }
        public static IntoColumns Name(string column)
        {
            return new IntoColumns().AddColumn(column);
        }
        public static IntoColumns Names(params string[] columns)
        {
            var inCol = new IntoColumns();
            foreach (var intoColumnsItem in columns)
            {
                inCol.AddColumn(intoColumnsItem);
            }
            return inCol;
        }


        public IntoColumns AddColumn(string name)
        {
            var t = new IntoColumnsItem(name);
            _columns.Add(t);
            return this;
        }


        public PgSqlCommand GetSqlCommand(TableDefinition tableDefinition)
        {
            var sqlCommand = new PgSqlCommand();

            if (_columns.Any())
                sqlCommand.AppendCommand($"{String.Join(", ", _columns.Select(c => c.GetSqlCommand(tableDefinition).Command))}");

            return sqlCommand;
        }
    }
}
