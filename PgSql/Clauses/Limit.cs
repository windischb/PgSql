using doob.PgSql.Interfaces;

namespace doob.PgSql.Clauses
{
    public class Limit : ISelectMember
    {
        private ILimitClauseItem _limitClauseItem;

        private Limit() { }
        private Limit(ILimitClauseItem limitClauseItem)
        {
            _limitClauseItem = limitClauseItem;
        }

        public static Limit WithNumber(LimitClauseNumber limitClauseNumber)
        {
            return new Limit(limitClauseNumber);
        }
        public static Limit WithNumber(int number)
        {
            return new Limit().SetNumber(number);
        }

        public Limit SetNumber(int number)
        {
            var n = new LimitClauseNumber(number);
            _limitClauseItem = n;
            return this;
        }


        public PgSqlCommand GetSqlCommand(TableDefinition tableDefinition)
        {
            var sqlCommand = new PgSqlCommand();

            if (_limitClauseItem != null)
            {
                var limit = _limitClauseItem.GetSqlCommand(tableDefinition).Command;
                sqlCommand.AppendCommand($"{limit}");
            }


            return sqlCommand;
        }
    }
}
