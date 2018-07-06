using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using doob.PgSql.Attributes;

namespace doob.PgSql.ExtensionMethods
{
    public static class ExpressionExtensions
    {
        public static string GetPropertyName<T, TField>(this Expression<Func<T, TField>> field) {
            
            var member = field.Body as MemberExpression;
            var unary = field.Body as UnaryExpression;
            var expr = member ?? unary?.Operand as MemberExpression;
            var name = String.Join(".", expr?.ToString().Split('.').Skip(1));

            var type = typeof(T);
            var prop = type.GetProperty(name);
            var colAttr = prop?.GetCustomAttribute<PgSqlColumnAttribute>();
            if (colAttr != null && !String.IsNullOrWhiteSpace(colAttr.Name))
                name = colAttr.Name;

            return name;
        }
    }
}
