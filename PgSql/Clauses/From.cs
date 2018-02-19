using System;
using System.Collections.Generic;
using System.Linq;
using doob.PgSql.Interfaces;

namespace doob.PgSql.Clauses
{
    public class From: ISelectMember, IDeleteMember
    {
        private readonly List<IFromSource> _fromSources = new List<IFromSource>();

        public static From TableOrView(string name)
        {
            return new From().AddTableOrView(name);
        }
        public static From TableOrView(string name, string alias)
        {
            return new From().AddTableOrView(name, alias);
        }
        public static From TableOrView(string schema, string name, string alias)
        {
            return new From().AddTableOrView(schema, name, alias);
        }


        public From AddTableOrView(FromSourceTableOrView sourceTableOrView)
        {
            _fromSources.Add(sourceTableOrView);
            return this;
        }
        public From AddTableOrView(string name)
        {
            var t = new FromSourceTableOrView(name);
            return AddTableOrView(t);
        }
        public From AddTableOrView(string name, string alias)
        {
            var t = new FromSourceTableOrView(name, alias);
            return AddTableOrView(t);
        }
        public From AddTableOrView(string schema, string name, string alias)
        {
            var t = new FromSourceTableOrView(schema, name, alias);
            return AddTableOrView(t);
        }

        


        public PgSqlCommand GetSqlCommand(TableDefinition tableDefinition)
        {
            var sqlCommand = new PgSqlCommand();

            if (_fromSources.Any())
                sqlCommand.AppendCommand($"{String.Join(", ", _fromSources.Select(fs => fs.GetSqlCommand(tableDefinition).Command))}");

            return sqlCommand;
        }
    }
}
