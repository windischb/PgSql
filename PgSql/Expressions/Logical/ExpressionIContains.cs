using System.Collections.Generic;

namespace doob.PgSql.Expressions.Logical
{
    public class ExpressionIContains : ExpressionBase
    {
        private readonly bool _ignoreCase;

        public ExpressionIContains(string propertyName, string value, bool ignoreCase) : base(propertyName)
        {
            _ignoreCase = ignoreCase;

            var arr = new List<string>() { value };
            SetValue("icontains", arr.ToArray());
        }

        protected override void _getSqlCommand(Column column)
        {

            if (_ignoreCase)
            {
                SetCommand($"lower(\"{ColumnName}\"::text)::text[] @> @{GetId("icontains")}");
            }
            else
            {
                SetCommand($"\"{ColumnName}\" @> @{GetId("contains")}");
            }


        }
    }
}