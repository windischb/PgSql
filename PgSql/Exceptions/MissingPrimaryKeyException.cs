using System;

namespace doob.PgSql.Exceptions
{

    public class MissingPrimaryKeyException : Exception
    {
        public MissingPrimaryKeyException()
        {
        }

        public MissingPrimaryKeyException(string message) : base(message)
        {
        }

        public MissingPrimaryKeyException(string message, Exception inner) : base(message, inner)
        {
        }

    }
}
