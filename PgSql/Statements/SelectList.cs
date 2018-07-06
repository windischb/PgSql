using System;
using System.Collections.Generic;
using System.Linq;
using doob.PgSql.Interfaces;

namespace doob.PgSql.Statements
{
    class SelectList : ISelectMember
    {
        private readonly List<ISelectListItem> _selectListItems = new List<ISelectListItem>();

        public SelectList AddColumn(SelectListColumn field)
        {
            _selectListItems.Add(field);
            return this;
        }
        public SelectList AddColumn(string fieldName)
        {
            var f = new SelectListColumn(fieldName);
            AddColumn(f);
            return this;
        }
        public SelectList AddColumn(string fieldName, string alias)
        {
            var f = new SelectListColumn(fieldName, alias);
            AddColumn(f);
            return this;
        }
        public SelectList AddColumn(string table, string fieldName, string alias)
        {
            var f = new SelectListColumn(table, fieldName, alias);
            AddColumn(f);
            return this;
        }

        public SelectList AddColumn(Column pgSqlColumn) {

            SelectListColumn col;
            if (String.IsNullOrWhiteSpace(pgSqlColumn.DbName)) {
                col = new SelectListColumn(pgSqlColumn.ClrName);
            } else {
                col = new SelectListColumn(pgSqlColumn.DbName, pgSqlColumn.ClrName);
            }
           
            col.DotnetType = pgSqlColumn.DotNetType;

            AddColumn(col);
            return this;
        }

        public PgSqlCommand GetSqlCommand(TableDefinition tableDefinition)
        {
            var sqlCommand = new PgSqlCommand();

            if (_selectListItems.Any())
            {
                var commands = new List<PgSqlCommand>();
                foreach (var selectListItem in _selectListItems)
                {
                    var comm = selectListItem.GetSqlCommand(tableDefinition);
                    commands.Add(comm);
                }


                sqlCommand.AppendCommand($"{String.Join(",", commands.Select(c => c.Command))}", commands.SelectMany(c => c.Parameters));
            }
            else
            {
                sqlCommand.AppendCommand("*");
            }

            return sqlCommand;
        }

        
    }

    public interface ISelectListItem : ISQLCommand
    {
        
    }
}
