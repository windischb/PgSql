using System.Collections.Generic;
using System.Linq;

namespace doob.PgSql.Expressions.Logical
{
    public class ExpressionAny<TField> : ExpressionBase
    {

        public ExpressionAny(string propertyName, IEnumerable<TField> value) : base(propertyName)
        {
            SetValue("any", value.ToArray());
            //OverrideValueType("any","uuid[]");
        }

        protected override void _getSqlCommand(Column column)
        {
            SetCommand($"\"{ColumnName}\" = ANY(@{GetId("any")})");
        }
    }
}