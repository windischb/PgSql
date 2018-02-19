using System;

namespace doob.PgSql.Exceptions
{

    public class TableAlreadyExistsException : Exception
    {
        public TableAlreadyExistsException()
        {
        }

        public TableAlreadyExistsException(string tableName) : base($"Table with name '{tableName}' already exists!")
        {
        }

        public TableAlreadyExistsException(string message, Exception inner) : base(message, inner)
        {
        }

    }
}
