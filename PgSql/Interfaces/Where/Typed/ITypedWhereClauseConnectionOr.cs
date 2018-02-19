namespace doob.PgSql.Interfaces.Where.Typed
{
    public interface ITypedWhereClauseConnectionOr<T> : IWhere
    {
        ITypedWhereClauseLogicalOr<T> Or();
    }

   
}
