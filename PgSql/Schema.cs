using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using doob.PgSql.CustomTypes;
using doob.PgSql.Exceptions;
using doob.PgSql.ExtensionMethods;
using doob.PgSql.Listener;
using doob.PgSql.Tables;
using Npgsql;

namespace doob.PgSql
{
    public class Schema
    {
        private ConnectionString _connectionString;

        public ConnectionString GetConnectionString() => _connectionString.Clone();
       
        public Schema() : this(ConnectionString.Build()) { }
        

        public Schema(string connectionString) : this(ConnectionString.Build(connectionString)) { }

       
        public Schema(ConnectionStringBuilder connectionBuilder) : this(connectionBuilder.GetConnection()) { }

        public Schema(ConnectionString connection)
        {
            var missing = new List<string>();

            if (String.IsNullOrWhiteSpace(connection.DatabaseName))
                missing.Add(nameof(connection.DatabaseName));

            if (String.IsNullOrWhiteSpace(connection.SchemaName))
                missing.Add(nameof(connection.SchemaName));

            if (missing.Any())
                throw new ArgumentNullException($"Missing Options to connect: '{String.Join(", ", missing)}'");

            _connectionString = ConnectionString.Build(connection);
        }



        public Server GetServer()
        {
            return new Server(GetConnectionString());
        }
        public Database GetDatabase()
        {
            return new Database(GetConnectionString());
        }

       
        public bool Exists()
        {
            return GetDatabase().SchemaExists(GetConnectionString().SchemaName);
        }
        public Schema EnsureExists()
        {
            GetDatabase().SchemaExists(GetConnectionString().SchemaName, true, false);
            return this;
        }

        public Schema WaitForExists(int checkIntervalInSeconds = 10)
        {
            return WaitForExists(TimeSpan.FromSeconds(checkIntervalInSeconds));
        }

        public Schema WaitForExists(TimeSpan checkInterval)
        {

            bool exists = false;
            do
            {
                exists = GetDatabase().EnsureExists().SchemaExists(GetConnectionString().SchemaName, false, false);

                if (!exists)
                    Task.Delay(checkInterval).GetAwaiter().GetResult();

            } while (!exists);
            
            return this;
        }
        public Schema EnsureNotExists()
        {
            GetDatabase().SchemaExists(GetConnectionString().SchemaName, false, true);
            return this;
        }
        public Schema CreateIfNotExists()
        {
            return new Database(GetConnectionString()).CreateIfNotExists()
                .CreateSchema(GetConnectionString().SchemaName, false);
        }


        public void Drop()
        {
            GetDatabase().SchemaDrop(GetConnectionString().SchemaName);
        }
        public void Drop(bool force)
        {
            GetDatabase().SchemaDrop(GetConnectionString().SchemaName, force);
        }
        public void Drop(bool force, bool throwIfNotExists)
        {
            GetDatabase().SchemaDrop(GetConnectionString().SchemaName, force, throwIfNotExists);
        }

        public string[] TableList()
        {
            var l = new List<string>();
            foreach (var exp in new PgSqlExecuter(GetConnectionString()).ExecuteReader(SQLStatements.GetTables(GetConnectionString().SchemaName)))
            {
                var obj = JSON.ToObject<Dictionary<string, object>>(exp);
                l.Add(obj["table_name"].ToString());
            }

            return l.ToArray();
        }
        public bool TableExists(string tableName)
        {
            return TableExists(tableName, false);
        }
        public bool TableExists(string tableName, bool throwIfNotExists)
        {
            return TableExists(tableName, throwIfNotExists, false);
        }
        public bool TableExists(string tableName, bool throwIfNotExists, bool throwIfAlreadyExists)
        {
            if (String.IsNullOrWhiteSpace(tableName))
                throw new ArgumentNullException(nameof(tableName));


            var exists = new PgSqlExecuter(GetConnectionString()).ExecuteScalar<bool>(SQLStatements.TableExists(tableName, GetConnectionString().SchemaName));


            if (!exists && throwIfNotExists)
                throw new TableDoesntExistsException(tableName);

            if (exists && throwIfAlreadyExists)
                throw new TableAlreadyExistsException(tableName);

            return exists;
        }


        public ObjectTable GetTable(string tableName)
        {
            var connstr = ConnectionString.Build(GetConnectionString()).WithTable(tableName);
            return new ObjectTable(connstr).NotTyped();
        }
        public ObjectTable CreateTable(string tableName, TableDefinition definition, bool throwIfAlreadyExists)
        {
            _createTable(tableName, definition, throwIfAlreadyExists);
            return GetTable(tableName);
        }
        public ObjectTable CreateTableInheritedFrom(string tableName, string inheritFromTable, TableDefinition additionalFields = null, bool throwIfAlreadyExists = false)
        {
            _createInheritedTable(tableName, inheritFromTable, additionalFields, throwIfAlreadyExists);
            return GetTable(tableName);
        }


        public TypedTable<T> GetTable<T>(string tableName)
        {
            var connstr = ConnectionString.Build(GetConnectionString()).WithTable(tableName);
            var tbl = new TypedTable<T>(connstr);
            return tbl;
        }
        public TypedTable<T> CreateTable<T>(string tableName, bool throwIfAlreadyExists = false)
        {
            var td = Build.TableDefinition<T>();
            _createTable(tableName, td, throwIfAlreadyExists);
            return GetTable<T>(tableName);
        }
        public async Task<TypedTable<T>> CreateTableAsync<T>(string tableName, bool throwIfAlreadyExists = false)
        {
            var td = Build.TableDefinition<T>();
            await _createTableAsync(tableName, td, throwIfAlreadyExists);
            return GetTable<T>(tableName);
        }
        public TypedTable<T> CreateTable<T>(string tableName, TableDefinition<T> definition, bool throwIfAlreadyExists = false)
        {
            _createTable(tableName, definition, throwIfAlreadyExists);
            return GetTable<T>(tableName);
        }
        public TypedTable<T> CreateTableInheritedFrom<T>(string tableName, string inheritFromTable, TableDefinition<T> additionalFields = null, bool throwIfAlreadyExists = false)
        {
            _createInheritedTable(tableName, inheritFromTable, additionalFields, throwIfAlreadyExists);
            return GetTable<T>(tableName);
        }


       
        public Schema TableDrop(string tableName, bool throwIfNotExists = false)
        {
            if (String.IsNullOrWhiteSpace(tableName))
                throw new ArgumentNullException(nameof(tableName));

            try
            {
                new PgSqlExecuter(GetConnectionString()).ExecuteNonQuery(SQLStatements.RemoveTable(tableName, GetConnectionString().SchemaName, throwIfNotExists));
            }
            catch (PostgresException ex) when (ex.SqlState == "42P01")
            {
                throw new TableDoesntExistsException(tableName);
            }

            return this;
        }

        private object _createTable(string tableName, TableDefinition definition, bool throwIfAlreadyExists)
        {
            if (String.IsNullOrWhiteSpace(tableName))
                throw new ArgumentNullException(nameof(tableName));

            tableName = $"\"{tableName.Trim("\"".ToCharArray())}\"";

            var pd = GetDatabase();

            foreach (var column in definition.Columns())
            {
                if (column.DotNetType == typeof(Guid?) || column.DotNetType == typeof(Guid))
                {
                    if(!pd.ExtensionExists("uuid-ossp"))
                        pd.ExtensionCreate("uuid-ossp", false);
                }


                if (column.DotNetType == typeof(PgSqlLTree))
                {
                    if (!pd.ExtensionExists("ltree"))
                        pd.ExtensionCreate("ltree", false);
                }
                    

            }


            try
            {
                var tablebuilder = definition.GetSqlDefinition(tableName, GetConnectionString().SchemaName, throwIfAlreadyExists);
                return new PgSqlExecuter(GetConnectionString()).ExecuteScalar(tablebuilder);
            }
            catch (PostgresException ex) when (ex.SqlState == "42P07")
            {
                throw new TableAlreadyExistsException(tableName);
            }

        }

        private Task _createTableAsync(string tableName, TableDefinition definition, bool throwIfAlreadyExists)
        {
            if (String.IsNullOrWhiteSpace(tableName))
                throw new ArgumentNullException(nameof(tableName));

            tableName = $"\"{tableName.Trim("\"".ToCharArray())}\"";

            var pd = GetDatabase();

            foreach (var column in definition.Columns())
            {
                if (column.DotNetType == typeof(Guid?) || column.DotNetType == typeof(Guid))
                {
                    if (!pd.ExtensionExists("uuid-ossp"))
                        pd.ExtensionCreate("uuid-ossp", false);
                }


                if (column.DotNetType == typeof(PgSqlLTree))
                {
                    if (!pd.ExtensionExists("ltree"))
                        pd.ExtensionCreate("ltree", false);
                }


            }


            try
            {
                var tablebuilder = definition.GetSqlDefinition(tableName, GetConnectionString().SchemaName, throwIfAlreadyExists);
                return new PgSqlExecuter(GetConnectionString()).ExecuteScalarAsync(tablebuilder);
            }
            catch (PostgresException ex) when (ex.SqlState == "42P07")
            {
                throw new TableAlreadyExistsException(tableName);
            }

        }
        private object _createInheritedTable(string tableName, string inheritFromTable, TableDefinition definition, bool throwIfAlreadyExists)
        {
            if (String.IsNullOrWhiteSpace(tableName))
                throw new ArgumentNullException(nameof(tableName));

            tableName = $"\"{tableName.Trim("\"".ToCharArray())}\"";

            if (String.IsNullOrWhiteSpace(inheritFromTable))
                throw new ArgumentNullException(nameof(inheritFromTable));

            inheritFromTable = $"\"{inheritFromTable}\"";

            var tblName = $"{(!String.IsNullOrWhiteSpace(GetConnectionString().SchemaName) ? $"{GetConnectionString().SchemaName}." : "")}{tableName}";
            var inherittblName = $"{(!String.IsNullOrWhiteSpace(GetConnectionString().SchemaName) ? $"{GetConnectionString().SchemaName}." : "")}{inheritFromTable}";
            try
            {


                var strBuilder = new StringBuilder();
                strBuilder.Append("CREATE TABLE");
                if (!throwIfAlreadyExists)
                {
                    strBuilder.Append(" IF NOT EXISTS");
                }
                strBuilder.AppendLine($" {tblName} (");
                if (definition != null)
                {
                    strBuilder.AppendLine(definition.GetInnerSqlDefinition());
                }
                strBuilder.Append($") INHERITS ({inherittblName})");

                return new PgSqlExecuter(GetConnectionString()).ExecuteScalar(strBuilder.ToString());
            }
            catch (PostgresException ex) when (ex.SqlState == "42P07")
            {
                throw new TableAlreadyExistsException(tableName);
            }
        }


        //public TableEventListener GetEventListener(bool forceNewInstance = false)
        //{
        //    return TableEventListener.GetEventLister(this, forceNewInstance);
        //}

        //public IObservable<TriggerNotification<T>> GetTypedEntryNotificationFor<T>(TypedTable<T> table, bool forceNewInstance = false)
        //{
        //    return TableEventListener.GetEventLister(this, forceNewInstance).TypedEntryOnEvent(table);
        //}

        //public IObservable<TriggerNotification<T>> GetTypedKeysNotificationFor<T>(TypedTable<T> table, bool forceNewInstance = false)
        //{
        //    return TableEventListener.GetEventLister(this, forceNewInstance).TypedKeysOnEvent(table);
        //}


        #region Function

        public bool FunctionExists(string functionName)
        {
            var command = $"SELECT EXISTS(SELECT 1 FROM information_schema.routines WHERE routines.specific_schema = '{GetConnectionString().SchemaName}' AND routines.routine_name = '{functionName}');";
            return new PgSqlExecuter(GetConnectionString()).ExecuteScalar<bool>(command);
        }

        #endregion

        

//        public bool EventTriggerExists(string triggerName)
//        {
//            var command = $"SELECT EXISTS(select 1 from pg_event_trigger where evtname = '{triggerName}');";
//            return new DbExecuter(ConnectionString).ExecuteScalar<bool>(command);
//        }

//        public void EventTriggerDrop(string triggerName, bool force)
//        {
//            var command = $"DROP EVENT TRIGGER IF EXISTS \"{triggerName}\"";

//            if (force)
//                command = $"{command} CASCADE";

//            new DbExecuter(ConnectionString).ExecuteNonQuery(command);
//        }

//        public void RegisterEventTrigger(bool overwriteIfExists = false)
//        {
//            var functionName = $"XA-Notify-SchemaEvent";

//            if (!FunctionExists(functionName) || overwriteIfExists)
//            {
//                var function1 = $@"
//CREATE OR REPLACE FUNCTION ""{ConnectionString.SchemaName}"".""{functionName}""() RETURNS event_trigger AS $$
//DECLARE
//    r RECORD;
//    notification json;
//    type text;
//BEGIN
    
//    FOR r IN SELECT * FROM pg_event_trigger_ddl_commands() LOOP

//            notification= json_build_object(
//                'action', upper(r.command_tag),
//                'schema', r.schema_name,
//                'identity', r.object_identity,
//                'type', upper(r.object_type)
//            );

//            PERFORM pg_notify('{functionName}', notification::text);

//    END LOOP;
//END;
//$$ LANGUAGE plpgsql;
//";
//                new DbExecuter(ConnectionString).ExecuteNonQuery(function1);
//            }

//            if (!FunctionExists($"{functionName}_dropped") || overwriteIfExists)
//            {
//                var function2 = $@"
//CREATE OR REPLACE FUNCTION ""{ConnectionString.SchemaName}"".""{functionName}_dropped""() RETURNS event_trigger AS $$
//DECLARE
//    r RECORD;
//    notification json;
//    action text;
//BEGIN
//    FOR r IN SELECT * FROM pg_event_trigger_dropped_objects() LOOP

//            notification= json_build_object(
//                'action', 'DROP ' || upper(r.object_type),
//                'schema', r.schema_name,
//                'identity', r.object_identity,
//                'type', upper(r.object_type)
//            );

//            PERFORM pg_notify('{functionName}', notification::text);

//    END LOOP;
//END;
//$$ LANGUAGE plpgsql;
//";
//                new DbExecuter(ConnectionString).ExecuteNonQuery(function2);

//            }

//            var trigger1Exists = EventTriggerExists($"{functionName}_Trigger::{ConnectionString.SchemaName}");

//            if (!trigger1Exists || overwriteIfExists)
//            {
//                if (trigger1Exists)
//                    EventTriggerDrop($"{functionName}_Trigger::{ConnectionString.SchemaName}", true);

//                var trigger1 = $"CREATE EVENT TRIGGER \"{functionName}_Trigger::{ConnectionString.SchemaName}\" ON ddl_command_end EXECUTE PROCEDURE \"{ConnectionString.SchemaName}\".\"{functionName}\"();";
//                new DbExecuter(ConnectionString).ExecuteNonQuery(trigger1);
//            }

//            var trigger2Exists = EventTriggerExists($"{functionName}_Trigger_dropped::{ConnectionString.SchemaName}");
//            if (!trigger2Exists || overwriteIfExists)
//            {
//                if (trigger2Exists)
//                    EventTriggerDrop($"{functionName}_Trigger_dropped::{ConnectionString.SchemaName}", true);

//                var trigger2 = $"CREATE EVENT TRIGGER \"{functionName}_Trigger_dropped::{ConnectionString.SchemaName}\" ON sql_drop EXECUTE PROCEDURE \"{ConnectionString.SchemaName}\".\"{functionName}_dropped\"();";
//                new DbExecuter(ConnectionString).ExecuteNonQuery(trigger2);
//            }

//        }


        #region Triggers
        public bool TriggersForTriggerFunctionExists(string triggerFunctionName)
        {
            return new PgSqlExecuter(GetConnectionString()).ExecuteScalar<bool>(SQLStatements.TriggerExistsForTriggerFunction(triggerFunctionName));
        }

        public void DropTriggerFunction(string triggerFunctionName)
        {
            new PgSqlExecuter(GetConnectionString()).ExecuteNonQuery(SQLStatements.DropTriggerFunction(triggerFunctionName));
        }

        public void CreateTriggerFunction(bool overwriteIfExists = false)
        {
            if (!FunctionExists("Notify-TableEvent") || overwriteIfExists)
            {
                new PgSqlExecuter(GetConnectionString()).ExecuteNonQuery(TableStatements.BuildTriggerFunction(GetConnectionString().SchemaName, "Notify-TableEvent"));
            }
        }
        #endregion


    }
}
