namespace doob.PgSql.Interfaces.Where.NotTyped
{
    public interface IWhereClauseConnectionBase : IWhere
    {
        IWhereClauseLogicalAnd And { get; }
        IWhereClauseLogicalOr Or { get; }
    }

   
}
