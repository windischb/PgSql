using System;

namespace doob.PgSql.Tables
{
    internal static class TableStatements
    {
        public static string BuildTriggerFunction(string schemaName, string functionName)
        {
            if (String.IsNullOrWhiteSpace(schemaName))
                throw new ArgumentNullException(nameof(schemaName));

            if (String.IsNullOrWhiteSpace(functionName))
                throw new ArgumentNullException(nameof(functionName));

            return $@"
CREATE OR REPLACE FUNCTION ""{schemaName}"".""{functionName}""() RETURNS TRIGGER AS $$

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
            'schema', TG_TABLE_SCHEMA,
            'table', TG_TABLE_NAME,
            'action', TG_OP,
            'data', return_object);
		
		PERFORM pg_notify('{functionName}', notification::text);

	    RETURN NEW;
    END
$$ LANGUAGE plpgsql;
            
    
";

        }

        public static string AddHistoryTrigger(Schema schema)
        {

            if (schema == null)
                throw new ArgumentNullException(nameof(schema));


            return $@"
CREATE OR REPLACE FUNCTION ""{schema.ConnectionString.SchemaName}"".""WriteHistory""() RETURNS TRIGGER AS $$

    DECLARE

        history_schema_name text;
        history_schema_table_name text;
        insert_query text;

    BEGIN

        history_schema_name:= '""' || TG_TABLE_SCHEMA || '#history""';
        history_schema_table_name:= history_schema_name || '.""' ||  TG_TABLE_NAME || '""';

        insert_query := 'INSERT INTO ' || history_schema_table_name || ' (""Id"",""Action"",""Old"",""New"",""Timestamp"") VALUES (DEFAULT,''' || TG_OP || ''',to_jsonb($1),to_jsonb($2),DEFAULT)';

        IF(TG_OP = 'INSERT') THEN
            EXECUTE insert_query USING NULL,NEW;
        END IF;

        IF(TG_OP = 'DELETE') THEN
            EXECUTE insert_query USING OLD,NULL;
        END IF;

        IF(TG_OP = 'UPDATE') THEN
            EXECUTE insert_query USING OLD,NEW;
            IF(OLD = NEW) THEN
                RETURN NULL;
            END IF;
        END IF;

        RETURN NEW;

    END
$$ LANGUAGE plpgsql;
";

        }



       

    }
}
