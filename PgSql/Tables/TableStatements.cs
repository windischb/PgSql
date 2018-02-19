using System;

namespace doob.PgSql.Tables
{
    internal static class TableStatements
    {
        public static string BuildTriggerFunction_ByReferenceKeys(string schemaName, string functionName, ListenMode mode)
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
            'mode', '{mode}',
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

        public static string BuildTriggerFunction_ByHistoryKeys(string schemaName, string functionName, ListenMode mode)
        {
            if (String.IsNullOrWhiteSpace(schemaName))
                throw new ArgumentNullException(nameof(schemaName));

            if (String.IsNullOrWhiteSpace(functionName))
                throw new ArgumentNullException(nameof(functionName));


            return $@"
CREATE OR REPLACE FUNCTION ""{schemaName}"".""{functionName}""() RETURNS TRIGGER AS $$

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
            'mode', '{mode}',
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

    }
}
