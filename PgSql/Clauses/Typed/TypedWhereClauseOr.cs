using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using doob.PgSql.CustomTypes;
using doob.PgSql.Expressions;
using doob.PgSql.Expressions.Logical;
using doob.PgSql.ExtensionMethods;
using doob.PgSql.Interfaces.Where.Typed;

namespace doob.PgSql.Clauses.Typed
{
    public class TypedWhereClauseOr<T> : ITypedWhereClauseLogicalOr<T>, ITypedWhereClauseConnectionOr<T>
    {
        protected List<ExpressionBase> Xpressions = new List<ExpressionBase>();

        internal TypedWhereClauseOr(ExpressionBase firstxpressions)
        {
            Xpressions.Add(firstxpressions);
        }

        public ITypedWhereClauseLogicalOr<T> Not()
        {
            var expr = new ExpressionNot();
            Xpressions.Add(expr);
            return this;
        }

        public ITypedWhereClauseConnectionOr<T> Eq(string propertyName, object value)
        {
            var expr = new ExpressionEqual(propertyName, value);
            Xpressions.Add(expr);
            return this;
        }

        public ITypedWhereClauseConnectionOr<T> Lt(string propertyName, object value)
        {
            var expr = new ExpressionLowerThan(propertyName, value);
            Xpressions.Add(expr);
            return this;
        }

        public ITypedWhereClauseConnectionOr<T> Gt(string propertyName, object value)
        {
            var expr = new ExpressionGreaterThan(propertyName, value);
            Xpressions.Add(expr);
            return this;
        }

        public ITypedWhereClauseConnectionOr<T> Lte(string propertyName, object value)
        {
            var expr = new ExpressionLowerThanOrEqual(propertyName, value);
            Xpressions.Add(expr);
            return this;
        }

        public ITypedWhereClauseConnectionOr<T> Gte(string propertyName, object value)
        {
            var expr = new ExpressionGreaterThanOrEqual(propertyName, value);
            Xpressions.Add(expr);
            return this;
        }

        public ITypedWhereClauseConnectionOr<T> Between(string propertyName, object min, object max)
        {
            var expr = new ExpressionBetween(propertyName, min, max);
            Xpressions.Add(expr);
            return this;
        }

        public ITypedWhereClauseConnectionOr<T> IsNull(string propertyName)
        {
            var expr = new ExpressionIsNull(propertyName);
            Xpressions.Add(expr);
            return this;
        }

        public ITypedWhereClauseConnectionOr<T> IsNotNull(string propertyName)
        {
            var expr = new ExpressionIsNotNull(propertyName);
            Xpressions.Add(expr);
            return this;
        }

        public ITypedWhereClauseConnectionOr<T> Any(string propertyName, params object[] value)
        {
            var expr = new ExpressionAny(propertyName, value);
            Xpressions.Add(expr);
            return this;
        }

        public ITypedWhereClauseConnectionOr<T> Contains(string propertyName, object value)
        {
            var expr = new ExpressionContains(propertyName, value);
            Xpressions.Add(expr);
            return this;
        }

        public ITypedWhereClauseConnectionOr<T> Contains(string propertyName, string value, bool ignoreCase)
        {
            var expr = new ExpressionIContains(propertyName, value, ignoreCase);
            Xpressions.Add(expr);
            return this;
        }

        public ITypedWhereClauseConnectionOr<T> Like(string propertyName, string value)
        {
            var expr = new ExpressionLike(propertyName, value, true);
            Xpressions.Add(expr);
            return this;
        }

        public ITypedWhereClauseConnectionOr<T> Like(string propertyName, string value, bool ignoreCase)
        {
            var expr = new ExpressionLike(propertyName, value, ignoreCase);
            Xpressions.Add(expr);
            return this;
        }

        public ITypedWhereClauseConnectionOr<T> Eq<TField>(Expression<Func<T, TField>> expression, TField value)
        {
            return Eq(expression.GetPropertyName(), value);
        }

        public ITypedWhereClauseConnectionOr<T> Lt<TField>(Expression<Func<T, TField>> expression, TField value)
        {
            return Lt(expression.GetPropertyName(), value);
        }

        public ITypedWhereClauseConnectionOr<T> Gt<TField>(Expression<Func<T, TField>> expression, TField value)
        {
            return Gt(expression.GetPropertyName(), value);
        }

        public ITypedWhereClauseConnectionOr<T> Lte<TField>(Expression<Func<T, TField>> expression, TField value)
        {
            return Lte(expression.GetPropertyName(), value);
        }

        public ITypedWhereClauseConnectionOr<T> Gte<TField>(Expression<Func<T, TField>> expression, TField value)
        {
            return Gte(expression.GetPropertyName(), value);
        }

        public ITypedWhereClauseConnectionOr<T> Between<TField>(Expression<Func<T, TField>> expression, TField min, TField max)
        {
            return Between(expression.GetPropertyName(), min, max);
        }

        public ITypedWhereClauseConnectionOr<T> IsNull<TField>(Expression<Func<T, TField>> expression)
        {
            return IsNull(expression.GetPropertyName());
        }

        public ITypedWhereClauseConnectionOr<T> IsNotNull<TField>(Expression<Func<T, TField>> expression)
        {
            return IsNotNull(expression.GetPropertyName());
        }

        public ITypedWhereClauseConnectionOr<T> Any<TField>(Expression<Func<T, TField>> expression, params object[] value)
        {
            return Any(expression.GetPropertyName(), value);
        }

        public ITypedWhereClauseConnectionOr<T> Contains<TField>(Expression<Func<T, TField>> expression, object value)
        {
            return Contains(expression.GetPropertyName(), value);
        }

        public ITypedWhereClauseConnectionOr<T> Contains<TField>(Expression<Func<T, TField>> expression, string value, bool ignoreCase)
        {
            return Contains(expression.GetPropertyName(), value, ignoreCase);
        }

        public ITypedWhereClauseConnectionOr<T> Like<TField>(Expression<Func<T, TField>> expression, string value)
        {
            return Like(expression.GetPropertyName(), value);
        }

        public ITypedWhereClauseConnectionOr<T> Like<TField>(Expression<Func<T, TField>> expression, string value, bool ignoreCase)
        {
            return Like(expression.GetPropertyName(), value, ignoreCase);
        }

        public ITypedWhereClauseLogicalOr<T> Or()
        {
            return this;
        }

        #region LTree

        public ITypedWhereClauseConnectionOr<T> LTreeMatch(string propertyName, string value)
        {
            var expr = new ExpressionLTreeMatch(propertyName, value);
            Xpressions.Add(expr);
            return this;
        }

        public ITypedWhereClauseConnectionOr<T> LTreeMatch(Expression<Func<T, PgSqlLTree>> expression, string value)
        {
            return LTreeMatch(expression.GetPropertyName(), value);
        }


        #endregion


        public PgSqlCommand GetSqlCommand(TableDefinition tableDefinition)
        {
            var command = new PgSqlCommand();
            var lsql = new List<string>();
            bool not = false;
            foreach (var xpression in Xpressions)
            {
                if (xpression is ExpressionNot)
                {
                    not = true;
                    continue;
                }

                ColumnBuilder column = null;
                if (xpression.ColumnName != null)
                {
                    column = tableDefinition.GetColumnBuilderByDbName(xpression.ColumnName);
                }
                var comm = xpression.GetSqlCommand(column);
                if (not)
                    comm.Command = $"NOT {comm.Command}";

                comm.Parameters.ForEach(p =>
                {
                    if (command.ParameterIdExists(p.UniqueId))
                    {
                        var newParam = p.RebuildWithNewId();
                        comm.Command = comm.Command.Replace($"@{p.UniqueId}", $"@{newParam.UniqueId}");
                        command.Parameters.Add(newParam);
                    }
                    else
                    {
                        command.Parameters.Add(p);
                    }
                });
                lsql.Add($"({comm.Command})");

                not = false;
            }
            command.Command = String.Join(" OR ", lsql);
            return command;
        }
    }
}