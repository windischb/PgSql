using doob.PgSql.Clauses;
using doob.PgSql.Interfaces;
using doob.PgSql.Interfaces.Where;

namespace doob.PgSql.Statements
{
    public class Delete : ISQLCommand
    {
        private From _fromClause;
        private IWhere _whereClause;
        private Returning _returning;

        public static Delete From(From from)
        {
            var del = new Delete();
            return del.AddClause(from);
        }

        public Delete Where(IWhere where)
        {
            return AddClause(where);
        }

        public Delete Returning(Returning returning)
        {
            return AddClause(returning);
        }

        public Delete AddClause(IDeleteMember clause)
        {
            switch (clause)
            {
                case From from:
                    _fromClause = from;
                    break;
                case IWhere where:
                    _whereClause = where;
                    break;
                case Returning returning:
                    _returning = returning;
                    break;
            }

            return this;
        }

        public PgSqlCommand GetSqlCommand()
        {
            return GetSqlCommand(null);
        }


        public PgSqlCommand GetSqlCommand(TableDefinition tableDefinition)
        {
            var sqlCommand = new PgSqlCommand();

            var fromComm = _fromClause.GetSqlCommand(tableDefinition);
            sqlCommand.AppendCommandLine($" DELETE FROM {fromComm.Command}", fromComm.Parameters);

            if (_whereClause != null)
            {
                var whereComm = _whereClause.GetSqlCommand(tableDefinition);
                sqlCommand.AppendCommandLine($"WHERE {whereComm.Command}", whereComm.Parameters);
            }

            if (_returning != null)
            {
                sqlCommand.AppendCommand($"RETURNING {_returning.GetSqlCommand(tableDefinition).Command}");
            }

            return sqlCommand;
        }
    }
}
