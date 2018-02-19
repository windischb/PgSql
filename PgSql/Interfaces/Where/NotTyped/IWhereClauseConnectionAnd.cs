namespace doob.PgSql.Interfaces.Where.NotTyped
{
    public interface IWhereClauseConnectionAnd : IWhere
    {
        IWhereClauseLogicalAnd And { get; }
    }

   
}
