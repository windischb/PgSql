using System;
using System.Linq.Expressions;
using doob.PgSql.Clauses.Typed;
using doob.PgSql.CustomTypes;
using doob.PgSql.Expressions;
using doob.PgSql.Expressions.Logical;
using doob.PgSql.ExtensionMethods;
using doob.PgSql.Interfaces.Where;
using doob.PgSql.Interfaces.Where.Typed;

namespace doob.PgSql.Clauses
{
    public class TypedWhereClause<T> : ITypedWhereClauseLogicalBase<T>, ITypedWhereClauseConnectionBase<T>, ITypedWhereClauseLogical<T>
    {

        private ExpressionBase _first;

        public IMergeWhereClause Merge(IWhere query)
        {
            return new MergeWhereClause(query);
        }

        public ITypedWhereClauseLogicalAnd<T> Not()
        {
            _first = new ExpressionNot();
            return And();
            //var andquery = new TypedWhereClauseAnd<T>(_first);
            //return andquery;
            //_first = new ExpressionNot();
            //return this;
        }

        public ITypedWhereClauseConnectionBase<T> Eq(string propertyName, object value)
        {
            _first = new ExpressionEqual(propertyName, value);
            return this;
        }

        public ITypedWhereClauseConnectionBase<T> Lt(string propertyName, object value)
        {
            _first = new ExpressionLowerThan(propertyName, value);
            return this;
        }

        public ITypedWhereClauseConnectionBase<T> Gt(string propertyName, object value)
        {
            _first = new ExpressionGreaterThan(propertyName, value);
            return this;
        }

        public ITypedWhereClauseConnectionBase<T> Lte(string propertyName, object value)
        {
            _first = new ExpressionLowerThanOrEqual(propertyName, value);
            return this;
        }

        public ITypedWhereClauseConnectionBase<T> Gte(string propertyName, object value)
        {
            _first = new ExpressionGreaterThanOrEqual(propertyName, value);
            return this;
        }

        public ITypedWhereClauseConnectionBase<T> Between(string propertyName, object min, object max)
        {
            _first = new ExpressionBetween(propertyName, min, max);
            return this;
        }

        public ITypedWhereClauseConnectionBase<T> IsNull(string propertyName)
        {
            _first = new ExpressionIsNull(propertyName);
            return this;
        }

        public ITypedWhereClauseConnectionBase<T> IsNotNull(string propertyName)
        {
            _first = new ExpressionIsNotNull(propertyName);
            return this;
        }

        public ITypedWhereClauseConnectionBase<T> Any(string propertyName, params object[] value)
        {
            _first = new ExpressionAny(propertyName, value);
            return this;
        }

        public ITypedWhereClauseConnectionBase<T> Contains(string propertyName, object value)
        {
            _first = new ExpressionContains(propertyName, value);
            return this;
        }

        public ITypedWhereClauseConnectionBase<T> Contains(string propertyName, string value, bool ignoreCase)
        {
            
            _first = new ExpressionIContains(propertyName, value, ignoreCase);
            return this;
        }

        public ITypedWhereClauseConnectionBase<T> Like(string propertyName, string value)
        {
            _first = new ExpressionLike(propertyName, value, true);
            return this;
        }

        public ITypedWhereClauseConnectionBase<T> Like(string propertyName, string value, bool ignoreCase)
        {
            _first = new ExpressionLike(propertyName, value, ignoreCase);
            return this;
        }

        public ITypedWhereClauseConnectionBase<T> Eq<TField>(Expression<Func<T, TField>> expression, TField value)
        {
            return Eq(expression.GetPropertyName(), value);
        }

        public ITypedWhereClauseConnectionBase<T> Lt<TField>(Expression<Func<T, TField>> expression, TField value)
        {
            return Lt(expression.GetPropertyName(), value);
        }

        public ITypedWhereClauseConnectionBase<T> Gt<TField>(Expression<Func<T, TField>> expression, TField value)
        {
            return Gt(expression.GetPropertyName(), value);
        }

        public ITypedWhereClauseConnectionBase<T> Lte<TField>(Expression<Func<T, TField>> expression, TField value)
        {
            return Lte(expression.GetPropertyName(), value);
        }

        public ITypedWhereClauseConnectionBase<T> Gte<TField>(Expression<Func<T, TField>> expression, TField value)
        {
            return Gte(expression.GetPropertyName(), value);
        }

        public ITypedWhereClauseConnectionBase<T> Between<TField>(Expression<Func<T, TField>> expression, TField min, TField max)
        {
            return Between(expression.GetPropertyName(), min, max);
        }

        public ITypedWhereClauseConnectionBase<T> IsNull<TField>(Expression<Func<T, TField>> expression)
        {
            return IsNull(expression.GetPropertyName());
        }

        public ITypedWhereClauseConnectionBase<T> IsNotNull<TField>(Expression<Func<T, TField>> expression)
        {
            return IsNotNull(expression.GetPropertyName());
        }

        public ITypedWhereClauseConnectionBase<T> Any<TField>(Expression<Func<T, TField>> expression, params object[] value)
        {
            return Any(expression.GetPropertyName(), value);
        }

        public ITypedWhereClauseConnectionBase<T> Contains<TField>(Expression<Func<T, TField>> expression, object value)
        {
            return Contains(expression.GetPropertyName(), value);
        }

        public ITypedWhereClauseConnectionBase<T> Contains<TField>(Expression<Func<T, TField>> expression, string value, bool ignoreCase)
        {
            return Contains(expression.GetPropertyName(), value, ignoreCase);
        }

        public ITypedWhereClauseConnectionBase<T> Like<TField>(Expression<Func<T, TField>> expression, string value)
        {
            return Like(expression.GetPropertyName(), value);
        }

        public ITypedWhereClauseConnectionBase<T> Like<TField>(Expression<Func<T, TField>> expression, string value, bool ignoreCase)
        {
            return Like(expression.GetPropertyName(), value, ignoreCase);
        }

        public ITypedWhereClauseLogicalAnd<T> And()
        {
            var andquery = new TypedWhereClauseAnd<T>(_first);
            return andquery;
        }

        public ITypedWhereClauseLogicalOr<T> Or()
        {
            var orquery = new TypedWhereClauseOr<T>(_first);
            return orquery;
        }

        #region LTree

        public ITypedWhereClauseConnectionBase<T> LTreeMatch(string propertyName, string value)
        {
            _first = new ExpressionLTreeMatch(propertyName, value);
            return this;
        }

        public ITypedWhereClauseConnectionBase<T> LTreeMatch(Expression<Func<T, PgSqlLTree>> expression, string value)
        {
            return LTreeMatch(expression.GetPropertyName(), value);
        }


        #endregion



        public PgSqlCommand GetSqlCommand(TableDefinition tableDefinition)
        {
            var column = tableDefinition?.GetColumnBuilderByClrName(_first.ColumnName) ?? tableDefinition?.GetColumnBuilderByDbName(_first.ColumnName);
            
            return _first.GetSqlCommand(column);
        }
    }
}
