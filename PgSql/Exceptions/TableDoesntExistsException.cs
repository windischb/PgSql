namespace doob.PgSql.Exceptions
{

    public class TableDoesntExistsException : System.Exception
    {
        public TableDoesntExistsException()
        {
        }

        public TableDoesntExistsException(string tableName) : base($"Table with name '{tableName}' doesn't exists!")
        {
        }

        public TableDoesntExistsException(string message, System.Exception inner) : base(message, inner)
        {
        }
    }
}
