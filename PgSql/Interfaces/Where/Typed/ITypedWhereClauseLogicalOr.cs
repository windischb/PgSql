using System;
using System.Linq.Expressions;
using doob.PgSql.CustomTypes;

namespace doob.PgSql.Interfaces.Where.Typed
{
    public interface ITypedWhereClauseLogicalOr<T>
    {
        ITypedWhereClauseLogicalOr<T> Not();

        ITypedWhereClauseConnectionOr<T> Eq(string propertyName, object value);
        ITypedWhereClauseConnectionOr<T> Lt(string propertyName, object value);
        ITypedWhereClauseConnectionOr<T> Gt(string propertyName, object value);
        ITypedWhereClauseConnectionOr<T> Lte(string propertyName, object value);
        ITypedWhereClauseConnectionOr<T> Gte(string propertyName, object value);
        ITypedWhereClauseConnectionOr<T> Between(string propertyName, object min, object max);
        ITypedWhereClauseConnectionOr<T> IsNull(string propertyName);
        ITypedWhereClauseConnectionOr<T> IsNotNull(string propertyName);
        ITypedWhereClauseConnectionOr<T> Any(string propertyName, params object[] value);
        ITypedWhereClauseConnectionOr<T> Contains(string propertyName, object value);
        ITypedWhereClauseConnectionOr<T> Contains(string propertyName, string value, bool ignoreCase);
        ITypedWhereClauseConnectionOr<T> Like(string propertyName, string value);
        ITypedWhereClauseConnectionOr<T> Like(string propertyName, string value, bool ignoreCase);

        ITypedWhereClauseConnectionOr<T> Eq<TField>(Expression<Func<T, TField>> expression, TField value);
        ITypedWhereClauseConnectionOr<T> Lt<TField>(Expression<Func<T, TField>> expression, TField value);
        ITypedWhereClauseConnectionOr<T> Gt<TField>(Expression<Func<T, TField>> expression, TField value);
        ITypedWhereClauseConnectionOr<T> Lte<TField>(Expression<Func<T, TField>> expression, TField value);
        ITypedWhereClauseConnectionOr<T> Gte<TField>(Expression<Func<T, TField>> expression, TField value);
        ITypedWhereClauseConnectionOr<T> Between<TField>(Expression<Func<T, TField>> expression, TField min, TField max);
        ITypedWhereClauseConnectionOr<T> IsNull<TField>(Expression<Func<T, TField>> expression);
        ITypedWhereClauseConnectionOr<T> IsNotNull<TField>(Expression<Func<T, TField>> expression);
        ITypedWhereClauseConnectionOr<T> Any<TField>(Expression<Func<T, TField>> expression, params object[] value);
        ITypedWhereClauseConnectionOr<T> Contains<TField>(Expression<Func<T, TField>> expression, object value);
        ITypedWhereClauseConnectionOr<T> Contains<TField>(Expression<Func<T, TField>> expression, string value, bool ignoreCase);
        ITypedWhereClauseConnectionOr<T> Like<TField>(Expression<Func<T, TField>> expression, string value);
        ITypedWhereClauseConnectionOr<T> Like<TField>(Expression<Func<T, TField>> expression, string value, bool ignoreCase);

        #region LTree
        ITypedWhereClauseConnectionOr<T> LTreeMatch(string propertyName, string value);
        ITypedWhereClauseConnectionOr<T> LTreeMatch(Expression<Func<T, PgSqlLTree>> expression, string value);
        #endregion
    }
}
