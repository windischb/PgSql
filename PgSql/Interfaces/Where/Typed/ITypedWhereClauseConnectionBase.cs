namespace doob.PgSql.Interfaces.Where.Typed
{
    public interface ITypedWhereClauseConnectionBase<T> : IWhere
    {
        ITypedWhereClauseLogicalAnd<T> And();
        ITypedWhereClauseLogicalOr<T> Or();
    }

   
}
