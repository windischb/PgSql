using doob.PgSql.Interfaces;

namespace doob.PgSql.Clauses
{
    public class OffsetClauseNumber : IOffsetClauseItem
    {
        public int Number { get; set; }

        private OffsetClauseNumber()
        {
            
        }
        public OffsetClauseNumber(int number)
        {
            Number = number;
        }

        public override string ToString()
        {
            return Number.ToString();
        }

        public PgSqlCommand GetSqlCommand(TableDefinition tableDefinition)
        {
            var sqlCommand = new PgSqlCommand();

            string num = Number.ToString();

            sqlCommand.AppendCommand(num);
            return sqlCommand;
        }
    }
}
