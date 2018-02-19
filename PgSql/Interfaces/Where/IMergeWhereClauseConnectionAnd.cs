namespace doob.PgSql.Interfaces.Where
{
    public interface IMergeWhereClauseConnectionAnd: IWhere
    {
        IMergeWhereClauseConnectionAnd AndQuery(IWhere query);
    }
}
