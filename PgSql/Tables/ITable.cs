namespace doob.PgSql.Tables
{



    public interface ITable
    {

        Server GetServer();
        Database GetDatabase();
        Schema GetSchema();
        ConnectionString GetConnectionString();

        TableDefinition TableDefinition { get; }

        bool TriggerExists(string triggerName);
        void TriggerCreate(string triggerName, string triggerFunctionName, bool overwriteIfExists = false);
        void TriggerDrop(string triggerName);

    }

   
    public interface ITable<T> : ITable
    {
        new TableDefinition<T> TableDefinition { get; }
    }
}