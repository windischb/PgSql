using System;

namespace doob.PgSql.Exceptions
{

    public class UnknownDefaultTypeValueException : Exception
    {

        public UnknownDefaultTypeValueException()
        {
        }

        public UnknownDefaultTypeValueException(Type type) : base($"Can't find DefaultValue for Type: {type.FullName}")
        {
        }

        public UnknownDefaultTypeValueException(Type type, Exception inner) : base($"Can't find DefaultValue for Type: {type.FullName}", inner)
        {
        }

    }
}
