using System;
using System.Collections.Generic;
using System.Linq;
using doob.PgSql.Interfaces;

namespace doob.PgSql.Clauses
{
    public class Returning : IInsertMember, IDeleteMember
    {

        private readonly List<IReturningValue> _returningValue;



        private Returning(params IReturningValue[] returningValues)
        {
            _returningValue = returningValues.ToList();
        }

        private Returning(List<IReturningValue> returningValues)
        {
            _returningValue = returningValues;
        }

        public static Returning Columns(params IReturningValue[] returningValues)
        {
            return new Returning(returningValues);
        }
        public static Returning Columns(params string[] columns)
        {
            var _columns = new List<IReturningValue>();
            foreach (var column in columns)
            {
                _columns.Add(new ReturningColumn(column));
            }
            return new Returning(_columns);
        }
        public static Returning Column(string column, string alias)
        {
            return new Returning(new ReturningColumn(column));
        }


        public Returning AndColumns(string column)
        {
            _returningValue.Add(new ReturningColumn(column));
            return this;
        }
        public Returning AndColumn(string column, string alias)
        {
            _returningValue.Add(new ReturningColumn(column, alias));
            return this;
        }


        public PgSqlCommand GetSqlCommand(TableDefinition tableDefinition)
        {
            var sqlCommand = new PgSqlCommand();

            if (_returningValue?.Any() == true)
                sqlCommand.AppendCommand($"{String.Join(", ", _returningValue.Select(r => r.GetSqlCommand(tableDefinition).Command))}");

            return sqlCommand;
        }
    }
}
