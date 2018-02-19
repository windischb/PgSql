namespace doob.PgSql.Expressions.Logical
{
    public class ExpressionBetween : ExpressionBase
    {
        
        public ExpressionBetween(string columnName, object min, object max) : base(columnName)
        {
            SetValue("min", min);
            SetValue("max", max);
        }

        protected override void _getSqlCommand(Column column)
        {
            SetCommand($"\"{ColumnName}\" BETWEEN @{GetId("min")} AND @{GetId("max")}");
        }

    }
}