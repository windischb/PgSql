using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using doob.PgSql.CustomTypes;

namespace doob.PgSql.Interfaces.Where.Typed
{
    public interface ITypedWhereClauseLogicalAnd<T>
    {
        ITypedWhereClauseLogicalAnd<T> Not();

        ITypedWhereClauseConnectionAnd<T> Eq(string propertyName, object value);
        ITypedWhereClauseConnectionAnd<T> Lt(string propertyName, object value);
        ITypedWhereClauseConnectionAnd<T> Gt(string propertyName, object value);
        ITypedWhereClauseConnectionAnd<T> Lte(string propertyName, object value);
        ITypedWhereClauseConnectionAnd<T> Gte(string propertyName, object value);
        ITypedWhereClauseConnectionAnd<T> Between(string propertyName, object min, object max);
        ITypedWhereClauseConnectionAnd<T> IsNull(string propertyName);
        ITypedWhereClauseConnectionAnd<T> IsNotNull(string propertyName);
        ITypedWhereClauseConnectionAnd<T> Any<TField>(string propertyName, IEnumerable<TField> value);
        ITypedWhereClauseConnectionAnd<T> Contains(string propertyName, object value);
        ITypedWhereClauseConnectionAnd<T> Contains(string propertyName, string value, bool ignoreCase);
        ITypedWhereClauseConnectionAnd<T> Like(string propertyName, string value);
        ITypedWhereClauseConnectionAnd<T> Like(string propertyName, string value, bool ignoreCase, bool invertOrder);

        ITypedWhereClauseConnectionAnd<T> Eq<TField>(Expression<Func<T, TField>> expression, TField value);
        ITypedWhereClauseConnectionAnd<T> Lt<TField>(Expression<Func<T, TField>> expression, TField value);
        ITypedWhereClauseConnectionAnd<T> Gt<TField>(Expression<Func<T, TField>> expression, TField value);
        ITypedWhereClauseConnectionAnd<T> Lte<TField>(Expression<Func<T, TField>> expression, TField value);
        ITypedWhereClauseConnectionAnd<T> Gte<TField>(Expression<Func<T, TField>> expression, TField value);
        ITypedWhereClauseConnectionAnd<T> Between<TField>(Expression<Func<T, TField>> expression, TField min, TField max);
        ITypedWhereClauseConnectionAnd<T> IsNull<TField>(Expression<Func<T, TField>> expression);
        ITypedWhereClauseConnectionAnd<T> IsNotNull<TField>(Expression<Func<T, TField>> expression);
        ITypedWhereClauseConnectionAnd<T> Any<TField, TListField>(Expression<Func<T, TField>> expression, IEnumerable<TListField> value);
        ITypedWhereClauseConnectionAnd<T> Contains<TField>(Expression<Func<T, TField>> expression, object value);
        ITypedWhereClauseConnectionAnd<T> Contains<TField>(Expression<Func<T, TField>> expression, string value, bool ignoreCase);
        ITypedWhereClauseConnectionAnd<T> Like<TField>(Expression<Func<T, TField>> expression, string value);
        ITypedWhereClauseConnectionAnd<T> Like<TField>(Expression<Func<T, TField>> expression, string value, bool ignoreCase);

        ITypedWhereClauseConnectionAnd<T> Like<TField>(string value, Expression<Func<T, TField>> expression);
        ITypedWhereClauseConnectionAnd<T> Like<TField>(string value, Expression<Func<T, TField>> expression, bool ignoreCase);

        #region LTree
        ITypedWhereClauseConnectionAnd<T> LTreeMatch(string propertyName, string value);
        ITypedWhereClauseConnectionAnd<T> LTreeMatch(Expression<Func<T, PgSqlLTree>> expression, string value);
        #endregion
    }

}
