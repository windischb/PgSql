namespace doob.PgSql.Attributes
{
    public class PgSqlDefaultValueAttribute : PgSqlAttribute
    {
        public string OnCreate { get; private set; }

        public PgSqlDefaultValueAttribute(string onCreate)
        {
            OnCreate = onCreate;
        }
    }
}
