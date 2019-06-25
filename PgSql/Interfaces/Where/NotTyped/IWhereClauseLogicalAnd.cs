using System.Collections.Generic;

namespace doob.PgSql.Interfaces.Where.NotTyped
{
    public interface IWhereClauseLogicalAnd
    {
        IWhereClauseLogicalAnd Not { get; }

        IWhereClauseConnectionAnd Eq(string propertyName, object value);
        IWhereClauseConnectionAnd Lt(string propertyName, object value);
        IWhereClauseConnectionAnd Gt(string propertyName, object value);
        IWhereClauseConnectionAnd Lte(string propertyName, object value);
        IWhereClauseConnectionAnd Gte(string propertyName, object value);
        IWhereClauseConnectionAnd Between(string propertyName, object min, object max);
        IWhereClauseConnectionAnd IsNull(string propertyName);
        IWhereClauseConnectionAnd IsNotNull(string propertyName);
        IWhereClauseConnectionAnd Any<TField>(string propertyName, IEnumerable<TField> value);
        IWhereClauseConnectionAnd Like(string propertyName, string value);
        IWhereClauseConnectionAnd Like(string propertyName, string value, bool ignoreCase, bool invertOrder);

        #region LTree
        IWhereClauseConnectionAnd LTreeMatch(string propertyName, string value);
        #endregion
    }
}
