using System;
using System.Text;
using doob.PgSql.ExtensionMethods;
using doob.PgSql.Tables;

namespace doob.PgSql
{
    internal static class SQLStatements
    {

        #region Server
        public static string GetAllSettings()
        {
            return "SELECT * FROM pg_settings";
        }

        #endregion

        #region Database

        public static string DatabaseListAll()
        {
            return "SELECT datname FROM pg_database WHERE datistemplate = false; ";
        }

        public static string DatabaseCreate(string databaseName)
        {
            if(String.IsNullOrWhiteSpace(databaseName))
                throw new ArgumentNullException(nameof(databaseName));

            databaseName = $"\"{databaseName.Trim("\"".ToCharArray())}\"";

            return $"CREATE DATABASE {databaseName};";
            
        }

        public static string DatabaseExists(string databaseName)
        {
            if (String.IsNullOrWhiteSpace(databaseName))
                throw new ArgumentNullException(nameof(databaseName));

            databaseName = $"{databaseName.Trim("\"".ToCharArray())}";

            return $"SELECT EXISTS(SELECT 1 FROM pg_database WHERE datname = '{databaseName}')";
        }

        public static string DatabaseRename(string databaseName, string newDatabaseName)
        {
            if (String.IsNullOrWhiteSpace(databaseName))
                throw new ArgumentNullException(nameof(databaseName));

            if (String.IsNullOrWhiteSpace(newDatabaseName))
                throw new ArgumentNullException(nameof(newDatabaseName));

            databaseName = $"\"{databaseName.Trim("\"".ToCharArray())}\"";
            newDatabaseName = $"\"{newDatabaseName.Trim("\"".ToCharArray())}\"";

            return $"ALTER DATABASE {databaseName} RENAME TO {newDatabaseName}";
        }

        public static string DatabaseDrop(string databaseName, bool throwIfNotExists)
        {
            if (String.IsNullOrWhiteSpace(databaseName))
                throw new ArgumentNullException(nameof(databaseName));

            databaseName = $"\"{databaseName.Trim("\"".ToCharArray())}\"";

            if (throwIfNotExists)
            {
                return $"DROP DATABASE {databaseName}";
            }
            else
            {
                return $"DROP DATABASE IF EXISTS {databaseName}";
            }
        }

        public static string DatabaseConnectionsGet(string databaseName)
        {
            if (String.IsNullOrWhiteSpace(databaseName))
                throw new ArgumentNullException(nameof(databaseName));

            databaseName = $"{databaseName.Trim("\"".ToCharArray())}";

            return $"SELECT * FROM pg_stat_activity WHERE datname = '{databaseName}';";
        }

        public static string DatabaseConnectionsDrop(string databaseName)
        {
            if (String.IsNullOrWhiteSpace(databaseName))
                throw new ArgumentNullException(nameof(databaseName));

            databaseName = $"{databaseName.Trim("\"".ToCharArray())}";

            return $"SELECT pg_terminate_backend (pg_stat_activity.pid) FROM pg_stat_activity WHERE pg_stat_activity.datname = '{databaseName}';";
        }

        
        #endregion

        #region Schema
        public static string SchemaGetAll()
        {
            return "SELECT schema_name FROM information_schema.schemata WHERE schema_name NOT LIKE 'pg_%' AND schema_name NOT LIKE 'information_schema'";
        }

        public static string SchemaCreate(string schemaName, bool throwIfAlreadyExists)
        {
            
            if (String.IsNullOrWhiteSpace(schemaName))
                throw new ArgumentNullException(nameof(schemaName));

            schemaName = $"\"{schemaName.Trim("\"".ToCharArray())}\"";

            if (throwIfAlreadyExists)
            {
                return $"CREATE SCHEMA {schemaName}";
            }

            return $"CREATE SCHEMA IF NOT EXISTS {schemaName}";
        }

        public static string SchemaExists(string schemaName)
        {
            if (String.IsNullOrWhiteSpace(schemaName))
                throw new ArgumentNullException(nameof(schemaName));

            schemaName = $"{schemaName.Trim("\"".ToCharArray())}";

            return $"SELECT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = '{schemaName}')";
        }

        public static string SchemaRename(string schemaName, string newSchemaName)
        {
            if (String.IsNullOrWhiteSpace(schemaName))
                throw new ArgumentNullException(nameof(schemaName));

            if (String.IsNullOrWhiteSpace(newSchemaName))
                throw new ArgumentNullException(nameof(newSchemaName));

            schemaName = $"\"{schemaName.Trim("\"".ToCharArray())}\"";
            newSchemaName = $"\"{newSchemaName.Trim("\"".ToCharArray())}\"";

            return $"ALTER SCHEMA {schemaName} RENAME TO {newSchemaName}";
        }

        public static string SchemaDrop(string schemaName, bool force = false, bool throwIfNotExists = false)
        {
            if (String.IsNullOrWhiteSpace(schemaName))
                throw new ArgumentNullException(nameof(schemaName));

            schemaName = $"\"{schemaName.Trim("\"".ToCharArray())}\"";

            var strbuilder = new StringBuilder();

            if (throwIfNotExists)
            {
                strbuilder.Append($"DROP SCHEMA {schemaName}");
            }
            else
            {
                strbuilder.Append($"DROP SCHEMA IF EXISTS {schemaName}");
            }
            
            if (force)
            {
                strbuilder.Append(" CASCADE");
            }
            else
            {
                strbuilder.Append(" RESTRICT");
            }

            return strbuilder.ToString();

        }

        
        #endregion

        #region Table

        public static string TableExists(string tableName, string schemaName)
        {
            if (String.IsNullOrWhiteSpace(tableName))
                throw new ArgumentNullException(nameof(tableName));

            if (String.IsNullOrWhiteSpace(schemaName))
                throw new ArgumentNullException(nameof(schemaName));

            return $"SELECT EXISTS(SELECT 1 FROM information_schema.tables WHERE table_schema = '{schemaName}' AND table_name = '{tableName}')";
        }
        public static string GetTables(string schemaName)
        {
            if (String.IsNullOrWhiteSpace(schemaName))
                throw new ArgumentNullException(nameof(schemaName));

            return $"SELECT table_name FROM information_schema.tables WHERE table_schema = '{schemaName}' ORDER BY table_name;";
        }

        public static string CreateTable(string tableName, string schemaName, string tableSchema, bool throwIfAlreadyExists)
        {
            if (String.IsNullOrWhiteSpace(tableName))
                throw new ArgumentNullException(nameof(tableName));

            if (String.IsNullOrWhiteSpace(schemaName))
                throw new ArgumentNullException(nameof(schemaName));

            if (String.IsNullOrWhiteSpace(tableSchema))
                throw new ArgumentNullException(nameof(tableSchema));

            schemaName = $"\"{schemaName.Trim("\"".ToCharArray())}\"";
            tableName = $"\"{tableName.Trim("\"".ToCharArray())}\"";

            var strBuilder = new StringBuilder();
            strBuilder.Append($"CREATE TABLE");
            if (!throwIfAlreadyExists)
                strBuilder.Append(" IF NOT EXISTS");
            strBuilder
                .AppendLine($" {schemaName}.{tableName} (")
                .Append(tableSchema)
                .AppendLine(")");

            return strBuilder.ToString();
        }

        public static string RemoveTable(string tableName, string schemaName, bool throwIfNotExists)
        {
            if (String.IsNullOrWhiteSpace(tableName))
                throw new ArgumentNullException(nameof(tableName));

            if (String.IsNullOrWhiteSpace(schemaName))
                throw new ArgumentNullException(nameof(schemaName));

            schemaName = $"\"{schemaName.Trim("\"".ToCharArray())}\"";
            tableName = $"\"{tableName.Trim("\"".ToCharArray())}\"";

            var strBuilder = new StringBuilder();
            strBuilder.Append($"DROP TABLE");
            if (!throwIfNotExists)
                strBuilder.Append(" IF EXISTS");
            strBuilder.Append($" {schemaName}.{tableName};");

            return strBuilder.ToString();

        }



        #endregion

        #region Colums

        public static string GetColumns(string tableName, string schemaName)
        {

            if (String.IsNullOrWhiteSpace(tableName))
                throw new ArgumentNullException(nameof(tableName));

            if (String.IsNullOrWhiteSpace(schemaName))
                throw new ArgumentNullException(nameof(schemaName));

            return  $@"
SELECT	t1.column_name,
    t1.is_nullable::bool,
	t1.column_default,
	(t1.ordinal_position -1) as ordinal_position,
	( 
		SELECT EXISTS
		(
			SELECT c.column_name
			FROM information_schema.key_column_usage AS c
			LEFT JOIN information_schema.table_constraints AS t
			ON t.constraint_name = c.constraint_name
			WHERE t.table_schema = '{schemaName}' AND t.table_name = '{tableName}' AND t.constraint_type = 'PRIMARY KEY' AND c.column_name = t1.column_name
		)
	) as is_primarykey,
    ( 
		SELECT EXISTS
		(
			SELECT c.column_name
			FROM information_schema.key_column_usage AS c
			LEFT JOIN information_schema.table_constraints AS t
			ON t.constraint_name = c.constraint_name
			WHERE t.table_schema = '{schemaName}' AND t.table_name = '{tableName}' AND t.constraint_type = 'UNIQUE' AND c.column_name = t1.column_name
		)
	) as is_unique,
    LTRIM(CASE WHEN (t1.data_type = 'ARRAY') THEN CONCAT(t1.udt_name::text,'[]')
         ELSE t1.udt_name::text
    END,'_') as pg_type

from information_schema.columns as t1 where table_name='{tableName}' AND table_schema='{schemaName}'
";
        }

        #endregion

        #region Function

        public static string FunctionExists(string schemaName, string functionName)
        {
            if (String.IsNullOrWhiteSpace(schemaName))
                throw new ArgumentNullException(nameof(schemaName));

            if (String.IsNullOrWhiteSpace(functionName))
                throw new ArgumentNullException(nameof(functionName));

            return $"SELECT EXISTS(SELECT 1 FROM information_schema.routines WHERE routines.specific_schema = '{schemaName}' AND routines.routine_name = '{functionName}');";
        }

        #endregion


        #region Listen/Notify

        public static ListenCommand ListenReferenceKeys(string schemaName, string tableName)
        {
            if (String.IsNullOrWhiteSpace(schemaName))
                throw new ArgumentNullException(nameof(schemaName));

            if (String.IsNullOrWhiteSpace(tableName))
                throw new ArgumentNullException(nameof(tableName));

            var lc = new ListenCommand();
            lc.ProcedureName = $"xa_n_referenceKeys";

            lc.CommandText = $@"
CREATE OR REPLACE FUNCTION ""{schemaName}"".""{lc.ProcedureName}""() RETURNS TRIGGER AS $$

    DECLARE

        js  jsonb;
        primarykeys text[];
	    tablename text;
	    return_object jsonb;
	    i text;
	    notification json;
        
    BEGIN

        tablename:= '""' || TG_TABLE_SCHEMA || '"".""' || TG_TABLE_NAME || '""';
	    primarykeys:= ARRAY((SELECT a.attname FROM pg_index i JOIN pg_attribute a ON a.attrelid = i.indrelid AND a.attnum = ANY(i.indkey) WHERE  i.indrelid = tablename::regclass AND i.indisprimary));
	    return_object:= '{{}}'::jsonb;
        

        IF(TG_OP = 'INSERT' OR TG_OP = 'UPDATE') THEN
            js:= to_jsonb(NEW);
            FOREACH i IN ARRAY primarykeys
	        LOOP
		        return_object:= (SELECT jsonb_insert(return_object,ARRAY[i] , to_jsonb(js->>i), true));
	        END LOOP;
	    END IF;

        IF(TG_OP = 'DELETE') THEN
            js:= to_jsonb(OLD);
            FOREACH i IN ARRAY primarykeys
	        LOOP
		        return_object:= (SELECT jsonb_insert(return_object,ARRAY[i] , to_jsonb(js->>i), true));
	        END LOOP;
	    END IF;

        notification = json_build_object(
		    'table', TG_TABLE_SCHEMA || '.' || TG_TABLE_NAME,
		    'action', TG_OP,
		    'data', return_object);
		RAISE NOTICE '%',notification;
		PERFORM pg_notify('{lc.ProcedureName}', notification::text);

	    RETURN NEW;
    END
$$ LANGUAGE plpgsql;
            
    
";

            return lc;
        }

        public static ListenCommand ListenHistoryKeys(string schemaName, string tableName)
        {
            if (String.IsNullOrWhiteSpace(schemaName))
                throw new ArgumentNullException(nameof(schemaName));

            if (String.IsNullOrWhiteSpace(tableName))
                throw new ArgumentNullException(nameof(tableName));

            var lc = new ListenCommand();
            lc.ProcedureName = $"xa_n_historyKeys";

            lc.CommandText = $@"
CREATE OR REPLACE FUNCTION ""{schemaName}"".""{lc.ProcedureName}""() RETURNS TRIGGER AS $$

    DECLARE

        history_schema_table_name text;
        history_table_name text;
        insert_query text;
        history_table_columns text;
        primarykeys text[];
        return_object json;
        cid uuid;
        notification json;
        
    BEGIN

        history_table_name:= '""' || TG_TABLE_NAME || '#history""';
        history_schema_table_name:='""' ||  TG_TABLE_SCHEMA || '"".' || history_table_name;
        history_table_columns:= (SELECT array_to_string(array(SELECT '""' || column_name::text || '""' FROM information_schema.columns WHERE table_schema = TG_TABLE_SCHEMA AND table_name = TG_TABLE_NAME || '#history'), ','));


        primarykeys:= ARRAY((SELECT '""' || a.attname || '""' FROM pg_index i JOIN pg_attribute a ON a.attrelid = i.indrelid AND a.attnum = ANY(i.indkey) WHERE  i.indrelid = history_schema_table_name::regclass AND i.indisprimary));

        insert_query := 'INSERT INTO ' || history_schema_table_name || ' (' || history_table_columns || ') VALUES (DEFAULT,''' || TG_OP || ''',DEFAULT,$1.*) RETURNING ""#id""';

        IF(TG_OP = 'INSERT') THEN
            EXECUTE insert_query USING NEW INTO cid;
        END IF;

        IF(TG_OP = 'DELETE') THEN
            EXECUTE insert_query USING OLD INTO cid;
        END IF;

        IF(TG_OP = 'UPDATE') THEN
            EXECUTE insert_query USING NEW INTO cid;
            IF(OLD = NEW) THEN
                RETURN NULL;
            END IF;
        END IF;

        return_object:=(SELECT json_build_object('#id',cid));


        notification= json_build_object(
            'table', TG_TABLE_SCHEMA || '.' || TG_TABLE_NAME,
            'action', TG_OP,
            'data', return_object);

        PERFORM pg_notify('{lc.ProcedureName}', notification::text);

        RETURN NEW;

    END
$$ LANGUAGE plpgsql;
            
    
";
            return lc;
        }

        public static string ListenHistoryKeysTrigger(string schemaName, string tableName)
        {

            var triggerName = $"xa_n_historyKeysTrigger_{schemaName}_{tableName}";
            var procedureName = $"xa_n_historyKeys";
            return $@"
do $$
BEGIN
    IF NOT EXISTS (select 1 from pg_trigger where not tgisinternal AND tgname = '{triggerName}') THEN
        CREATE TRIGGER ""{triggerName}""
            AFTER INSERT OR UPDATE OR DELETE ON ""{schemaName}"".""{tableName}""
            FOR EACH ROW EXECUTE PROCEDURE ""{schemaName}"".""{procedureName}""();
    END IF;
END;
$$ LANGUAGE plpgsql;
";
        }

        public static string ListenReferenceKeysTrigger(string schemaName, string tableName)
        {

            var triggerName = $"xa_n_referenceKeysTrigger_{schemaName}_{tableName}";
            var procedureName = $"xa_n_referenceKeys";
            return $@"
do $$
BEGIN
    IF NOT EXISTS (select 1 from pg_trigger where not tgisinternal AND tgname = '{triggerName}') THEN
        CREATE TRIGGER ""{triggerName}""
            AFTER INSERT OR UPDATE OR DELETE ON ""{schemaName}"".""{tableName}""
            FOR EACH ROW EXECUTE PROCEDURE ""{schemaName}"".""{procedureName}""();
    END IF;
END;
$$ LANGUAGE plpgsql;
";
        }


        public static ListenCommand ListenOnDatabaseEvents()
        {

            var lc = new ListenCommand();
            lc.ProcedureName = $"xa_n_dbEvent";

            lc.CommandText = $@"
CREATE OR REPLACE FUNCTION {lc.ProcedureName}() RETURNS event_trigger AS $$
DECLARE
    r RECORD;
    notification json;
BEGIN
    
    FOR r IN SELECT * FROM pg_event_trigger_ddl_commands() LOOP
        notification= json_build_object(
            'action', r.command_tag,
            'schema', r.schema_name,
            'table', r.object_identity
        );

        PERFORM pg_notify('{lc.ProcedureName}', notification::text);
    END LOOP;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION {lc.ProcedureName}_dropped() RETURNS event_trigger AS $$
DECLARE
    r RECORD;
    notification json;
    action text;
BEGIN
    
    FOR r IN SELECT * FROM pg_event_trigger_dropped_objects() LOOP

        
        notification= json_build_object(
            'action', 'DROP ' || upper(r.object_type),
            'schema', r.schema_name,
            'table', r.object_identity
        );

        PERFORM pg_notify('{lc.ProcedureName}', notification::text);
    END LOOP;
END;
$$ LANGUAGE plpgsql;
";

            return lc;

        }

        public static string ListenOnDatabaseEventsTrigger()
        {

            var triggerName = $"xa_n_dbEventTrigger";
            var procedureName = $"xa_n_dbEvent";
            return $@"
do $$
BEGIN
    IF NOT EXISTS (select 1 from pg_event_trigger where evtname = '{triggerName}') THEN
        CREATE EVENT TRIGGER ""{triggerName}""
            ON ddl_command_end EXECUTE PROCEDURE {procedureName}();
    END IF;

    IF NOT EXISTS (select 1 from pg_event_trigger where evtname = '{triggerName}_dropped') THEN
        CREATE EVENT TRIGGER ""{triggerName}_dropped""
            ON sql_drop EXECUTE PROCEDURE {procedureName}_dropped();
    END IF;

END;
$$ LANGUAGE plpgsql;
";
        }
        #endregion

        #region Extension

        public static string ExtensionCreate(string extensionName, bool throwIfAlreadyExists)
        {

            if (String.IsNullOrWhiteSpace(extensionName))
                throw new ArgumentNullException(nameof(extensionName));

            if (throwIfAlreadyExists)
            {
                return $"CREATE EXTENSION \"{extensionName}\"";
            }
            else
            {
                return $"CREATE EXTENSION IF NOT EXISTS \"{extensionName}\"";
            }

        }

        public static string ExtensionDrop(string extensionName, bool throwIfNotExists)
        {

            if (String.IsNullOrWhiteSpace(extensionName))
                throw new ArgumentNullException(nameof(extensionName));

            if (throwIfNotExists)
            {
                return $"DROP EXTENSION \"{extensionName}\"";
            }
            else
            {
                return $"DROP EXTENSION IF EXISTS \"{extensionName}\"";
            }

        }

        public static string ExtensionList()
        {
            return $"SELECT extname AS \"name\",extversion AS \"version\" FROM pg_extension;";
        }

        public static string ExtensionExists(string extensionName)
        {

            if (String.IsNullOrWhiteSpace(extensionName))
                throw new ArgumentNullException(nameof(extensionName));

            return $"SELECT EXISTS(SELECT 1 FROM pg_extension WHERE extname = '{extensionName}');";
        }

        #endregion

        #region Types
        public static string DomainCreate(string name, string postgresTypeName)
        {

            if (String.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));

            name = $"\"{name.Trim("\"".ToCharArray())}\"";
            postgresTypeName = $"\"{postgresTypeName.Trim("\"".ToCharArray())}\"";

            var strbuilder = new StringBuilder();
            strbuilder.AppendLine($"CREATE DOMAIN {name} AS {postgresTypeName}");
            return strbuilder.ToString();
        }

        public static string DomainDrop(string name, bool throwIfNotExists)
        {

            if (String.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));

            name = $"\"{name.Trim("\"".ToCharArray())}\"";

            if (throwIfNotExists)
            {
                return $"DROP DOMAIN {name};";
            }
            else
            {
                return $"DROP DOMAIN IF EXISTS {name};";
            }

        }

        public static string DomainList()
        {

            return
                "SELECT typname AS \"name\" FROM pg_catalog.pg_type JOIN pg_catalog.pg_namespace ON pg_namespace.oid = pg_type.typnamespace WHERE typtype = 'd' AND nspname = 'public'";
        }

        public static string DomainExists(string name)
        {

            if (String.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));

            name = $"{name.Trim("\"".ToCharArray())}";

            return $"SELECT EXISTS(SELECT 1 FROM pg_catalog.pg_type JOIN pg_catalog.pg_namespace ON pg_namespace.oid = pg_type.typnamespace WHERE typname ='{name}' AND typtype = 'd' AND nspname = 'public')";
        }


        #endregion





        #region TriggerFunction

        public static string TriggerExistsForTriggerFunction(string triggerName)
        {
            triggerName = triggerName.EnsureEndsWith("_Trigger");
            return $"SELECT EXISTS (SELECT 1 FROM  information_schema.triggers WHERE trigger_name LIKE '{triggerName}::%')";
        }

        public static string DropTriggerFunction(string triggerFunctionName, string schema = "public")
        {
            return $"DROP FUNCTION IF EXISTS \"{schema}\".\"{triggerFunctionName}\"()";
        }

        #endregion


        #region Trigger

        public static string TableTriggerDrop(string triggerName, ITable table)
        {
            triggerName = triggerName.EnsureEndsWith("_Trigger");
            return $"DROP TRIGGER IF EXISTS \"{triggerName}::{table.GetConnectionString().SchemaName}.{table.GetConnectionString().TableName}\" ON \"{table.GetConnectionString().SchemaName}\".\"{table.GetConnectionString().TableName}\";";
        }

        public static string TableTriggerExists(string triggerName, ITable table)
        {
            triggerName = triggerName.EnsureEndsWith("_Trigger");
            return $"SELECT EXISTS(select 1 from pg_trigger where tgname = '{triggerName}::{table.GetConnectionString().SchemaName}.{table.GetConnectionString().TableName}');";
        }

        public static string TableTriggerCreate(string triggerName, string triggerFunctionName, ITable table)
        {
            triggerName = triggerName.EnsureEndsWith("_Trigger");
            return $"CREATE TRIGGER \"{triggerName}::{table.GetConnectionString().SchemaName}.{table.GetConnectionString().TableName}\" AFTER INSERT OR UPDATE OR DELETE ON \"{table.GetConnectionString().SchemaName}\".\"{table.GetConnectionString().TableName}\" FOR EACH ROW EXECUTE PROCEDURE \"{table.GetConnectionString().SchemaName}\".\"{triggerFunctionName}\"();";
        }


        #endregion

    }

    public class ListenCommand
    {
        public string ProcedureName { get; set; }
        public string CommandText { get; set; }
    }
}
