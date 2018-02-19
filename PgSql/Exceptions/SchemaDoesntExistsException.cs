using System;

namespace doob.PgSql.Exceptions
{

    public class SchemaDoesntExistsException : Exception
    {

        public SchemaDoesntExistsException()
        {
        }

        public SchemaDoesntExistsException(string schemaName) : base($"Schema with name '{schemaName}' doesn't exists!")
        {
        }

        public SchemaDoesntExistsException(string message, Exception inner) : base(message, inner)
        {
        }

    }
}
