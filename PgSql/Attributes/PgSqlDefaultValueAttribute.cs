namespace doob.PgSql.Attributes
{
    public class PgSqlDefaultValueAttribute : PgSqlValueAttribute
    {

        public PgSqlDefaultValueAttribute(string value)
        {
            Value = value;
        }
    }
}
