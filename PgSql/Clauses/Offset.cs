using doob.PgSql.Interfaces;

namespace doob.PgSql.Clauses
{
    public class Offset : ISelectMember
    {
        private IOffsetClauseItem _offsetClauseItem;

        private Offset() { }
        private Offset(IOffsetClauseItem offsetClauseItem)
        {
            _offsetClauseItem = offsetClauseItem;
        }

        public static Offset WithNumber(OffsetClauseNumber offsetClauseNumber)
        {
            return new Offset(offsetClauseNumber);
        }
        public static Offset WithNumber(int number)
        {
            return new Offset().SetNumber(number);
        }

        public Offset SetNumber(int number)
        {
            var n = new OffsetClauseNumber(number);
            _offsetClauseItem = n;
            return this;
        }


        public PgSqlCommand GetSqlCommand(TableDefinition tableDefinition)
        {
            var sqlCommand = new PgSqlCommand();

            if (_offsetClauseItem != null)
                sqlCommand.AppendCommand($"{_offsetClauseItem}");

            return sqlCommand;
        }
    }
}
