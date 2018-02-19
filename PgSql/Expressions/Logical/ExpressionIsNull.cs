namespace doob.PgSql.Expressions.Logical
{
    public class ExpressionIsNull : ExpressionBase
    {

        public ExpressionIsNull(string propertyName) : base(propertyName)
        {

        }

        protected override void _getSqlCommand(Column column)
        {
            SetCommand($"\"{ColumnName}\" IS NULL");
        }

    }
}