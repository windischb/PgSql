namespace doob.PgSql.Interfaces.Where
{
    public interface IMergeWhereClauseConnectionOr : IWhere
    {
        IMergeWhereClauseConnectionOr OrQuery(IWhere query);
    }
}
