namespace doob.PgSql.Interfaces
{
    public interface ISQLCommand
    {
        PgSqlCommand GetSqlCommand(TableDefinition tableDefinition);
    }
}
