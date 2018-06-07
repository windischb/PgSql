using System;
using System.Collections.Generic;
using System.Text;
using doob.PgSql.CustomTypes;
using doob.PgSql.Exceptions;
using doob.PgSql.ExtensionMethods;
using doob.PgSql.Tables;
using Npgsql;

namespace doob.PgSql
{
    public class Schema
    {
        private ConnectionString _connectionString;
        public ConnectionString ConnectionString
        {
            get
            {
                if (_connectionString == null)
                    _connectionString = ConnectionString.Build();

                return _connectionString;
            }
        }

        public Schema()
        {
            _connectionString = ConnectionString.Build();
        }
        public Schema(string connectionString) 
        {
            _connectionString = ConnectionString.Build(connectionString);
        }
        public Schema(ConnectionString connection)
        {
            _connectionString = ConnectionString.Build(connection);
        }
        public Schema(ConnectionStringBuilder connectionBuilder)
        {
            _connectionString = ConnectionString.Build(connectionBuilder);
        }

        public Server GetServer()
        {
            return new Server(ConnectionString);
        }
        public Database GetDatabase()
        {
            return new Database(ConnectionString);
        }

       
        public bool Exists()
        {
            return GetDatabase().SchemaExists(ConnectionString.SchemaName);
        }
        public Schema EnsureExists()
        {
            GetDatabase().SchemaExists(ConnectionString.SchemaName, true, false);
            return this;
        }
        public Schema EnsureNotExists()
        {
            GetDatabase().SchemaExists(ConnectionString.SchemaName, false, true);
            return this;
        }
        public Schema CreateIfNotExists()
        {
            return new Database(ConnectionString).CreateIfNotExists()
                .CreateSchema(ConnectionString.SchemaName, false);
        }


        public void Drop()
        {
            GetDatabase().SchemaDrop(ConnectionString.SchemaName);
        }
        public void Drop(bool force)
        {
            GetDatabase().SchemaDrop(ConnectionString.SchemaName, force);
        }
        public void Drop(bool force, bool throwIfNotExists)
        {
            GetDatabase().SchemaDrop(ConnectionString.SchemaName, force, throwIfNotExists);
        }

        public string[] TableList()
        {
            var l = new List<string>();
            foreach (var exp in new DbExecuter(ConnectionString).ExecuteReader(SQLStatements.GetTables(ConnectionString.SchemaName)))
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


            var exists = new DbExecuter(ConnectionString).ExecuteScalar<bool>(SQLStatements.TableExists(tableName, ConnectionString.SchemaName));


            if (!exists && throwIfNotExists)
                throw new TableDoesntExistsException(tableName);

            if (exists && throwIfAlreadyExists)
                throw new TableAlreadyExistsException(tableName);

            return exists;
        }


        public ObjectTable GetTable(string tableName)
        {
            var connstr = ConnectionString.Build(ConnectionString).WithTable(tableName);
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
            var connstr = ConnectionString.Build(ConnectionString).WithTable(tableName);
            var tbl = new TypedTable<T>(connstr);
            return tbl;
        }
        public TypedTable<T> CreateTable<T>(string tableName, bool throwIfAlreadyExists = false)
        {
            var td = TableDefinition.FromType<T>();
            _createTable(tableName, td, throwIfAlreadyExists);
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


        public void TableDrop(string tableName)
        {
            TableDrop(tableName, false);
        }
        public void TableDrop(string tableName, bool throwIfNotExists)
        {
            if (String.IsNullOrWhiteSpace(tableName))
                throw new ArgumentNullException(nameof(tableName));

            try
            {
                new DbExecuter(ConnectionString).ExecuteNonQuery(SQLStatements.RemoveTable(tableName, ConnectionString.SchemaName, throwIfNotExists));
            }
            catch (PostgresException ex) when (ex.SqlState == "42P01")
            {
                throw new TableDoesntExistsException(tableName);
            }

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
                var tablebuilder = definition.GetSqlDefinition(tableName, ConnectionString.SchemaName, throwIfAlreadyExists);
                return new DbExecuter(ConnectionString).ExecuteScalar(tablebuilder);
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

            var tblName = $"{(!String.IsNullOrWhiteSpace(ConnectionString.SchemaName) ? $"{ConnectionString.SchemaName}." : "")}{tableName}";
            var inherittblName = $"{(!String.IsNullOrWhiteSpace(ConnectionString.SchemaName) ? $"{ConnectionString.SchemaName}." : "")}{inheritFromTable}";
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

                return new DbExecuter(ConnectionString).ExecuteScalar(strBuilder.ToString());
            }
            catch (PostgresException ex) when (ex.SqlState == "42P07")
            {
                throw new TableAlreadyExistsException(tableName);
            }
        }





        #region Function

        public bool FunctionExists(string functionName)
        {
            var command = $"SELECT EXISTS(SELECT 1 FROM information_schema.routines WHERE routines.specific_schema = '{ConnectionString.SchemaName}' AND routines.routine_name = '{functionName}');";
            return new DbExecuter(ConnectionString).ExecuteScalar<bool>(command);
        }

        #endregion

        #region Triggers

        public bool EventTriggerExists(string triggerName)
        {
            var command = $"SELECT EXISTS(select 1 from pg_event_trigger where evtname = '{triggerName}');";
            return new DbExecuter(ConnectionString).ExecuteScalar<bool>(command);
        }

        public void EventTriggerDrop(string triggerName, bool force)
        {
            var command = $"DROP EVENT TRIGGER IF EXISTS \"{triggerName}\"";

            if (force)
                command = $"{command} CASCADE";

            new DbExecuter(ConnectionString).ExecuteNonQuery(command);
        }

        public void RegisterEventTrigger(bool overwriteIfExists = false)
        {
            var functionName = $"XA-Notify-SchemaEvent";

            if (!FunctionExists(functionName) || overwriteIfExists)
            {
                var function1 = $@"
CREATE OR REPLACE FUNCTION ""{ConnectionString.SchemaName}"".""{functionName}""() RETURNS event_trigger AS $$
DECLARE
    r RECORD;
    notification json;
    type text;
BEGIN
    
    FOR r IN SELECT * FROM pg_event_trigger_ddl_commands() LOOP

            notification= json_build_object(
                'action', upper(r.command_tag),
                'schema', r.schema_name,
                'identity', r.object_identity,
                'type', upper(r.object_type)
            );

            PERFORM pg_notify('{functionName}', notification::text);

    END LOOP;
END;
$$ LANGUAGE plpgsql;
";
                new DbExecuter(ConnectionString).ExecuteNonQuery(function1);
            }

            if (!FunctionExists($"{functionName}_dropped") || overwriteIfExists)
            {
                var function2 = $@"
CREATE OR REPLACE FUNCTION ""{ConnectionString.SchemaName}"".""{functionName}_dropped""() RETURNS event_trigger AS $$
DECLARE
    r RECORD;
    notification json;
    action text;
BEGIN
    FOR r IN SELECT * FROM pg_event_trigger_dropped_objects() LOOP

            notification= json_build_object(
                'action', 'DROP ' || upper(r.object_type),
                'schema', r.schema_name,
                'identity', r.object_identity,
                'type', upper(r.object_type)
            );

            PERFORM pg_notify('{functionName}', notification::text);

    END LOOP;
END;
$$ LANGUAGE plpgsql;
";
                new DbExecuter(ConnectionString).ExecuteNonQuery(function2);

            }

            var trigger1Exists = EventTriggerExists($"{functionName}_Trigger::{ConnectionString.SchemaName}");

            if (!trigger1Exists || overwriteIfExists)
            {
                if (trigger1Exists)
                    EventTriggerDrop($"{functionName}_Trigger::{ConnectionString.SchemaName}", true);

                var trigger1 = $"CREATE EVENT TRIGGER \"{functionName}_Trigger::{ConnectionString.SchemaName}\" ON ddl_command_end EXECUTE PROCEDURE \"{ConnectionString.SchemaName}\".\"{functionName}\"();";
                new DbExecuter(ConnectionString).ExecuteNonQuery(trigger1);
            }

            var trigger2Exists = EventTriggerExists($"{functionName}_Trigger_dropped::{ConnectionString.SchemaName}");
            if (!trigger2Exists || overwriteIfExists)
            {
                if (trigger2Exists)
                    EventTriggerDrop($"{functionName}_Trigger_dropped::{ConnectionString.SchemaName}", true);

                var trigger2 = $"CREATE EVENT TRIGGER \"{functionName}_Trigger_dropped::{ConnectionString.SchemaName}\" ON sql_drop EXECUTE PROCEDURE \"{ConnectionString.SchemaName}\".\"{functionName}_dropped\"();";
                new DbExecuter(ConnectionString).ExecuteNonQuery(trigger2);
            }

        }

        #endregion

      
    }
}
