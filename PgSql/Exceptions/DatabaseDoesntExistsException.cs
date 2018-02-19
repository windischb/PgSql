namespace doob.PgSql.Exceptions
{

    public class DatabaseDoesntExistsException : System.Exception
    {

        public DatabaseDoesntExistsException()
        {
        }

        public DatabaseDoesntExistsException(string databaseName) : base($"Database with name '{databaseName}' doesn't exists!")
        {
        }

        public DatabaseDoesntExistsException(string message, System.Exception inner) : base(message, inner)
        {
        }
    }
}
