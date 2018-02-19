namespace doob.PgSql.Expressions.Logical
{
    public class ExpressionLowerThan : ExpressionBase
    {

        public ExpressionLowerThan(string propertyName, object value) : base(propertyName)
        {
            SetValue("lt", value);
        }

        protected override void _getSqlCommand(Column column)
        {
            SetCommand($"\"{ColumnName}\" < @{GetId("lt")}");
        }
    }
}