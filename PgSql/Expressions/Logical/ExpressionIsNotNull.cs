namespace doob.PgSql.Expressions.Logical
{
    public class ExpressionIsNotNull : ExpressionBase
    {

        public ExpressionIsNotNull(string propertyName) : base(propertyName)
        {

        }

        protected override void _getSqlCommand(Column column)
        {
            SetCommand($"\"{ColumnName}\" IS NOT NULL");
        }

    }
}