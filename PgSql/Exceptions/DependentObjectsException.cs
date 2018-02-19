using System;

namespace doob.PgSql.Exceptions
{

    public class DependentObjectsException : System.Exception
    {

        public DependentObjectsException()
        {
        }

        public DependentObjectsException(string details) : base($"'Cant't delete Schema because of Dependent Objects exists{(!String.IsNullOrWhiteSpace(details) ? ": " : String.Empty)}{details}'")
        {
        }

        public DependentObjectsException(string message, System.Exception inner) : base(message, inner)
        {
        }

    }
}
