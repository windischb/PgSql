namespace doob.PgSql.Expressions.Logical
{
    public class ExpressionLowerThanOrEqual : ExpressionBase
    {

        public ExpressionLowerThanOrEqual(string propertyName, object value) : base(propertyName)
        {
            SetValue("lte", value);
        }

        protected override void _getSqlCommand(Column column)
        {
            SetCommand($"\"{ColumnName}\" <= @{GetId("lte")}");
        }
    }
}