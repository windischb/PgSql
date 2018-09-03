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

BEGIN;

SELECT pg_advisory_xact_lock({GetRandomLong()});


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
            
COMMIT;
";

        }

        public static string AddHistoryTrigger(Schema schema)
        {

            if (schema == null)
                throw new ArgumentNullException(nameof(schema));


            return $@"

BEGIN;

SELECT pg_advisory_xact_lock({GetRandomLong()});

CREATE OR REPLACE FUNCTION ""{schema.GetConnectionString().SchemaName}"".""WriteHistory""() RETURNS TRIGGER AS $$

    DECLARE

        history_schema_name text;
        history_schema_table_name text;
        insert_query text;
        changesOld hstore;
        changesNew hstore;
        changedKeys text[] := '{{}}';
    BEGIN

        history_schema_name:= '""' || TG_TABLE_SCHEMA || '#history""';
        history_schema_table_name:= history_schema_name || '.""' ||  TG_TABLE_NAME || '""';

        
        insert_query := 'INSERT INTO ' || history_schema_table_name || ' (""Id"",""Action"",""Old"",""New"",""ChangesOld"",""ChangesNew"",""ChangedKeys"",""Timestamp"") VALUES (DEFAULT,''' || TG_OP || ''',to_jsonb($1),to_jsonb($2),to_jsonb($3),to_jsonb($4),$5,DEFAULT)';

        IF(TG_OP = 'INSERT') THEN
            EXECUTE insert_query USING NULL,NEW,NULL,NULL,changedKeys;
        END IF;

        IF(TG_OP = 'DELETE') THEN
            EXECUTE insert_query USING OLD,NULL,NULL,NULL,changedKeys;
        END IF;

        IF(TG_OP = 'UPDATE') THEN
            IF(OLD = NEW) THEN
                RETURN NULL;
            END IF;
            changesOld := hstore(OLD) - hstore(NEW);
            changesNew := hstore(NEW) - hstore(OLD);
            changedKeys := akeys(changesNew);
            EXECUTE insert_query USING OLD,NEW,changesOld,changesNew,changedKeys;
            
        END IF;

        RETURN NEW;

    END
$$ LANGUAGE plpgsql;

COMMIT;
";

        }


        private static long GetRandomLong()
        {
            Random rnd = new Random();

            byte[] buf = new byte[8];
            rnd.NextBytes(buf);
            long longRand = BitConverter.ToInt64(buf, 0);

            long result = (Math.Abs(longRand % (2000000000000000 - 1000000000000000)) + 1000000000000000);

            long random_seed = (long)rnd.Next(1000, 5000);
            random_seed = random_seed * result + rnd.Next(1000, 5000);

            return ((long)(random_seed / 655) % 10000000000000001);
        }
       

    }
}
