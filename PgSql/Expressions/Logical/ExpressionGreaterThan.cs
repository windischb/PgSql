namespace doob.PgSql.Expressions.Logical
{
    public class ExpressionGreaterThan : ExpressionBase
    {

        public ExpressionGreaterThan(string propertyName, object value) : base(propertyName)
        {
            SetValue("gt", value);
        }

        protected override void _getSqlCommand(Column column)
        {
            SetCommand($"\"{ColumnName}\" > @{GetId("gt")}");
        }
    }
}