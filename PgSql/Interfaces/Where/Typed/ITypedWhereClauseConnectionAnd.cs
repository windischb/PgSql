namespace doob.PgSql.Interfaces.Where.Typed
{
    public interface ITypedWhereClauseConnectionAnd<T> : IWhere
    {
        ITypedWhereClauseLogicalAnd<T> And();
    }

   
}
