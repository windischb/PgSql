using System;

namespace doob.PgSql.Exceptions
{
    public class ColumnAlreadyExistsException : Exception
    {

        public ColumnAlreadyExistsException()
        {
        }

        public ColumnAlreadyExistsException(string columnName) : base($"Column with name '{columnName}' already exists!")
        {
        }

        public ColumnAlreadyExistsException(string message, Exception inner) : base(message, inner)
        {
        }

    }
}
