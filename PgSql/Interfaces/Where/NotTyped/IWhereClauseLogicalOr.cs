namespace doob.PgSql.Interfaces.Where.NotTyped
{
    public interface IWhereClauseLogicalOr
    {
        IWhereClauseLogicalOr Not { get; }

        IWhereClauseConnectionOr Eq(string propertyName, object value);
        IWhereClauseConnectionOr Lt(string propertyName, object value);
        IWhereClauseConnectionOr Gt(string propertyName, object value);
        IWhereClauseConnectionOr Lte(string propertyName, object value);
        IWhereClauseConnectionOr Gte(string propertyName, object value);
        IWhereClauseConnectionOr Between(string propertyName, object min, object max);
        IWhereClauseConnectionOr IsNull(string propertyName);
        IWhereClauseConnectionOr IsNotNull(string propertyName);
        IWhereClauseConnectionOr Any(string propertyName, params object[] value);
        IWhereClauseConnectionOr Like(string propertyName, string value);
        IWhereClauseConnectionOr Like(string propertyName, string value, bool ignoreCase);

        #region LTree
        IWhereClauseConnectionOr LTreeMatch(string propertyName, string value);
        #endregion
    }
}
