namespace doob.PgSql.Expressions.Connection
{
    public class ExpressionAnd : ExpressionBase
    {
        public ExpressionAnd() : base(null)
        {
        }

        protected override void _getSqlCommand(Column column)
        {
            SetCommand("AND");
        }
    }
}