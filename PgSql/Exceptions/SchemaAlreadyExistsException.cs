namespace doob.PgSql.Exceptions
{

    public class SchemaAlreadyExistsException : System.Exception
    {

        public SchemaAlreadyExistsException()
        {
        }

        public SchemaAlreadyExistsException(string schemaName) : base($"Schema with name '{schemaName}' already exists!")
        {
        }

        public SchemaAlreadyExistsException(string message, System.Exception inner) : base(message, inner)
        {
        }

    }
}
