namespace doob.PgSql.Attributes
{
    public class PgSqlValueAttribute : PgSqlAttribute
    {
        public object Value { get; protected set; }
    }
}