namespace doob.PgSql.Expressions.Connection
{
    public class ExpressionOr : ExpressionBase
    {
        public ExpressionOr() : base(null)
        {
        }

        protected override void _getSqlCommand(Column column)
        {
            SetCommand("OR");
        }
    }
}