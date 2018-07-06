using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using doob.PgSql.ExtensionMethods;
using Newtonsoft.Json.Linq;

namespace doob.PgSql.QueryGeneration
{
    public static class BinaryExpressionOperators
    {
        private static readonly Dictionary<ExpressionType, string> TextValueOperators =
            new Dictionary<ExpressionType, string>()
            {
                { ExpressionType.Equal,                 " = " },
                { ExpressionType.NotEqual,              " != " },
                { ExpressionType.GreaterThan,           " > " },
                { ExpressionType.GreaterThanOrEqual,    " >= " },
                { ExpressionType.LessThan,              " < " },
                { ExpressionType.LessThanOrEqual,       " <= " },

                { ExpressionType.Add,                   " + " },
                { ExpressionType.AddChecked,            " + " },
                { ExpressionType.Subtract,              " - " },
                { ExpressionType.SubtractChecked,       " - " },
                { ExpressionType.Multiply,              " * " },
                { ExpressionType.MultiplyChecked,       " * " },
                { ExpressionType.Divide,                " / " },
                { ExpressionType.Modulo,                " % " },

                { ExpressionType.And,                   " & " },
                { ExpressionType.Or,                    " | " },
                { ExpressionType.ExclusiveOr,           " # " },
                { ExpressionType.LeftShift,             " << " },
                { ExpressionType.RightShift,            " >> " },

                { ExpressionType.AndAlso,               " AND " },
                { ExpressionType.OrElse,                " OR " }
            };

        private static readonly Dictionary<ExpressionType, string> JsonValueOperators =
            new Dictionary<ExpressionType, string>()
            {
                { ExpressionType.Equal,                 " = " },
                { ExpressionType.NotEqual,              " != " },
                { ExpressionType.GreaterThan,           " > " },
                { ExpressionType.GreaterThanOrEqual,    " >= " },
                { ExpressionType.LessThan,              " < " },
                { ExpressionType.LessThanOrEqual,       " <= " },

                { ExpressionType.Add,                   " + " },
                { ExpressionType.AddChecked,            " + " },
                { ExpressionType.Subtract,              " - " },
                { ExpressionType.SubtractChecked,       " - " },
                { ExpressionType.Multiply,              " * " },
                { ExpressionType.MultiplyChecked,       " * " },
                { ExpressionType.Divide,                " / " },
                { ExpressionType.Modulo,                " % " },

                { ExpressionType.And,                   " & " },
                { ExpressionType.Or,                    " | " },
                { ExpressionType.ExclusiveOr,           " # " },
                { ExpressionType.LeftShift,             " << " },
                { ExpressionType.RightShift,            " >> " },

                { ExpressionType.AndAlso,               " AND " },
                { ExpressionType.OrElse,                " OR " }
            };


        private static string GetJsonOperator(ExpressionType expressionType)
        {
            if (JsonValueOperators.ContainsKey(expressionType))
                return JsonValueOperators[expressionType];

            return TextValueOperators[expressionType];
        }


        public static string GetOperator(ExpressionType expressionType, Column column)
        {

            var jTokenType = column?.DotNetType?.GetJTokenType() ?? JTokenType.String;

            switch (jTokenType)
            {
                case JTokenType.Object:
                {
                    return GetJsonOperator(expressionType);
                }

                default:
                {
                    return TextValueOperators[expressionType];
                }
            }


        }
    }
}
