using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using doob.PgSql.CustomTypes;
using doob.PgSql.Expressions;
using doob.PgSql.Expressions.Logical;
using doob.PgSql.ExtensionMethods;
using doob.PgSql.Interfaces.Where.Typed;

namespace doob.PgSql.Clauses.Typed
{
    public class TypedWhereClauseAnd<T> : ITypedWhereClauseLogicalAnd<T>, ITypedWhereClauseConnectionAnd<T>
    {

        protected List<ExpressionBase> Xpressions = new List<ExpressionBase>();

        internal TypedWhereClauseAnd(ExpressionBase firstxpressions)
        {
            Xpressions.Add(firstxpressions);
        }

        public ITypedWhereClauseLogicalAnd<T> Not()
        {
            var expr = new ExpressionNot();
            Xpressions.Add(expr);
            return this;
        }

        public ITypedWhereClauseConnectionAnd<T> Eq(string propertyName, object value)
        {
            var expr = new ExpressionEqual(propertyName, value);
            Xpressions.Add(expr);
            return this;
        }

        public ITypedWhereClauseConnectionAnd<T> Lt(string propertyName, object value)
        {
            var expr = new ExpressionLowerThan(propertyName, value);
            Xpressions.Add(expr);
            return this;
        }

        public ITypedWhereClauseConnectionAnd<T> Gt(string propertyName, object value)
        {
            var expr = new ExpressionGreaterThan(propertyName, value);
            Xpressions.Add(expr);
            return this;
        }

        public ITypedWhereClauseConnectionAnd<T> Lte(string propertyName, object value)
        {
            var expr = new ExpressionLowerThanOrEqual(propertyName, value);
            Xpressions.Add(expr);
            return this;
        }

        public ITypedWhereClauseConnectionAnd<T> Gte(string propertyName, object value)
        {
            var expr = new ExpressionGreaterThanOrEqual(propertyName, value);
            Xpressions.Add(expr);
            return this;
        }

        public ITypedWhereClauseConnectionAnd<T> Between(string propertyName, object min, object max)
        {
            var expr = new ExpressionBetween(propertyName, min, max);
            Xpressions.Add(expr);
            return this;
        }

        public ITypedWhereClauseConnectionAnd<T> IsNull(string propertyName)
        {
            var expr = new ExpressionIsNull(propertyName);
            Xpressions.Add(expr);
            return this;
        }

        public ITypedWhereClauseConnectionAnd<T> IsNotNull(string propertyName)
        {
            var expr = new ExpressionIsNotNull(propertyName);
            Xpressions.Add(expr);
            return this;
        }

        public ITypedWhereClauseConnectionAnd<T> Any<TField>(string propertyName, IEnumerable<TField> value)
        {
            var expr = new ExpressionAny<TField>(propertyName, value);
            Xpressions.Add(expr);
            return this;
        }

        public ITypedWhereClauseConnectionAnd<T> Contains(string propertyName, object value)
        {
            var expr = new ExpressionContains(propertyName, value);
            Xpressions.Add(expr);
            return this;
        }

        public ITypedWhereClauseConnectionAnd<T> Contains(string propertyName, string value, bool ignoreCase)
        {
            var expr = new ExpressionIContains(propertyName, value, ignoreCase);
            Xpressions.Add(expr);
            return this;
        }

        public ITypedWhereClauseConnectionAnd<T> Like(string propertyName, string value)
        {
            var expr = new ExpressionLike(propertyName, value, true, false);
            Xpressions.Add(expr);
            return this;
        }

        public ITypedWhereClauseConnectionAnd<T> Like(string propertyName, string value, bool ignoreCase, bool invertOrder)
        {
            var expr = new ExpressionLike(propertyName, value, ignoreCase, invertOrder);
            Xpressions.Add(expr);
            return this;
        }
        
        public ITypedWhereClauseConnectionAnd<T> Eq<TField>(Expression<Func<T, TField>> expression, TField value)
        {
            return Eq(expression.GetPropertyName(), value);
        }

        public ITypedWhereClauseConnectionAnd<T> Lt<TField>(Expression<Func<T, TField>> expression, TField value)
        {
            return Lt(expression.GetPropertyName(), value);
        }

        public ITypedWhereClauseConnectionAnd<T> Gt<TField>(Expression<Func<T, TField>> expression, TField value)
        {
            return Gt(expression.GetPropertyName(), value);
        }

        public ITypedWhereClauseConnectionAnd<T> Lte<TField>(Expression<Func<T, TField>> expression, TField value)
        {
            return Lte(expression.GetPropertyName(), value);
        }

        public ITypedWhereClauseConnectionAnd<T> Gte<TField>(Expression<Func<T, TField>> expression, TField value)
        {
            return Gte(expression.GetPropertyName(), value);
        }

        public ITypedWhereClauseConnectionAnd<T> Between<TField>(Expression<Func<T, TField>> expression, TField min, TField max)
        {
            return Between(expression.GetPropertyName(), min, max);
        }

        public ITypedWhereClauseConnectionAnd<T> IsNull<TField>(Expression<Func<T, TField>> expression)
        {
            return IsNull(expression.GetPropertyName());
        }

        public ITypedWhereClauseConnectionAnd<T> IsNotNull<TField>(Expression<Func<T, TField>> expression)
        {
            return IsNotNull(expression.GetPropertyName());
        }

        public ITypedWhereClauseConnectionAnd<T> Any<TField, TListField>(Expression<Func<T, TField>> expression, IEnumerable<TListField> value)
        {
            return Any(expression.GetPropertyName(), value?.Cast<object>());
        }

        public ITypedWhereClauseConnectionAnd<T> Contains<TField>(Expression<Func<T, TField>> expression, object value)
        {
            return Contains(expression.GetPropertyName(), value);
        }

        public ITypedWhereClauseConnectionAnd<T> Contains<TField>(Expression<Func<T, TField>> expression, string value, bool ignoreCase)
        {
            return Contains(expression.GetPropertyName(), value, ignoreCase);
        }

        public ITypedWhereClauseConnectionAnd<T> Like<TField>(Expression<Func<T, TField>> expression, string value)
        {
            return Like(expression.GetPropertyName(), value);
        }

        public ITypedWhereClauseConnectionAnd<T> Like<TField>(Expression<Func<T, TField>> expression, string value, bool ignoreCase)
        {
            return Like(expression.GetPropertyName(), value, ignoreCase, false);
        }

        public ITypedWhereClauseConnectionAnd<T> Like<TField>(string value, Expression<Func<T, TField>> expression)
        {
            return Like(expression.GetPropertyName(), value, false, true);
        }

        public ITypedWhereClauseConnectionAnd<T> Like<TField>(string value, Expression<Func<T, TField>> expression, bool ignoreCase)
        {
            return Like(expression.GetPropertyName(), value, ignoreCase, true);
        }


        #region LTree

        public ITypedWhereClauseConnectionAnd<T> LTreeMatch(string propertyName, string value)
        {
            var expr = new ExpressionLTreeMatch(propertyName, value);
            Xpressions.Add(expr);
            return this;
        }


        public ITypedWhereClauseConnectionAnd<T> LTreeMatch(Expression<Func<T, PgSqlLTree>> expression, string value)
        {
            return LTreeMatch(expression.GetPropertyName(), value);
        }


        #endregion



        public ITypedWhereClauseLogicalAnd<T> And()
        {
            return this;
        }


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
                    column = tableDefinition.GetColumnBuilderByDbName(xpression.ColumnName) ?? tableDefinition.GetColumnBuilderByClrName(xpression.ColumnName);
                }
                var comm = xpression.GetSqlCommand(column);
                if (not)
                    comm.Command = $"NOT {comm.Command}";

                comm.Parameters.ForEach(p =>
                {

                    if (p.Value.GetType().IsEnum)
                    {
                        p.Value = p.Value.ToString();
                    }

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
            command.Command = String.Join(" AND ", lsql);
            return command;
        }
    }
}