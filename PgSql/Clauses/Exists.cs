using doob.PgSql.Interfaces;
using doob.PgSql.Statements;

namespace doob.PgSql.Clauses
{
    public class Exists : ISelectMember
    {
        private Select _select;

        private Exists() { }
        public Exists(Select select)
        {
            _select = select;
        }


        public PgSqlCommand GetSqlCommand(TableDefinition tableDefinition)
        {
            var sqlCommand = new PgSqlCommand();

            var selectCommand = _select.GetSqlCommand(tableDefinition);
            sqlCommand.AppendCommand($"EXISTS ({selectCommand.Command})", selectCommand.Parameters);

            return sqlCommand;
        }
    }
}
