using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using doob.PgSql.CustomTypes;

namespace doob.PgSql.Interfaces.Where.Typed
{
    public interface ITypedWhereClauseLogicalBase<T>
    {
        IMergeWhereClause Merge(IWhere query);

        ITypedWhereClauseLogicalAnd<T> Not();

        ITypedWhereClauseConnectionBase<T> Eq(string propertyName, object value);
        ITypedWhereClauseConnectionBase<T> Lt(string propertyName, object value);
        ITypedWhereClauseConnectionBase<T> Gt(string propertyName, object value);
        ITypedWhereClauseConnectionBase<T> Lte(string propertyName, object value);
        ITypedWhereClauseConnectionBase<T> Gte(string propertyName, object value);
        ITypedWhereClauseConnectionBase<T> Between(string propertyName, object min, object max);
        ITypedWhereClauseConnectionBase<T> IsNull(string propertyName);
        ITypedWhereClauseConnectionBase<T> IsNotNull(string propertyName);
        ITypedWhereClauseConnectionBase<T> Any<TField>(string propertyName, IEnumerable<TField> value);
        ITypedWhereClauseConnectionBase<T> Contains(string propertyName, object value);
        ITypedWhereClauseConnectionBase<T> Contains(string propertyName, string value, bool ignoreCase);
        ITypedWhereClauseConnectionBase<T> Like(string propertyName, string value);
        ITypedWhereClauseConnectionBase<T> Like(string propertyName, string value, bool ignoreCase, bool invertOrder);

        ITypedWhereClauseConnectionBase<T> Eq<TField>(Expression<Func<T, TField>> expression, TField value);
        ITypedWhereClauseConnectionBase<T> Lt<TField>(Expression<Func<T, TField>> expression, TField value);
        ITypedWhereClauseConnectionBase<T> Gt<TField>(Expression<Func<T, TField>> expression, TField value);
        ITypedWhereClauseConnectionBase<T> Lte<TField>(Expression<Func<T, TField>> expression, TField value);
        ITypedWhereClauseConnectionBase<T> Gte<TField>(Expression<Func<T, TField>> expression, TField value);
        ITypedWhereClauseConnectionBase<T> Between<TField>(Expression<Func<T, TField>> expression, TField min, TField max);
        ITypedWhereClauseConnectionBase<T> IsNull<TField>(Expression<Func<T, TField>> expression);
        ITypedWhereClauseConnectionBase<T> IsNotNull<TField>(Expression<Func<T, TField>> expression);
        ITypedWhereClauseConnectionBase<T> Any<TField, TListField>(Expression<Func<T, TField>> expression, IEnumerable<TListField> value);
        ITypedWhereClauseConnectionBase<T> Contains<TField>(Expression<Func<T, TField>> expression, object value);
        ITypedWhereClauseConnectionBase<T> Contains<TField>(Expression<Func<T, TField>> expression, string value, bool ignoreCase);
        ITypedWhereClauseConnectionBase<T> Like<TField>(Expression<Func<T, TField>> expression, string value);
        ITypedWhereClauseConnectionBase<T> Like<TField>(Expression<Func<T, TField>> expression, string value, bool ignoreCase);

        ITypedWhereClauseConnectionBase<T> Like<TField>(string value, Expression<Func<T, TField>> expression);
        ITypedWhereClauseConnectionBase<T> Like<TField>(string value, Expression<Func<T, TField>> expression, bool ignoreCase);

        #region LTree
        ITypedWhereClauseConnectionBase<T> LTreeMatch(string propertyName, string value);
        ITypedWhereClauseConnectionBase<T> LTreeMatch(Expression<Func<T, PgSqlLTree>> expression, string value);
        #endregion

    }
}
