namespace doob.PgSql.Expressions.Logical
{
    public class ExpressionGreaterThanOrEqual : ExpressionBase
    {

        public ExpressionGreaterThanOrEqual(string propertyName, object value) : base(propertyName)
        {
            SetValue("gte", value);
        }

        protected override void _getSqlCommand(Column column)
        {
            SetCommand($"\"{ColumnName}\" >= @{GetId("gte")}");
        }
    }
}