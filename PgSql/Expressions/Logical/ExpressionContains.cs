using System.Collections.Generic;

namespace doob.PgSql.Expressions.Logical
{
    public class ExpressionContains: ExpressionBase
    {

        public ExpressionContains(string propertyName, object value) : base(propertyName)
        {
            var arr = new List<object>(){value};
            SetValue("contains", arr.ToArray());
        }

        protected override void _getSqlCommand(Column column)
        {

            SetCommand($"\"{ColumnName}\" @> @{GetId("contains")}");
        }
    }
}