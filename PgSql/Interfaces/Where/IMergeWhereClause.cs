namespace doob.PgSql.Interfaces.Where
{
    public interface IMergeWhereClause
    {
        IMergeWhereClauseConnectionAnd AndQuery(IWhere query);
        IMergeWhereClauseConnectionOr OrQuery(IWhere query);
    }
}
