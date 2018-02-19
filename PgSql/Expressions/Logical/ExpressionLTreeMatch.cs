using doob.PgSql.CustomTypes;

namespace doob.PgSql.Expressions.Logical
{
    public class ExpressionLTreeMatch : ExpressionBase
    {

        public ExpressionLTreeMatch(string propertyName, string value) : base(propertyName)
        {
            SetValue("~", new PgSqlLTree(value));
            
        }

        protected override void _getSqlCommand(Column column)
        {
            SetCommand($"\"{ColumnName}\" ~ @{GetId("~")}");
        }
    }

    
}