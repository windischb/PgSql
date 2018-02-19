namespace doob.PgSql.Interfaces.Where.NotTyped
{
    public interface IWhereClauseConnectionOr : IWhere
    {
        IWhereClauseLogicalOr Or { get; }
    }

   
}
