using System.Linq;

namespace doob.PgSql.Expressions.Logical
{
    public class ExpressionAny : ExpressionBase
    {

        public ExpressionAny(string propertyName, params object[] value) : base(propertyName)
        {
            SetValue("any", value.ToArray());
        }

        protected override void _getSqlCommand(Column column)
        {
            SetCommand($"\"{ColumnName}\" = ANY(@{GetId("any")})");
        }
    }
}