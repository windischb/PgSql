namespace doob.PgSql.Interfaces.Where.NotTyped
{
    public interface IWhereClauseLogicalBase : IWhere
    {
        IMergeWhereClause Merge(IWhere query);

        IWhereClauseLogicalBase Not { get; }

        IWhereClauseConnectionBase Eq(string propertyName, object value);
        IWhereClauseConnectionBase Lt(string propertyName, object value);
        IWhereClauseConnectionBase Gt(string propertyName, object value);
        IWhereClauseConnectionBase Lte(string propertyName, object value);
        IWhereClauseConnectionBase Gte(string propertyName, object value);
        IWhereClauseConnectionBase Between(string propertyName, object min, object max);
        IWhereClauseConnectionBase IsNull(string propertyName);
        IWhereClauseConnectionBase IsNotNull(string propertyName);
        IWhereClauseConnectionBase Any(string propertyName, params object[] value);
        IWhereClauseConnectionBase Like(string propertyName, string value);
        IWhereClauseConnectionBase Like(string propertyName, string value, bool ignoreCase);

        #region LTree
        IWhereClauseConnectionBase LTreeMatch(string propertyName, string value);
        #endregion

    }
}
