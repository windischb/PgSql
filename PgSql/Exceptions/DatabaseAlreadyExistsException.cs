using System;

namespace doob.PgSql.Exceptions
{

    public class DatabaseAlreadyExistsException : Exception
    {

        public DatabaseAlreadyExistsException()
        {
        }

        public DatabaseAlreadyExistsException(string databaseName) : base($"Database with name '{databaseName}' already exists!")
        {
        }

        public DatabaseAlreadyExistsException(string message, Exception inner) : base(message, inner)
        {
        }

    }
}
